using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using static Prisma.Application.Common.Constants.AppClaims;

namespace Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;

public class UpdateLessonDetailsCommandHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService,
    UserManager<User> _userManager)
    : IRequestHandler<UpdateLessonDetailsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(Roles.Teacher))
            throw new UnauthorizedException("Only teachers can modify lesson structures.");

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new TeacherLessonWithDetailsSpecification(request.Id);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.Id);

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Price = request.Price;
        lesson.PrerequisiteId = request.PrerequisiteLessonId;

        // ربط الفيديو الأساسي لو كان مدرس مختار نمط فيديو واحد للدرس كله 'single'
        lesson.VideoUrl = request.VideoMode == "single" ? request.LessonVideoFileName : null;

        // 4. تحديث الـ Chapters (الـ Sections) - مسح وإعادة بناء بالـ SortOrder
        lesson.Sections.Clear();
        if (request.Chapters != null && request.VideoMode == "per-chapter")
        {
            int order = 1;
            foreach (var ch in request.Chapters)
            {
                lesson.Sections.Add(new Section
                {
                    Title = ch.Name,
                    ContentURL = ch.VideoFileName, // تخزين اسم ملف الفيديو في الـ ContentURL الخاص بالـ Section
                    SortOrder = order++
                });
            }
        }

        // 5. تحديث أو مسح الـ Assignment بناءً على الـ Toggle الفعلي
        if (request.AssignmentEnabled)
        {
            if (lesson.Assignment == null)
            {
                lesson.Assignment = new Assignment
                {
                    ContentURL = request.AssignmentFileTypes, // ربط الملف المرفق
                    DueDate = request.AssignmentDueDate ?? DateTimeOffset.UtcNow.AddDays(7)
                };
            }
            else
            {
                lesson.Assignment.ContentURL = request.AssignmentFileTypes;
                lesson.Assignment.DueDate = request.AssignmentDueDate ?? DateTimeOffset.UtcNow.AddDays(7);
            }
        }
        else
        {
            // حذف الواجب تماماً من قاعدة البيانات وتصفير الـ Foreign Key
            lesson.Assignment = null;
            lesson.AssignmentId = null;
        }

        // 6. الحفظ المركزي الآمن بدون ميثود UpdateAsync المكسورة
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Lesson structure updated successfully");
    }
}