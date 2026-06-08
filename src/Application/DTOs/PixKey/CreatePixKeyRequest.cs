namespace Application.DTOs;

public class CreatePixKeyRequest
{
    public int AccountId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
