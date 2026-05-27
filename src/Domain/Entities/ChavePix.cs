namespace ApiService.Domain.Entities;

public class ChavePix
{
    public int Id { get; set; }
    
    public int ContaId { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; 
    public Conta? Conta { get; set; }
}