using Application.DTOs;
using ApiService.Domain.Entities;

namespace Application.Interfaces;

public interface IAccountService
{
    Task<List<Account>> GetAllAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<Account> CreateAsync(CreateAccountRequest request);
    Task<Account?> UpdateAsync(int id, UpdateAccountRequest request);
    Task<bool> DeleteAsync(int id);
}
