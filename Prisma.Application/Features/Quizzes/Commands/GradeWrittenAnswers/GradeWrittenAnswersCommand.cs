
using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Commands.GradeWrittenAnswers;

public record GradeWrittenAnswersCommand(
    int AttemptId,
    List<WrittenAnswerGradeDto> Grades
) : IRequest<Result>;

