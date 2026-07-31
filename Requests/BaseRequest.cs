namespace Sulozeqi_BackEnd.Requests;

public class BaseRequest
{
    public Guid CorrelationId { get; set; } = Guid.Empty;
}