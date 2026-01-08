using System;

namespace RingGeneral.Core.Models;

/// <summary>
/// Attributs mentaux cachés (0-20) - JAMAIS affichés au joueur.
/// Utilisés uniquement pour le calcul interne des personnalités visibles.
/// </summary>
public class MentalAttributes
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // Worker, Staff, Trainee

    // 10 Attributs Mentaux (0-20, cachés)
    public int Professionalism { get; set; } = 10;
    public int Ambition { get; set; } = 10;
    public int Loyalty { get; set; } = 10;
    public int Ego { get; set; } = 10;
    public int Resilience { get; set; } = 10;
    public int Adaptability { get; set; } = 10;
    public int Creativity { get; set; } = 10;
    public int WorkEthic { get; set; } = 10;
    public int SocialSkills { get; set; } = 10;
    public int Temperament { get; set; } = 10; // 0=volatile, 20=calm

    public DateTime LastUpdated { get; set; } = DateTime.Now;

    // Calculated property (internal use only, never displayed)
    public int AverageScore =>
        (Professionalism + Ambition + Loyalty + Ego + Resilience +
         Adaptability + Creativity + WorkEthic + SocialSkills + Temperament) / 10;

    /// <summary>
    /// Clone avec ajustements (pour événements)
    /// </summary>
    public MentalAttributes Clone()
    {
        return new MentalAttributes
        {
            Id = this.Id,
            EntityId = this.EntityId,
            EntityType = this.EntityType,
            Professionalism = this.Professionalism,
            Ambition = this.Ambition,
            Loyalty = this.Loyalty,
            Ego = this.Ego,
            Resilience = this.Resilience,
            Adaptability = this.Adaptability,
            Creativity = this.Creativity,
            WorkEthic = this.WorkEthic,
            SocialSkills = this.SocialSkills,
            Temperament = this.Temperament,
            LastUpdated = DateTime.Now
        };
    }
}
