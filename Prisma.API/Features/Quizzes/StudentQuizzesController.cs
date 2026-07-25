using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Quizzes.Commands.ReportSecurityEvent;
using Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;
using Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using Prisma.Application.Features.Quizzes.Queries.GetQuizForTaking;
using Prisma.Application.Features.Quizzes.Queries.GetQuizResult;
using Prisma.Application.Features.Quizzes.Queries.GetStudentQuizzesList;
using Prisma.Domain.Enums;

namespace Prisma.API.Features.Quizzes;

[Authorize(Roles = AppRoles.Student)]
[Route("api/v1/student/quizzes")]
public class StudentQuizzesController(ISender sender) : ApiController
{
    [HttpGet]
    public async Task<ActionResult> GetList([FromQuery] string? filter, CancellationToken ct)
    {
        var result = await sender.Send(new GetStudentQuizzesListQuery(filter), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{quizId:int}")]
    public async Task<ActionResult> GetForTaking(int quizId, CancellationToken ct)
    {
        var result = await sender.Send(new GetQuizForTakingQuery(quizId), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{quizId:int}/result")]
    public async Task<ActionResult> GetResult(int quizId, CancellationToken ct)
    {
        var result = await sender.Send(new GetQuizResultQuery(quizId), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("attempts/{attemptId:int}/answer")]
    public async Task<ActionResult> SaveAnswer(int attemptId, [FromBody] SaveAnswerRequest body, CancellationToken ct)
    {
        var result =
            await sender.Send(new SaveQuizAnswerCommand(attemptId, body.QuestionId, body.ChoiceId, body.TextAnswer),
                ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPost("attempts/{attemptId:int}/submit")]
    public async Task<ActionResult> Submit(int attemptId, CancellationToken ct)
    {
        var result = await sender.Send(new SubmitQuizAttemptCommand(attemptId), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }


    [HttpPost("attempts/{attemptId:int}/security-event")]
    public async Task<ActionResult> ReportSecurityEvent(
        int attemptId, [FromBody] ReportSecurityEventRequest body, CancellationToken ct)
    {
        var result = await sender.Send(new ReportSecurityEventCommand(attemptId, body.EventType), ct);
        return Ok(result);
    }
}

public record SaveAnswerRequest(int QuestionId, int? ChoiceId, string? TextAnswer);

public record ReportSecurityEventRequest(SecurityEventType EventType);