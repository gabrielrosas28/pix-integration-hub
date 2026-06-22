// Application/Interfaces/IApplicationDbContext.cs
using ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

/// <summary>
/// Abstração da persistência exposta à camada Application.
/// Permite que os handlers (CQRS) acessem o banco sem depender do tipo
/// concreto ApplicationDbContext (que vive na Infrastructure), evitando
/// referência circular Application -> Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Charge> Cobrancas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
