using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Api;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<ApiResponse<string>> GetValutes(DateOnly date)
    {
        var response = 
            await _httpClient.GetAsync($"https://cbr.ru/scripts/XML_daily.asp?date_req={date}");

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<string>.Failure(response.ReasonPhrase);
        }

        string xml = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(xml))
        {
            return ApiResponse<string>.Failure("Пустой ответ");
        }
        return ApiResponse<string>.Success(xml);
    }
}