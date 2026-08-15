using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;
using Prisma.Application.Features.TeacherStudents;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;
using Prisma.Domain.Specifications.Teacher;
using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Tests.Features.RedeemCode;

public class RedeemCodeCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<IRepository<Student, Guid>>();

    private readonly IRepository<GeneratedCode, int> _generatedCodeRepo =
        Substitute.For<IRepository<GeneratedCode, int>>();

    private readonly IRepository<RedeemCodeEntity, int> _batchRepo =
        Substitute.For<IRepository<RedeemCodeEntity, int>>();

    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();

    private readonly IRepository<TeacherStudent, int> _teacherStudentRepo =
        Substitute.For<IRepository<TeacherStudent, int>>();

    private readonly RedeemCodeCommandHandler _sut;

    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _teacherId = Guid.NewGuid();

    public RedeemCodeCommandHandlerTests()
    {
        _currentUser.UserId.Returns(_studentId);

        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _unitOfWork.GetOrCreateRepository<GeneratedCode, int>().Returns(_generatedCodeRepo);
        _unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>().Returns(_batchRepo);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _unitOfWork.GetOrCreateRepository<TeacherStudent, int>().Returns(_teacherStudentRepo);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Default: no existing pairing, so happy-path tests create one without extra setup
        _teacherStudentRepo.AnyAsync(Arg.Any<TeacherStudentPairSpec>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _sut = new RedeemCodeCommandHandler(_unitOfWork, _currentUser);
    }

    private Student MakeStudent(int academicYearId = 1) => new()
    {
        Id = _studentId,
        FirstName = "محمد",
        LastName = "أحمد",
        AcademicYearId = academicYearId,
    };

    private GeneratedCode MakeAvailableCode(int batchId = 1) => new()
    {
        Id = 10,
        Code = "ABCD-EFGH",
        BatchId = batchId,
        RedeemedByStudentId = null,
    };

    private CodeBatchLessonInfo MakeBatchInfo(int lessonId = 5, int academicYearId = 1, Guid? teacherId = null) =>
        new(lessonId, academicYearId, teacherId ?? _teacherId);

    private void SetupBatch(CodeBatchLessonInfo batchInfo)
    {
        _batchRepo.FirstOrDefaultAsync(
                Arg.Any<CodeBatchWithProjectionSpec<CodeBatchLessonInfo>>(),
                Arg.Any<CancellationToken>())
            .Returns(batchInfo);
    }

    [Fact]
    public async Task Handle_WhenAllValid_CreatesEnrollmentAndReturnsResponse()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent(academicYearId: 1));

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        SetupBatch(MakeBatchInfo(lessonId: 5, academicYearId: 1));

        _enrollmentRepo.FirstOrDefaultAsync(
                Arg.Any<EnrollmentByStudentAndLessonSpec>(),
                Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllValidAndNoPairingExists_CreatesTeacherStudentPairing()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent(academicYearId: 1));

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        SetupBatch(MakeBatchInfo(lessonId: 5, academicYearId: 1));

        _enrollmentRepo.FirstOrDefaultAsync(
                Arg.Any<EnrollmentByStudentAndLessonSpec>(),
                Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        // Act
        await _sut.Handle(new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        _teacherStudentRepo.Received(1).Add(Arg.Is<TeacherStudent>(
            ts => ts.TeacherId == _teacherId && ts.StudentId == _studentId));
    }

    [Fact]
    public async Task Handle_WhenPairingAlreadyExists_DoesNotAddDuplicate()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent(academicYearId: 1));

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        SetupBatch(MakeBatchInfo(lessonId: 5, academicYearId: 1));

        _enrollmentRepo.FirstOrDefaultAsync(
                Arg.Any<EnrollmentByStudentAndLessonSpec>(),
                Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        _teacherStudentRepo.AnyAsync(Arg.Any<TeacherStudentPairSpec>(), Arg.Any<CancellationToken>())
            .Returns(true); // pairing already exists

        // Act
        await _sut.Handle(new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        _teacherStudentRepo.DidNotReceive().Add(Arg.Any<TeacherStudent>());
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyUsed_ThrowsBadRequestException()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent());

        var usedCode = MakeAvailableCode();
        usedCode.RedeemedByStudentId = Guid.NewGuid(); // already redeemed

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(usedCode);

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("اتستخدم"));
    }

    [Fact]
    public async Task Handle_WhenLessonMismatch_ThrowsBadRequestException()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent(academicYearId: 1));

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        SetupBatch(MakeBatchInfo(lessonId: 99, academicYearId: 1)); // different lesson

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None); // requested lessonId=5

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("مش للدرس ده"));
    }

    [Fact]
    public async Task Handle_WhenAcademicYearMismatch_ThrowsBadRequestException()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent(academicYearId: 1)); // student is in year 1

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        SetupBatch(MakeBatchInfo(lessonId: 5, academicYearId: 2)); // batch is for year 2

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("الكود ده مش للسنة الدراسية بتاعتك");
    }

    [Fact]
    public async Task Handle_WhenStudentAlreadyEnrolled_ThrowsBadRequestException()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent(academicYearId: 1));

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        SetupBatch(MakeBatchInfo(lessonId: 5, academicYearId: 1));

        _enrollmentRepo.FirstOrDefaultAsync(
                Arg.Any<EnrollmentByStudentAndLessonSpec>(),
                Arg.Any<CancellationToken>())
            .Returns(new Enrollment { IsDeleted = false }); // active enrollment exists

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("عندك الدرس ده"));
    }

    [Fact]
    public async Task Handle_WhenStudentNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenStudentHasNoAcademicYear_ThrowsBadRequestException()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(new Student
            {
                Id = _studentId,
                FirstName = "محمد",
                LastName = "أحمد",
                AcademicYearId = null, // not assigned
            });

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("academic year"));
    }

    [Fact]
    public async Task Handle_WhenCodeNotFound_ThrowsBadRequestException()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent());

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns((GeneratedCode?)null);

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("WRONG-CODE", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("الكود غلط"));
    }

    [Fact]
    public async Task Handle_WhenBatchNotFound_ReturnsNotFound()
    {
        // Arrange
        _studentRepo.GetByIdAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(MakeStudent());

        _generatedCodeRepo.FirstOrDefaultAsync(
                Arg.Any<GeneratedCodeByValueSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(MakeAvailableCode(batchId: 1));

        _batchRepo.FirstOrDefaultAsync(
                Arg.Any<CodeBatchWithProjectionSpec<CodeBatchLessonInfo>>(),
                Arg.Any<CancellationToken>())
            .Returns((CodeBatchLessonInfo?)null);

        // Act
        var result = await _sut.Handle(
            new RedeemCodeCommand("ABCD-EFGH", 5), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}