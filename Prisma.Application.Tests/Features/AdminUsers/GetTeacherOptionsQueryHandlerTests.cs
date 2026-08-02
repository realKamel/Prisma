using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Users.Queries.GetTeacherOptions;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class GetTeacherOptionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly IRepository<Domain.Entities.UserAggregate.Teacher, Guid> _teacherRepo =
        Substitute.For<IRepository<Domain.Entities.UserAggregate.Teacher, Guid>>();

    private readonly GetTeacherOptionsQueryHandler _sut;

    public GetTeacherOptionsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Domain.Entities.UserAggregate.Teacher, Guid>()
            .Returns(_teacherRepo);
        _sut = new GetTeacherOptionsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsTeacherOptionsWithConcatenatedNames()
    {
        // Arrange
        var teachers = new List<Domain.Entities.UserAggregate.Teacher>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "خالد",
                SecondName = "عبدالله",
                ThirdName = null,
                LastName = "فؤاد"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "ليلى",
                SecondName = null,
                ThirdName = null,
                LastName = "كمال"
            },
        };

        _teacherRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(teachers);

        // Act
        var result = await _sut.Handle(new GetTeacherOptionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(t => t.Name == "خالد عبدالله فؤاد");
        // null SecondName/ThirdName should be skipped, not leave stray double spaces
        result.Value.Should().Contain(t => t.Name == "ليلى كمال");
    }
}