using FluentAssertions;
using MediatR;
using NSubstitute;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminActivities;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminStats;
using Prisma.Application.Features.Users.Queries.GetAdminProfile;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class GetAdminProfileQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<User, Guid> _userRepo = Substitute.For<IRepository<User, Guid>>();
    private readonly ISender _mediator = Substitute.For<ISender>();
    private readonly GetAdminProfileQueryHandler _sut;

    public GetAdminProfileQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<User, Guid>().Returns(_userRepo);
        _sut = new GetAdminProfileQueryHandler(_unitOfWork, _mediator);
    }

    [Fact]
    public async Task Handle_WhenTargetIsNotAnAdmin_ThrowsNotFoundException()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid() };
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(student);

        // Act
        var act = () => _sut.Handle(new GetAdminProfileQuery(student.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_TranslatesEnglishKpiIdsToArabicLabels()
    {
        // Arrange
        var admin = new Admin { Id = Guid.NewGuid(), FirstName = "أحمد", LastName = "علي" };
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(admin);

        var statsData = new AdminStatsResponseDto(
            DateTimeOffset.UtcNow,
            new List<KpiDto>
            {
                new("students", 120, 5),
                new("revenue", 45000, 12),
                new("lessons-sold", 80, -2),
                new("uptime", 99.9m, 0),
            },
            1000,
            new List<RevenueWeekDto>());

        _mediator.Send(Arg.Any<GetAdminStatsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<AdminStatsResponseDto>.Success(statsData));
        _mediator.Send(Arg.Any<GetAdminActivitiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<AdminActivityDto>>.Success(new List<AdminActivityDto>()));

        // Act
        var result = await _sut.Handle(new GetAdminProfileQuery(admin.Id), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Name.Should().Be("أحمد علي");
        result.Data.Stats.Should().Contain(s => s.Label == "الطلاب" && s.Value.Contains("120"));
        result.Data.Stats.Should().Contain(s => s.Label == "الإيرادات" && s.Value.Contains("45") && s.Value.Contains("ج.م"));
        result.Data.Stats.Should().Contain(s => s.Label == "الدروس المباعة" && s.Value.Contains("80"));
        result.Data.Stats.Should().Contain(s => s.Label == "نسبة التشغيل" && s.Value.Contains("99.9"));
    }

    [Fact]
    public async Task Handle_WhenUnknownKpiId_FallsBackToRawIdInsteadOfDroppingIt()
    {
        // Arrange — guards against a future KPI silently disappearing instead
        // of surfacing untranslated (see MapKpi's default arm).
        var admin = new Admin { Id = Guid.NewGuid(), FirstName = "أحمد", LastName = "علي" };
        _userRepo.FirstOrDefaultAsync(Arg.Any<UserByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(admin);

        var statsData = new AdminStatsResponseDto(
            DateTimeOffset.UtcNow,
            new List<KpiDto> { new("new-metric", 10, 0) },
            0,
            new List<RevenueWeekDto>());

        _mediator.Send(Arg.Any<GetAdminStatsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<AdminStatsResponseDto>.Success(statsData));
        _mediator.Send(Arg.Any<GetAdminActivitiesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<List<AdminActivityDto>>.Success(new List<AdminActivityDto>()));

        // Act
        var result = await _sut.Handle(new GetAdminProfileQuery(admin.Id), CancellationToken.None);

        // Assert
        result.Data.Stats.Should().ContainSingle();
        result.Data.Stats.Single().Label.Should().Be("new-metric");
    }
}