using BankingHub.Application.Common.Exceptions;
using BankingHub.Domain.Repositories;
using BankingHub.Domain.ValueObjects;
using MediatR;

namespace BankingHub.Application.Queries.GetPixChargeStatus;

public sealed class GetPixChargeStatusHandler
    : IRequestHandler<GetPixChargeStatusQuery, PixChargeStatusResponse>
{
    private readonly IPixChargeRepository _chargeRepository;

    public GetPixChargeStatusHandler(IPixChargeRepository chargeRepository)
    {
        _chargeRepository = chargeRepository;
    }

    public async Task<PixChargeStatusResponse> Handle(
        GetPixChargeStatusQuery query,
        CancellationToken ct)
    {
        var charge = await _chargeRepository.GetByTxIdAsync(TxId.From(query.TxId), ct)
            ?? throw new NotFoundException("PixCharge", query.TxId);

        return new PixChargeStatusResponse(
            TxId: charge.TxId.Value,
            BankId: charge.BankId,
            ChargeType: charge.ChargeType.ToString(),
            Status: charge.Status.ToString(),
            Emv: charge.Emv.Value,
            PixLink: charge.PixLink,
            CreatedAt: charge.CreatedAt,
            PaidAt: charge.PaidAt,
            PaidAmount: charge.PaidAmount?.Value);
    }
}
