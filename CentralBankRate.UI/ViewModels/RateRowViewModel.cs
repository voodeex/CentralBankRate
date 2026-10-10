using System;
using CentralBankRate.Core.Formatting;
using CentralBankRate.Core.Models;

namespace CentralBankRate.UI.ViewModels;

public sealed class RateRowViewModel
{
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

    public string UnitRateText => RateFormat.UnitRate(UnitRate);

    public bool HasNominal => Nominal != 1;

    public string NominalText => RateFormat.NominalRate(Value, Nominal);

    public string ChangeText => RateFormat.Change(ChangePercent);

    public bool IsUp => ChangePercent > 0;
    public bool IsDown => ChangePercent < 0;
    public bool IsFlat => !IsUp && !IsDown;

    public bool Matches(string query) =>
        CharCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        Name.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
