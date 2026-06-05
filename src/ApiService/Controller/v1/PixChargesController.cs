using BankingHub.Application.Commands.CreatePixCharge;
using BankingHub.Application.Queries.GetPixChargeStatus;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankingHub.API.Controllers.v1;

/// <summary>
/// API para gerenciamento de cobranças Pix.
/// Versão: v1
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/pix-charges")]
[Produces("application/json")]
public class PixChargesController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<PixChargesController> _logger;

	public PixChargesController(IMediator mediator, ILogger<PixChargesController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Cria uma nova cobrança Pix.
	/// </summary>
	/// <remarks>
	/// Exemplo de request:
	///
	///     POST /api/v1/pix-charges
	///     {
	///         "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	///         "chargeType": "COBV",
	///         "amount": 150.75,
	///         "pixKey": "+5511999999999",
	///         "dueDate": "2025-01-15",
	///         "payerMessage": "Pagamento ref. pedido #12345"
	///     }
	/// </remarks>
	/// <param name="request">Dados da cobrança</param>
	/// <returns>Dados da cobrança criada com QR Code</returns>
	/// <response code="201">Cobrança criada com sucesso</response>
	/// <response code="400">Dados inválidos</response>
	/// <response code="404">Invoice não encontrada</response>
	/// <response code="502">Erro na comunicação com o banco</response>
	[HttpPost]
	[ProducesResponseType(typeof(CreatePixChargeResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
	public async Task<IActionResult> CreateCharge(
		[FromBody] CreatePixChargeRequest request,
		CancellationToken ct)
	{
		_logger.LogInformation(
			"Criando cobrança Pix: Invoice={InvoiceId}, Tipo={Type}, Valor={Amount}",
			request.InvoiceId, request.ChargeType, request.Amount);

		var command = new CreatePixChargeCommand(
			InvoiceId: request.InvoiceId,
			ChargeType: request.ChargeType,
			Amount: request.Amount,
			PixKey: request.PixKey,
			DueDate: request.DueDate,
			ExpiresInSeconds: request.ExpiresInSeconds,
			PayerMessage: request.PayerMessage);

		var result = await _mediator.Send(command, ct);

		var response = new CreatePixChargeResponse(
			TxId: result.TxId,
			Status: result.Status,
			Emv: result.Emv,
			PixLink: result.PixLink,
			QrCodeBase64: GenerateQrCodeImage(result.Emv));

		return CreatedAtAction(
			nameof(GetChargeStatus),
			new { txId = result.TxId },
			response);
	}

	/// <summary>
	/// Consulta o status de uma cobrança Pix.
	/// </summary>
	/// <param name="txId">Identificador único da cobrança (txid)</param>
	/// <returns>Status atual da cobrança</returns>
	[HttpGet("{txId}")]
	[ProducesResponseType(typeof(PixChargeStatusDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetChargeStatus(
		[FromRoute] string txId,
		CancellationToken ct)
	{
		var query = new GetPixChargeStatusQuery(txId);
		var result = await _mediator.Send(query, ct);

		return Ok(result);
	}

	/// <summary>
	/// Força a reconciliação de uma cobrança.
	/// Útil para verificar se um pagamento foi realizado.
	/// </summary>
	[HttpPost("{txId}/reconcile")]
	[ProducesResponseType(typeof(ReconciliationResponse), StatusCodes.Status200OK)]
	public async Task<IActionResult> ReconcileCharge(
		[FromRoute] string txId,
		CancellationToken ct)
	{
		var command = new ReconcileChargeCommand(txId);
		var result = await _mediator.Send(command, ct);

		return Ok(new ReconciliationResponse(
			TxId: txId,
			Status: result.Status,
			IsPaid: result.IsPaid,
			PaidAt: result.PaidAt,
			PaidAmount: result.PaidAmount));
	}

	private static string GenerateQrCodeImage(string emv)
	{
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emv));
	}
}

public sealed record CreatePixChargeRequest(
	Guid InvoiceId,
	string ChargeType,
	decimal Amount,
	string PixKey,
	DateOnly? DueDate,
	int? ExpiresInSeconds,
	string? PayerMessage);

public sealed record CreatePixChargeResponse(
	string TxId,
	string Status,
	string Emv,
	string? PixLink,
	string QrCodeBase64);

public sealed record ReconcileChargeCommand(string TxId) : IRequest<ReconciliationResult>;

public sealed record ReconciliationResult(
	string Status,
	bool IsPaid,
	DateTime? PaidAt,
	decimal? PaidAmount);

public sealed record ReconciliationResponse(
	string TxId,
	string Status,
	bool IsPaid,
	DateTime? PaidAt,
	decimal? PaidAmount);