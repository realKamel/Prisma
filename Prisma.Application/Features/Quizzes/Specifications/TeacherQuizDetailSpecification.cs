using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Application.Features.Quizzes.Specifications;


public class TeacherQuizDetailSpecification
    : Specification<Quiz, TeacherQuizDetailProjection>
{
    public TeacherQuizDetailSpecification(int quizId)
    {
        Query.Where(q => q.Id == quizId);

        Query.Select(q => new TeacherQuizDetailProjection
        {
            QuizId = q.Id,
            Title = q.Title,
            Description = q.Description,
            Scope = q.Scope,

            LessonId = q.LessonId,
            LessonTitle = q.Lesson != null ? q.Lesson.Title : null,

            AcademicYearId = q.AcademicYearId,
            AcademicYearName = q.AcademicYear != null
                ? q.AcademicYear.Title
                : null,

            TimeInMinutes = q.TimeInMinutes,
            TotalDegree = q.TotalDegree,
            AvailableFrom = q.AvailableFrom,
            DueDate = q.DueDate,

            PendingGradingCount = q.Attempts.Count(a =>
                a.Status == QuizAttemptStatus.Submitted),

            SubmittedCount = q.Attempts.Count(a =>
                a.Status == QuizAttemptStatus.Submitted ||
                a.Status == QuizAttemptStatus.Graded),

            AverageDegree = q.Attempts
                .Where(a => a.Status == QuizAttemptStatus.Graded)
                .Select(a => (decimal?)a.Degree)
                .Average(),

            HasAttempts = q.Attempts.Any(),

            HasUngradedAttempts = q.Attempts.Any(a =>
                a.Status != QuizAttemptStatus.Graded)

        });
    }
}