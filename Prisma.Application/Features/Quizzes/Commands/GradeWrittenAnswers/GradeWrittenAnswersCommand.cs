using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.GradeWrittenAnswers;

public record GradeWrittenAnswersCommand(int AttemptId, IList<WrittenAnswerGradeDto> Grades)
    : IRequest<Result<GradeWrittenAnswersResultDto>>;
