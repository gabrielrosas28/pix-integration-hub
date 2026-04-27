namespace ApiService.Domain.Entities;

public class Conta
{
    public int ClientId { get; set; } // PK
    public string Nome { get; set; }
    public string NumeroConta { get; set; }

    public Secret Secret { get; set; }
}