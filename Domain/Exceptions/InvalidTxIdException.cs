// Domain/Exceptions/InvalidTxIdException.cs
namespace Domain.Exceptions;

public class InvalidTxIdException : DomainException
{
    public InvalidTxIdException(string message) : base(message)
    {
    }
}