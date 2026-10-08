namespace CentralBankRate.Core.Models;

public class CurrentRateChange
{
    public Valute Rate { get; init; }
    public decimal? ChangePercent { get; init; }
}