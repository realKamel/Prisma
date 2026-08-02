using MediatR;
using Prisma.Application.Common.DTOs.Auth;
using Ardalis.Result;


namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public record GetUserInfoQuery() : IRequest<Result<LoginCredentials>>;