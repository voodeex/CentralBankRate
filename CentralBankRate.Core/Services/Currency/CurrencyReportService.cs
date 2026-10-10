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
    
    public async Task<Result<ReportData>> LoadReport(DateOnly date, CancellationToken cancellationToken = default)
    {
       var currentData = await LoadRates(date, cancellationToken);
       if (!currentData.IsSuccess || currentData.Value is null)
       {
           return Result<ReportData>.Failure(currentData.Error ?? "Не удалось загрузить курсы");
       }

       // На выходные и праздники ЦБ курс не устанавливает и отдаёт последний действующий.
       // Поэтому предыдущий день отсчитываем от фактической даты курса, а не от запрошенной:
       // иначе в воскресенье и понедельник суббота сравнивалась бы сама с собой
       var previousData = await LoadRates(currentData.Value.Date.AddDays(-1), cancellationToken);
       if (!previousData.IsSuccess || previousData.Value is null)
       {
           return Result<ReportData>.Failure(previousData.Error ?? "Не удалось загрузить курсы");
       }
       
       ReportData report = CalculateReportData(currentData.Value, previousData.Value);
       
       return Result<ReportData>.Success(report);
    }

    private async Task<Result<ValCurs>> LoadRates(DateOnly date, CancellationToken cancellationToken)
    {
        var response = await _apiService.GetValutes(date, cancellationToken);
        if (!response.IsSuccess || response.Value is null)
        {
            return Result<ValCurs>.Failure($"Не удалось загрузить курсы на {date:dd.MM.yyyy}. {response.Error}");
        }

        var parsed = _parser.Parse(response.Value);
        if (!parsed.IsSuccess || parsed.Value is null)
        {
            return Result<ValCurs>.Failure($"Не удалось разобрать курсы на {date:dd.MM.yyyy}. {parsed.Error}");
        }

        return parsed;
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

    public async Task<Result<bool>> SendReport(ReportData data, string email, CancellationToken cancellationToken = default)
    {
        byte[] document;
        try
        {
            document = _reportService.GenerateReport(data);
        }
        catch (Exception exception)
        {
            return Result<bool>.Failure($"Не удалось сформировать PDF-отчёт: {exception.Message}");
        }

        var result = await _senderService.SendAsync(email, $"Отчет по курсу валют за {data.ActualDate}", "", document,
            $"Отчет по курсу валют за {data.ActualDate}.pdf", cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.Error ?? "Не удалось отправить отчёт");
        }
        return Result<bool>.Success(true);
    }
}