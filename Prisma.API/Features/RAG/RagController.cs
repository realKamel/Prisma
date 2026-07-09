using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RAG.Commands.AskRagQuestion;
using Prisma.Application.Features.RAG.Commands.DeleteSession;
using Prisma.Application.Features.RAG.Dto;
using Prisma.Application.Features.RAG.Queries.GetAllSessions;
using Prisma.Application.Features.RAG.Queries.GetSession;

namespace Prisma.API.Features.RAG;

public class RagController(IMediator sender) : ApiController
{
    [HttpGet]
    public async Task<ActionResult> GetAllRagSessions(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllSessionsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetRagSession(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetDetailedRagSessionQuery(id), ct);
        return Ok(result);
    }

    // [HttpPost]
    // public async Task<ActionResult> CreateRagSession(CreateConversationCommand command, CancellationToken ct)
    // {
    //     var result = await sender.Send(command, ct);
    //     return Ok(result);
    // }

    [HttpPost]
    [Consumes("application/json")]
    [Produces("text/event-stream")]
    [ProducesResponseType<Result<AskRagQuestionCommandResponse>>(StatusCodes.Status200OK)]
    public async Task AskRagSession(AskRagQuestionCommand command, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var tokenStream = sender.CreateStream(command, ct);
        var responseStream = Response.Body;
        await foreach (var chunk in tokenStream)
        {
            // If 'chunk' is just a raw string string:
            // string sseLine = $"data: {chunk}\n\n";

            // If 'chunk' is an object, serialize it first:
            var sseLine = $"data: {JsonSerializer.Serialize(chunk)}\n\n";

            // 4. Write the token immediately to the network pipe
            await responseStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(sseLine), ct);
            await responseStream.FlushAsync(ct); // Forces it out of the server buffer to the client
        }

        // Signal to Client that we are done
        await responseStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("data: [DONE]\n\n"), ct);
        await responseStream.FlushAsync(ct);
    }

    // [HttpPut("{id}")]
    // public async Task<ActionResult> UpdateRagSession(Guid id, string question, CancellationToken ct)
    // {
    //     var result = await sender.Send(new AskRagQuestionCommand(id, question), ct);
    //     return Ok(result);
    // }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRagSession(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteSessionCommand(id), ct);
        return Ok(result);
    }
}