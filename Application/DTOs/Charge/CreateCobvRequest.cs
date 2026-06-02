namespace Application.DTOs;


public class CreateCobvRequest
{
    public string InvoiceID       { get; set; } = string.Empty;
    public string ChargeType      { get; set; } = "COBV";
    public decimal? Amount        { get; set; }
    public string PixKey          { get; set; } = string.Empty;
    public DateOnly? DueDate      { get; set; }
    public int? ExpiresInSeconds  { get; set; }
    public string PayerMessage    { get; set; } = string.Empty;
}
