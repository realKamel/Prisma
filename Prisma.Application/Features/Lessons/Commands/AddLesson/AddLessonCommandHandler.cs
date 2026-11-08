using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.AddLesson;

internal sealed class AddLessonCommandHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : ICommandHandler<AddLessonCommand, Result<AddLessonResponse>>
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
                new AssistantByIdWithProjectionSpec<Guid?>(userId, x => x.TeacherId),
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
            return lessonResult.Map();
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

        // 6. Persist to the database
        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson, int>();

        lessonRepository.Add(lesson);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return response with pre-signed upload URLs
        return new AddLessonResponse(LessonId: lesson.PublicId);
    }
}
