namespace BankingHub.Domain.Aggregates.Invoice;

public enum InvoiceStatus
{
    Open = 0,
    Paid = 1,
    Canceled = 2,
    Overdue = 3
}
