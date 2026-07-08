using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserEditDto>>;