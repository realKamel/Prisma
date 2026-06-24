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
        // 1. تحقق الأمان والـ Role (مدرس)
        var userId = _currentUserService.UserId;
        if (userId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(AppRoles.Teacher))
            throw new UnauthorizedException("Only teachers can create lessons.");

        // 2. إنشاء كائن الدرس وحالته Drafted
        var lesson = new Lesson
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            PrerequisiteId = request.PrerequisiteLessonId, // المابينج الصح للـ Self-Relation
            Status = LessonStatus.Active
        };

        // 3. إضافة الفصول (Sections)
        if (request.Chapters != null)
        {
            int order = 1;
            foreach (var ch in request.Chapters)
            {
                lesson.Sections.Add(new Section
                {
                    Title = ch.Name,
                    ContentURL = ch.VideoFileName, // اللينك الـ cdn المبعوت في الـ JSON هينزل هنا صح
                    SortOrder = order++
                });
            }
        }

        // 4. إضافة الواجب (Assignment)
        if (request.AssignmentEnabled)
        {
            lesson.Assignment = new Assignment
            {
                ContentURL = request.AssignmentFileTypes,
                DueDate = request.AssignmentDueDate ?? DateTimeOffset.UtcNow.AddDays(7)
            };
        }

        // 5. الحفظ في الداتا بيز
        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        lessonRepository.Add(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(lesson.Id);
    }
}