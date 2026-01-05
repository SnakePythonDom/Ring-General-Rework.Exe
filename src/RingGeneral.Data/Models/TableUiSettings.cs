namespace RingGeneral.Data.Models;

public enum TableSortDirection
{
    Ascending,
    Descending
}

public sealed record TableSortSetting(string ColumnId, TableSortDirection Direction);

public sealed record TableUiSettings(
    string? Recherche,
    string? FiltreType,
    string? FiltreStatut,
    IReadOnlyDictionary<string, bool> ColonnesVisibles,
    IReadOnlyList<string> ColonnesOrdre,
    IReadOnlyList<TableSortSetting> Tris);
