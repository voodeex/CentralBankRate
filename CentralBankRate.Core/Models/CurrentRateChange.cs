namespace CentralBankRate.Core.Models;

public class CurrentRateChange
{
    public Valute Rate { get; private set; }
    public decimal? ChangePercent { get; private set; }
}