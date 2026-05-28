// Domain/Aggregates/PixCharge/PixChargeStatus.cs
namespace Domain.Aggregates.PixCharge;

public enum PixChargeStatus 
{ 
    Active, 
    Paid, 
    Expired, 
    Canceled, 
    Unknown 
}