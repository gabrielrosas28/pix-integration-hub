using ApiService.Infrastructure.Data;
using Application.DTOs;
using Application.Interfaces;
using ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CobrancaService : ICobrancaService
{
    private readonly ApplicationDbContext _db;

    public CobrancaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CobResponse> CreateCobAsync(CreateCobRequest request, CancellationToken cancellationToken = default)
    {
        var txId = Guid.NewGuid().ToString("N");

        var emv = $"EMV:{txId}";
        var pixLink = $"pix://{txId}";

        var cobranca = new Cobranca
        {
            TxId = txId,
            InvoiceID = request.InvoiceID,
            ChargeType = request.ChargeType,
            Amount = request.Amount,
            PixKey = request.PixKey,
            DueDate = request.DueDate?.ToDateTime(TimeOnly.MinValue),
            ExpiresInSeconds = request.ExpiresInSeconds,
            PayerMessage = request.PayerMessage,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            Raw = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Cobrancas.Add(cobranca);
        await _db.SaveChangesAsync(cancellationToken);

        return new CobResponse
        {
            TxId = txId,
            Status = cobranca.Status,
            Emv = emv,
            PixLink = pixLink
        };
    }

    public async Task<CobResponse> CreateCobvAsync(CreateCobvRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DueDate is null && request.ExpiresInSeconds is null)
        {
            throw new ArgumentException("Cobv requires DueDate or ExpiresInSeconds");
        }

        var txId = Guid.NewGuid().ToString("N");
        var emv = $"EMV:{txId}";
        var pixLink = $"pix://{txId}";

        var cobranca = new Cobranca
        {
            TxId = txId,
            InvoiceID = request.InvoiceID,
            ChargeType = "Cobv",
            Amount = request.Amount,
            PixKey = request.PixKey,
            DueDate = request.DueDate?.ToDateTime(TimeOnly.MinValue),
            ExpiresInSeconds = request.ExpiresInSeconds,
            PayerMessage = request.PayerMessage,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            Raw = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Cobrancas.Add(cobranca);
        await _db.SaveChangesAsync(cancellationToken);

        return new CobResponse
        {
            TxId = txId,
            Status = cobranca.Status,
            Emv = emv,
            PixLink = pixLink
        };
    }
}
