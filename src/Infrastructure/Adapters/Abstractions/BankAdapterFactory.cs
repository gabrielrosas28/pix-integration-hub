using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Adapters.Abstractions;

public sealed class BankAdapterFactory : IBankAdapterFactory
{
    private readonly Dictionary<string, IBankPixAdapter> _adapters;
    private readonly ILogger<BankAdapterFactory> _logger;

    public BankAdapterFactory(
        IEnumerable<IBankPixAdapter> adapters,
        ILogger<BankAdapterFactory> logger)
    {
        _adapters = adapters.ToDictionary(
            a => a.BankId,
            StringComparer.OrdinalIgnoreCase);

        _logger = logger;

        _logger.LogInformation(
            "BankAdapterFactory initialized with {Count} adapters: {Banks}",
            _adapters.Count,
            string.Join(", ", _adapters.Keys));
    }

    public IBankPixAdapter Get(string bankId)
    {
        if (!_adapters.TryGetValue(bankId, out var adapter))
        {
            _logger.LogError("Adapter not found for bank: {BankId}", bankId);
            throw new KeyNotFoundException(
                $"Adapter not found for bank: {bankId}. " +
                $"Available banks: {string.Join(", ", _adapters.Keys)}");
        }

        return adapter;
    }

    public IReadOnlyCollection<string> GetAvailableBanks()
        => _adapters.Keys.ToList().AsReadOnly();

    public bool IsSupported(string bankId)
        => _adapters.ContainsKey(bankId);
}
