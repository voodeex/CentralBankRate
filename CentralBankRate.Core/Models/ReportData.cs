namespace CentralBankRate.Core.Models;

public class ReportData
{
    public DateOnly ActualDate { get; private set; }
    public DateOnly PreviousDate { get; private set; }
    
    public IReadOnlyList<CurrentRateChange> Rates { get; private set; }
    
    public IReadOnlyList<CurrentRateChange> TopGainers { get; private set; }
    public IReadOnlyList<CurrentRateChange> TopLosers { get; private set; }
    
    
}