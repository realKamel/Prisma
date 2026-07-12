using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Assignments.Dtos;

namespace Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionDetail;

public record GetAssignmentSubmissionDetailQuery(int SubmissionId)
    : IRequest<Result<AssignmentSubmissionDetailDto>>;
