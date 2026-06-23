using System;
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
using static Prisma.Application.Common.Constants.AppClaims;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;

public class CreateLessonDetailsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager)
    : IRequestHandler<CreateLessonDetailsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        // 1. تحقق الأمان والـ Role للمدرس
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher))
            throw new UnauthorizedException("Only teachers can create lessons.");

        // 2. إنشاء كائن الـ Lesson مع إعطاء قيم افتراضية للحقول الـ Required في قاعدة البيانات 🌟
        var lesson = new Lesson
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PrerequisiteId = request.PrerequisiteLessonId,
            Status = LessonStatus.Drafted,          // الحقل الأساسي الـ Enum
            Duration = TimeSpan.Zero,               // قيمة افتراضية آمنة لمنع الـ DB Null Exception
            IsEligible = true                       // قيمة افتراضية آمنة
        };

        if (request.Chapters != null)
        {
            int order = 1;
            foreach (var ch in request.Chapters)
            {
                lesson.Sections.Add(new Section
                {
                    Title = ch.Name,
                    ContentURL = ch.VideoFileName,
                    SortOrder = order++,
                    Duration = TimeSpan.Zero,       // حقل مطلوب Non-nullable في كيان الـ Section 🌟
                    IsPreview = false
                });
            }
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        await lessonRepository.AddAsync(lesson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. إضافة الـ Assignment صراحةً وربطه بالـ LessonId الناتج 🌟
        if (request.AssignmentEnabled)
        {
            var assignment = new Assignment
            {
                LessonId = lesson.Id,               // الربط الصريح عبر الـ Foreign Key
                ContentURL = request.AssignmentFileTypes,
                DueDate = request.AssignmentDueDate ?? DateTimeOffset.UtcNow.AddDays(7)
            };

            var assignmentRepository = _unitOfWork.GetOrCreateRepository<Assignment, int>();
            await assignmentRepository.AddAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // حفظ الـ Assignment

            lesson.AssignmentId = assignment.Id;
            lessonRepository.Update(lesson);
            await _unitOfWork.SaveChangesAsync(cancellationToken); 
        }

        return Result<int>.Success(lesson.Id);
    }
}