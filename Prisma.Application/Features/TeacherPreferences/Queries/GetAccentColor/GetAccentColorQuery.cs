using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.TeacherPreferences.Dtos;

namespace Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;

public sealed record GetAccentColorQuery(string TeacherEmail) : IRequest<Result<AccentColorDto>>;
