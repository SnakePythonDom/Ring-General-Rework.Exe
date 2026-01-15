namespace RingGeneral.Core.Enums;

/// <summary>
/// Types d'ères pour une compagnie de catch.
/// Influence la structure des shows, les types de matchs dominants, et les attentes du public.
/// </summary>
public enum EraType
{
    /// <summary>
    /// Ère technique - Focus sur le workrate et la qualité des matchs
    /// Match types dominants: Singles techniques, Iron Man, Best of 3
    /// Attentes: Qualité en ring, storytelling subtil
    /// </summary>
    Technical,

    /// <summary>
    /// Ère Entertainment - Focus sur le spectacle et les storylines
    /// Match types dominants: Gimmick matches, segments longs, celebrity appearances
    /// Attentes: Drama, segments mémorables, personnages forts
    /// </summary>
    Entertainment,

    /// <summary>
    /// Ère Hardcore - Focus sur la violence et l'extrême
    /// Match types dominants: Hardcore, Death Match, No DQ, TLC
    /// Attentes: Sang, armes, spots dangereux
    /// </summary>
    Hardcore,

    /// <summary>
    /// Ère Sports Entertainment - Mix équilibré
    /// Match types dominants: Mix de tout, polyvalence
    /// Attentes: Variété, surprises, production TV élevée
    /// </summary>
    SportsEntertainment,

    /// <summary>
    /// Ère Lucha Libre - Focus sur la rapidité et l'aérien
    /// Match types dominants: Tag team, trios, multi-man matches
    /// Attentes: Rapidité, high-flying, choreography
    /// </summary>
    LuchaLibre,

    /// <summary>
    /// Ère Strong Style - Focus sur le réalisme et la dureté
    /// Match types dominants: Singles stiff, MMA-inspired
    /// Attentes: Crédibilité, frappe réaliste, intensité
    /// </summary>
    StrongStyle,

    /// <summary>
    /// Ère Developmental - Focus sur le développement de jeunes talents
    /// Match types dominants: Matches d'apprentissage, squash matches
    /// Attentes: Progression visible, storytelling simple
    /// </summary>
    Developmental,

    /// <summary>
    /// Ère Mainstream - Focus sur l'audience mainstream
    /// Match types dominants: Accessible, family-friendly
    /// Attentes: Contenu pour tous publics, merchandising
    /// </summary>
    Mainstream
}

/// <summary>
/// Vitesse de transition entre ères
/// </summary>
public enum EraTransitionSpeed
{
    /// <summary>
    /// Transition très lente (12+ mois) - Changement progressif et imperceptible
    /// </summary>
    VerySlow,

    /// <summary>
    /// Transition lente (6-12 mois) - Changement graduel
    /// </summary>
    Slow,

    /// <summary>
    /// Transition modérée (3-6 mois) - Changement visible
    /// </summary>
    Moderate,

    /// <summary>
    /// Transition rapide (1-3 mois) - Changement marqué, risque de choc
    /// </summary>
    Fast,

    /// <summary>
    /// Transition brutale (<1 mois) - Révolution, fort risque de rejet
    /// </summary>
    Brutal
}
