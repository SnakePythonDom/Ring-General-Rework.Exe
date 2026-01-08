using RingGeneral.Core.Models;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository pour gérer les styles de catch (CatchStyles)
/// </summary>
public interface ICatchStyleRepository
{
    /// <summary>
    /// Charge tous les styles de catch actifs
    /// </summary>
    Task<IReadOnlyList<CatchStyle>> GetAllActiveStylesAsync();

    /// <summary>
    /// Charge un style de catch par son ID
    /// </summary>
    Task<CatchStyle?> GetStyleByIdAsync(string styleId);

    /// <summary>
    /// Retourne les styles compatibles avec les préférences d'un Owner
    /// </summary>
    Task<IReadOnlyList<CatchStyle>> GetCompatibleStylesAsync(string preferredProductType);

    /// <summary>
    /// Calcule le bonus/malus de rating basé sur l'adéquation entre style et match
    /// </summary>
    double CalculateStyleMatchBonus(CatchStyle style, int matchWorkrate, int matchEntertainment, int matchHardcore);
}
