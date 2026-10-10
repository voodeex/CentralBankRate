using System.Text.Json;
using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Configurations;

public class ConfigLoad
{
    public static Result<SmtpSettings> Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "config.json");
        try
        {
            string json = File.ReadAllText(path);

            var settings = JsonSerializer.Deserialize<SmtpSettings>(json);

            return settings is null ? Result<SmtpSettings>.Failure("Конфиг пуст.")
                : Result<SmtpSettings>.Success(settings);
        }
        catch (FileNotFoundException)
        {
            return Result<SmtpSettings>.Failure(
                $"Не найден файл настроек SMTP: {path}. Создайте его по образцу config.example.json.");
        }
        catch (JsonException exception)
        {
            return Result<SmtpSettings>.Failure($"config.json содержит некорректный JSON: {exception.Message}");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Result<SmtpSettings>.Failure($"Не удалось прочитать config.json: {exception.Message}");
        }
    }

}