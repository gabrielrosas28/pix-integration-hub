using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class AccountRepository
{
    private readonly ApplicationDbContext _context;
    public AccountRepository(ApplicationDbContext context) => _context = context;

    public Task<List<Account>> GetAllAsync() => _context.Accounts.ToListAsync();
    public Task<Account?> GetByIdAsync(int id) => _context.Accounts.FindAsync(id).AsTask();

    public async Task<Account> AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> UpdateAsync(int id, Action<Account> update)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account is null) return null;
        update(account);
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

public sealed class SecretRepository
{
    private readonly ApplicationDbContext _context;
    public SecretRepository(ApplicationDbContext context) => _context = context;

    public Task<List<Secret>> GetAllAsync() => _context.Secrets.ToListAsync();
    public Task<Secret?> GetByIdAsync(int id) => _context.Secrets.FindAsync(id).AsTask();

    public async Task<Secret> AddAsync(Secret secret)
    {
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();
        return secret;
    }

    public async Task<Secret?> UpdateAsync(int id, Action<Secret> update)
    {
        var secret = await _context.Secrets.FindAsync(id);
        if (secret is null) return null;
        update(secret);
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

public sealed class PixKeyRepository
{
    private readonly ApplicationDbContext _context;
    public PixKeyRepository(ApplicationDbContext context) => _context = context;

    public Task<List<PixKey>> GetAllAsync()
        => _context.PixKeys.Include(p => p.Account).ToListAsync();

    public Task<PixKey?> GetByIdAsync(int id)
        => _context.PixKeys.Include(p => p.Account).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PixKey> AddAsync(PixKey pixKey)
    {
        _context.PixKeys.Add(pixKey);
        await _context.SaveChangesAsync();
        return pixKey;
    }

    public async Task<PixKey?> UpdateAsync(int id, Action<PixKey> update)
    {
        var pixKey = await _context.PixKeys.FindAsync(id);
        if (pixKey is null) return null;
        update(pixKey);
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

public sealed class ChargeRepository
{
    private readonly ApplicationDbContext _context;
    public ChargeRepository(ApplicationDbContext context) => _context = context;

    public Task<Charge?> GetByTxIdAsync(string txId)
        => _context.Charges.FirstOrDefaultAsync(c => c.TxId == txId);

    public async Task<Charge> AddAsync(Charge charge)
    {
        _context.Charges.Add(charge);
        await _context.SaveChangesAsync();
        return charge;
    }

    public async Task UpdateStatusAsync(string txId, string status)
    {
        var charge = await _context.Charges.FirstOrDefaultAsync(c => c.TxId == txId);
        if (charge is null) return;
        charge.Status = status;
        charge.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
