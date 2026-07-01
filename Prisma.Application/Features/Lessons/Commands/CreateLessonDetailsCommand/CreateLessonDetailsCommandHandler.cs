using Ardalis.Specification;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;

public class CreateLessonDetailsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager,
    IStorageService storageService)
    : IRequestHandler<CreateLessonDetailsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant) && !roles.Contains(AppRoles.Admin))
            throw new UnauthorizedException("Only teachers and assistants can create lessons.");

        var lesson = new Lesson
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PrerequisiteId = request.PrerequisiteLessonId,
            Status = request.IsPublished ? LessonStatus.Active : LessonStatus.Drafted,
            Outcomes = request.Outcomes ?? new List<string>()
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var storageKey = $"lessons/thumbnails/{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";
            using var stream = request.ImageFile.OpenReadStream();
            await storageService.UploadFileAsync("prisma", storageKey, stream, request.ImageFile.ContentType, cancellationToken);
            lesson.ImageThumbnailUrl = storageKey;
        }

        if (request.Chapters != null)
        {
            int order = 1;
            foreach (var ch in request.Chapters)
            {
                lesson.Sections.Add(new Section
                {
                    Title = ch.Name,
                    ContentURL = ch.VideoFileName,
                    SortOrder = order++
                });
            }
        }

        if (request.AssignmentEnabled && request.AssignmentFile != null && request.AssignmentFile.Length > 0)
        {
            var storageKey = $"assignments/{Guid.NewGuid()}{Path.GetExtension(request.AssignmentFile.FileName)}";
            using var stream = request.AssignmentFile.OpenReadStream();
            await storageService.UploadFileAsync("prisma", storageKey, stream, request.AssignmentFile.ContentType, cancellationToken);

            lesson.Assignment = new Assignment
            {
                Title = Path.GetFileNameWithoutExtension(request.AssignmentFile.FileName),
                ContentURL = storageKey,
                DueDate = request.AssignmentDueDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(7)
            };
        }

        if (request.AcademicYearIds != null && request.AcademicYearIds.Any())
        {
            var academicYearIds = request.AcademicYearIds.Distinct().ToList();
            var academicYearRepository = _unitOfWork.GetOrCreateRepository<AcademicYear, int>();

            var validYears = await academicYearRepository.ListAsync(
                new AcademicYearsByIdsSpecification(academicYearIds), cancellationToken);

            if (validYears.Count != academicYearIds.Count)
                throw new BadRequestException("invalid academic year");

            foreach (var yearId in academicYearIds)
            {
                lesson.AcademicYears.Add(new AcademicYearLesson { AcademicYearId = yearId });
            }
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        lessonRepository.Add(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(lesson.Id);
    }
}

