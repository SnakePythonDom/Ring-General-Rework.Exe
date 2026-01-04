namespace RingGeneral.Data.Database;

public sealed record DbValidationResult(bool EstValide, IReadOnlyList<string> Erreurs)
{
    public static DbValidationResult Ok() => new(true, Array.Empty<string>());
}
