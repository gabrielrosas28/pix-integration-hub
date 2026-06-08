namespace ApiService.Domain.Entities;

public class Charge
{
    public int Id { get; set; }
    public string TxId { get; set; } = string.Empty;
    public string InvoiceID { get; set; } = string.Empty;
    public string ChargeType { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string PixKey { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int? ExpiresInSeconds { get; set; }
    public string PayerMessage { get; set; } = string.Empty;
    public string Status { get; set; } = "created";
    public string Emv { get; set; } = string.Empty;
    public string PixLink { get; set; } = string.Empty;
    public string Raw { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
