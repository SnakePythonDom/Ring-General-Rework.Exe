namespace RingGeneral.Core.Enums;

/// <summary>
/// Stade du cycle de vie d'une fédération
/// </summary>
public enum LifecycleStage
{
    /// <summary>
    /// Fondation - Début de la compagnie
    /// </summary>
    Foundation,

    /// <summary>
    /// Croissance - Expansion active
    /// </summary>
    Growth,

    /// <summary>
    /// Maturité - Stabilité, succès établi
    /// </summary>
    Maturity,

    /// <summary>
    /// Niche - Spécialisation, stabilité sans croissance
    /// </summary>
    Niche,

    /// <summary>
    /// Déclin - Perte d'audience, difficultés
    /// </summary>
    Decline,

    /// <summary>
    /// Renaissance - Retour après déclin
    /// </summary>
    Renaissance
}
