using Application.Interfaces;
using ApiService.Domain.Entities;
using BankingHub.Application.Commands.CreateInvoice;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Commands.CreateInvoice;


public sealed class CreateInvoiceHandler
    : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<CreateInvoiceHandler> _logger;

    public CreateInvoiceHandler(
        IApplicationDbContext db,
        ILogger<CreateInvoiceHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CreateInvoiceResult> Handle(
        CreateInvoiceCommand cmd,
        CancellationToken ct)
    {
        if (cmd.Amount <= 0)
            throw new ArgumentException("O valor da Invoice deve ser positivo.");

        if (cmd.DueDate < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("A data de vencimento não pode ser no passado.");

        if (string.IsNullOrWhiteSpace(cmd.BankId))
            throw new ArgumentException("O banco deve ser informado.");

        var invoiceId = Guid.NewGuid();

        // Persiste a Invoice como cobrança pendente ainda sem TxId
        var cobranca = new Charge
        {
            TxId       = string.Empty,   // Será preenchido ao criar o PixCharge
            InvoiceID  = invoiceId.ToString(),
            ChargeType = "PENDING",
            Amount     = cmd.Amount,
            PixKey     = string.Empty,
            Status     = "open",
            Raw        = cmd.ExternalReference ?? string.Empty,
            CreatedAt  = DateTime.UtcNow
        };

        _db.Cobrancas.Add(cobranca);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Invoice criada: InvoiceId={InvoiceId}, Banco={BankId}, Valor={Amount}",
            invoiceId, cmd.BankId, cmd.Amount);

        return new CreateInvoiceResult(
            InvoiceId: invoiceId,
            Status:    "open",
            Amount:    cmd.Amount,
            DueDate:   cmd.DueDate,
            BankId:    cmd.BankId);
    }
}
