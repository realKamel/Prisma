using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Bookmarks.Commands.CreateBookmark;
using Prisma.Application.Features.Bookmarks.Commands.DeleteBookmark;
using Prisma.Application.Features.Bookmarks.Commands.UpdateBookmarkNote;
using Prisma.Application.Features.Bookmarks.Common;
using Prisma.Application.Features.Bookmarks.Queries.GetSectionBookmarks;

namespace Prisma.API.Features.Bookmarks;

[Authorize(Roles = AppRoles.Student)]
public sealed class BookmarksController(ISender mediator) : ApiController
{
    [HttpGet("sections/{sectionId:int}")]
    [ExpectedFailures(ResultStatus.CriticalError, ResultStatus.Error, ResultStatus.Unauthorized)]
    public async Task<Result<List<BookmarkDto>>> GetSectionBookmarks(
        int sectionId,
        CancellationToken ct
    )
    {
        return await mediator.Send(new GetSectionBookmarksQuery(sectionId), ct);
    }

    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    [HttpPost("sections/{sectionId:int}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized,
        ResultStatus.Forbidden,
        ResultStatus.NotFound,
        ResultStatus.Conflict
    )]
    public async Task<Result<BookmarkDto>> CreateBookmark(
        int sectionId,
        [FromBody] CreateBookmarkRequest request,
        CancellationToken ct
    )
    {
        return await mediator.Send(
            new CreateBookmarkCommand(sectionId, request.PositionSeconds, request.Note),
            ct
        );
    }

    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    [HttpPut("{bookmarkId:guid}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized,
        ResultStatus.NotFound
    )]
    public async Task<Result<BookmarkDto>> UpdateBookmarkNote(
        Guid bookmarkId,
        [FromBody] UpdateBookmarkNoteRequest request,
        CancellationToken ct
    )
    {
        return await mediator.Send(new UpdateBookmarkNoteCommand(bookmarkId, request.Note), ct);
    }

    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    [HttpDelete("{bookmarkId:guid}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.NotFound
    )]
    public async Task<Result> DeleteBookmark(Guid bookmarkId, CancellationToken ct)
    {
        return await mediator.Send(new DeleteBookmarkCommand(bookmarkId), ct);
    }
}
public sealed record CreateBookmarkRequest(int PositionSeconds, string? Note);

public sealed record UpdateBookmarkNoteRequest(string? Note);