namespace ApiService.Domain.Entities;

public class Conta
{
    public int ClientId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NumeroConta { get; set; } = string.Empty;

    public Secret? Secret { get; set; }
}