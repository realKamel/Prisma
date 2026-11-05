using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Assignments.Queries.GetLessonsAvailableForAssignment;

public class GetLessonsAvailableForAssignmentQueryHandler(
    IUnitOfWork unitOfWork, 
    ICurrentUserService currentUserService, 
    IIdentityService identityService)
    : IRequestHandler<GetLessonsAvailableForAssignmentQuery, Result<List<LessonOptionDto>>>
{
    public async Task<Result<List<LessonOptionDto>>> Handle(GetLessonsAvailableForAssignmentQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId.Value, ct);

        if (user is null)
            return Result.NotFound();

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var teacherId = userId.Value;

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lessons = await lessonRepo.ListAsync(
            new LessonsAvailableForAssignmentSpecification(teacherId), ct);

        return Result<List<LessonOptionDto>>.Success(lessons);
    }
}

internal class LessonsAvailableForAssignmentSpecification : Specification<Lesson, LessonOptionDto>
{
    public LessonsAvailableForAssignmentSpecification(Guid teacherId)
    {
        Query
            .Where(l => l.Assignment != null && l.TeacherId == teacherId)
            .Select(l => new LessonOptionDto
            {
                LessonId = l.Id,
                Title = l.Title ?? string.Empty
            });
    }
}
