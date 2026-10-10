using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CentralBankRate.Core.Models;
using CentralBankRate.Core.Services.Currency;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CentralBankRate.UI.ViewModels;

public enum NotificationKind
{
    None,
    Success,
    Error
}

public partial class MainViewModel : ViewModelBase
{
    private const string DateFormat = "dd.MM.yyyy";

    private readonly ICurrencyReportService? _reportService;
    private ReportData? _report;
    private IReadOnlyList<RateRowViewModel> _allRates = [];

    public MainViewModel(ICurrencyReportService reportService)
    {
        _reportService = reportService;
    }
    
    public MainViewModel()
    {
    }

    public DateTime MaxDate { get; } = DateTime.Today;

    public ObservableCollection<RateRowViewModel> Rates { get; } = [];
    public ObservableCollection<RateRowViewModel> TopGainers { get; } = [];
    public ObservableCollection<RateRowViewModel> TopLosers { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    private DateTime? _selectedDate = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string _recipientEmail = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStatusOk))]
    [NotifyPropertyChangedFor(nameof(IsStatusError))]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusText = "Выберите дату и загрузите курсы";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReport))]
    [NotifyPropertyChangedFor(nameof(RatesDateText))]
    [NotifyPropertyChangedFor(nameof(IsDateShifted))]
    [NotifyPropertyChangedFor(nameof(DateShiftText))]
    [NotifyPropertyChangedFor(nameof(LeadersPlaceholderText))]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private DateOnly? _ratesDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviousDateText))]
    private DateOnly? _previousDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDateShifted))]
    [NotifyPropertyChangedFor(nameof(DateShiftText))]
    private DateOnly? _requestedDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsErrorNotification))]
    [NotifyPropertyChangedFor(nameof(IsSuccessNotification))]
    [NotifyPropertyChangedFor(nameof(IsStatusOk))]
    [NotifyPropertyChangedFor(nameof(IsStatusError))]
    private NotificationKind _notificationKind;

    [ObservableProperty]
    private string _notificationText = string.Empty;

    public bool HasReport => RatesDate is not null;

    public string RatesDateText => RatesDate?.ToString(DateFormat) ?? "—";
    public string PreviousDateText => PreviousDate?.ToString(DateFormat) ?? "—";

    public bool IsDateShifted => RatesDate is { } actual && RequestedDate is { } requested && actual != requested;

    public string DateShiftText => IsDateShifted
        ? $"На {RequestedDate!.Value.ToString(DateFormat)} ЦБ не устанавливал новый курс — показаны курсы, действующие на {RatesDateText}"
        : string.Empty;

    public string RatesCountText => Rates.Count == _allRates.Count
        ? $"{_allRates.Count}"
        : $"{Rates.Count} из {_allRates.Count}";

    public bool IsNothingFound => HasReport && Rates.Count == 0;

    public bool HasTopGainers => TopGainers.Count > 0;
    public bool HasTopLosers => TopLosers.Count > 0;

    public string LeadersPlaceholderText => HasReport ? "Нет данных за этот день" : "Появится после загрузки данных";

    public bool IsErrorNotification => NotificationKind == NotificationKind.Error;
    public bool IsSuccessNotification => NotificationKind == NotificationKind.Success;

    public bool IsStatusOk => !IsBusy && !IsErrorNotification;
    public bool IsStatusError => !IsBusy && IsErrorNotification;

    private bool CanLoad() => !IsBusy && SelectedDate is not null;

    private bool CanSend() => !IsBusy && HasReport && !string.IsNullOrWhiteSpace(RecipientEmail);

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        if (_reportService is null || SelectedDate is not { } selected)
        {
            return;
        }

        var date = DateOnly.FromDateTime(selected);
        if (selected.Date > MaxDate)
        {
            ShowError("Нельзя выбрать дату позже сегодняшней.");
            return;
        }

        DismissNotification();
        IsBusy = true;
        StatusText = $"Загрузка курсов на {date.ToString(DateFormat)} с сайта ЦБ РФ…";

        try
        {
            var result = await _reportService.LoadReport(date);
            if (!result.IsSuccess || result.Value is null)
            {
                ShowError(result.Error ?? "Не удалось загрузить курсы валют.");
                StatusText = "Ошибка загрузки";
                return;
            }

            ApplyReport(result.Value, date);
            StatusText = $"Загружены курсы на {RatesDateText} (валют: {_allRates.Count})";
        }
        catch (Exception ex)
        {
            ShowError($"Непредвиденная ошибка при загрузке: {ex.Message}");
            StatusText = "Ошибка загрузки";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (_reportService is null || _report is null)
        {
            return;
        }

        var email = RecipientEmail.Trim();

        DismissNotification();
        IsBusy = true;
        StatusText = "Формирование PDF-отчёта и отправка письма…";

        try
        {
            var result = await _reportService.SendReport(_report, email);
            if (!result.IsSuccess)
            {
                ShowError(result.Error ?? "Не удалось отправить отчёт.");
                StatusText = "Ошибка отправки";
                return;
            }

            ShowSuccess($"Отчёт на {RatesDateText} отправлен на {email}");
            StatusText = "Отчёт отправлен";
        }
        catch (Exception ex)
        {
            ShowError($"Непредвиденная ошибка при отправке: {ex.Message}");
            StatusText = "Ошибка отправки";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void DismissNotification()
    {
        NotificationKind = NotificationKind.None;
        NotificationText = string.Empty;
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyReport(ReportData report, DateOnly requestedDate)
    {
        _report = report;
        _allRates = ToRows(report.Rates).ToList();

        Fill(TopGainers, ToRows(report.TopGainers));
        Fill(TopLosers, ToRows(report.TopLosers));
        OnPropertyChanged(nameof(HasTopGainers));
        OnPropertyChanged(nameof(HasTopLosers));

        RequestedDate = requestedDate;
        PreviousDate = report.PreviousDate;
        RatesDate = report.ActualDate;

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        var rows = query.Length == 0 ? _allRates : _allRates.Where(r => r.Matches(query));

        Fill(Rates, rows);
        OnPropertyChanged(nameof(RatesCountText));
        OnPropertyChanged(nameof(IsNothingFound));
    }

    public void ShowError(string message)
    {
        NotificationText = message;
        NotificationKind = NotificationKind.Error;
    }

    private void ShowSuccess(string message)
    {
        NotificationText = message;
        NotificationKind = NotificationKind.Success;
    }
    private static IEnumerable<RateRowViewModel> ToRows(IEnumerable<CurrentRateChange>? source) =>
        source?.Select(RateRowViewModel.From) ?? Enumerable.Empty<RateRowViewModel>();

    private static void Fill(ObservableCollection<RateRowViewModel> target, IEnumerable<RateRowViewModel> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}
