using FluentAssertions;
using NSubstitute;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.AcademicYears.Queries;

public class GetAllAcademicYearsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<AcademicYear, int> _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();
    private readonly GetAllAcademicYearsQueryHandler _sut;

    public GetAllAcademicYearsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        _sut = new GetAllAcademicYearsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenAcademicYearsExist_ReturnsMappedAcademicYearOptionDtos()
    {
        // Arrange
        var query = new GetAllAcademicYearsQuery();
        var fakeYears = new List<AcademicYear>
        {
            new() { Id = 1, Title = "الصف الأول الإعدادي" },
            new() { Id = 2, Title = "الصف الثاني الإعدادي" },
            new() { Id = 3, Title = "الصف الثالث الإعدادي" }
        };

        // عمل Mock للـ ListAsync لتستقبل الـ Specification وترجع البيانات الوهمية
        _academicYearRepo.ListAsync(Arg.Any<AllAcademicYearsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeYears);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue(); // التأكد من أن العملية نجحت
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(3);

        result.Data[0].Id.Should().Be(1);
        result.Data[0].Name.Should().Be("الصف الأول الإعدادي");

        result.Data[1].Id.Should().Be(2);
        result.Data[1].Name.Should().Be("الصف الثاني الإعدادي");

        result.Data[2].Id.Should().Be(3);
        result.Data[2].Name.Should().Be("الصف الثالث الإعدادي");
    }

    [Fact]
    public async Task Handle_WhenNoAcademicYearsExist_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllAcademicYearsQuery();

        _academicYearRepo.ListAsync(Arg.Any<AllAcademicYearsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<AcademicYear>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeEmpty(); // التأكد أن القائمة فارغة تماماً وليست Null
    }
}