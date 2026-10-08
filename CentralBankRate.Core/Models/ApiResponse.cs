namespace CentralBankRate.Core.Models;

// Представляет ответ от запроса к API
public class ApiResponse<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    public static ApiResponse<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Value = value
        };
    }
    
    public static ApiResponse<T> Failure(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Описание ошибки не должно быть пустым.",nameof(error));
        }
        
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Error =  error
        };
    }
}