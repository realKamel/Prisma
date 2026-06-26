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
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;

public class CreateLessonDetailsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager)
    : IRequestHandler<CreateLessonDetailsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher))
            throw new UnauthorizedException("Only teachers can create lessons.");

        // إنشاء كائن الدرس مع الحقول الجديدة
        var lesson = new Lesson
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PrerequisiteId = request.PrerequisiteLessonId,
            Status = request.IsPublished ? LessonStatus.Active : LessonStatus.Drafted,
            ImageThumbnailUrl = request.ImageUrl, // 🌟 تسكين رابط الصورة
            Outcomes = request.Outcomes           // 🌟 تسكين المخرجات التعليمية
        };

        // إضافة الفصول
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

        // إضافة الواجب
        if (request.AssignmentEnabled)
        {
            lesson.Assignment = new Assignment
            {
                ContentURL = request.AssignmentFileTypes,
                DueDate = request.AssignmentDueDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(7)
            };
        }

        // ربط المراحل الدراسية المختارة بالدرس
        if (request.AcademicYearIds != null && request.AcademicYearIds.Any())
        {
            var academicYearRepository = _unitOfWork.GetOrCreateRepository<AcademicYear, int>();
            foreach (var id in request.AcademicYearIds)
            {
                var academicYear = await academicYearRepository.GetByIdAsync(id, cancellationToken);
                if (academicYear != null)
                {
                    //lesson.AcademicYears.Add(academicYear);
                }
            }
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        lessonRepository.Add(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(lesson.Id);
    }
}