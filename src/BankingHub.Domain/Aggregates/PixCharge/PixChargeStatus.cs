namespace BankingHub.Domain.Aggregates.PixCharge;

public enum PixChargeStatus
{
    Active = 0,
    Paid = 1,
    Expired = 2,
    Canceled = 3,
    Unknown = 99
}
