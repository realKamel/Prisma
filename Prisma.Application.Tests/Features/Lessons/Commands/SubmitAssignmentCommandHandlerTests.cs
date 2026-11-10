using Ardalis.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.AssignmentSubmissions.Commands.SubmitAssignment;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class SubmitAssignmentCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Assignment, int> _assignmentRepo = Substitute.For<
        IRepository<Assignment, int>
    >();
    private readonly SubmitAssignmentCommandHandler _sut;

    public SubmitAssignmentCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Assignment, int>().Returns(_assignmentRepo);
        _sut = new SubmitAssignmentCommandHandler(_unitOfWork, _storage, _currentUser);
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotFound_ThrowsBadRequestException()
    {
        _assignmentRepo
            .FirstOrDefaultAsync(
                Arg.Any<AssignmentWithEnrollmentSpec>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Assignment?)null);

        var command = new SubmitAssignmentCommand(Guid.NewGuid(), Substitute.For<IFormFile>());

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

        var assignment = new Assignment
        {
            Lesson = new Lesson { Enrollments = new List<Enrollment>() },
        };
        _assignmentRepo
            .FirstOrDefaultAsync(
                Arg.Any<AssignmentWithEnrollmentSpec>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(assignment);

        var command = new SubmitAssignmentCommand(Guid.NewGuid(), Substitute.For<IFormFile>());

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
            Lesson = new Lesson { Enrollments = [new Enrollment { StudentId = studentId }] },
            Submissions = [],
        };

        _assignmentRepo
            .FirstOrDefaultAsync(
                Arg.Any<AssignmentWithEnrollmentSpec>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(assignment);

        _storage
            .UploadFileAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        var command = new SubmitAssignmentCommand(Guid.NewGuid(), mockFile);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var submission = assignment.Submissions.Should().ContainSingle().Subject;
        submission.StudentId.Should().Be(studentId);
        submission.AssignmentId.Should().Be(assignment.Id);
        submission.Title.Should().Be("homework.pdf");
        submission.FileUrl.Should().StartWith($"assignments/{assignment.Id}/students/{studentId}/");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
