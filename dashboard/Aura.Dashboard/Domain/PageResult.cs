namespace Aura.Dashboard.Domain;

public sealed record PageResult<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize)
{ public int PageCount => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize)); }
