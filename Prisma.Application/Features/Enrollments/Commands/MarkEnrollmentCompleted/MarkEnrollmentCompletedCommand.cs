using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Enrollments.Commands.MarkEnrollmentCompleted;

public sealed record MarkEnrollmentCompletedCommand(Guid EnrollmentId) : IRequest<Result>;