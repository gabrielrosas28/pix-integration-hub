using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface ICredentialService
{
    Task<List<Credential>> GetAllAsync();

    Task<Credential?> GetByIdAsync(int id);

    Task<Credential> CreateAsync(CreateCredentialRequest request);

    Task<Credential?> UpdateAsync(int id, UpdateCredentialRequest request);

    Task<bool> DeleteAsync(int id);
}
