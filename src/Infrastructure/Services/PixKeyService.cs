using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PixKeyService : IPixKeyService
{
    private readonly ApplicationDbContext _context;

    public PixKeyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PixKey>> GetAllAsync()
    {
        return await _context.PixKeys.Include(p => p.Account).ToListAsync();
    }

    public async Task<PixKey?> GetByIdAsync(int id)
    {
        return await _context.PixKeys.Include(p => p.Account).FirstOrDefaultAsync(p => p.Id == id);
    }

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
        if (pixKey == null) return null;

        pixKey.Key = request.Key;
        pixKey.Type = request.Type;

        await _context.SaveChangesAsync();

        return pixKey;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pixKey = await _context.PixKeys.FindAsync(id);
        if (pixKey == null) return false;

        _context.PixKeys.Remove(pixKey);
        await _context.SaveChangesAsync();

        return true;
    }
}
