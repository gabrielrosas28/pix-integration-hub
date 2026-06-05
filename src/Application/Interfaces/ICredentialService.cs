using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface ICredentialService // Alterado de ISecretService
{
    Task<List<Credential>> GetAllAsync(); // Alterado de Secret para Credential

    Task<Credential?> GetByIdAsync(int id); // Alterado de Secret para Credential

    Task<Credential> CreateAsync(CreateCredentialRequest request); // Alterado de CreateSecretRequest

    Task<Credential?> UpdateAsync(int id, UpdateCredentialRequest request); // Alterado de UpdateSecretRequest

    Task<bool> DeleteAsync(int id);
}