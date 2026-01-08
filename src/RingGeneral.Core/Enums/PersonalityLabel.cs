namespace RingGeneral.Core.Enums;

/// <summary>
/// Labels de personnalité visibles (FM-style).
/// Incomplets et interprétatifs par design - reflètent une évaluation subjective
/// du staff basée sur les attributs mentaux cachés.
/// </summary>
public enum PersonalityLabel
{
    // === POSITIVE ===
    /// <summary>
    /// Professionnel, fiable, respectueux des règles
    /// </summary>
    Professional,

    /// <summary>
    /// Fidèle, loyal à la compagnie et ses collègues
    /// </summary>
    Loyal,

    /// <summary>
    /// Ambitieux, veut atteindre le sommet
    /// </summary>
    Ambitious,

    /// <summary>
    /// Créatif, innovant dans son approche
    /// </summary>
    Creative,

    /// <summary>
    /// Résilient, rebondit après les échecs
    /// </summary>
    Resilient,

    /// <summary>
    /// Adaptable, s'ajuste aux changements
    /// </summary>
    Adaptable,

    /// <summary>
    /// Travailleur acharné, éthique de travail exemplaire
    /// </summary>
    Hardworking,

    // === NEUTRAL ===
    /// <summary>
    /// Équilibré, pas de traits dominants
    /// </summary>
    Balanced,

    /// <summary>
    /// Réservé, discret
    /// </summary>
    Reserved,

    /// <summary>
    /// Confiant en soi
    /// </summary>
    Confident,

    // === NEGATIVE ===
    /// <summary>
    /// Égocentrique, se met toujours en avant
    /// </summary>
    Egotistic,

    /// <summary>
    /// Volatile, imprévisible, peut exploser
    /// </summary>
    Volatile,

    /// <summary>
    /// Démotivé, manque d'ambition
    /// </summary>
    Unmotivated,

    /// <summary>
    /// Rebelle, conteste l'autorité
    /// </summary>
    Rebellious,

    /// <summary>
    /// Opportuniste, cherche son intérêt avant tout
    /// </summary>
    Opportunistic,

    // === COMPLEXE (traits multiples) ===
    /// <summary>
    /// Professionnel mais égocentrique
    /// </summary>
    ProfessionalButEgotistic,

    /// <summary>
    /// Ambitieux mais volatile
    /// </summary>
    AmbitiousButVolatile,

    /// <summary>
    /// Créatif mais rebelle
    /// </summary>
    CreativeButRebellious,

    /// <summary>
    /// Loyal mais peu ambitieux
    /// </summary>
    LoyalButUnambitious,

    /// <summary>
    /// Confiant mais arrogant
    /// </summary>
    ConfidentButArrogant
}

/// <summary>
/// Raisons de changement de personnalité
/// </summary>
public enum PersonalityChangeReason
{
    /// <summary>
    /// Suite à un succès majeur (title win, main event push)
    /// </summary>
    Success,

    /// <summary>
    /// Suite à un échec (push raté, heat perdu)
    /// </summary>
    Failure,

    /// <summary>
    /// Suite à un trauma (blessure grave, scandale)
    /// </summary>
    Trauma,

    /// <summary>
    /// Évolution naturelle dans le temps
    /// </summary>
    Growth,

    /// <summary>
    /// Suite à un changement d'environnement (nouvelle compagnie)
    /// </summary>
    EnvironmentChange,

    /// <summary>
    /// Suite à une intervention (discussion avec owner/booker)
    /// </summary>
    Intervention
}
