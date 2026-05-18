using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface ISecretService
{
    Task<List<Secret>> GetAllAsync();

    Task<Secret?> GetByIdAsync(int id);

    Task<Secret> CreateAsync(CreateSecretRequest request);

    Task<Secret?> UpdateAsync(int id, UpdateSecretRequest request);

    Task<bool> DeleteAsync(int id);
}
