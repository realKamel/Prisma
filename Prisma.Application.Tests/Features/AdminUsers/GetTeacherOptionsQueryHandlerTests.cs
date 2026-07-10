using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Users.Queries.GetTeacherOptions;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class GetTeacherOptionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Teacher, Guid> _teacherRepo = Substitute.For<IRepository<Teacher, Guid>>();
    private readonly GetTeacherOptionsQueryHandler _sut;

    public GetTeacherOptionsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Teacher, Guid>().Returns(_teacherRepo);
        _sut = new GetTeacherOptionsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsTeacherOptionsWithConcatenatedNames()
    {
        // Arrange
        var teachers = new List<Teacher>
        {
            new() { Id = Guid.NewGuid(), FirstName = "خالد", SecondName = "عبدالله", ThirdName = null, LastName = "فؤاد" },
            new() { Id = Guid.NewGuid(), FirstName = "ليلى", SecondName = null, ThirdName = null, LastName = "كمال" },
        };

        _teacherRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(teachers);

        // Act
        var result = await _sut.Handle(new GetTeacherOptionsQuery(), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(t => t.Name == "خالد عبدالله فؤاد");
        // null SecondName/ThirdName should be skipped, not leave stray double spaces
        result.Data.Should().Contain(t => t.Name == "ليلى كمال");
    }
}