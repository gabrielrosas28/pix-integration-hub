using System.Text.Json;
using Asp.Versioning;
using BankingHub.Application.Commands.ProcessWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankingHub.API.Controllers.v1;

/// <summary>
/// API para recebimento de webhooks dos bancos.
/// Versão: v1
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webhooks")]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IMediator mediator, ILogger<WebhooksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Recebe e processa um webhook de banco.
    /// </summary>
    [HttpPost("{bankId}")]
    [ProducesResponseType(typeof(ProcessWebhookResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Receive(
        [FromRoute] string bankId,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        JsonElement body;
        try
        {
            body = JsonDocument.Parse(rawBody).RootElement.Clone();
        }
        catch (JsonException)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid JSON payload.",
                Detail = "Webhook body must be a valid JSON document.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var headers = Request.Headers
            .ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation("Webhook recebido do banco {BankId}", bankId);

        var command = new ProcessWebhookCommand(bankId, headers, body);
        var result = await _mediator.Send(command, ct);

        return Accepted(result);
    }
}