using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Specifications;

public class TeacherQuizzesListSpecification
    : Specification<Quiz, TeacherQuizzesListProjection>
{
    public TeacherQuizzesListSpecification(QuizScope scope, string? search)
    {
        Query.Where(q => q.Scope == scope);

        if (!string.IsNullOrWhiteSpace(search))
            Query.Where(q => q.Title != null && q.Title.Contains(search));

        Query.Select(q => new TeacherQuizzesListProjection
        {
            QuizId = q.Id,
            Title = q.Title,
            Description = q.Description,
            TimeInMinutes = q.TimeInMinutes,
            TotalDegree = q.TotalDegree,
            AvailableFrom = q.AvailableFrom,
            DueDate = q.DueDate,

            QuestionsCount = q.Questions.Count,

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

            HasUngradedAttempts = q.Attempts.Any(a => a.Status != QuizAttemptStatus.Graded),
        });
    }
}