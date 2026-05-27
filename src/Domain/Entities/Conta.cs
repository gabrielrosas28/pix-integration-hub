namespace ApiService.Domain.Entities;

public class Conta
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int? SecretId { get; set; }
    public Secret? Secret { get; set; }

    public string Documento { get; set; } = string.Empty;

    public int BankId { get; set; }

    public string NumeroConta { get; set; } = string.Empty;

    public string Agencia { get; set; } = string.Empty;

}