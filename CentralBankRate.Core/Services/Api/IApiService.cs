using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Api;

public interface IApiService
{ 
    Task<Result<string>> GetValutes(DateOnly date);
}