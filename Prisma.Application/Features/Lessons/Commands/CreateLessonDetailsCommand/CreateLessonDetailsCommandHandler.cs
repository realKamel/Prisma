using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.BackgroundJobs;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetailsCommand;

public class CreateLessonDetailsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    IIdentityService _userManager,
    IStorageService storageService,
    IBackgroundJobService backgroundJobService)
    : IRequestHandler<CreateLessonDetails.CreateLessonDetailsCommand, Result<CreateLessonResponse>>
{
    public async Task<Result<CreateLessonResponse>> Handle(CreateLessonDetails.CreateLessonDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId.Value);
        if (user is null)
        {
            return Result.Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher) && !roles.Contains(AppRoles.Assistant) && !roles.Contains(AppRoles.Admin))
            return Result.Unauthorized("Only teachers and assistants can create lessons.");

        Guid? teacherId;

        if (roles.Contains(AppRoles.Teacher))
        {
            teacherId = user.Id;
        }
        else if (roles.Contains(AppRoles.Assistant))
        {
            if (user is not Assistant assistant)
                return Result.Error("Assistant record is missing teacher assignment.");

            if (assistant.TeacherId is null)
                return Result.Error("This assistant is not assigned to a teacher.");

            teacherId = assistant.TeacherId;
        }
        else // Admin
        {
            if (request.TeacherId is null)
                return Result.Error("Admin-created lessons require an explicit teacher.");

            var teacherExists = await _userManager.FindByIdAsync(request.TeacherId.Value) is Teacher;
            if (!teacherExists)
                return Result.Error("Specified teacher does not exist.");

            teacherId = request.TeacherId;
        }

        var lesson = new Lesson
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PrerequisiteId = request.PrerequisiteLessonId,
            Status = request.IsPublished ? LessonStatus.Active : LessonStatus.Drafted,
            Outcomes = request.Outcomes ?? new List<string>(),
            TeacherId = teacherId
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var storageKey = $"lessons/thumbnails/{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";
            await using var stream = request.ImageFile.OpenReadStream();
            await storageService.UploadFileAsync(storageService.DefaultBucketName, storageKey, stream,
                request.ImageFile.ContentType, cancellationToken);
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
            await using var stream = request.AssignmentFile.OpenReadStream();
            await storageService.UploadFileAsync(storageService.DefaultBucketName, storageKey, stream,
                request.AssignmentFile.ContentType, cancellationToken);

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
                return Result.Error("invalid academic year");

            foreach (var yearId in academicYearIds)
            {
                lesson.AcademicYears.Add(new AcademicYearLesson { AcademicYearId = yearId });
            }
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        lessonRepository.Add(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        List<int> sectionIds = lesson.Sections.Select(s => s.Id).ToList();

        var response = new CreateLessonResponse(lesson.Id, sectionIds);

        //backgroundJobService.Enqueue<ILessonTranscriptAndSummarizationJob>(job =>
        //    job.TranscriptAndSummarize(lesson.Id, cancellationToken));

        return Result<CreateLessonResponse>.Success(response);
    }
}