using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    UserManager<User> _userManager)
    : IRequestHandler<UpdateLessonDetailsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        // 1. تحقق الأمان والـ Roles
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher))
            throw new UnauthorizedException("Only teachers can modify lesson structures.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();

        // 2. استخدام الـ Specification الجديدة المسمية باسم الهاندلر 🌟
        var spec = new UpdateLessonDetailsSpecification(request.Id);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.Id);

        // 3. تحديث البيانات الأساسية والمخرجات والصورة
        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Price = request.Price;
        lesson.PrerequisiteId = request.PrerequisiteLessonId;
        lesson.Status = request.IsPublished ? LessonStatus.Active : LessonStatus.Drafted;
        lesson.ImageThumbnailUrl = request.ImageUrl;
        lesson.Outcomes = request.Outcomes ?? new List<string>();

        // 4. تحديث الفصول بأمان
        lesson.Sections.Clear();
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

        // 5. تعديل الواجب
        if (request.AssignmentEnabled)
        {
            if (lesson.Assignment == null)
            {
                lesson.Assignment = new Assignment
                {
                    ContentURL = request.AssignmentFileTypes,
                    DueDate = request.AssignmentDueDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(7),
                };
            }
            else
            {
                lesson.Assignment.ContentURL = request.AssignmentFileTypes;
                lesson.Assignment.DueDate = request.AssignmentDueDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(7);
            }
        }
        else
        {
            lesson.Assignment = null;
            lesson.AssignmentId = null;
        }

        if (request.AcademicYearIds != null)
        {
            var academicYearRepository = _unitOfWork.GetOrCreateRepository<AcademicYear, int>();

            var allAcademicYears = await academicYearRepository.ListAsync(cancellationToken);

            var selectedYears = allAcademicYears
                .Where(ay => request.AcademicYearIds.Contains(ay.Id) && !ay.IsDeleted)
                .ToList();

            lesson.AcademicYears = selectedYears;
        }

        lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Lesson structure updated successfully");
    }
}