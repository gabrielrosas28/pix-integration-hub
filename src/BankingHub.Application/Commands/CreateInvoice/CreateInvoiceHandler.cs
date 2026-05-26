using BankingHub.Application.Interfaces;
using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.Repositories;
using BankingHub.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Commands.CreateInvoice;

public sealed class CreateInvoiceHandler
    : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResult>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<CreateInvoiceHandler> _logger;

    public CreateInvoiceHandler(
        IInvoiceRepository invoiceRepository,
        IBankAdapterFactory adapterFactory,
        ILogger<CreateInvoiceHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task<CreateInvoiceResult> Handle(
        CreateInvoiceCommand cmd,
        CancellationToken ct)
    {
        if (!_adapterFactory.IsSupported(cmd.BankId))
            throw new Common.Exceptions.ValidationException(
                $"Bank '{cmd.BankId}' is not supported. Available: {string.Join(", ", _adapterFactory.GetAvailableBanks())}");

        var invoice = Invoice.Create(
            amount: Money.BRL(cmd.Amount),
            dueDate: cmd.DueDate,
            bankId: cmd.BankId,
            externalReference: cmd.ExternalReference);

        await _invoiceRepository.AddAsync(invoice, ct);

        _logger.LogInformation(
            "Invoice created: Id={InvoiceId}, Bank={BankId}, Amount={Amount}",
            invoice.Id, invoice.BankId, invoice.Amount);

        return new CreateInvoiceResult(invoice.Id.Value, invoice.Status.ToString());
    }
}
