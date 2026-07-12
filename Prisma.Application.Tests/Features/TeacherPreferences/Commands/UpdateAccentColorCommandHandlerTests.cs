
using Ardalis.Specification;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using TeacherPreferencesEntity = Prisma.Domain.Entities.TeacherPreferences;

namespace Prisma.Application.Tests.Features.TeacherPreferences.Commands;

public class UpdateAccentColorCommandHandlerTests
{

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<TeacherPreferencesEntity, Guid> _repository = Substitute.For<IRepository<TeacherPreferencesEntity, Guid>>();
    private readonly UpdateAccentColorCommandHandler _handler;

    private static readonly Guid TeacherId = Guid.NewGuid();
    private static readonly UpdateAccentColorCommand ValidCommand = new(AccentColor.Purple);

    public UpdateAccentColorCommandHandlerTests()
    {
        _unitOfWork
            .GetOrCreateRepository<TeacherPreferencesEntity, Guid>()
            .Returns(_repository);

        _handler = new UpdateAccentColorCommandHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsFailure()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("المستخدم غير مصرح له", result.Message);

        await _repository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPreferencesDoNotExist_CreatesNewPreferencesWithRequestedColor()
    {
        // Arrange
        _currentUserService.UserId.Returns(TeacherId);

        _repository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>())
            .Returns((TeacherPreferencesEntity?)null);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _repository.Received(1).Add(
            Arg.Is<TeacherPreferencesEntity>(p =>
                p.Id == TeacherId &&
                p.AccentColor == ValidCommand.AccentColor));

        _repository.DidNotReceive().Update(Arg.Any<TeacherPreferencesEntity>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPreferencesExist_UpdatesExistingPreferences()
    {
        // Arrange
        _currentUserService.UserId.Returns(TeacherId);

        var existingPreferences = TeacherPreferencesEntity.CreateDefault(TeacherId);

        _repository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>())
            .Returns(existingPreferences);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(ValidCommand.AccentColor, existingPreferences.AccentColor);

        _repository.Received(1).Update(
            Arg.Is<TeacherPreferencesEntity>(p => p.AccentColor == ValidCommand.AccentColor));

        _repository.DidNotReceive().Add(Arg.Any<TeacherPreferencesEntity>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveSucceeds_ReturnsSuccessMessage()
    {
        // Arrange
        _currentUserService.UserId.Returns(TeacherId);

        _repository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>())
            .Returns((TeacherPreferencesEntity?)null);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("تم حفظ اللون بنجاح", result.Message);
        Assert.Null(result.Errors);
    }

}
