using System.Globalization;
using System.Xml.Serialization;
using CentralBankRate.Core.Models;
using CentralBankRate.Core.Models.Dtos;

namespace CentralBankRate.Core.Services.Parser;

public class XmlParser : IParser
{
    public Result<ValCurs> Parse(string data)
    {
        var serialiazer = new XmlSerializer(typeof(ValCursXmlDto));

        using var reader = new StringReader(data);
        var dto = (ValCursXmlDto?)serialiazer.Deserialize(reader);
        if (dto.Rates is null)
        {
            return Result<ValCurs>.Failure("Ответ сервиса отсутствует");
        }

        

        if (!DateOnly.TryParseExact(
          dto.DateText,
          "dd.MM.yyyy",
          CultureInfo.InvariantCulture, 
          DateTimeStyles.None,
          out var date))
        {
            return Result<ValCurs>.Failure("Некорректная дата курсов");
        }
        
        if (dto.DateText is null || dto.Rates.Count == 0)
        {
            return Result<ValCurs>.Failure("Список валют пуст");
        }
        
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var rates = new List<Valute>(dto.Rates.Count);


        foreach (var item in dto.Rates)
        {
            if (item is null)
            {
                return Result<ValCurs>.Failure("В списке валют найдена пустая запись");
            }

            if (string.IsNullOrWhiteSpace(item.Id) ||
                string.IsNullOrWhiteSpace(item.CharCode) ||
                string.IsNullOrWhiteSpace(item.NumCode) ||
                string.IsNullOrWhiteSpace(item.Name)
               )
            {
                return Result<ValCurs>.Failure($"У валюты {item.Id ?? "Без ID"} отсутствуют обязательные поля");
            }

            if (item.Nominal <= 0)
            {
                return Result<ValCurs>.Failure($"У валюты {item.Id}, некорректный номинал");
            }

            if (!decimal.TryParse(item.UnitRate,NumberStyles.Float, culture, out var unitrate) ||
                unitrate <= 0
               )
            {
                return Result<ValCurs>.Failure($"У валюты {item.Id} некорректное цена за единицу");
            }

            if (!decimal.TryParse(item.Value,NumberStyles.Float, culture, out var value) ||
                value <= 0
               )
            {
                return Result<ValCurs>.Failure($"У валюты {item.Id} некорректное значение курса");
            }

            rates.Add(new Valute
            {
                Id =  item.Id,
                NumCode =  item.NumCode,
                CharCode =  item.CharCode,
                Nominal =  item.Nominal,
                Name = item.Name,
                Value = value,
                UnitRate = unitrate,
            });
        }

        return Result<ValCurs>.Success(new ValCurs
        {
            Date = date,
            Rates = rates,
            Name = dto.Name
        });


    }
}