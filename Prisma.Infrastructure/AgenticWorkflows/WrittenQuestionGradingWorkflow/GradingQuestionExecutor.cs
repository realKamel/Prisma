using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs.Ai;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Infrastructure.AgenticWorkflows.WrittenQuestionGradingWorkflow;

public partial class GradingQuestionExecutor(IServiceScopeFactory scopeFactory) :
    Executor("GradingQuestionExecutor")
{
    [MessageHandler]
    private async ValueTask HandleAsync(AttemptAnswerDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var attemptRepo = uow.GetOrCreateRepository<AttemptAnswer, int>();
        var agent = scope.ServiceProvider
            .GetRequiredKeyedService<AIAgent>(AIAgentRole.ChatAgent.GradingAgent);


        var attempt =
            await attemptRepo.GetByIdAsync(new AttemptAnswerWithQuestion(message.QuizAttemptId),
                cancellationToken);

        var result = await agent
            .RunAsync<AttemptAnswerResponseDto>(GetPrompt(message), cancellationToken: cancellationToken);

        attempt.IsCorrect = result.Result.IsCorrect;
        attempt.Score = result.Result.Score;
        attemptRepo.Update(attempt);

        await uow.SaveChangesAsync(cancellationToken);
    }

    private string GetPrompt(AttemptAnswerDto message)
    {
        return $"""
                 You are an expert academic evaluator.
                 Your task is to grade a student's answer to a quiz question by comparing it against a provided model answer. 

                 ### Input Data
                 You will be provided with the following data points for the evaluation:
                 - Quiz Attempt ID: {message.QuizAttemptId}
                 - Question: {message.Question}
                 - Model Answer: {message.ModelAnswer}
                 - Student Answer: {message.StudentAnswer}
                 - Maximum Score: {message.MaxScore}

                 ### Grading Criteria
                 1. **Analyze the Student Answer:** Evaluate the core concepts, accuracy, and completeness of the student's response relative to the Model Answer.
                 2. **Handle Edge Cases:**
                    - If the `StudentAnswer` is null, empty, or completely irrelevant, award a score of `0` and mark `IsCorrect` as `false`.
                    - If the answer is partially correct, award a partial score proportionally (e.g., 1.5 out of 3.0) and mark `IsCorrect` as `false` (only mark true for full/near-full credit).
                    - Minor grammatical or spelling errors should not penalize the score unless they change the technical meaning of the answer.
                 3. **Determine Score:** Calculate the `Score` as a decimal. It must be between `0` and the `MaxScore`.
                 4. **Determine Correctness:** Set `IsCorrect` to `true` if the student earned full or nearly full credit (e.g., ≥ 90% of the MaxScore). Otherwise, set it to `false`.

                """;
    }
}