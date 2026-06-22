using Domain.Aggregates.Account;

namespace ApiService.Domain.Entities;

public class PixKey
{
    public int Id { get; set; }
    
    public int AccountId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; 
    public Account? Account { get; set; }
}