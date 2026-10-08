using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Api;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Result<string>> GetValutes(DateOnly date)
    {
        var response = 
            await _httpClient.GetAsync($"https://cbr.ru/scripts/XML_daily.asp?date_req={date}");

        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Failure(response.ReasonPhrase);
        }

        string xml = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(xml))
        {
            return Result<string>.Failure("Пустой ответ");
        }
        return Result<string>.Success(xml);
    }
}