namespace RingGeneral.Core.Enums;

/// <summary>
/// Objectifs stratégiques d'un brand dans une structure multi-brand
/// </summary>
public enum BrandObjective
{
    /// <summary>
    /// Flagship - Brand principal, focus sur le prestige et les revenus
    /// Attentes: Meilleurs talents, meilleurs shows, profitabilité maximale
    /// Exemples: WWE Raw, AEW Dynamite
    /// </summary>
    Flagship,

    /// <summary>
    /// Development - Brand de développement de jeunes talents
    /// Attentes: Formation, progression, découverte de futurs stars
    /// Exemples: WWE NXT, AEW Collision (partiellement)
    /// </summary>
    Development,

    /// <summary>
    /// Experimental - Brand expérimental pour tester concepts
    /// Attentes: Innovation, risques créatifs, formats alternatifs
    /// Exemples: WWE ECW (2006), Lucha Underground
    /// </summary>
    Experimental,

    /// <summary>
    /// Mainstream - Brand grand public family-friendly
    /// Attentes: Accessibilité, merchandising, sponsors
    /// Exemples: WWE SmackDown (era PG), Saturday Night's Main Event
    /// </summary>
    Mainstream,

    /// <summary>
    /// Prestige - Brand prestige workrate-oriented
    /// Attentes: Qualité en ring, matchs 4+ étoiles, hardcore fans
    /// Exemples: ROH (early 2000s), NJPW Strong
    /// </summary>
    Prestige,

    /// <summary>
    /// Regional - Brand régional pour marché local
    /// Attentes: Identité locale, stars régionales, TV locale
    /// Exemples: CMLL (Mexico), AJPW (Japan)
    /// </summary>
    Regional,

    /// <summary>
    /// Women - Brand dédié aux femmes
    /// Attentes: Focus total sur division féminine
    /// Exemples: Stardom, AEW Rampage (partiellement)
    /// </summary>
    Women,

    /// <summary>
    /// Touring - Brand tournée internationale
    /// Attentes: Shows internationaux, exposure globale
    /// Exemples: WWE international tours
    /// </summary>
    Touring
}

/// <summary>
/// Type de hiérarchie d'une compagnie
/// </summary>
public enum CompanyHierarchyType
{
    /// <summary>
    /// Mono-brand simple: Owner -> Booker -> Staff
    /// </summary>
    MonoBrand,

    /// <summary>
    /// Multi-brand: Owner -> Head Booker -> Brand Bookers -> Staff
    /// </summary>
    MultiBrand
}
