using System.Runtime.CompilerServices;
using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Report;

public interface IReportService
{
    byte[] GenerateReport(ReportData data);
}