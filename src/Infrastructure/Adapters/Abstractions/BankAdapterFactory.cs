// Path: BankingHub.Infrastructure/BankAdapters/BankAdapterFactory.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using BankingHub.Application.Interfaces;

namespace BankingHub.Infrastructure.BankAdapters;

 /// <summary>
 /// Factory para obter o adapter correto baseado no BankId.
 /// Registra todos os adapters via DI e os disponibiliza por chave.
 /// </summary>
public sealed class BankAdapterFactory : IBankAdapterFactory
{
    private readonly Dictionary<string, IBankPixAdapter> _adapters;
    private readonly ILogger<BankAdapterFactory> _logger;

    public BankAdapterFactory(IEnumerable<IBankPixAdapter> adapters, ILogger<BankAdapterFactory> logger)
    {
        // Agrupa os adapters pelo identificador do banco transformado em caixa alta
        _adapters = adapters.ToDictionary(a => a.BankId, StringComparer.OrdinalIgnoreCase);

        _logger = logger;

        _logger.LogInformation("BankAdapterFactory inicializada com {Count} adapters: {Banks}", 
            _adapters.Count, string.Join(", ", _adapters.Keys));
    }

    public IBankPixAdapter Get(string bankId)
    {
        if (!_adapters.TryGetValue(bankId.ToUpperInvariant(), out var adapter))
        {
            _logger.LogError("Adapter não encontrado para banco: {BankId}", bankId);
            throw new KeyNotFoundException(
                $"Adapter não encontrado para o banco: {bankId}. " +
                $"Bancos disponíveis: {string.Join(", ", _adapters.Keys)}");
        }

        return adapter;
    }

    public IReadOnlyCollection<string> GetAvailableBanks() => _adapters.Keys.ToList().AsReadOnly();

    public bool IsSupported(string bankId) => _adapters.ContainsKey(bankId);
}