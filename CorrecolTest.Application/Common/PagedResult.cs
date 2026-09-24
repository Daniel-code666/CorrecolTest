namespace CorrecolTest.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
