using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.Quizzes.Commands.CreateQuiz;
using Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;
using Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;
using Prisma.Application.Features.Quizzes.Queries.GetQuizStudents;
using Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;
using Prisma.Domain.Enums;

namespace Prisma.API.Features.Quizzes;

[Route("api/v1/teacher/quizzes")]
[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Assistant}")]
public class TeacherQuizzesController(ISender sender) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuizCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("available-lessons")]
    public async Task<IActionResult> GetAvailableLessons(CancellationToken ct)
    {
        var result = await sender.Send(new GetLessonsAvailableForQuizQuery(), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllAcademicYearsQuery(), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string scope,
        [FromQuery] string? search,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        if (!Enum.TryParse<QuizScope>(scope, true, out var quizScope))
            return BadRequest("Invalid scope value.");
        var result = await sender.Send(new GetTeacherQuizzesListQuery(quizScope, search, status), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:int}/students")]
    public async Task<IActionResult> GetStudents(
        int id,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetQuizStudentsQuery(id, search, status, page, pageSize), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{quizId:int}")]
    public async Task<IActionResult> DeleteQuiz(int quizId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteQuizCommand(quizId), ct);

        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}