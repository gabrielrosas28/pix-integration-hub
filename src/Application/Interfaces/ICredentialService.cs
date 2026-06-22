using Application.DTOs;
using ApiService.Domain.Entities;
using Domain.Aggregates.Credential;
using Credential = Domain.Aggregates.Credential.Credential;

namespace Application.Interfaces;

public interface ICredentialService // Alterado de ISecretService
{
    Task<List<Credential>> GetAllAsync(); // Alterado de Secret para Credential

    Task<Credential?> GetByIdAsync(Guid id); // Alterado de Secret para Credential

    Task<Credential> CreateAsync(CreateCredentialRequest request); // Alterado de CreateSecretRequest

    Task<Credential?> UpdateAsync(Guid id, UpdateCredentialRequest request); // Alterado de UpdateSecretRequest

    Task<bool> DeleteAsync(Guid id);
}