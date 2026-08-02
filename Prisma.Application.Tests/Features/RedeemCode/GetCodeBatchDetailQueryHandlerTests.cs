using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeBatchDetail;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Tests.Features.RedeemCodes.Queries;

public class GetCodeBatchDetailQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<RedeemCodeEntity, int> _batchRepo = Substitute.For<IRepository<RedeemCodeEntity, int>>();
    private readonly GetCodeBatchDetailQueryHandler _sut;

    private readonly Guid _teacherId = Guid.NewGuid();

    public GetCodeBatchDetailQueryHandlerTests()
    {
        _currentUser.UserId.Returns(_teacherId);
        _unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>().Returns(_batchRepo);
        _sut = new GetCodeBatchDetailQueryHandler(_unitOfWork, _currentUser);
    }

    [Fact]
    public async Task Handle_WhenBatchExists_ReturnsBatchWithCodes()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var redeemedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var fakeBatch = new RedeemCodeEntity
        {
            Id = 1,
            AcademicYearId = 1,
            AcademicYear = new AcademicYear { Title = "الأول الثانوي" },
            LessonId = 1,
            Lesson = new Domain.Entities.LessonAggregate.Lesson { Title = "الكهرباء الساكنة" },
            TotalCodes = 2,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            GeneratedCodes = new List<GeneratedCode>
            {
                new()
                {
                    Id = 1,
                    Code = "ABCD-EFGH",
                    RedeemedByStudentId = studentId,
                    RedeemedAt = redeemedAt,
                    RedeemedByStudent = new Student
                    {
                        FirstName = "محمد",
                        SecondName = null,
                        ThirdName = null,
                        LastName = "أحمد",
                    },
                },
                new()
                {
                    Id = 2,
                    Code = "IJKL-MNOP",
                    RedeemedByStudentId = null,
                    RedeemedAt = null,
                    RedeemedByStudent = null,
                },
            },
        };

        _batchRepo.FirstOrDefaultAsync(
            Arg.Any<CodeBatchByIdSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(fakeBatch);

        // Act
        var result = await _sut.Handle(new GetCodeBatchDetailQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Codes.Should().HaveCount(2);
        result.Value.UsedCodes.Should().Be(1);
        result.Value.TotalCodes.Should().Be(2);

        var usedCode = result.Value.Codes.First(c => c.Code == "ABCD-EFGH");
        usedCode.Status.Should().Be("used");
        usedCode.UsedBy.Should().Be("محمد أحمد");

        var availableCode = result.Value.Codes.First(c => c.Code == "IJKL-MNOP");
        availableCode.Status.Should().Be("available");
        availableCode.UsedBy.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenBatchNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _batchRepo.FirstOrDefaultAsync(
            Arg.Any<CodeBatchByIdSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns((RedeemCodeEntity?)null);

        // Act
        var result = await _sut.Handle(new GetCodeBatchDetailQuery(99), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(new GetCodeBatchDetailQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }
}