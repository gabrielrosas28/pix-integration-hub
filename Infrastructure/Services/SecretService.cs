using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SecretService : ISecretService
{
    private readonly ApplicationDbContext _context;

    public SecretService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Secret>> GetAllAsync()
    {
        return await _context.Secrets.ToListAsync();
    }

    public async Task<Secret?> GetByIdAsync(int id)
    {
        return await _context.Secrets.FindAsync(id);
    }

    public async Task<Secret> CreateAsync(CreateSecretRequest request)
    {
        var secret = new Secret
        {
            ClientId = request.ClientId,
            ClienteSecret = request.ClienteSecret,
            Certificado = request.Certificado,
            SenhaCertificado = request.SenhaCertificado,
            ContaId = request.ContaId
        };

        _context.Secrets.Add(secret);

        await _context.SaveChangesAsync();

        return secret;
    }

    public async Task<Secret?> UpdateAsync(int id, UpdateSecretRequest request)
    {
        var secret = await _context.Secrets.FindAsync(id);

        if (secret == null)
            return null;

        secret.ClientId = request.ClientId;
        secret.ClienteSecret = request.ClienteSecret;
        secret.Certificado = request.Certificado;
        secret.SenhaCertificado = request.SenhaCertificado;
        secret.ContaId = request.ContaId;

        await _context.SaveChangesAsync();

        return secret;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var secret = await _context.Secrets.FindAsync(id);

        if (secret is null)
            return false;

        _context.Secrets.Remove(secret);

        await _context.SaveChangesAsync();

        return true;
    }
}
