using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace Prisma.Application.Features.Storage.Commands.MuxWebhook;

public record MuxWebhookCommand(string AssetId, string PlaybackId, int SectionId) : IRequest;