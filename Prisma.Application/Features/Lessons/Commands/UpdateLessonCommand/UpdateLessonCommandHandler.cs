using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;

public class UpdateLessonDetailsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager,
    IStorageService storageService)
    : IRequestHandler<UpdateLessonDetailsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant)  && !roles.Contains(AppRoles.Admin))
            throw new UnauthorizedException("Only teachers and assistants can modify lesson structures.");
        var storageKeysToDelete = new List<string>();

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        var spec = new UpdateLessonDetailsSpecification(request.Id);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.Id);

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Price = request.Price;
        lesson.PrerequisiteId = request.PrerequisiteLessonId;
        lesson.Status = request.IsPublished ? LessonStatus.Active : LessonStatus.Drafted;
        lesson.Outcomes = request.Outcomes ?? new List<string>();

        if (request.Chapters != null)
        {
            var existingSections = lesson.Sections.ToList();
            var incomingUrls = request.Chapters.Select(ch => ch.VideoFileName).ToHashSet();

            foreach (var s in existingSections.Where(s => !incomingUrls.Contains(s.ContentURL)))
                lesson.Sections.Remove(s); // TODO trigger storage deletion here

            int order = 1;
            foreach (var ch in request.Chapters)
            {
                var section = existingSections.FirstOrDefault(x => x.ContentURL == ch.VideoFileName);

                if (section is null)
                {
                    lesson.Sections.Add(new Section
                    {
                        Title = ch.Name,
                        ContentURL = ch.VideoFileName,
                        SortOrder = order++
                    });
                    // TODO new section -> trigger video upload flow here
                }
                else
                {
                    section.Title = ch.Name;
                    section.SortOrder = order++;
                }
            }
        }

        if (request.AssignmentEnabled)
        {
            if (request.AssignmentFile != null && request.AssignmentFile.Length > 0)
            {
                var storageKey = $"assignments/{Guid.NewGuid()}{Path.GetExtension(request.AssignmentFile.FileName)}";
                using var stream = request.AssignmentFile.OpenReadStream();
                await storageService.UploadFileAsync("prisma", storageKey, stream, request.AssignmentFile.ContentType, cancellationToken);

                if (lesson.Assignment is null)
                {
                    lesson.Assignment = new Assignment
                    {
                        Title = Path.GetFileNameWithoutExtension(request.AssignmentFile.FileName),
                        ContentURL = storageKey,
                        DueDate = request.AssignmentDueDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(7),
                    };
                }
                else
                {
                    if (lesson.Assignment.ContentURL != null)
                        storageKeysToDelete.Add(lesson.Assignment.ContentURL);
                    lesson.Assignment.Title = Path.GetFileNameWithoutExtension(request.AssignmentFile.FileName);
                    lesson.Assignment.ContentURL = storageKey;
                    if (request.AssignmentDueDate.HasValue)
                        lesson.Assignment.DueDate = request.AssignmentDueDate.Value.ToUniversalTime();
                }
            }
            else if (lesson.Assignment != null)
            {
                if (request.AssignmentDueDate.HasValue)
                    lesson.Assignment.DueDate = request.AssignmentDueDate.Value.ToUniversalTime();
            }
        }
        else if (lesson.Assignment != null)
        {
            if (lesson.Assignment.ContentURL != null)
                storageKeysToDelete.Add(lesson.Assignment.ContentURL);
            lesson.Assignment = null;
            lesson.AssignmentId = null;
        }

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var storageKey = $"lessons/thumbnails/{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";
            using var stream = request.ImageFile.OpenReadStream();
            await storageService.UploadFileAsync("prisma", storageKey, stream, request.ImageFile.ContentType, cancellationToken);

            if (!string.IsNullOrEmpty(lesson.ImageThumbnailUrl))
                storageKeysToDelete.Add(lesson.ImageThumbnailUrl);

            lesson.ImageThumbnailUrl = storageKey;
        }

        if (request.AcademicYearIds != null)
        {
            var existingYearIds = lesson.AcademicYears.Select(ay => ay.AcademicYearId).ToHashSet();
            var incomingYearIds = request.AcademicYearIds.ToHashSet();

            var toRemove = lesson.AcademicYears
                .Where(ay => !incomingYearIds.Contains(ay.AcademicYearId))
                .ToList();
            foreach (var ay in toRemove)
                lesson.AcademicYears.Remove(ay);

            var toAdd = incomingYearIds.Except(existingYearIds);
            foreach (var yearId in toAdd)
            {
                lesson.AcademicYears.Add(new AcademicYearLesson
                {
                    AcademicYearId = yearId,
                    LessonId = lesson.Id
                });
            }
        }
        lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        foreach (var key in storageKeysToDelete)
        {
            try
            {
                await storageService.DeleteFileAsync("prisma", key, cancellationToken);
            }
            catch
            {
                // TODO log
            }
        }
        return Result<string>.Success("Lesson structure updated successfully");
    }
}