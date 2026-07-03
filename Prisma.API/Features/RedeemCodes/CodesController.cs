using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RedeemCodes.Commands.CreateCodeBatch;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeBatchDetail;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;
using Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;

namespace Prisma.API.Features.RedeemCodes;

[Authorize(Roles = AppRoles.Teacher)]
public class CodesController(ISender mediator) : ApiController
{
    [HttpGet("batches")]
    [ProducesResponseType<Result<List<CodeBatchListItemDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBatches(
        [FromQuery] int? academicYearId,
        [FromQuery] int? lessonId,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeacherCodeBatchesQuery(academicYearId, lessonId), ct);
        return Ok(result);
    }

    [HttpGet("batches/{batchId:int}")]
    [ProducesResponseType<Result<CodeBatchDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchDetail([FromRoute] int batchId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCodeBatchDetailQuery(batchId), ct);
        return Ok(result);
    }

    [HttpPost("batches")]
    [ProducesResponseType<Result<CreateCodeBatchResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBatch([FromBody] CreateCodeBatchCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("academic-years")]
    [ProducesResponseType<Result<List<Application.Features.AcademicYears.Dtos.AcademicYearOptionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAcademicYears(CancellationToken ct)
    {
        var result = await mediator.Send(
            new Application.Features.AcademicYears.Queries.GetAllAcademicYears.GetAllAcademicYearsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("lessons")]
    [ProducesResponseType<Result<List<CodeLessonOptionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLessons(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCodeLessonOptionsQuery(), ct);
        return Ok(result);
    }
}