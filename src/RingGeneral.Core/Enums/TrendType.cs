namespace RingGeneral.Core.Enums;

/// <summary>
/// Type de tendance (portée géographique)
/// </summary>
public enum TrendType
{
    /// <summary>
    /// Tendance mondiale - Affecte toutes les régions
    /// </summary>
    Global,

    /// <summary>
    /// Tendance régionale - Affecte une région spécifique (ex: Amérique du Nord, Europe)
    /// </summary>
    Regional,

    /// <summary>
    /// Tendance locale - Affecte un marché local spécifique
    /// </summary>
    Local
}

/// <summary>
/// Catégorie de tendance
/// </summary>
public enum TrendCategory
{
    /// <summary>
    /// Tendance de style de catch (ex: "Lucha Boom", "Strong Style Era")
    /// </summary>
    Style,

    /// <summary>
    /// Tendance de format de show (ex: "Tournament Era", "PPV Focus")
    /// </summary>
    Format,

    /// <summary>
    /// Tendance d'audience (ex: "Family-Friendly Boom", "Hardcore Revival")
    /// </summary>
    Audience
}

/// <summary>
/// Niveau de compatibilité entre une tendance et l'ADN d'un roster
/// </summary>
public enum CompatibilityLevel
{
    /// <summary>
    /// C > 1.2 : Alignement parfait, bonus croissance
    /// </summary>
    Alignment,

    /// <summary>
    /// 0.8 < C < 1.2 : Transition possible mais lente
    /// </summary>
    Hybridation,

    /// <summary>
    /// C < 0.8 : Incompatibilité, boost niche
    /// </summary>
    Refusal
}
