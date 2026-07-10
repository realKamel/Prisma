using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Commands.CreateCodeBatch;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;

using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Tests.Features.RedeemCode;

public class CreateCodeBatchCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<AcademicYearLesson, int> _ayLessonRepo = Substitute.For<IRepository<AcademicYearLesson, int>>();
    private readonly IRepository<AcademicYearTeacher, int> _ayTeacherRepo = Substitute.For<IRepository<AcademicYearTeacher, int>>();
    private readonly IRepository<RedeemCodeEntity, int> _batchRepo = Substitute.For<IRepository<RedeemCodeEntity, int>>();
    private readonly CreateCodeBatchCommandHandler _sut;

    private readonly Guid _teacherId = Guid.NewGuid();

    public CreateCodeBatchCommandHandlerTests()
    {
        _currentUser.UserId.Returns(_teacherId);

        _unitOfWork.GetOrCreateRepository<AcademicYearLesson, int>().Returns(_ayLessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYearTeacher, int>().Returns(_ayTeacherRepo);
        _unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>().Returns(_batchRepo);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _sut = new CreateCodeBatchCommandHandler(_unitOfWork, _currentUser);
    }

    [Fact]
    public async Task Handle_WhenValid_ReturnsBatchWithCorrectCodeCount()
    {
        // Arrange
        var command = new CreateCodeBatchCommand(1, 2, 10, null);

        _ayLessonRepo.AnyAsync(
            Arg.Any<AcademicYearLessonExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _ayTeacherRepo.AnyAsync(
            Arg.Any<TeacherAcademicYearExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Codes.Should().HaveCount(10);
        result.Data.Codes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Handle_WhenPrefixProvided_CodesStartWithPrefix()
    {
        // Arrange
        var command = new CreateCodeBatchCommand(1, 2, 5, "PHY");

        _ayLessonRepo.AnyAsync(
            Arg.Any<AcademicYearLessonExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _ayTeacherRepo.AnyAsync(
            Arg.Any<TeacherAcademicYearExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Codes.Should().AllSatisfy(c => c.Should().StartWith("PHY-"));
    }

    [Fact]
    public async Task Handle_WhenLessonNotInAcademicYear_ThrowsBadRequestException()
    {
        // Arrange
        var command = new CreateCodeBatchCommand(1, 2, 10, null);

        _ayLessonRepo.AnyAsync(
            Arg.Any<AcademicYearLessonExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*lesson*academic year*");
    }

    [Fact]
    public async Task Handle_WhenTeacherHasNoAccessToAcademicYear_ThrowsForbiddenException()
    {
        // Arrange
        var command = new CreateCodeBatchCommand(1, 2, 10, null);

        _ayLessonRepo.AnyAsync(
            Arg.Any<AcademicYearLessonExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _ayTeacherRepo.AnyAsync(
            Arg.Any<TeacherAcademicYearExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);
        var command = new CreateCodeBatchCommand(1, 2, 10, null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenValid_SavesChanges()
    {
        // Arrange
        var command = new CreateCodeBatchCommand(1, 2, 10, null);

        _ayLessonRepo.AnyAsync(
            Arg.Any<AcademicYearLessonExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _ayTeacherRepo.AnyAsync(
            Arg.Any<TeacherAcademicYearExistsSpecification>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}