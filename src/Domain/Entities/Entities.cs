// These entities are used by the ApplicationDbContext (EF Core persistence layer).
// They map directly to the database tables via the existing migrations.

namespace Domain.Entities;

public class Account
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? SecretId { get; set; }
    public Secret? Secret { get; set; }
    public string Document { get; set; } = string.Empty;
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Agency { get; set; } = string.Empty;
}

public class Secret
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
    public int? AccountId { get; set; }
}

public class PixKey
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Account? Account { get; set; }
}

public class Charge
{
    public int Id { get; set; }
    public string TxId { get; set; } = string.Empty;
    public string InvoiceId { get; set; } = string.Empty;
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

public class Audit
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string TxId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Raw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ActionPayload { get; set; } = string.Empty;
    public string? ConfirmationPayload { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ConfirmationTime { get; set; }
}
