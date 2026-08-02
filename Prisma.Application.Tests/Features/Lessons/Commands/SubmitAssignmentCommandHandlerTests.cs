using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class SubmitAssignmentCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Assignment, int> _assignmentRepo = Substitute.For<IRepository<Assignment, int>>();
    private readonly SubmitAssignmentCommandHandler _sut;

    public SubmitAssignmentCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Assignment, int>().Returns(_assignmentRepo);
        _sut = new SubmitAssignmentCommandHandler(_unitOfWork, _storage, _currentUser);
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotFound_ThrowsBadRequestException()
    {
        _assignmentRepo.FirstOrDefaultAsync(Arg.Any<AssignmentWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns((Assignment?)null);

        var command = new SubmitAssignmentCommand(1, Substitute.For<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("لا يوجد واجب لهذا الدرس");
    }

    [Fact]
    public async Task Handle_WhenStudentNotEnrolled_ThrowsBadRequestException()
    {
        var studentId = Guid.NewGuid();
        _currentUser.UserId.Returns(studentId);

        var assignment = new Assignment { Lesson = new Lesson { Enrollments = new List<Enrollment>() } };
        _assignmentRepo.FirstOrDefaultAsync(Arg.Any<AssignmentWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new SubmitAssignmentCommand(1, Substitute.For<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("غير مصرح لك بتسليم هذا الواجب");
    }

    [Fact]
    public async Task Handle_ValidRequest_UploadsFileAndSavesSubmission()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _currentUser.UserId.Returns(studentId);

        var mockFile = Substitute.For<IFormFile>();
        mockFile.FileName.Returns("homework.pdf");
        mockFile.ContentType.Returns("application/pdf");
        mockFile.OpenReadStream().Returns(new MemoryStream());

        var assignment = new Assignment
        {
            Id = 10,
            Lesson = new Lesson { Enrollments = new List<Enrollment> { new Enrollment { StudentId = studentId } } },
            Submissions = new List<AssignmentSubmission>()
        };

        _assignmentRepo.FirstOrDefaultAsync(Arg.Any<AssignmentWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        // محاكاة رفع الملف
        _storage.UploadFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new SubmitAssignmentCommand(1, mockFile);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("تم التسليم بنجاح!");

        assignment.Submissions.Should().ContainSingle();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}