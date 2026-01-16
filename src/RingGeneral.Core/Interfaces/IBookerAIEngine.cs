using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Models.Owner;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le moteur de décisions "émotionnelles" du Booker.
/// Gère les traumatismes (souvenirs négatifs) et les biais (relations).
/// </summary>
public interface IBookerAIEngine
{
    /// <summary>
    /// Calcule un score de préférence (0-100) pour un worker donné.
    /// Basé sur les souvenirs (BookerMemory) et les relations (WorkerRelation).
    /// </summary>
    double GetPreferenceScore(string bookerId, string workerId);

    /// <summary>
    /// Récupère la liste des workers "traumatisants" pour le booker.
    /// (Ceux qui ont causé des blessures ou des matchs catastrophiques).
    /// </summary>
    IEnumerable<string> GetTraumaWorkers(string bookerId);

    /// <summary>
    /// Récupère les chouchous (biais positifs) du booker.
    /// </summary>
    IEnumerable<string> GetFavorites(string bookerId);

    /// <summary>
    /// Calcule l'impact d'une décision sur le moral et la réputation du booker.
    /// </summary>
    void ProcessDecisionImpact(string bookerId, string decisionType, string targetId, bool isBiased);

    /// <summary>
    /// Génère automatiquement une carte de show basée sur les préférences et l'état de la compagnie.
    /// </summary>
    List<SegmentDefinition> GenerateAutoBooking(
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments = null,
        AutoBookingConstraints? constraints = null);

    (string Worker1Id, string Worker2Id)? ProposeMainEvent(string bookerId, List<string> availableWorkers, int showImportance);
    void ApplyMemoryDecay(string bookerId);
    void ApplyMemoryDecay(string bookerId, int weeksPassed);
    int EvaluateMatchQuality(string bookerId, int matchRating, int fanReaction, string worker1Id, string worker2Id);
    void CreateMemoryFromMatch(string bookerId, string companyId, int score, string description);
}
