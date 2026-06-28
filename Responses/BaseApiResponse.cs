namespace Sulozeqi_BackEnd.Responses;

public class BaseApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    
    public static BaseApiResponse<T> Ok(T data, string message = "Success") => 
        new() { Success = true, Message = message, Data = data };
    
    public static BaseApiResponse<T> Fail(string message) => 
        new() { Success = false, Message = message, Data = default };
}