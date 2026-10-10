using System.Globalization;

namespace CentralBankRate.Core.Formatting;


public static class RateFormat
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");
    public static string UnitRate(decimal unitRate) => unitRate.ToString("#,##0.0000####", Ru);
    
    public static string NominalRate(decimal value, int nominal) => $"{value.ToString("#,##0.0000", Ru)} за {nominal.ToString("#,##0", Ru)} ед.";

    public static string Change(decimal? changePercent) => changePercent is { } change ? $"{change.ToString("+0.00;−0.00;0.00", Ru)}\u00A0%" : "-";
}
