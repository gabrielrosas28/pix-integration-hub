// BankingHub.Domain/Aggregates/Invoice/InvoiceStatus.cs
namespace BankingHub.Domain.Aggregates.Invoice;

public enum InvoiceStatus
{
    Open,
    Paid,
    Canceled,
    Overdue
}