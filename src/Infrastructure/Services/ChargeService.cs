using ApiService.Infrastructure.Data;
using Application.DTOs;
using Application.Interfaces;
using ApiService.Domain.Entities;

namespace Infrastructure.Services;

public class ChargeService : IChargeService
{
    private readonly ApplicationDbContext _db;

    public ChargeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CobResponse> CreateCobAsync(CreateCobRequest request, CancellationToken cancellationToken = default)
    {
        var txId = Guid.NewGuid().ToString("N");
        var emv = $"EMV:{txId}";
        var pixLink = $"pix://{txId}";

        var charge = new Charge
        {
            TxId = txId,
            InvoiceID = request.InvoiceID,
            ChargeType = request.ChargeType,
            Amount = request.Amount,
            PixKey = request.PixKey,
            DueDate = request.DueDate,
            ExpiresInSeconds = request.ExpiresInSeconds,
            PayerMessage = request.PayerMessage,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            Raw = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Charges.Add(charge);
        await _db.SaveChangesAsync(cancellationToken);

        return new CobResponse
        {
            TxId = txId,
            Status = charge.Status,
            Emv = emv,
            PixLink = pixLink
        };
    }

    public async Task<CobResponse> CreateCobvAsync(CreateCobvRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DueDate is null && request.ExpiresInSeconds is null)
            throw new ArgumentException("CobV requires DueDate or ExpiresInSeconds");

        var txId = Guid.NewGuid().ToString("N");
        var emv = $"EMV:{txId}";
        var pixLink = $"pix://{txId}";

        var charge = new Charge
        {
            TxId = txId,
            InvoiceID = request.InvoiceID,
            ChargeType = "CobV",
            Amount = request.Amount,
            PixKey = request.PixKey,
            DueDate = request.DueDate,
            ExpiresInSeconds = request.ExpiresInSeconds,
            PayerMessage = request.PayerMessage,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            Raw = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Charges.Add(charge);
        await _db.SaveChangesAsync(cancellationToken);

        return new CobResponse
        {
            TxId = txId,
            Status = charge.Status,
            Emv = emv,
            PixLink = pixLink
        };
    }
}
