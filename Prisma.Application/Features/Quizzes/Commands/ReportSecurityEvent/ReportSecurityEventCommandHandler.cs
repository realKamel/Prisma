using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.ReportSecurityEvent;

public class ReportSecurityEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        :IRequestHandler<ReportSecurityEventCommand, Result>
{
    public async Task<Result> Handle(ReportSecurityEventCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;

        var attempt = await unitOfWork.GetOrCreateRepository<QuizAttempt, int>()
            .FirstOrDefaultAsync(new AttemptByIdAndStudentSpecification(request.AttemptId, studentId), ct);

        if (attempt is null || attempt.Status != QuizAttemptStatus.InProgress)
            return Result.Success(); 

        switch (request.EventType)
        {
            case SecurityEventType.TabSwitch:
                attempt.TabSwitchCount++;
                break;
            case SecurityEventType.CopyPasteAttempt:
                attempt.CopyPasteAttemptCount++;
                break;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
