using Domain.Entities;

namespace Application.Interfaces;

public interface IAccountService
{
    Task<List<Account>> GetAllAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<Account> CreateAsync(CreateAccountRequest request);
    Task<Account?> UpdateAsync(int id, UpdateAccountRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface ISecretService
{
    Task<List<Secret>> GetAllAsync();
    Task<Secret?> GetByIdAsync(int id);
    Task<Secret> CreateAsync(CreateSecretRequest request);
    Task<Secret?> UpdateAsync(int id, UpdateSecretRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IPixKeyService
{
    Task<List<PixKey>> GetAllAsync();
    Task<PixKey?> GetByIdAsync(int id);
    Task<PixKey> CreateAsync(CreatePixKeyRequest request);
    Task<PixKey?> UpdateAsync(int id, UpdatePixKeyRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IChargeService
{
    Task<ChargeDto> CreateCobAsync(CreateCobRequest request, CancellationToken ct = default);
    Task<ChargeDto> CreateCobVAsync(CreateCobVRequest request, CancellationToken ct = default);
}
