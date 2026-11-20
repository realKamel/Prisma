using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Students.Common;

namespace Prisma.Application.Features.Students.Commands.FinishPractice;

public sealed record FinishPracticeCommand
    : ICommand<Result<FinishPracticeResultDto>>;