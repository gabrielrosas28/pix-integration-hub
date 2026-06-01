using MediatR;

namespace BankingHub.Application.Commands.CreatePixCharge;

/// 
/// Command para criar uma nova cobrança Pix.
/// Commands são imutáveis e representam a intenção de realizar uma ação
/// 

public sealed record CreatePixChargeCommand(
Guid InvoiceId,
string ChargeType, // "COB" ou "COBV"
decimal Amount,
string PixKey,
DateOnly? DueDate,
int? ExpiresInSeconds,
string? PayerMessage) : IRequest;

public sealed record CreatePixChargeResult(
string TxId,
string Status,
string Emv,
string? PixLink);