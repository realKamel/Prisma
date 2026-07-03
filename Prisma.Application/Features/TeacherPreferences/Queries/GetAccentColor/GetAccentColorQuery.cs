using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.TeacherPreferences.Dtos;

namespace Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;

public sealed record GetAccentColorQuery(string TeacherEmail) : IRequest<Result<AccentColorDto>>;
