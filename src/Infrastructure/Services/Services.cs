using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    public AccountService(ApplicationDbContext context) => _context = context;

    public Task<List<Account>> GetAllAsync() => _context.Accounts.ToListAsync();

    public async Task<Account?> GetByIdAsync(int id) => await _context.Accounts.FindAsync(id);

    public async Task<Account> CreateAsync(CreateAccountRequest request)
    {
        var account = new Account
        {
            SecretId = request.SecretId,
            Document = request.Document,
            BankId = request.BankId,
            AccountNumber = request.AccountNumber,
            Agency = request.Agency
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> UpdateAsync(int id, UpdateAccountRequest request)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account is null) return null;
        account.SecretId = request.SecretId;
        account.Document = request.Document;
        account.BankId = request.BankId;
        account.AccountNumber = request.AccountNumber;
        account.Agency = request.Agency;
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account is null) return false;
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return true;
    }
}

public sealed class SecretService : ISecretService
{
    private readonly ApplicationDbContext _context;
    public SecretService(ApplicationDbContext context) => _context = context;

    public Task<List<Secret>> GetAllAsync() => _context.Secrets.ToListAsync();

    public async Task<Secret?> GetByIdAsync(int id) => await _context.Secrets.FindAsync(id);

    public async Task<Secret> CreateAsync(CreateSecretRequest request)
    {
        var secret = new Secret
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            Certificate = request.Certificate,
            CertificatePassword = request.CertificatePassword,
            AccountId = request.AccountId
        };
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();
        return secret;
    }

    public async Task<Secret?> UpdateAsync(int id, UpdateSecretRequest request)
    {
        var secret = await _context.Secrets.FindAsync(id);
        if (secret is null) return null;
        secret.ClientId = request.ClientId;
        secret.ClientSecret = request.ClientSecret;
        secret.Certificate = request.Certificate;
        secret.CertificatePassword = request.CertificatePassword;
        secret.AccountId = request.AccountId;
        await _context.SaveChangesAsync();
        return secret;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var secret = await _context.Secrets.FindAsync(id);
        if (secret is null) return false;
        _context.Secrets.Remove(secret);
        await _context.SaveChangesAsync();
        return true;
    }
}

public sealed class PixKeyService : IPixKeyService
{
    private readonly ApplicationDbContext _context;
    public PixKeyService(ApplicationDbContext context) => _context = context;

    public Task<List<PixKey>> GetAllAsync()
        => _context.PixKeys.Include(p => p.Account).ToListAsync();

    public Task<PixKey?> GetByIdAsync(int id)
        => _context.PixKeys.Include(p => p.Account).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PixKey> CreateAsync(CreatePixKeyRequest request)
    {
        var pixKey = new PixKey
        {
            AccountId = request.AccountId,
            Key = request.Key,
            Type = request.Type
        };
        _context.PixKeys.Add(pixKey);
        await _context.SaveChangesAsync();
        return pixKey;
    }

    public async Task<PixKey?> UpdateAsync(int id, UpdatePixKeyRequest request)
    {
        var pixKey = await _context.PixKeys.FindAsync(id);
        if (pixKey is null) return null;
        pixKey.Key = request.Key;
        pixKey.Type = request.Type;
        await _context.SaveChangesAsync();
        return pixKey;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pixKey = await _context.PixKeys.FindAsync(id);
        if (pixKey is null) return false;
        _context.PixKeys.Remove(pixKey);
        await _context.SaveChangesAsync();
        return true;
    }
}

public sealed class ChargeService : IChargeService
{
    private readonly ApplicationDbContext _context;

    public ChargeService(ApplicationDbContext context) => _context = context;

    public async Task<ChargeDto> CreateCobAsync(CreateCobRequest request, CancellationToken ct = default)
    {
        var txId = GenerateTxId();
        var emv = $"00020126{txId}";  // placeholder — real EMV comes from bank adapter
        var pixLink = $"pix.example.com/{txId}";

        var charge = new Charge
        {
            TxId = txId,
            InvoiceId = request.InvoiceId,
            ChargeType = "Cob",
            Amount = request.Amount,
            PixKey = request.PixKey,
            DueDate = request.DueDate,
            ExpiresInSeconds = request.ExpiresInSeconds,
            PayerMessage = request.PayerMessage,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            CreatedAt = DateTime.UtcNow
        };

        _context.Charges.Add(charge);
        await _context.SaveChangesAsync(ct);

        return new ChargeDto { TxId = txId, Status = charge.Status, Emv = emv, PixLink = pixLink };
    }

    public async Task<ChargeDto> CreateCobVAsync(CreateCobVRequest request, CancellationToken ct = default)
    {
        if (request.DueDate is null && request.ExpiresInSeconds is null)
            throw new ArgumentException("CobV requires DueDate or ExpiresInSeconds.");

        var txId = GenerateTxId();
        var emv = $"00020126{txId}";
        var pixLink = $"pix.example.com/{txId}";

        var charge = new Charge
        {
            TxId = txId,
            InvoiceId = request.InvoiceId,
            ChargeType = "CobV",
            Amount = request.Amount,
            PixKey = request.PixKey,
            DueDate = request.DueDate,
            ExpiresInSeconds = request.ExpiresInSeconds,
            PayerMessage = request.PayerMessage,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            CreatedAt = DateTime.UtcNow
        };

        _context.Charges.Add(charge);
        await _context.SaveChangesAsync(ct);

        return new ChargeDto { TxId = txId, Status = charge.Status, Emv = emv, PixLink = pixLink };
    }

    /// <summary>
    /// Generates a TxId following BACEN standard: up to 35 alphanumeric chars.
    /// </summary>
    private static string GenerateTxId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..10].ToUpper();
        return $"PIX{timestamp}{random}"[..35];
    }
}
