using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.TeacherPreferences.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teacher;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;

public sealed class GetAccentColorQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAccentColorQuery, Result<AccentColorDto>>
{
    public async Task<Result<AccentColorDto>> Handle(GetAccentColorQuery request, CancellationToken ct)
    {
        var userRepository = unitOfWork.GetOrCreateRepository<User, Guid>();
        var teacher = await userRepository.FirstOrDefaultAsync(new UserByEmailSpecification(request.TeacherEmail), ct);

        if (teacher is null)
        {
            return Result<AccentColorDto>.Success(new AccentColorDto(AccentColor.Purple));
        }

        var repository = unitOfWork.GetOrCreateRepository<Prisma.Domain.Entities.TeacherPreferences, Guid>();
        var preferences = await repository.FirstOrDefaultAsync(
            new TeacherPreferencesByTeacherIdSpecification(teacher.Id), ct);

        var accentColor = preferences?.AccentColor ?? AccentColor.Purple;

        return Result<AccentColorDto>.Success(new AccentColorDto(accentColor));
    }
}