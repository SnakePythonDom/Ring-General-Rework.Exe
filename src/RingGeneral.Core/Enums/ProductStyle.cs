namespace RingGeneral.Core.Enums;

/// <summary>
/// Styles de produit pour les créatifs et les ères.
/// Influence les préférences et les biais créatifs.
/// </summary>
public enum ProductStyle
{
    /// <summary>
    /// Technical - Workrate technique élevé
    /// </summary>
    Technical,

    /// <summary>
    /// Brawler - Bagarre, frappe, brutalité
    /// </summary>
    Brawler,

    /// <summary>
    /// HighFlyer - Aérien, high-flying
    /// </summary>
    HighFlyer,

    /// <summary>
    /// PowerHouse - Big men, force, domination
    /// </summary>
    PowerHouse,

    /// <summary>
    /// Storyteller - Focus narratif, promos
    /// </summary>
    Storyteller,

    /// <summary>
    /// Hardcore - Violence, armes, sang
    /// </summary>
    Hardcore,

    /// <summary>
    /// Comedy - Humour, gimmicks comiques
    /// </summary>
    Comedy,

    /// <summary>
    /// Realistic - Réalisme, MMA-style
    /// </summary>
    Realistic,

    /// <summary>
    /// Theatrical - Théâtral, over-the-top
    /// </summary>
    Theatrical
}

/// <summary>
/// Biais de type de worker pour les créatifs
/// </summary>
public enum WorkerTypeBias
{
    /// <summary>
    /// Pas de biais particulier
    /// </summary>
    None,

    /// <summary>
    /// Préférence pour les Big Men (>250 lbs)
    /// </summary>
    BigMen,

    /// <summary>
    /// Préférence pour les cruiserweights (<220 lbs)
    /// </summary>
    Cruiserweights,

    /// <summary>
    /// Préférence pour les vétérans établis
    /// </summary>
    Veterans,

    /// <summary>
    /// Préférence pour les jeunes rookies
    /// </summary>
    Rookies,

    /// <summary>
    /// Préférence pour les workers techniques
    /// </summary>
    TechnicalWorkers,

    /// <summary>
    /// Préférence pour les entertainers/talkers
    /// </summary>
    Entertainers,

    /// <summary>
    /// Préférence pour les hommes
    /// </summary>
    Men,

    /// <summary>
    /// Préférence pour les femmes
    /// </summary>
    Women,

    /// <summary>
    /// Préférence pour les workers locaux/régionaux
    /// </summary>
    LocalTalent,

    /// <summary>
    /// Préférence pour les stars internationales
    /// </summary>
    InternationalStars
}
