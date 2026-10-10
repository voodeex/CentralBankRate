using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Currency;

public interface ICurrencyReportService
{
    Task<Result<ReportData>> LoadReport(DateOnly date);
    
    Task<Result<bool>> SendReport(ReportData data, string email);
    
    
}