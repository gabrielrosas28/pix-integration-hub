using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;

    public AccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task<Account> CreateAsync(CreateAccountRequest request)
    {
        var account = new Account
        {
            CredentialId = request.CredentialId,
            Document = request.Document,
            BankId = request.BankId,
            AccountNumber = request.AccountNumber,
            Branch = request.Branch
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    public async Task<Account?> UpdateAsync(int id, UpdateAccountRequest request)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
            return null;

        account.CredentialId = request.CredentialId;
        account.Document = request.Document;
        account.BankId = request.BankId;
        account.AccountNumber = request.AccountNumber;
        account.Branch = request.Branch;

        await _context.SaveChangesAsync();

        return account;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account is null)
            return false;

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return true;
    }
}
