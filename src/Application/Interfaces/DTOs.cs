namespace Application.Interfaces;

// ---- Account DTOs ----

public class CreateAccountRequest
{
    public int SecretId { get; set; }
    public string Document { get; set; } = string.Empty;
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Agency { get; set; } = string.Empty;
}

public class UpdateAccountRequest
{
    public int SecretId { get; set; }
    public string Document { get; set; } = string.Empty;
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Agency { get; set; } = string.Empty;
}

// ---- Secret DTOs ----

public class CreateSecretRequest
{
    public int ClientId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
    public int? AccountId { get; set; }
}

public class UpdateSecretRequest
{
    public int ClientId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
    public int? AccountId { get; set; }
}

// ---- PixKey DTOs ----

public class CreatePixKeyRequest
{
    public int AccountId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class UpdatePixKeyRequest
{
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

// ---- Charge DTOs ----

public class CreateCobRequest
{
    public string InvoiceId { get; set; } = string.Empty;
    public string ChargeType { get; set; } = "Cob";
    public decimal? Amount { get; set; }
    public string PixKey { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int? ExpiresInSeconds { get; set; }
    public string PayerMessage { get; set; } = string.Empty;
}

public class CreateCobVRequest
{
    public string InvoiceId { get; set; } = string.Empty;
    public string ChargeType { get; set; } = "CobV";
    public decimal? Amount { get; set; }
    public string PixKey { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int? ExpiresInSeconds { get; set; }
    public string PayerMessage { get; set; } = string.Empty;
}

public class ChargeDto
{
    public string TxId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Emv { get; set; } = string.Empty;
    public string PixLink { get; set; } = string.Empty;
}
