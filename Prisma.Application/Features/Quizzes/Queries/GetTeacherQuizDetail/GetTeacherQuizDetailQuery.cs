using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizDetail;

public record GetTeacherQuizDetailQuery(int QuizId)
    : IRequest<Result<TeacherQuizDetailDto>>;