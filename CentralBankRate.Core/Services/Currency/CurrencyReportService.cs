using CentralBankRate.Core.Models;
using CentralBankRate.Core.Services.Api;
using CentralBankRate.Core.Services.Parser;
using CentralBankRate.Core.Services.Report;
using CentralBankRate.Core.Services.Sender;

namespace CentralBankRate.Core.Services.Currency;

public class CurrencyReportService : ICurrencyReportService
{
    private readonly IApiService _apiService;
    private readonly IParser _parser;
    private readonly IReportService _reportService;
    private readonly ISenderService _senderService;

    public CurrencyReportService(IApiService apiService, IParser parser, IReportService reportService,  ISenderService senderService)
    {
        _apiService = apiService;
        _parser = parser;
        _reportService = reportService;
        _senderService = senderService;
    }
    
    public async Task<Result<ReportData>> LoadReport(DateOnly date)
    {
       var result =  await _apiService.GetValutes(date);
       if (!result.IsSuccess || result.Value is null)
       {
           return Result<ReportData>.Failure("Ошибка загрузки репорта");
       }
       var currentData =  _parser.Parse(result.Value);
       if (!currentData.IsSuccess || currentData.Value is null)
       {
           return Result<ReportData>.Failure("Ошибка парсинга репорта");
       }
       
       result =  await _apiService.GetValutes(date.AddDays(-1));
       if (!result.IsSuccess || result.Value is null)
       {
           return Result<ReportData>.Failure("Ошибка загрузки репорта");
       }
       var previousData =  _parser.Parse(result.Value);
       if (!previousData.IsSuccess || previousData.Value is null)
       {
           return Result<ReportData>.Failure("Ошибка парсинга репорта");
       }
       
       ReportData report = CalculateReportData(currentData.Value, previousData.Value);
       
       return Result<ReportData>.Success(report);
    }

    private static ReportData CalculateReportData(ValCurs current, ValCurs previous)
    {
        List<CurrentRateChange> rateChanges = new List<CurrentRateChange>();
            
        var previousById = previous.Rates.ToDictionary(rate => rate.Id);
        foreach (var rate in current.Rates)
        {
            if (previousById.TryGetValue(rate.Id, out var previousRate))
            {
                rateChanges.Add(new CurrentRateChange
                {
                    Rate = rate,
                    ChangePercent = (rate.UnitRate / previousRate.UnitRate) * 100m - 100m
                });
            }
            else
            {
                rateChanges.Add(new CurrentRateChange
                {
                    Rate = rate,
                    ChangePercent = null
                });
            }
        }

        ReportData data = new ReportData
        {
            ActualDate = current.Date,
            PreviousDate = previous.Date,
            Rates = rateChanges,
            TopGainers = rateChanges.OrderByDescending(rate => rate.ChangePercent).Take(3).ToList(),
            TopLosers = rateChanges.OrderBy(rate => rate.ChangePercent).Take(3).ToList(),
            AverrageRateChange = rateChanges.Average(rate => rate.ChangePercent).Value
            
            
        };

        return data;
    }

    public async Task<Result<bool>>  SendReport(ReportData data, string email)
    {
        var document = _reportService.GenerateReport(data);
        var result =await _senderService.SendAsync(email,$"Отчет по курсу валют за {data.ActualDate}","" , document, $"Отчет по курсу валют за {data.ActualDate}.pdf" );
        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.Error);
        }
        return Result<bool>.Success(true);
    }
}