using MediatR;
using Prisma.Application.Common.DTOs.Auth;
using Prisma.Application.Common.Responses.Generic;


namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public record GetUserInfoQuery() : IRequest<Result<LoginCredentials>>;