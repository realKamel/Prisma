using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Quizzes.Commands.GradeWrittenAnswers;
using Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetGradingAttemptDetail;
using Prisma.Application.Features.Quizzes.Queries.GetGradingList;
using Prisma.Domain.Enums;

namespace Prisma.API.Features.Quizzes;


[Route("api/v1/teacher/grading")]
[Authorize(Roles = "Teacher,Assistant")]
public class GradingController(ISender sender) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] QuizScope scope,
        [FromQuery] string? search,
        [FromQuery] string? status,
         [FromQuery] int? quizId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetGradingListQuery(scope, search, status,quizId, page, pageSize), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{attemptId:int}")]
    public async Task<IActionResult> GetAttemptDetail(int attemptId, CancellationToken ct)
    {
        var result = await sender.Send(new GetGradingAttemptDetailQuery(attemptId), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{attemptId:int}/grade")]
    public async Task<IActionResult> Grade(int attemptId,
    [FromBody] GradeWrittenAnswersRequest body,
    CancellationToken ct)
    {
        var result = await sender.Send(
            new GradeWrittenAnswersCommand(attemptId, body.Grades), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{attemptId:int}/override-score")]
    public async Task<IActionResult> OverrideScore(
    int attemptId,
    [FromBody] OverrideScoreRequest body,
    CancellationToken ct)
    {
        var result = await sender.Send(
            new OverrideAttemptScoreCommand(attemptId, body.PenaltyScore), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    public record GradeWrittenAnswersRequest(List<WrittenAnswerGradeDto> Grades);
    public record OverrideScoreRequest(decimal PenaltyScore);
}
