using BankingHub.Application.Common.Exceptions;
using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.Repositories;
using MediatR;

namespace BankingHub.Application.Queries.GetInvoice;

public sealed class GetInvoiceHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceQuery query, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(InvoiceId.From(query.InvoiceId), ct)
            ?? throw new NotFoundException("Invoice", query.InvoiceId);

        return new InvoiceDto(
            Id: invoice.Id.Value,
            Amount: invoice.Amount.Value,
            Currency: invoice.Amount.Currency,
            DueDate: invoice.DueDate,
            Status: invoice.Status.ToString(),
            TxId: invoice.TxId?.Value,
            ExternalReference: invoice.ExternalReference,
            CreatedAt: invoice.CreatedAt,
            PaidAt: invoice.PaidAt,
            BankId: invoice.BankId);
    }
}
