namespace CentralBankRate.Core.Models;

public class ValCurs
{
    public DateOnly Date { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyList<Valute> Rates { get; private set; }
    
}