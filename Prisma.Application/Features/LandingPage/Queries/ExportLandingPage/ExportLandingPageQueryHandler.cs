using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Exceptions;

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
            throw new NotFoundException("Teacher", request.email);
        }

        return Result<TeacherLandingSettings>.Success(teacher.TeacherLandingSettings);
    }
}