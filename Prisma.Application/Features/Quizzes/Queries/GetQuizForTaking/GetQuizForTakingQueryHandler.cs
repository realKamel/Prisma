using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Commands.StartOrGetQuizAttempt;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetQuizStructure;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizForTaking;

public class GetQuizForTakingQueryHandler(ISender sender)
    : IRequestHandler<GetQuizForTakingQuery, Result<QuizTakingDto>>
{
    public async Task<Result<QuizTakingDto>> Handle(GetQuizForTakingQuery request, CancellationToken ct)
    {

        var attemptResult = await sender.Send(new StartOrGetQuizAttemptCommand(request.QuizId), ct);
        if (!attemptResult.IsSuccess)
            return Result<QuizTakingDto>.Error(string.Join(" | ", attemptResult.Errors));

        var structureResult = await sender.Send(new GetQuizStructureQuery(request.QuizId), ct);
        if (!structureResult.IsSuccess)
            return Result<QuizTakingDto>.Error(string.Join(" | ", structureResult.Errors));

        var structure = structureResult.Value;
        var attempt = attemptResult.Value;

        var dto = new QuizTakingDto
        {
            AttemptId = attempt.AttemptId,
            QuizId = structure.QuizId,
            Title = structure.Title,
            TeacherName = structure.TeacherName,
            Subject = structure.Subject,
            Instructions = structure.Instructions,
            DurationMinutes = structure.DurationMinutes,
            RemainingSeconds = attempt.RemainingSeconds,
            Questions = structure.Questions.Select(q =>
            {
                attempt.SavedAnswers.TryGetValue(q.QuestionId, out var saved);
                return new QuizQuestionTakingDto
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    Type = q.Type,
                    Degree = q.Degree,
                    Choices = q.Choices,
                    SelectedChoiceId = saved?.SelectedChoiceId,
                    SavedTextAnswer = saved?.TextAnswer
                };
            }).ToList()
        };

        return Result<QuizTakingDto>.Success(dto);
    }
}