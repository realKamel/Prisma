using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.Quizzes.Commands.CreateQuiz;
using Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;
using Prisma.Application.Features.Quizzes.Commands.ExtractQuestionsFromPdf;
using Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;
using Prisma.Application.Features.Quizzes.Dtos;
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
    public async Task<Result<TeacherQuizListItemDto>> Create([FromBody] CreateQuizCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result;
    }

    [HttpGet("available-lessons")]
    public async Task<Result<List<LessonOptionDto>>> GetAvailableLessons(CancellationToken ct)
    {
        var result = await sender.Send(new GetLessonsAvailableForQuizQuery(), ct);
        return result;
    }

    [HttpGet("academic-years")]
    public async Task<Result<List<AcademicYearOptionDto>>> GetAcademicYears(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllAcademicYearsQuery(), ct);
        return result;
    }

    [HttpGet]
    public async Task<Result<TeacherQuizzesListResponseDto>> GetList(
        [FromQuery] string scope,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<QuizScope>(scope, true, out var quizScope))
            return Result<TeacherQuizzesListResponseDto>.Error("Invalid scope value.");
        var result = await sender.Send(new GetTeacherQuizzesListQuery(quizScope, search, status, page, pageSize), ct);
        return result;
    }

    [HttpGet("{id:int}")]
    public async Task<Result<TeacherQuizDetailDto>> GetDetail(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTeacherQuizDetailQuery(id), ct);
        return result;
    }

    [HttpGet("{id:int}/students")]
    public async Task<Result<QuizStudentsResponseDto>> GetStudents(
        int id,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetQuizStudentsQuery(id, search, status, page, pageSize), ct);
        return result;
    }

    [HttpDelete("{quizId:int}")]
    public async Task<Result> DeleteQuiz(int quizId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteQuizCommand(quizId), ct);
        return result;
    }


    // ========== NEW AI EXTRACTION ENDPOINTS ==========

    [HttpPost("extract/upload")]
    public async Task<Result<ExtractionJobDto>> UploadAndExtract(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Result<ExtractionJobDto>.Error("لم يتم رفع أي ملف");

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return Result<ExtractionJobDto>.Error("يسمح فقط بملفات PDF");

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var result = await sender.Send(new ExtractQuestionsFromPdfCommand(file.FileName, filePath), ct);
        return result;
    }

    [HttpGet("extract/status/{jobId:int}")]
    public async Task<Result<ExtractionProgressDto>> GetExtractionStatus(int jobId, CancellationToken ct)
    {
        var result = await sender.Send(new GetExtractionStatusQuery(jobId), ct);
        return result;
    }
}