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
        return await _context.ChavesPix.Include(c => c.Account).ToListAsync();
    }

    public async Task<PixKey?> GetByIdAsync(int id)
    {
        return await _context.ChavesPix.Include(c => c.Account).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PixKey> CreateAsync(CreatePixKeyRequest request)
    {
        var chavePix = new PixKey
        {
            AccountId = request.AccountId,
            Key = request.Key,
            Type = request.Type
        };

        _context.ChavesPix.Add(chavePix);
        await _context.SaveChangesAsync();
        return chavePix;
    }

    public async Task<PixKey?> UpdateAsync(int id, UpdatePixKeyRequest request)
    {
        var chavePix = await _context.ChavesPix.FindAsync(id);
        if (chavePix == null) return null;

        chavePix.Key = request.Key;
        chavePix.Type = request.Type;

        await _context.SaveChangesAsync();
        return chavePix;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var chavePix = await _context.ChavesPix.FindAsync(id);
        if (chavePix == null) return false;

        _context.ChavesPix.Remove(chavePix);
        await _context.SaveChangesAsync();
        return true;
    }
}
