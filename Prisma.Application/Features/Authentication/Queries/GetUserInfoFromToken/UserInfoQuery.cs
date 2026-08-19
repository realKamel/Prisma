using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs.Auth;

namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public record GetUserInfoQuery() : IRequest<Result<LoginCredentials>>;
