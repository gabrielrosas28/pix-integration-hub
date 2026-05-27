namespace Application.DTOs;

public class CreateChavePixRequest
{
    public int ContaId { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}