using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Ardalis.Result;

namespace Prisma.Application.Features.LandingPage.Queries.ExportLandingPage;

public class ExportLandingPageQueryHandler(UserManager<User> _userManager)
    : IRequestHandler<ExportLandingPageQuery, Result<TeacherLandingSettings>>
{
    public async Task<Result<TeacherLandingSettings>> Handle(ExportLandingPageQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await _userManager.Users
            .OfType<Teacher>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.email, cancellationToken);

        if (teacher == null)
        {
            return Result.NotFound($"Teacher with id '{request.email}' was not found");
        }

        return Result<TeacherLandingSettings>.Success(teacher.TeacherLandingSettings);
    }
}