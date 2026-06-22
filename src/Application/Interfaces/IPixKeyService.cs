using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface IPixKeyService
{
    Task<List<PixKey>> GetAllAsync();
    Task<PixKey?> GetByIdAsync(int id);
    Task<PixKey> CreateAsync(CreatePixKeyRequest request);
    Task<PixKey?> UpdateAsync(int id, UpdatePixKeyRequest request);
    Task<bool> DeleteAsync(int id);
}