using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Extenions;
using Prisma.Application.Features.Lessons.Helpers;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.AddLesson;

internal sealed class AddLessonCommandHandler(
    ICurrentUserService currentUserService,
    IStorageService storageService,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddLessonCommand, Result<AddLessonResponse>>
{
    public async Task<Result<AddLessonResponse>> Handle(
        AddLessonCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId!.Value;
        Guid teacherId = Guid.Empty;

        // 1. Resolve Teacher ID based on Role
        if (currentUserService.Role == AppRoles.Teacher)
        {
            teacherId = userId;
        }
        else if (currentUserService.Role == AppRoles.Assistant)
        {
            var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
            var teacherAssignmentId = await assistantRepo.FirstOrDefaultAsync(
                new GetAssistantByIdWithProjection<Guid?>(userId, x => x.TeacherId),
                cancellationToken
            );

            if (teacherAssignmentId is null || teacherAssignmentId == Guid.Empty)
            {
                return Result.Error("This assistant is not assigned to a teacher.");
            }

            teacherId = teacherAssignmentId.Value;
        }
        else if (currentUserService.Role == AppRoles.Admin)
        {
            // If Admin creates it, ensure your command supports passing a TeacherId,
            // or handle it according to your admin business logic.
            return Result.Error("Admin lesson creation flow needs explicit teacher assignment.");
        }

        // 2. Create the Lesson Domain Entity
        var lessonResult = Lesson.Create(
            request.Title,
            request.Description,
            request.PrerequisiteLessonId,
            false,
            [.. request.Outcomes],
            teacherId,
            request.Price,
            request.Currency,
            request.ValidityDurationInSeconds
        );

        if (!lessonResult.IsSuccess)
        {
            return Result.Invalid(lessonResult.ValidationErrors);
        }

        var lesson = lessonResult.Value;

        // 3. Handle Academic Years if provided
        if (request.AcademicYearIds.Count != 0)
        {
            var academicYearIds = request.AcademicYearIds.Distinct().ToList();
            var academicYearRepository = unitOfWork.GetOrCreateRepository<AcademicYear, int>();

            var validYears = await academicYearRepository.ListAsync(
                new AcademicYearsByIdsSpecification(academicYearIds),
                cancellationToken
            );

            if (validYears.Count != academicYearIds.Count)
            {
                return Result.Error("One or more academic years are invalid.");
            }

            foreach (var year in validYears)
            {
                lesson.AttachToAcademicYear(year);
            }
        }

        // 4. Generate Pre-signed Cover Upload URL
        var coverStorageKey = UrlsHelper.ConstructLessonCoverObjectKey(teacherId, lesson.PublicId);
        var coverStorageResult = await storageService.GetPreSignedUploadUrlAsync(
            coverStorageKey,
            1.DayInSeconds(),
            cancellationToken: cancellationToken
        );

        if (!coverStorageResult.IsSuccess)
        {
            return Result.Error(new ErrorList(coverStorageResult.Errors));
        }

        // 5. Generate Pre-signed Assignment Upload URL (if enabled)
        string? assignmentStorageUrl = null;

        if (request.AssignmentEnabled)
        {
            string? assignmentStorageKey = null;
            assignmentStorageKey = UrlsHelper.ConstructLessonAssignmentObjectKey(
                teacherId,
                lesson.PublicId
            );
            var assignmentStorageResult = await storageService.GetPreSignedUploadUrlAsync(
                assignmentStorageKey,
                1.DayInSeconds(),
                cancellationToken: cancellationToken
            );

            if (!assignmentStorageResult.IsSuccess)
            {
                return Result.Error(new ErrorList(assignmentStorageResult.Errors));
            }

            assignmentStorageUrl = assignmentStorageResult.Value; // Extract the actual URL string

            var assignment = new Assignment
            {
                Title = $"{request.Title} Assignment",
                ContentURL = assignmentStorageKey, // stores the object storage key reference
                DueDate =
                    request.AssignmentDueDate?.ToUniversalTime()
                    ?? DateTimeOffset.UtcNow.AddDays(7),
                Grade = request.AssignmentGrade ?? 0,
            };
            lesson.AddAssignment(assignment);
        }

        // 6. Persist to the database
        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();
        lessonRepository.Add(lesson);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return response with pre-signed upload URLs
        return new AddLessonResponse(
            LessonId: lesson.PublicId,
            CoverUploadUrl: coverStorageResult.Value,
            AssignmentUploadUrl: assignmentStorageUrl
        );
    }
}
