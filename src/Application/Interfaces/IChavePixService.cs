using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface IChavePixService
{
    Task<List<ChavePix>> GetAllAsync();
    Task<ChavePix?> GetByIdAsync(int id);
    Task<ChavePix> CreateAsync(CreateChavePixRequest request);
    Task<ChavePix?> UpdateAsync(int id, UpdateChavePixRequest request);
    Task<bool> DeleteAsync(int id);
}