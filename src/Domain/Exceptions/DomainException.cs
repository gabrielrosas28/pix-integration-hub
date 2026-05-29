// BankingHub.Domain/Exceptions/DomainException.cs
namespace Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}