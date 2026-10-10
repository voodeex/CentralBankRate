using System;
using System.Globalization;
using CentralBankRate.Core.Models;

namespace CentralBankRate.UI.ViewModels;

public sealed class RateRowViewModel
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public RateRowViewModel(string charCode, string name, int nominal, decimal value, decimal unitRate, decimal? changePercent)
    {
        CharCode = charCode;
        Name = name;
        Nominal = nominal;
        Value = value;
        UnitRate = unitRate;
        ChangePercent = changePercent;
    }

    public static RateRowViewModel From(CurrentRateChange change) =>
        new(change.Rate.CharCode, change.Rate.Name, change.Rate.Nominal, change.Rate.Value, change.Rate.UnitRate,
            change.ChangePercent);

    public string CharCode { get; }
    public string Name { get; }
    public int Nominal { get; }
    public decimal Value { get; }
    public decimal UnitRate { get; }
    public decimal? ChangePercent { get; }

    public string UnitRateText => UnitRate.ToString("#,##0.0000####", Ru);

    public bool HasNominal => Nominal != 1;

    public string NominalText => $"{Value.ToString("#,##0.0000", Ru)} за {Nominal.ToString("#,##0", Ru)} ед.";

    public string ChangeText => ChangePercent is { } change
        ? $"{change.ToString("+0.00;−0.00;0.00", Ru)} %"
        : "—";

    public bool IsUp => ChangePercent > 0;
    public bool IsDown => ChangePercent < 0;
    public bool IsFlat => !IsUp && !IsDown;

    public bool Matches(string query) =>
        CharCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        Name.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
