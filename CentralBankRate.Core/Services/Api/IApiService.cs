using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Api;

public interface IApiService
{ 
    Task<ApiResponse<string>> GetValutes(DateOnly date);
}