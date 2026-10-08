namespace CentralBankRate.Core.Models;

public class Valute
{
    public string Id { get; init; }
    public string NumCode { get; init; }
    public string CharCode { get; init; }
    public int Nominal { get; init; }
    public string Name { get; init; }
    public decimal Value { get; init; }
    public decimal UnitRate { get; init; }
    
}                                                                       