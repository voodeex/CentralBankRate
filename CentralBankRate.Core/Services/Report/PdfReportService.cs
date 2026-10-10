using CentralBankRate.Core.Formatting;
using CentralBankRate.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CentralBankRate.Core.Services.Report;

public class PdfReportService : IReportService
{
    public byte[] GenerateReport(ReportData data)
    {
        QuestPDF.Settings.License = LicenseType.Evaluation;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("Отчёт по курсам валют")
                            .SemiBold()
                            .FontSize(24)
                            .FontColor(Colors.Black);

                        column.Item()
                            .PaddingTop(5)
                            .Text($"Курсы на {data.ActualDate:dd.MM.yyyy} · изменение к {data.PreviousDate:dd.MM.yyyy}")
                            .FontSize(12);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        column.Item()
                            .Text("Курсы валют")
                            .SemiBold()
                            .FontSize(16);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(125);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(2).Padding(8)
                                    .Text("Код").SemiBold();

                                header.Cell().BorderBottom(2).Padding(8)
                                    .Text("Название").SemiBold();

                                header.Cell().BorderBottom(2).Padding(8)
                                    .AlignRight()
                                    .Text("Курс за 1 ед., руб.").SemiBold();

                                header.Cell().BorderBottom(2).Padding(8)
                                    .AlignRight()
                                    .Text("Изменение").SemiBold();
                            });

                            foreach (var item in data.Rates)
                            {
                                table.Cell().Padding(8)
                                    .Text(item.Rate.CharCode);

                                table.Cell().Padding(8)
                                    .Text(item.Rate.Name);
                                
                                table.Cell().Padding(8)
                                    .AlignRight()
                                    .Column(rate =>
                                    {
                                        rate.Item().AlignRight()
                                            .Text(RateFormat.UnitRate(item.Rate.UnitRate));

                                        if (item.Rate.Nominal != 1)
                                        {
                                            rate.Item().AlignRight()
                                                .Text(RateFormat.NominalRate(item.Rate.Value, item.Rate.Nominal))
                                                .FontSize(8)
                                                .FontColor(Colors.Grey.Darken1);
                                        }
                                    });

                                table.Cell().Padding(8)
                                    .AlignRight()
                                    .Text(RateFormat.Change(item.ChangePercent));
                            }
                        });

                        column.Item()
                            .PaddingTop(20)
                            .Text("Итоги дня")
                            .SemiBold()
                            .FontSize(18);

                        column.Item()
                            .Text("Топ-3 роста")
                            .SemiBold()
                            .FontSize(14);

                        foreach (var item in data.TopGainers)
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text($"{item.Rate.CharCode} — {item.Rate.Name}");

                                    row.ConstantItem(100)
                                        .AlignRight()
                                        .Text(RateFormat.Change(item.ChangePercent));
                                });
                        }

                        column.Item()
                            .PaddingTop(10)
                            .Text("Топ-3 падения")
                            .SemiBold()
                            .FontSize(14);

                        foreach (var item in data.TopLosers)
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text($"{item.Rate.CharCode} — {item.Rate.Name}");

                                    row.ConstantItem(100)
                                        .AlignRight()
                                        .Text(RateFormat.Change(item.ChangePercent));
                                });
                        }

                        column.Item()
                            .PaddingTop(15)
                            .BorderTop(1)
                            .PaddingVertical(10)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("Среднее изменение по всем валютам")
                                    .SemiBold();

                                row.ConstantItem(100)
                                    .AlignRight()
                                    .Text(RateFormat.Change(data.AverrageRateChange))
                                    .SemiBold();
                            });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Страница ");
                        text.CurrentPageNumber();
                        text.Span(" из ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();

        return doc;
    }
}