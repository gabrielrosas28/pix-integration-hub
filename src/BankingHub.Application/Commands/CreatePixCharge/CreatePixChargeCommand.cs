using MediatR;

namespace BankingHub.Application.Commands.CreatePixCharge;

/// <summary>
/// Command to create a new Pix charge.
/// Commands are immutable and represent the intent to perform an action.
/// </summary>
public sealed record CreatePixChargeCommand(
    Guid InvoiceId,
    string ChargeType,
    decimal Amount,
    string PixKey,
    DateOnly? DueDate,
    int? ExpiresInSeconds,
    string? PayerMessage) : IRequest<CreatePixChargeResult>;

public sealed record CreatePixChargeResult(
    string TxId,
    string Status,
    string Emv,
    string? PixLink);
