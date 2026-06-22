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
        return await _context.Contas.ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Contas.FindAsync(id);
    }

    public async Task<Account> CreateAsync(CreateAccountRequest request)
    {
        var conta = new Account
        {
            CredentialId = request.CredentialId, // Alterado de SecretId para CredentialId
            Document = request.Document,
            BankId = request.BankId,
            AccountNumber = request.AccountNumber,
            Agency = request.Agency
        };

        _context.Contas.Add(conta);

        await _context.SaveChangesAsync();

        return conta;
    }

    public async Task<Account?> UpdateAsync(int id, UpdateAccountRequest request)
    {
        var conta = await _context.Contas.FindAsync(id);

        if (conta == null)
            return null;

        conta.CredentialId = request.CredentialId; // Alterado de SecretId para CredentialId
        conta.Document = request.Document;
        conta.BankId = request.BankId;
        conta.AccountNumber = request.AccountNumber;
        conta.Agency = request.Agency;

        await _context.SaveChangesAsync();

        return conta;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var conta = await _context.Contas.FindAsync(id);

        if (conta is null)
            return false;

        _context.Contas.Remove(conta);

        await _context.SaveChangesAsync();

        return true;
    }
}