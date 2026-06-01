using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountService _service;
    public AccountsController(IAccountService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _service.GetByIdAsync(id);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        var account = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAccountRequest request)
    {
        var account = await _service.UpdateAsync(id, request);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/[controller]")]
public sealed class SecretsController : ControllerBase
{
    private readonly ISecretService _service;
    public SecretsController(ISecretService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var secret = await _service.GetByIdAsync(id);
        return secret is null ? NotFound() : Ok(secret);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSecretRequest request)
    {
        var secret = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = secret.Id }, secret);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSecretRequest request)
    {
        var secret = await _service.UpdateAsync(id, request);
        return secret is null ? NotFound() : Ok(secret);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/pix-keys")]
public sealed class PixKeysController : ControllerBase
{
    private readonly IPixKeyService _service;
    public PixKeysController(IPixKeyService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var key = await _service.GetByIdAsync(id);
        return key is null ? NotFound() : Ok(key);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePixKeyRequest request)
    {
        var key = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = key.Id }, key);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePixKeyRequest request)
    {
        var key = await _service.UpdateAsync(id, request);
        return key is null ? NotFound() : Ok(key);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/v1/charges")]
public sealed class ChargesController : ControllerBase
{
    private readonly IChargeService _service;
    public ChargesController(IChargeService service) => _service = service;

    /// <summary>Creates an immediate Pix charge (Cob).</summary>
    [HttpPost("cob")]
    public async Task<IActionResult> CreateCob(
        CreateCobRequest request, CancellationToken ct)
    {
        var result = await _service.CreateCobAsync(request, ct);
        return CreatedAtAction(null, new { txId = result.TxId }, result);
    }

    /// <summary>Creates a scheduled Pix charge (CobV).</summary>
    [HttpPost("cobv")]
    public async Task<IActionResult> CreateCobV(
        CreateCobVRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateCobVAsync(request, ct);
            return CreatedAtAction(null, new { txId = result.TxId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

[ApiController]
[Route("api/v1/webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly MediatR.IMediator _mediator;
    private readonly Microsoft.Extensions.Logging.ILogger<WebhooksController> _logger;

    public WebhooksController(
        MediatR.IMediator mediator,
        Microsoft.Extensions.Logging.ILogger<WebhooksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Receives payment notification webhook from the bank.</summary>
    [HttpPost("{bankId}")]
    public async Task<IActionResult> Receive(
        string bankId,
        [Microsoft.AspNetCore.Mvc.FromBody] System.Text.Json.JsonElement body,
        CancellationToken ct)
    {
        var headers = Request.Headers
            .ToDictionary(h => h.Key, h => h.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

        var command = new Application.Commands.ProcessWebhook.ProcessWebhookCommand(
            BankId: bankId,
            Headers: headers,
            Body: body);

        var result = await _mediator.Send(command, ct);

        if (!result.Accepted)
        {
            _logger.LogWarning("Webhook rejected for bank {BankId}: {Reason}", bankId, result.Reason);
            return BadRequest(new { error = result.Reason });
        }

        return Ok();
    }
}
