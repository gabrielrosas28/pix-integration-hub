using System.Collections.Generic;

namespace BankingHub.Application.Interfaces;

/// <summary>
/// Contrato para a fábrica de adapters bancários.
/// Fornece métodos para obter um adapter por identificador de banco
/// e para consultar bancos disponíveis.
/// </summary>
public interface IBankAdapterFactory
{
    IBankPixAdapter Get(string bankId);

    IReadOnlyCollection<string> GetAvailableBanks();

    bool IsSupported(string bankId);
}