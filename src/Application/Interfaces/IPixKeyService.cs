using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface IChavePixService
{
    Task<List<ChavePix>> GetAllAsync();
    Task<ChavePix?> GetByIdAsync(int id);
    Task<ChavePix> CreateAsync(CreatePixKeyRequest request);
    Task<ChavePix?> UpdateAsync(int id, UpdatePixKeyRequest request);
    Task<bool> DeleteAsync(int id);
}