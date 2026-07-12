using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetStudentQuizzesList;

public record GetStudentQuizzesListQuery(string? Filter) 
    : IRequest<Result<StudentQuizzesListResponseDto>>;

