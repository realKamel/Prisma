using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Exceptions;
using Resend;
using EmailMessage = Prisma.Application.Abstractions.Services.Email.EmailMessage;

namespace Prisma.Infrastructure.ExternalServices.EmailService;

internal sealed partial class ResendEmailSender(
    ILogger<ResendEmailSender> logger,
    IResend resend,
    IOptions<ResendOptions> options
) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = new Resend.EmailMessage
        {
            From = options.Value.From,
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
        };

        email.To.Add(message.To);

        if (message.TextBody is not null)
            email.TextBody = message.TextBody;

        var key = IdempotencyKey.New<Guid>("prisma-email", message.IdempotencyKey);

        var result = await resend.EmailSendAsync(key, email, cancellationToken);

        if (!result.Success)
        {
            var error = result.Exception?.Message ?? "Unknown error";

            LogFailedMessage(logger, null, error);

            throw new EmailDeliveryException(error);
        }

        LogEmailSent(logger, result.Content.ToString());
    }

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Information,
        Message = "Successfully submitted email to Resend. MessageId: {MessageId}"
    )]
    private static partial void LogEmailSent(ILogger logger, string? messageId);

    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Error,
        Message = "Failed to send email. MessageId: {MessageId}. Errors: {Error}"
    )]
    private static partial void LogFailedMessage(ILogger logger, string? messageId, string error);
}
