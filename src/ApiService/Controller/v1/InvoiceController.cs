using Asp.Versioning;
using BankingHub.Application.Commands.CreateInvoice;
using BankingHub.Application.Queries.GetInvoice;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankingHub.API.Controllers.v1;

/// <summary>
/// API para gerenciamento de faturas.
/// Versão: v1
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/invoices")]
[Produces("application/json")]
public class InvoiceController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<InvoiceController> _logger;

	public InvoiceController(IMediator mediator, ILogger<InvoiceController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Cria uma nova fatura.
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(CreateInvoiceResult), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> CreateInvoice(
		[FromBody] CreateInvoiceRequest request,
		CancellationToken ct)
	{
		_logger.LogInformation(
			"Criando invoice: Amount={Amount}, DueDate={DueDate}, BankId={BankId}",
			request.Amount, request.DueDate, request.BankId);

		var command = new CreateInvoiceCommand(
			Amount: request.Amount,
			DueDate: request.DueDate,
			BankId: request.BankId,
			ExternalReference: request.ExternalReference);

		var result = await _mediator.Send(command, ct);

		return CreatedAtAction(
			nameof(GetInvoiceById),
			new { invoiceId = result.InvoiceId },
			result);
	}

	/// <summary>
	/// Consulta uma fatura pelo identificador.
	/// </summary>
	[HttpGet("{invoiceId:guid}")]
	[ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetInvoiceById(
		[FromRoute] Guid invoiceId,
		CancellationToken ct)
	{
		var query = new GetInvoiceQuery(invoiceId);
		var result = await _mediator.Send(query, ct);

		return result is null ? NotFound() : Ok(result);
	}
}

public sealed record CreateInvoiceRequest(
	decimal Amount,
	DateOnly DueDate,
	string BankId,
	string? ExternalReference);