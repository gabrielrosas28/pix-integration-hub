namespace BankingHub.Domain.Exceptions;

public sealed class InvalidTxIdException : DomainException
{
    public InvalidTxIdException(string message) : base(message) { }
}
