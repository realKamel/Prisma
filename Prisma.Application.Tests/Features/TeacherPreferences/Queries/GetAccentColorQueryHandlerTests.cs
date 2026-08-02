using Ardalis.Specification;
using NSubstitute;
using Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using TeacherPreferencesEntity = Prisma.Domain.Entities.TeacherPreferences;


namespace Prisma.Application.Tests.Features.TeacherPreferences.Queries;

public class GetAccentColorQueryHandlerTests
{

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<User, Guid> _userRepository = Substitute.For<IRepository<User, Guid>>();
    private readonly IRepository<TeacherPreferencesEntity, Guid> _preferencesRepository =
        Substitute.For<IRepository<TeacherPreferencesEntity, Guid>>();
    private readonly GetAccentColorQueryHandler _handler;

    private static readonly GetAccentColorQuery ValidQuery = new("teacher@example.com");

    public GetAccentColorQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<User, Guid>().Returns(_userRepository);
        _unitOfWork.GetOrCreateRepository<TeacherPreferencesEntity, Guid>().Returns(_preferencesRepository);

        _handler = new GetAccentColorQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTeacherNotFound_ReturnsDefaultPurpleWithoutQueryingPreferences()
    {
        // Arrange
        _userRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(AccentColor.Purple, result.Value.AccentColor);

        await _preferencesRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTeacherFoundButNoPreferencesSaved_ReturnsDefaultPurple()
    {
        // Arrange
        var teacher = new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };

        _userRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(teacher);

        _preferencesRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>())
            .Returns((TeacherPreferencesEntity?)null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(AccentColor.Purple, result.Value.AccentColor);
    }

    [Fact]
    public async Task Handle_WhenTeacherHasSavedPreferences_ReturnsSavedAccentColor()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var teacher = new User { Id = teacherId, FirstName = "John", LastName = "Doe" };

        _userRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(teacher);

        var savedPreferences = TeacherPreferencesEntity.CreateDefault(teacherId);
        savedPreferences.UpdateAccentColor(AccentColor.Teal);

        _preferencesRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<TeacherPreferencesEntity>>(), Arg.Any<CancellationToken>())
            .Returns(savedPreferences);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(AccentColor.Teal, result.Value.AccentColor);
    }

}
