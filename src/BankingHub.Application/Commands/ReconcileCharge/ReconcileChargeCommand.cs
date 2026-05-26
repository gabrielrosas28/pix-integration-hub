using MediatR;

namespace BankingHub.Application.Commands.ReconcileCharge;

/// <summary>
/// Forces an active reconciliation of a charge against the bank.
/// This is the only path that may transition an Invoice/PixCharge to Paid.
/// </summary>
public sealed record ReconcileChargeCommand(
    string TxId,
    string? BankId = null) : IRequest<ReconcileChargeResult>;

public sealed record ReconcileChargeResult(
    string Status,
    bool IsPaid,
    DateTimeOffset? PaidAt,
    decimal? PaidAmount);
