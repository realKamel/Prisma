using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.Quizzes.Commands.CreateQuiz;
using Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;
using Prisma.Application.Features.Quizzes.Commands.ExtractQuestionsFromPdf;
using Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;
using Prisma.Application.Features.Quizzes.Queries.GetExtractionStatus;
using Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;
using Prisma.Application.Features.Quizzes.Queries.GetQuizStudents;
using Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizDetail;
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTeacherQuizDetailQuery(id), ct);
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

    


    // ========== NEW AI EXTRACTION ENDPOINTS ==========

    [HttpPost("extract/upload")]
    public async Task<IActionResult> UploadAndExtract(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(Result.Failure("لم يتم رفع أي ملف"));

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(Result.Failure("يسمح فقط بملفات PDF"));

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var result = await sender.Send(new ExtractQuestionsFromPdfCommand(file.FileName, filePath), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpGet("extract/status/{jobId:int}")]
    public async Task<IActionResult> GetExtractionStatus(int jobId, CancellationToken ct)
    {
        var result = await sender.Send(new GetExtractionStatusQuery(jobId), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}