using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface IContaService
{
    Task<List<Conta>> GetAllAsync();

    Task<Conta?> GetByIdAsync(int id);

    Task<Conta> CreateAsync(CreateContaRequest request);

    Task<Conta?> UpdateAsync(int id, UpdateContaRequest request);

    Task<bool> DeleteAsync(int id);
}