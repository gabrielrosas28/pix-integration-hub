using Application.DTOs;
using Application.Interfaces;
using Domain.Aggregates.Credential;
using ApiService.Infrastructure.Data;
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
        return await _context.Credentials.ToListAsync();
    }

    public async Task<Credential?> GetByIdAsync(Guid id)
    {
        var credentialId = CredentialId.From(id);
        return await _context.Credentials
            .FirstOrDefaultAsync(c => c.Id == credentialId);
    }

    public async Task<Credential> CreateAsync(CreateCredentialRequest request)
    {
        // Usa a factory do agregado (garante invariantes de domínio)
        var credential = Credential.Create(
            clientId:            request.ClientId,
            clientSecret:        request.ClientSecretValue,
            certificate:         request.Certificate,
            certificatePassword: request.CertificatePassword);

        _context.Credentials.Add(credential);
        await _context.SaveChangesAsync();

        return credential;
    }

    public async Task<Credential?> UpdateAsync(Guid id, UpdateCredentialRequest request)
    {
        var credentialId = CredentialId.From(id);
        var credential = await _context.Credentials
            .FirstOrDefaultAsync(c => c.Id == credentialId);

        if (credential is null)
            return null;

        // O agregado só permite atualizar o certificado/senha (regra de domínio)
        credential.UpdateCertificate(request.Certificate, request.CertificatePassword);

        await _context.SaveChangesAsync();

        return credential;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var credentialId = CredentialId.From(id);
        var credential = await _context.Credentials
            .FirstOrDefaultAsync(c => c.Id == credentialId);

        if (credential is null)
            return false;

        _context.Credentials.Remove(credential);
        await _context.SaveChangesAsync();

        return true;
    }
}
