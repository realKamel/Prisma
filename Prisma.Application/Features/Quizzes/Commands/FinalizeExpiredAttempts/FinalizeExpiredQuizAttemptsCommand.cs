using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace Prisma.Application.Features.Quizzes.Commands.FinalizeExpiredAttempts;

public record FinalizeExpiredQuizAttemptsCommand : IRequest;