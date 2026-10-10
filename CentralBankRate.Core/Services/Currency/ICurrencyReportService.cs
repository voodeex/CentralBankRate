using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Currency;

public interface ICurrencyReportService
{
    Task<Result<ReportData>> LoadReport(DateOnly date, CancellationToken cancellationToken = default);
    
    Task<Result<bool>> SendReport(ReportData data, string email, CancellationToken cancellationToken = default);
    
    
}