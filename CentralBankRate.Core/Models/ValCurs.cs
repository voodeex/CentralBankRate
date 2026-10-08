namespace CentralBankRate.Core.Models;

public class ValCurs
{
    public DateOnly Date { get; init; }
    public string Name { get; init; }
    public IReadOnlyList<Valute> Rates { get; init; }
    
}