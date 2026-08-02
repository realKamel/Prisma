using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.API.Features.TeacherStudents.Requests;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.TeacherStudents.Commands.AddStudent;
using Prisma.Application.Features.TeacherStudents.Commands.GrantLesson;
using Prisma.Application.Features.TeacherStudents.Commands.RevokeLesson;
using Prisma.Application.Features.TeacherStudents.Commands.SendReport;
using Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentLessons;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;
using Prisma.Application.Features.TeacherStudents.Queries.GetTeacherLessonsForGrant;

namespace Prisma.API.Features.TeacherStudents;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin},{AppRoles.Assistant}")]
[ApiController]
public class TeacherStudentsController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<Result<List<StudentListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllStudentsQuery(), cancellationToken);
        return Result<List<StudentListItemDto>>.Success(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<Result<StudentListItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentDetailsQuery(id), cancellationToken);
        return result;
    }

    [HttpGet("{id:guid}/lessons")]
    public async Task<Result<List<StudentLessonDto>>> GetLessons(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentLessonsQuery(id), cancellationToken);
        return Result<List<StudentLessonDto>>.Success(result);
    }

    [HttpGet("{id:guid}/activities")]
    public async Task<Result<List<StudentActivityDto>>> GetActivities(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentActivitiesQuery(id), cancellationToken);
        return Result<List<StudentActivityDto>>.Success(result);
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<Result<StudentStatsDto>> GetStats(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentStatsQuery(id), cancellationToken);
        return Result<StudentStatsDto>.Success(result);
    }

    [HttpGet("lessons-for-grant")]
    public async Task<Result<List<LessonForGrantDto>>> GetLessonsForGrant(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherLessonsForGrantQuery(), cancellationToken);
        return Result<List<LessonForGrantDto>>.Success(result);
    }

    [HttpGet("lessons")]
    public async Task<Result<List<LessonForGrantDto>>> GetAllLessons(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherLessonsForGrantQuery(), cancellationToken);
        return Result<List<LessonForGrantDto>>.Success(result);
    }

    [HttpGet("academic-years")]
    public async Task<Result<List<AcademicYearOptionDto>>> GetAcademicYears(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), cancellationToken);
        return result;
    }

    [HttpPost]
    public async Task<Result> Create([FromBody] AddStudentRequest request, CancellationToken cancellationToken)
    {
        var command = new AddStudentCommand(
            request.FirstName,
            request.SecondName,
            request.ThirdName,
            request.LastName,
            request.Mobile,
            request.Email,
            request.Password,
            request.Grade,
            request.ParentMobile);

        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpPut("{id:guid}")]
    public async Task<Result> Update(Guid id, [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStudentCommand(
            id,
            request.FirstName,
            request.SecondName,
            request.ThirdName,
            request.LastName,
            request.Mobile,
            request.Email,
            request.NewPassword,
            request.Grade,
            request.ParentMobile);

        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpPost("grant")]
    public async Task<Result> GrantLesson([FromBody] GrantLessonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GrantLessonCommand(
            request.StudentId,
            request.LessonId,
            request.ValidityDays,
            request.Note);

        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpDelete("{studentId:guid}/lessons/{lessonId:int}")]
    public async Task<Result> RevokeLesson(Guid studentId, int lessonId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RevokeLessonCommand(studentId, lessonId), cancellationToken);
        return result;
    }

    [HttpPost("reports/send")]
    public async Task<Result> SendReport([FromBody] SendReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SendReportCommand(
            request.StudentIds,
            request.ReportType,
            request.DateFrom,
            request.DateTo);

        var result = await mediator.Send(command, cancellationToken);
        return result;
    }
}