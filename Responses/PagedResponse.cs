namespace Sulozeqi_BackEnd.Responses;

public class PagedResponse<T> : BaseResponse<List<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResponse()
    {
        Success = true;
        Message = "Success";
        Data = [];
    }
}
