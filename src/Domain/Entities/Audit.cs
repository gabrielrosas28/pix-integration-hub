namespace ApiService.Domain.Entities;

public class Auditoria
{
    public int AuditoriaId { get; set; }

    
    public int ContaId { get; set; }
    public DateTime HorarioRegistro { get; set; }
    public string StatusPagamento { get; set; } = string.Empty;
    public int TxId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Raw { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string PayloadAcao { get; set; } = string.Empty;
    public string? PayloadConfirmacao { get; set; } = string.Empty;
    public DateTime? DataPagamento { get; set; }
    public DateTime? HorarioConfirmacao { get; set; }
}