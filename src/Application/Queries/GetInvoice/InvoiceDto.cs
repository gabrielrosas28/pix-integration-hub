namespace BankingHub.Application.Queries.GetInvoice;


public sealed record InvoiceDto(
    string InvoiceId,
    string TxId,
    string ChargeType,
    decimal? Amount,
    string Status,
    string Emv,
    string PixLink,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
