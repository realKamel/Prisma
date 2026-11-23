using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Bookmarks.Common;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications;
using Prisma.Domain.Specifications.Sections;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Features.Bookmarks.Commands.CreateBookmark;

public sealed class CreateBookmarkCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<CreateBookmarkCommand, Result<BookmarkDto>>
{
    private const string DuplicateMessage =
        "You already have a bookmark at this second. Edit its note instead.";

    private sealed record SectionInfo(int LessonId, TimeDuration? TimeDuration);

    public async Task<Result<BookmarkDto>> Handle(
        CreateBookmarkCommand request,
        CancellationToken ct
    )
    {
        var studentId = currentUserService.UserId;

        if (studentId is null)
            return Result<BookmarkDto>.Unauthorized();

        var section = await unitOfWork
            .GetOrCreateRepository<Section, int>()
            .FirstOrDefaultAsync(
                new SectionWithProjectionSpec<SectionInfo>(
                    request.SectionId,
                    s => new SectionInfo(s.LessonId, s.TimeDuration)
                ),
                ct
            );

        if (section is null)
            return Result<BookmarkDto>.NotFound();

        // Access: active or done enrollment that hasn't expired
        var now = DateTimeOffset.UtcNow;
        var enrollments = await unitOfWork
            .GetOrCreateRepository<Enrollment, int>()
            .ListAsync(new EnrollmentsForWatchingSpecification(studentId.Value, section.LessonId), ct);

        if (!enrollments.Any(e => !e.IsExpired(now)))
            return Result<BookmarkDto>.Forbidden();

        // Section.Duration can be null (see the Section.Create bug), so only check when present
        if (
            section.TimeDuration?.Value is { } duration
            && request.PositionSeconds > duration.TotalSeconds
        )
        {
            return Result<BookmarkDto>.Invalid(
                new ValidationError(
                    nameof(request.PositionSeconds),
                    "Position is beyond the end of the video."
                )
            );
        }

        var repository = unitOfWork.GetOrCreateRepository<Bookmark, int>();

        var existing = await repository.FirstOrDefaultAsync(
            new BookmarkByStudentSectionAndPositionSpecification(
                studentId.Value,
                request.SectionId,
                request.PositionSeconds
            ),
            ct
        );

        if (existing is not null)
            return Result<BookmarkDto>.Conflict(DuplicateMessage);

        var count = await repository.CountAsync(
            new BookmarksByStudentAndSectionSpecification(studentId.Value, request.SectionId),
            ct
        );

        if (count >= Bookmark.MaxPerSection)
        {
            return Result<BookmarkDto>.Invalid(
                new ValidationError(
                    nameof(request.SectionId),
                    $"You can have at most {Bookmark.MaxPerSection} bookmarks per section."
                )
            );
        }

        var created = Bookmark.Create(
            studentId.Value,
            request.SectionId,
            request.PositionSeconds,
            request.Note
        );

        if (!created.IsSuccess)
            return Result<BookmarkDto>.Invalid(created.ValidationErrors);

        repository.Add(created.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            // Two requests raced past the duplicate check
            return Result<BookmarkDto>.Conflict(DuplicateMessage);
        }

        return Result.Success(BookmarkDto.From(created.Value));
    }
}