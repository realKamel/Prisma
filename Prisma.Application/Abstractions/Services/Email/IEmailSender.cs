using Prisma.Application.Abstractions.Services.Email;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Abstractions.Services;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
