namespace RingGeneral.Data.Repositories;

public sealed record Pagination(int Page, int TaillePage)
{
    public int Offset => Math.Max(Page - 1, 0) * TaillePage;
}
