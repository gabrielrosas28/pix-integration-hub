namespace BankingHub.Application.Exceptions;

/// <summary>
/// Raised when an integration with an external system (bank, queue, etc.) fails
/// at the application boundary. Infrastructure may raise more specific subtypes.
/// </summary>
public class IntegrationException : Exception
{
    public IntegrationException(string message) : base(message) { }
    public IntegrationException(string message, Exception inner) : base(message, inner) { }
}
