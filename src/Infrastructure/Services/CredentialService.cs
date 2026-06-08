using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CredentialService : ICredentialService
{
    private readonly ApplicationDbContext _context;

    public CredentialService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Credential>> GetAllAsync()
    {
        return await _context.Credentials.ToListAsync();
    }

    public async Task<Credential?> GetByIdAsync(int id)
    {
        return await _context.Credentials.FindAsync(id);
    }

    public async Task<Credential> CreateAsync(CreateCredentialRequest request)
    {
        var credential = new Credential
        {
            ClientId = request.ClientId,
            ClientSecretValue = request.ClientSecretValue,
            Certificado = request.Certificado,
            SenhaCertificado = request.SenhaCertificado,
            ContaId = request.ContaId
        };

        _context.Credentials.Add(credential);
        await _context.SaveChangesAsync();

        return credential;
    }

    public async Task<Credential?> UpdateAsync(int id, UpdateCredentialRequest request)
    {
        var credential = await _context.Credentials.FindAsync(id);

        if (credential == null)
            return null;

        credential.ClientId = request.ClientId;
        credential.ClientSecretValue = request.ClientSecretValue;
        credential.Certificado = request.Certificado;
        credential.SenhaCertificado = request.SenhaCertificado;
        credential.ContaId = request.ContaId;

        await _context.SaveChangesAsync();

        return credential;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var credential = await _context.Credentials.FindAsync(id);

        if (credential is null)
            return false;

        _context.Credentials.Remove(credential);
        await _context.SaveChangesAsync();

        return true;
    }
}
