using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;


namespace Prisma.Application.Tests.Features.Lessons.Commands;

public class DeleteAssignmentSubmissionCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IRepository<Assignment, int> _assignmentRepo = Substitute.For<IRepository<Assignment, int>>();
    private readonly DeleteSubmissionCommandHandler _sut;

    public DeleteAssignmentSubmissionCommandHandlerTests()
    {
        _storage.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Assignment, int>().Returns(_assignmentRepo);
        _sut = new DeleteSubmissionCommandHandler(_unitOfWork, _currentUser, _storage);
    }

    [Fact]
    public async Task Handle_WhenStudentNotAuthenticated_ThrowsUnauthorizedException()
    {
        _currentUser.UserId.Returns((Guid?)null);
        var command = new DeleteSubmissionCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("سجل دخولك اولا");
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotFound_ThrowsNotFoundException()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _assignmentRepo.FirstOrDefaultAsync(Arg.Any<AssignmentWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns((Assignment?)null);

        var command = new DeleteSubmissionCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDueDatePassed_ThrowsBadRequestException()
    {
        var studentId = Guid.NewGuid();
        _currentUser.UserId.Returns(studentId);

        var assignment = new Assignment { DueDate = DateTimeOffset.UtcNow.AddDays(-1) };
        _assignmentRepo.FirstOrDefaultAsync(Arg.Any<AssignmentWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new DeleteSubmissionCommand(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("انتهى الموعد النهائي للتسليم");
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesFileAndRemovesSubmission()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _currentUser.UserId.Returns(studentId);

        var submission = new AssignmentSubmission { StudentId = studentId, FileUrl = "path/to/file.pdf" };
        var assignment = new Assignment
        {
            DueDate = DateTimeOffset.UtcNow.AddDays(1), Submissions = new List<AssignmentSubmission> { submission }
        };

        _assignmentRepo.FirstOrDefaultAsync(Arg.Any<AssignmentWithEnrollmentSpec>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new DeleteSubmissionCommand(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("تم حذف التسليم بنجاح");

        // التأكد من استدعاء حذف الملف من الـ Storage
        await _storage.Received(1).DeleteFileAsync("prisma", "path/to/file.pdf", Arg.Any<CancellationToken>());

        // التأكد من حذف التسليم من قائمة الـ Submissions وحفظ التغييرات
        assignment.Submissions.Should().NotContain(submission);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}