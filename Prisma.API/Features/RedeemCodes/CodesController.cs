using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;
using Prisma.Application.Features.RedeemCodes.Commands.CreateCodeBatch;
using Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;
using Prisma.Application.Features.RedeemCodes.Dtos;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeBatchDetail;
using Prisma.Application.Features.RedeemCodes.Queries.GetCodeLessonOptions;
using Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;

namespace Prisma.API.Features.RedeemCodes;

public class CodesController(ISender mediator) : ApiController
{
    [HttpGet("batches")]
    [Authorize(Roles = AppRoles.Teacher + "," + AppRoles.Assistant)]
    [ProducesResponseType<Result<List<CodeBatchListItemDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Result<List<CodeBatchListItemDto>>> GetBatches(
        [FromQuery] int? academicYearId,
        [FromQuery] int? lessonId,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetTeacherCodeBatchesQuery(academicYearId, lessonId), ct);
        return result;
    }

    [HttpGet("batches/{batchId:int}")]
    [Authorize(Roles = AppRoles.Teacher + "," + AppRoles.Assistant)]
    [ProducesResponseType<Result<CodeBatchDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Result<CodeBatchDetailDto>> GetBatchDetail(
        [FromRoute] int batchId,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCodeBatchDetailQuery(batchId), ct);
        return result;
    }

    [HttpPost("batches")]
    [Authorize(Roles = AppRoles.Teacher + "," + AppRoles.Assistant)]
    [ProducesResponseType<Result<CreateCodeBatchResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<Result<CreateCodeBatchResponse>> CreateBatch(
        [FromBody] CreateCodeBatchCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result;
    }

    [HttpGet("academic-years")]
    [Authorize(Roles = AppRoles.Teacher + "," + AppRoles.Assistant)]
    [ProducesResponseType<Result<List<AcademicYearOptionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Result<List<AcademicYearOptionDto>>> GetAcademicYears(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllAcademicYearsQuery(), ct);
        return result;
    }

    [HttpGet("lessons")]
    [Authorize(Roles = AppRoles.Teacher + "," + AppRoles.Assistant)]
    [ProducesResponseType<Result<List<CodeLessonOptionDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Result<List<CodeLessonOptionDto>>> GetLessons(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCodeLessonOptionsQuery(), ct);
        return result;
    }

    [HttpPost("redeem")]
    [Authorize(Roles = AppRoles.Student)]
    [ProducesResponseType<Result<RedeemCodeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Result<RedeemCodeResponse>> Redeem(
        [FromBody] RedeemCodeCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result;
    }
}