using System;
using System.ComponentModel.DataAnnotations;

namespace RingGeneral.Core.Models.Booker;

/// <summary>
/// Représente un booker avec préférences créatives et capacité d'auto-booking.
/// Influence le style de booking et peut prendre des décisions autonomes si activé.
/// </summary>
public sealed record Booker
{
    /// <summary>
    /// Identifiant unique du booker
    /// </summary>
    [Required]
    public required string BookerId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie actuelle
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Nom du booker
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>
    /// Score de créativité (0-100)
    /// - 0-30: Préfère formules éprouvées, peu créatif
    /// - 31-70: Mix de créativité et tradition
    /// - 71-100: Très créatif, aime innovations et surprises
    /// </summary>
    [Range(0, 100)]
    public required int CreativityScore { get; init; }

    /// <summary>
    /// Score de logique (0-100)
    /// - 0-30: Décisions émotionnelles, peu cohérentes
    /// - 31-70: Équilibre logique/émotion
    /// - 71-100: Très logique, cohérence long terme
    /// </summary>
    [Range(0, 100)]
    public required int LogicScore { get; init; }

    /// <summary>
    /// Résistance au biais (0-100)
    /// - 0-30: Facilement influencé par relations personnelles
    /// - 31-70: Résistance modérée
    /// - 71-100: Décisions purement méritocratiques
    /// </summary>
    [Range(0, 100)]
    public required int BiasResistance { get; init; }

    /// <summary>
    /// Style de booking préféré: "Long-Term", "Short-Term", "Flexible"
    /// - Long-Term: Storylines de 6+ mois, slow burn
    /// - Short-Term: Matches one-shot, changements rapides
    /// - Flexible: Adapte selon situation
    /// </summary>
    [Required]
    public required string PreferredStyle { get; init; }

    /// <summary>
    /// Type de produit préféré: "Hardcore", "Puroresu", "Technical", "Entertainment", "Balanced"
    /// - Hardcore: Stipulations violentes, matches extrêmes
    /// - Puroresu: Matchs longs et techniques, strong style
    /// - Technical: Wrestling technique pur, soumissions
    /// - Entertainment: Segments narratifs, promos, angles
    /// - Balanced: Mix équilibré de tous les styles
    /// </summary>
    [Required]
    public string PreferredProductType { get; init; } = "Balanced";

    /// <summary>
    /// Aime pousser les underdogs (workers peu populaires)
    /// </summary>
    public bool LikesUnderdog { get; init; }

    /// <summary>
    /// Aime utiliser les vétérans établis
    /// </summary>
    public bool LikesVeteran { get; init; }

    /// <summary>
    /// Aime les ascensions rapides (rookie to champion en 6 mois)
    /// </summary>
    public bool LikesFastRise { get; init; }

    /// <summary>
    /// Aime les slow burns (storylines lentes et méthodiques)
    /// </summary>
    public bool LikesSlowBurn { get; init; }

    /// <summary>
    /// Auto-booking activé (le booker AI prend les décisions)
    /// </summary>
    public bool IsAutoBookingEnabled { get; init; }

    /// <summary>
    /// Statut d'emploi: "Active", "Inactive", "Fired"
    /// </summary>
    [Required]
    public required string EmploymentStatus { get; init; }

    /// <summary>
    /// Date d'embauche
    /// </summary>
    public required DateTime HireDate { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que le Booker respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(BookerId))
        {
            errorMessage = "BookerId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
        {
            errorMessage = "Name doit contenir au moins 2 caractères";
            return false;
        }

        var validStyles = new[] { "Long-Term", "Short-Term", "Flexible" };
        if (!validStyles.Contains(PreferredStyle))
        {
            errorMessage = $"PreferredStyle doit être: {string.Join(", ", validStyles)}";
            return false;
        }

        var validProductTypes = new[] { "Hardcore", "Puroresu", "Technical", "Entertainment", "Balanced" };
        if (!validProductTypes.Contains(PreferredProductType))
        {
            errorMessage = $"PreferredProductType doit être: {string.Join(", ", validProductTypes)}";
            return false;
        }

        var validStatuses = new[] { "Active", "Inactive", "Fired" };
        if (!validStatuses.Contains(EmploymentStatus))
        {
            errorMessage = $"EmploymentStatus doit être: {string.Join(", ", validStatuses)}";
            return false;
        }

        if (CreativityScore is < 0 or > 100)
        {
            errorMessage = "CreativityScore doit être entre 0 et 100";
            return false;
        }

        if (LogicScore is < 0 or > 100)
        {
            errorMessage = "LogicScore doit être entre 0 et 100";
            return false;
        }

        if (BiasResistance is < 0 or > 100)
        {
            errorMessage = "BiasResistance doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si le booker est actuellement actif
    /// </summary>
    public bool IsActive() => EmploymentStatus == "Active";

    /// <summary>
    /// Détermine si le booker peut être utilisé pour auto-booking
    /// </summary>
    public bool CanAutoBook() => IsAutoBookingEnabled && IsActive();

    /// <summary>
    /// Calcule le score de cohérence du booker (créativité + logique) / 2
    /// </summary>
    public int GetConsistencyScore() => (CreativityScore + LogicScore) / 2;

    /// <summary>
    /// Détermine si le booker favoriserait un certain type de worker
    /// </summary>
    public bool WouldFavorWorkerType(string workerType)
    {
        return workerType switch
        {
            "Underdog" => LikesUnderdog,
            "Veteran" => LikesVeteran,
            "FastRiser" => LikesFastRise,
            "SlowBurn" => LikesSlowBurn,
            _ => false
        };
    }

    /// <summary>
    /// Retourne le profil créatif du booker sous forme textuelle
    /// </summary>
    public string GetCreativeProfile()
    {
        if (CreativityScore >= 70 && LogicScore >= 70)
            return "Genius Booker"; // Créatif ET logique

        if (CreativityScore >= 70)
            return "Creative Visionary"; // Très créatif

        if (LogicScore >= 70)
            return "Strategic Planner"; // Très logique

        if (CreativityScore <= 30 && LogicScore <= 30)
            return "Chaotic Booker"; // Peu créatif ET peu logique

        return "Balanced Booker"; // Mix
    }
}
