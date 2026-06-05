using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CredentialService : ICredentialService // Alterado herança e nome da classe
{
    private readonly ApplicationDbContext _context;

    public CredentialService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Credential>> GetAllAsync()
    {
        return await _context.Credentials.ToListAsync(); // Alterado de Secrets para Credentials
    }

    public async Task<Credential?> GetByIdAsync(int id)
    {
        return await _context.Credentials.FindAsync(id); // Alterado de Secrets para Credentials
    }

    public async Task<Credential> CreateAsync(CreateCredentialRequest request)
    {
        var credential = new Credential // Alterado de Secret para Credential
        {
            ClientId = request.ClientId,
            ClientSecretValue = request.ClientSecretValue, // Alterado para refletir a nova propriedade da entidade
            Certificado = request.Certificado,
            SenhaCertificado = request.SenhaCertificado
        };

        _context.Credentials.Add(credential); // Alterado para Credentials

        await _context.SaveChangesAsync();

        return credential;
    }

    public async Task<Credential?> UpdateAsync(int id, UpdateCredentialRequest request)
    {
        var credential = await _context.Credentials.FindAsync(id); // Alterado de Secrets para Credentials

        if (credential == null)
            return null;

        credential.ClientId = request.ClientId;
        credential.ClientSecretValue = request.ClientSecretValue; // Alterado
        credential.Certificado = request.Certificado;
        credential.SenhaCertificado = request.SenhaCertificado;

        await _context.SaveChangesAsync();

        return credential;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var credential = await _context.Credentials.FindAsync(id); // Alterado de Secrets para Credentials

        if (credential is null)
            return false;

        _context.Credentials.Remove(credential); // Alterado para Credentials

        await _context.SaveChangesAsync();

        return true;
    }
}