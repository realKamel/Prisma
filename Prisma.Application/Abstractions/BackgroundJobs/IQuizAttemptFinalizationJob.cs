using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Application.Abstractions.BackgroundJobs;

public interface IQuizAttemptFinalizationJob
{
    Task FinalizeExpiredAttempts(CancellationToken cancellationToken = default);
}