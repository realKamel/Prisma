using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Assignments.Commands.GradeAssignmentSubmission;
using Prisma.Application.Features.Assignments.Commands.ReleaseAssignmentGradingLock;
using Prisma.Application.Features.Assignments.Dtos;
using Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionDetail;
using Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionsList;

namespace Prisma.API.Features.Assignments;

[Route("api/v1/teacher/assignments")]
[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Assistant}")]
public class TeacherAssignmentsController(ISender sender) : ApiController
{
    [HttpGet]
    public async Task<Result<AssignmentSubmissionsResponseDto>> GetList(
        [FromQuery] string? search,
        [FromQuery] int? lessonId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAssignmentSubmissionsListQuery(search, lessonId, status, page, pageSize), ct);
        return result;
    }

    [HttpGet("{submissionId:int}")]
    public async Task<Result<AssignmentSubmissionDetailDto>> GetDetail(int submissionId, CancellationToken ct)
    {
        var result = await sender.Send(
            new GetAssignmentSubmissionDetailQuery(submissionId), ct);
        return result;
    }

    [HttpPost("{submissionId:int}/grade")]
    public async Task<Result> Grade(
        int submissionId,
        [FromBody] GradeSubmissionRequest body,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GradeAssignmentSubmissionCommand(submissionId, body.Score, body.Note), ct);
        return result;
    }

    [HttpPost("{submissionId:int}/release-lock")]
    public async Task<Result> ReleaseLock(int submissionId, CancellationToken ct)
    {
        var result = await sender.Send(
            new ReleaseAssignmentGradingLockCommand(submissionId), ct);
        return result;
    }
}

public record GradeSubmissionRequest(int Score, string? Note);