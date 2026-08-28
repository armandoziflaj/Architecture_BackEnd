namespace Sulozeqi_BackEnd.Responses;

public interface IApiResponse
{
    bool Success { get; set; }
    string Message { get; set; }
}

public class BaseResponse<T> : IApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    
    public static BaseResponse<T> Ok(T data, string message = "Success") => 
        new() { Success = true, Message = message, Data = data };
    
    public static BaseResponse<T> Fail(string message) => 
        new() { Success = false, Message = message, Data = default };
}