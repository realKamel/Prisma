using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;

public sealed record UpdateAccentColorCommand(AccentColor AccentColor) : IRequest<Result>;
