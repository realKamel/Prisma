using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Prisma.API.Common;
using Prisma.Application.Features.Payments.HandleCallback;
using Prisma.Application.Features.Payments.InitiatePayment;
using Prisma.Infrastructure.Services.PaymentService;

namespace Prisma.API.Features.Payments;

public class PaymentsController : ApiController
{
    private readonly IMediator _mediator;

    private readonly PaymobSettings _settings;

public PaymentsController(IMediator mediator, IOptions<PaymobSettings> options)
{
    _mediator = mediator;
    _settings = options.Value;
}

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonObject payload, [FromQuery] string hmac)
    {
        if (!VerifyHmac(payload, hmac))
            return Unauthorized();

        var obj = payload["obj"]?.AsObject();
        var success = obj?["success"]?.GetValue<bool>() ?? false;
        var orderId = obj?["order"]?["id"]?.ToString() ?? "";
        var transactionId = obj?["id"]?.ToString() ?? "";

        await _mediator.Send(new HandlePaymentCallbackCommand(orderId, success, transactionId));
        return Ok();
    }

    private bool VerifyHmac(JsonObject payload, string receivedHmac)
    {
        var obj = payload["obj"]?.AsObject();
        if (obj is null) return false;

        var fields = new[]
        {
        obj["amount_cents"]?.ToString(),
        obj["created_at"]?.ToString(),
        obj["currency"]?.ToString(),
        obj["error_occured"]?.ToString(),
        obj["has_parent_transaction"]?.ToString(),
        obj["id"]?.ToString(),
        obj["integration_id"]?.ToString(),
        obj["is_3d_secure"]?.ToString(),
        obj["is_auth"]?.ToString(),
        obj["is_capture"]?.ToString(),
        obj["is_refunded"]?.ToString(),
        obj["is_standalone_payment"]?.ToString(),
        obj["is_voided"]?.ToString(),
        obj["order"]?["id"]?.ToString(),
        obj["owner"]?.ToString(),
        obj["pending"]?.ToString(),
        obj["source_data"]?["pan"]?.ToString(),
        obj["source_data"]?["sub_type"]?.ToString(),
        obj["source_data"]?["type"]?.ToString(),
        obj["success"]?.ToString(),
    };

        var concatenated = string.Concat(fields);
        var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
        var messageBytes = Encoding.UTF8.GetBytes(concatenated);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        var computedHmac = Convert.ToHexString(hashBytes).ToLower();

        return computedHmac == receivedHmac;
    }
}