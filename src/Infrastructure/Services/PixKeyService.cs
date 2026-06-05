using Application.DTOs;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ChavePixService : IChavePixService
{
    private readonly ApplicationDbContext _context;

    public ChavePixService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChavePix>> GetAllAsync()
    {
        return await _context.ChavesPix.Include(c => c.Conta).ToListAsync();
    }

    public async Task<ChavePix?> GetByIdAsync(int id)
    {
        return await _context.ChavesPix.Include(c => c.Conta).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ChavePix> CreateAsync(CreateChavePixRequest request)
    {
        var chavePix = new ChavePix
        {
            ContaId = request.ContaId,
            Chave = request.Chave,
            Tipo = request.Tipo
        };

        _context.ChavesPix.Add(chavePix);
        await _context.SaveChangesAsync();
        return chavePix;
    }

    public async Task<ChavePix?> UpdateAsync(int id, UpdateChavePixRequest request)
    {
        var chavePix = await _context.ChavesPix.FindAsync(id);
        if (chavePix == null) return null;

        chavePix.Chave = request.Chave;
        chavePix.Tipo = request.Tipo;

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
