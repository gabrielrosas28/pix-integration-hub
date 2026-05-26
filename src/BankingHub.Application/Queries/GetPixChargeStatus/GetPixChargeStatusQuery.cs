using MediatR;

namespace BankingHub.Application.Queries.GetPixChargeStatus;

public sealed record GetPixChargeStatusQuery(string TxId) : IRequest<PixChargeStatusResponse>;

public sealed record PixChargeStatusResponse(
    string TxId,
    string BankId,
    string ChargeType,
    string Status,
    string Emv,
    string? PixLink,
    DateTime CreatedAt,
    DateTime? PaidAt,
    decimal? PaidAmount);
