using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Features.TeacherStudents.Requests;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.TeacherStudents.Commands.AddStudent;
using Prisma.Application.Features.TeacherStudents.Commands.GrantLesson;
using Prisma.Application.Features.TeacherStudents.Commands.RevokeLesson;
using Prisma.Application.Features.TeacherStudents.Commands.SendReport;
using Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;
using Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentLessons;
using Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;
using Prisma.Application.Features.TeacherStudents.Queries.GetTeacherLessonsForGrant;

namespace Prisma.API.Features.TeacherStudents;

[Authorize(Roles = "Teacher")]
[Route("api/[controller]")]
[ApiController]
public class TeacherStudentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllStudentsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentDetailsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/lessons")]
    public async Task<IActionResult> GetLessons(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentLessonsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/activities")]
    public async Task<IActionResult> GetActivities(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentActivitiesQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetStats(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStudentStatsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("lessons-for-grant")]
    public async Task<IActionResult> GetLessonsForGrant(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherLessonsForGrantQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> GetAllLessons(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeacherLessonsForGrantQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddStudentRequest request, CancellationToken cancellationToken)
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
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
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
        return Ok(result);
    }

    [HttpPost("grant")]
    public async Task<IActionResult> GrantLesson([FromBody] GrantLessonRequest request, CancellationToken cancellationToken)
    {
        var command = new GrantLessonCommand(
            request.StudentId,
            request.LessonId,
            request.ValidityDays,
            request.Note);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{studentId:guid}/lessons/{lessonId:int}")]
    public async Task<IActionResult> RevokeLesson(Guid studentId, int lessonId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RevokeLessonCommand(studentId, lessonId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("reports/send")]
    public async Task<IActionResult> SendReport([FromBody] SendReportRequest request, CancellationToken cancellationToken)
    {
        var command = new SendReportCommand(
            request.StudentIds,
            request.ReportType,
            request.DateFrom,
            request.DateTo);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}