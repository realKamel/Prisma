using MediatR;
using Prisma.Application.Abstractions.Services;
namespace Prisma.Application.Features.Storage.Queries.GetUploadUrl;

public record GetUploadUrlQuery(int SectionId) : IRequest<VideoUploadResult>;