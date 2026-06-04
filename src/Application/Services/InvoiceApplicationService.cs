using BankingHub.Application.Commands.CreateInvoice;
using BankingHub.Application.Commands.CreatePixCharge;
using BankingHub.Application.DTOs;
using BankingHub.Application.Queries.GetInvoice;
using MediatR;

namespace BankingHub.Application.Services;


public class InvoiceApplicationService
{
    private readonly IMediator _mediator;

    public InvoiceApplicationService(IMediator mediator)
    {
        _mediator = mediator;
    }

    
    public async Task<ChargeResponseDto> CreateInvoiceWithChargeAsync(
        ChargeRequestDto request,
        CancellationToken ct = default)
    {
        // 1. Cria a Invoice
        var invoiceResult = await _mediator.Send(
            new CreateInvoiceCommand(
                Amount:            request.Amount,
                DueDate:           request.DueDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                BankId:            "ITAU",
                ExternalReference: null),
            ct);

        // 2. Cria a cobrança Pix vinculada à Invoice
        var chargeResult = await _mediator.Send(
            new CreatePixChargeCommand(
                InvoiceId:        invoiceResult.InvoiceId,
                ChargeType:       request.ChargeType,
                Amount:           request.Amount,
                PixKey:           request.PixKey,
                DueDate:          request.DueDate,
                ExpiresInSeconds: request.ExpiresInSeconds,
                PayerMessage:     request.PayerMessage),
            ct);

        return new ChargeResponseDto(
            TxId:          chargeResult.TxId,
            Status:        chargeResult.Status,
            Emv:           chargeResult.Emv,
            PixLink:       chargeResult.PixLink,
            QrCodeBase64:  string.Empty); // QR Code gerado na camada de Presentation
    }

    
    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _mediator.Send(new GetInvoiceQuery(invoiceId), ct);
    }
}
