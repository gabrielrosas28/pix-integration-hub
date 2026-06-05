using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ContaService : IContaService
{
    private readonly ApplicationDbContext _context;

    public ContaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Conta>> GetAllAsync()
    {
        return await _context.Contas.ToListAsync();
    }

    public async Task<Conta?> GetByIdAsync(int id)
    {
        return await _context.Contas.FindAsync(id);
    }

    public async Task<Conta> CreateAsync(CreateContaRequest request)
    {
        var conta = new Conta
        {
            CredentialId = request.CredentialId, // Alterado de SecretId para CredentialId
            Documento = request.Documento,
            BankId = request.BankId,
            NumeroConta = request.NumeroConta,
            Agencia = request.Agencia
        };

        _context.Contas.Add(conta);

        await _context.SaveChangesAsync();

        return conta;
    }

    public async Task<Conta?> UpdateAsync(int id, UpdateContaRequest request)
    {
        var conta = await _context.Contas.FindAsync(id);

        if (conta == null)
            return null;

        conta.CredentialId = request.CredentialId; // Alterado de SecretId para CredentialId
        conta.Documento = request.Documento;
        conta.BankId = request.BankId;
        conta.NumeroConta = request.NumeroConta;
        conta.Agencia = request.Agencia;

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