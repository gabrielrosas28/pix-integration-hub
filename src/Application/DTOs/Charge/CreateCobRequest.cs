using System;

namespace Application.DTOs;

public class CreateCobRequest
{
    public string InvoiceID { get; set; } = string.Empty;
    public string ChargeType { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string PixKey { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int? ExpiresInSeconds { get; set; }
    public string PayerMessage { get; set; } = string.Empty;
}
