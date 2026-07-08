using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetTeacherOptions;

public record GetTeacherOptionsQuery : IRequest<Result<List<TeacherOptionDto>>>;