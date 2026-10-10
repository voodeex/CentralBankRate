namespace CentralBankRate.Core.Models;

public class ReportData
{
    public DateOnly ActualDate { get; init; }
    public DateOnly PreviousDate { get; init; }
    
    public IReadOnlyList<CurrentRateChange> Rates { get; init; }
    
    public IReadOnlyList<CurrentRateChange> TopGainers { get; init; }
    public IReadOnlyList<CurrentRateChange> TopLosers { get; init; }
    
    public decimal AverrageRateChange { get; init; }
}