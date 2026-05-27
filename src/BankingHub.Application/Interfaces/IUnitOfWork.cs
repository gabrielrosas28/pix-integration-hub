namespace BankingHub.Application.Interfaces;

/// <summary>
/// Coordinates transactional commit across repositories.
/// The infrastructure layer implements this against EF Core (DbContext.SaveChangesAsync).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
