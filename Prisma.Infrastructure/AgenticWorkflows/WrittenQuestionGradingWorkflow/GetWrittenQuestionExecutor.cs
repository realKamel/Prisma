using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Common.DTOs.Ai;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Infrastructure.AgenticWorkflows.WrittenQuestionGradingWorkflow;

public partial class GetWrittenQuestionExecutor(IServiceScopeFactory scopeFactory)
    : Executor("GetWrittenQuestionExecutor")
{
    [MessageHandler]
    private async ValueTask<AttemptAnswerDto?> HandleAsync(int message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var attemptRepo = uow.GetOrCreateRepository<AttemptAnswer, int>();
        var spec = new AttemptAnswerWithQuestion<AttemptAnswerDto>(message
            ,
            x => new AttemptAnswerDto(x.QuizAttemptId,
                x.Question.Title,
                ((WrittenQuestion)x.Question).Answer,
                x.TextAnswer,
                x.Score.Value)
        );
        return await attemptRepo.FirstOrDefaultAsync(spec, cancellationToken);
    }
}