using MediatR;
using Domain.Aggregates.Invoice;
using Domain.Aggregates.PixCharge;
using Domain.Repositories;
using Domain.ValueObjects;
using Application.Interfaces;

namespace Application.Queries;

// ---- Invoice DTOs ----

public sealed record InvoiceDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly DueDate,
    string Status,
    string BankId,
    string? TxId,
    string? ExternalReference,
    DateTime CreatedAt,
    DateTime? PaidAt);

public sealed record PixChargeStatusDto(
    string TxId,
    string Status,
    string ChargeType,
    decimal? Amount,
    string Emv,
    string? PixLink,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ---- GetInvoice Query ----

public sealed record GetInvoiceQuery(Guid InvoiceId) : IRequest<InvoiceDto?>;

public sealed class GetInvoiceHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _repository;

    public GetInvoiceHandler(IInvoiceRepository repository)
        => _repository = repository;

    public async Task<InvoiceDto?> Handle(GetInvoiceQuery query, CancellationToken ct)
    {
        var invoice = await _repository.GetByIdAsync(InvoiceId.From(query.InvoiceId), ct);

        if (invoice is null)
            return null;

        return new InvoiceDto(
            Id: invoice.Id.Value,
            Amount: invoice.Amount.Value,
            Currency: invoice.Amount.Currency,
            DueDate: invoice.DueDate,
            Status: invoice.Status.ToString(),
            BankId: invoice.BankId,
            TxId: invoice.TxId?.Value,
            ExternalReference: invoice.ExternalReference,
            CreatedAt: invoice.CreatedAt,
            PaidAt: invoice.PaidAt);
    }
}

// ---- GetPixChargeStatus Query ----

public sealed record GetPixChargeStatusQuery(string TxId) : IRequest<PixChargeStatusDto?>;

public sealed class GetPixChargeStatusHandler : IRequestHandler<GetPixChargeStatusQuery, PixChargeStatusDto?>
{
    private readonly IPixChargeRepository _repository;
    private readonly IBankAdapterFactory _adapterFactory;

    public GetPixChargeStatusHandler(
        IPixChargeRepository repository,
        IBankAdapterFactory adapterFactory)
    {
        _repository = repository;
        _adapterFactory = adapterFactory;
    }

    public async Task<PixChargeStatusDto?> Handle(GetPixChargeStatusQuery query, CancellationToken ct)
    {
        var txId = TxId.From(query.TxId);
        var charge = await _repository.GetByTxIdAsync(txId, ct);

        if (charge is null)
            return null;

        // Always query the bank for the latest status
        var adapter = _adapterFactory.Get(charge.BankId);
        var bankStatus = await adapter.GetChargeStatusAsync(query.TxId, charge.ChargeType, ct);

        return new PixChargeStatusDto(
            TxId: query.TxId,
            Status: bankStatus.Status.ToString(),
            ChargeType: charge.ChargeType.ToString(),
            Amount: bankStatus.PaidAmount ?? charge.Amount?.Value,
            Emv: charge.Emv.Value,
            PixLink: charge.PixLink,
            CreatedAt: charge.CreatedAt,
            UpdatedAt: charge.UpdatedAt);
    }
}
