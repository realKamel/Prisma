using MediatR;
using Ardalis.Result;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;

public sealed record UpdateAccentColorCommand(AccentColor AccentColor) : IRequest<Result>;
