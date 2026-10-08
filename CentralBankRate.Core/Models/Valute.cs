namespace CentralBankRate.Core.Models;

public class Valute
{
    public string Id { get; private set; }
    public string NumCode { get; private set; }
    public string CharCode { get; private set; }
    public int Nominal { get; private set; }
    public string Name { get; private set; }
    public decimal Value { get; private set; }
    public decimal UnitRate { get; private set; }
    
}                                                                       