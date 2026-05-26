namespace BankingHub.Application.Interfaces;

/// <summary>
/// Resolves the proper <see cref="IBankPixAdapter"/> for a given BankId.
/// Implemented in Infrastructure by aggregating all registered adapters.
/// </summary>
public interface IBankAdapterFactory
{
    IBankPixAdapter Get(string bankId);

    IReadOnlyCollection<string> GetAvailableBanks();

    bool IsSupported(string bankId);
}
