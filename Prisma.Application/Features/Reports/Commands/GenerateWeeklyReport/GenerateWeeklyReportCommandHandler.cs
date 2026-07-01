using MediatR;

namespace Prisma.Application.Features.Reports.Commands.GenerateWeeklyReport;

internal class GenerateWeeklyReportCommandHandler : IRequestHandler<GenerateWeeklyReportCommand>
{
    public Task Handle(GenerateWeeklyReportCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
