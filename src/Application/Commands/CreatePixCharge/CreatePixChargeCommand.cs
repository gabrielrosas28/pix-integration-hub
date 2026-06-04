using MediatR;

namespace BankingHub.Application.Commands.CreatePixCharge;


public sealed record CreatePixChargeCommand(
    Guid InvoiceId,
    string ChargeType,          // "COB" ou "COBV"
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
