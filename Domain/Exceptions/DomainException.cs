// BankingHub.Domain/Exceptions/DomainException.cs
namespace BankingHub.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}