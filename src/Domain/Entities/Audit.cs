namespace ApiService.Domain.Entities;

public class Audit
{
    public int AuditoriaId { get; set; }

    
    public int AccountId { get; set; }
    public DateTime RegistrationTime { get; set; }
    public string StatusPayment { get; set; } = string.Empty;
    public int TxId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Raw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PayloadAction { get; set; } = string.Empty;
    public string? PayloadConfirmation { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public DateTime? ConfirmationTime { get; set; }
}