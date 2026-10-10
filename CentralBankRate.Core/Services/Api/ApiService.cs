using CentralBankRate.Core.Models;
using System.Text;

namespace CentralBankRate.Core.Services.Api;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<string>> GetValutes(DateOnly date, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response =
                await _httpClient.GetAsync($"https://cbr.ru/scripts/XML_daily.asp?date_req={date}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Failure($"Сайт ЦБ РФ ответил ошибкой HTTP {(int)response.StatusCode} {response.ReasonPhrase}".TrimEnd());
            }

            // Регистрация кодировщика для поддержки windows-1251
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string xml = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(xml))
            {
                return Result<string>.Failure("Сайт ЦБ РФ вернул пустой ответ");
            }
            return Result<string>.Success(xml);
        }
        catch (HttpRequestException exception)
        {
            return Result<string>.Failure($"Нет связи с сайтом ЦБ РФ: {exception.Message}");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {

            return Result<string>.Failure("Сайт ЦБ РФ не ответил вовремя");
        }
    }
}
