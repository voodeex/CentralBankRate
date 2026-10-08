namespace CentralBankRate.Core.Models;

// Представляет ответ от запроса к API
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    public static Result<T> Success(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        
        return new Result<T>
        {
            IsSuccess = true,
            Value = value
        };
    }
    
    public static Result<T> Failure(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Описание ошибки не должно быть пустым.",nameof(error));
        }
        
        return new Result<T>
        {
            IsSuccess = false,
            Error =  error
        };
    }
}