// Domain/Aggregates/Invoice/InvoiceStatus.cs
namespace Domain.Aggregates.Invoice;

public enum InvoiceStatus
{
    Open,
    Paid,
    Canceled,
    Overdue
}