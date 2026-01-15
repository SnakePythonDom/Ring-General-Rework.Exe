namespace RingGeneral.Core.Enums;

/// <summary>
/// Type de niche pour une fédération spécialisée
/// </summary>
public enum NicheType
{
    /// <summary>
    /// Niche Hardcore - Violence extrême, spots dangereux
    /// </summary>
    Hardcore,

    /// <summary>
    /// Niche Technique - Workrate pur, qualité technique
    /// </summary>
    Technical,

    /// <summary>
    /// Niche Lucha Libre - High-flying, tradition mexicaine
    /// </summary>
    Lucha,

    /// <summary>
    /// Niche Strong Style - Puroresu japonais, réalisme
    /// </summary>
    StrongStyle,

    /// <summary>
    /// Niche Entertainment - Spectacle, storylines complexes
    /// </summary>
    Entertainment
}

/// <summary>
/// Taille maximale d'une compagnie
/// </summary>
public enum CompanySize
{
    /// <summary>
    /// Promotion locale (1 ville/région)
    /// </summary>
    Local,

    /// <summary>
    /// Plusieurs régions
    /// </summary>
    Regional,

    /// <summary>
    /// Échelle nationale
    /// </summary>
    National,

    /// <summary>
    /// Multi-pays
    /// </summary>
    International,

    /// <summary>
    /// Mondiale (WWE, AEW level)
    /// </summary>
    Worldwide
}

/// <summary>
/// Objectif assignable à une filiale
/// </summary>
public enum ChildCompanyObjective
{
    /// <summary>
    /// Tester gimmicks risquées
    /// </summary>
    Entertainment,

    /// <summary>
    /// Maintenir présence sur style spécifique
    /// </summary>
    Niche,

    /// <summary>
    /// Booker IA avec liberté totale
    /// </summary>
    Independence,

    /// <summary>
    /// Centre de formation (existant)
    /// </summary>
    Development
}
