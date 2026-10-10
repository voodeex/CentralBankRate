using System.Text.Json;
using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Configurations;

public class ConfigLoad
{
    public static Result<SmtpSettings> Load()
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "config.json");

            string json = File.ReadAllText(path);

            var settings = JsonSerializer.Deserialize<SmtpSettings>(json);

            return settings is null ? Result<SmtpSettings>.Failure("Конфиг пуст.")
                : Result<SmtpSettings>.Success(settings);
        }
        catch (Exception ex)
        {
            return Result<SmtpSettings>.Failure(
                "Не удалось прочитать config.json. Проверьте наличие и содержимое файла.");
        }
    }

}