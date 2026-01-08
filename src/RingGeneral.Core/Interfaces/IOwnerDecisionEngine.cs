using RingGeneral.Core.Models.Owner;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le moteur de décisions stratégiques du propriétaire.
/// Influence les décisions à long terme: fréquence des shows, budgets, priorités.
/// </summary>
public interface IOwnerDecisionEngine
{
    /// <summary>
    /// Détermine si le propriétaire approuve un budget donné
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="proposedBudget">Budget proposé</param>
    /// <param name="companyTreasury">Trésorerie actuelle de la compagnie</param>
    /// <returns>True si approuvé</returns>
    bool ApprovesBudget(string ownerId, decimal proposedBudget, decimal companyTreasury);

    /// <summary>
    /// Calcule la fréquence optimale de shows selon la vision du propriétaire
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="currentWeek">Semaine actuelle</param>
    /// <returns>Nombre de semaines jusqu'au prochain show</returns>
    int GetOptimalShowFrequency(string ownerId, int currentWeek);

    /// <summary>
    /// Détermine si le propriétaire accepterait une décision risquée
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="riskLevel">Niveau de risque (0-100)</param>
    /// <param name="potentialReward">Récompense potentielle (estimation)</param>
    /// <returns>True si risque accepté</returns>
    bool WouldAcceptRisk(string ownerId, int riskLevel, decimal potentialReward);

    /// <summary>
    /// Détermine la priorité stratégique du moment pour le propriétaire
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="companyFinances">Situation financière actuelle</param>
    /// <param name="fanSatisfaction">Satisfaction des fans (0-100)</param>
    /// <param name="rosterMorale">Moral du roster (0-100)</param>
    /// <returns>Priorité: "Financial", "FanSatisfaction", "TalentDevelopment"</returns>
    string GetCurrentPriority(string ownerId, decimal companyFinances, int fanSatisfaction, int rosterMorale);

    /// <summary>
    /// Détermine si le propriétaire souhaite embaucher un nouveau talent
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="talentCost">Coût du talent</param>
    /// <param name="talentPopularity">Popularité du talent (0-100)</param>
    /// <param name="talentSkill">Compétence du talent (0-100)</param>
    /// <param name="currentRosterSize">Taille actuelle du roster</param>
    /// <returns>True si embauche recommandée</returns>
    bool ShouldHireTalent(
        string ownerId,
        decimal talentCost,
        int talentPopularity,
        int talentSkill,
        int currentRosterSize);

    /// <summary>
    /// Évalue la satisfaction du propriétaire avec la performance de la compagnie
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="financialPerformance">Performance financière (-100 à +100)</param>
    /// <param name="creativePerformance">Performance créative (0-100)</param>
    /// <param name="fanGrowth">Croissance de la base de fans (-100 à +100)</param>
    /// <returns>Score de satisfaction (0-100)</returns>
    int CalculateOwnerSatisfaction(
        string ownerId,
        int financialPerformance,
        int creativePerformance,
        int fanGrowth);

    /// <summary>
    /// Détermine si le propriétaire souhaite remplacer le booker actuel
    /// </summary>
    /// <param name="ownerId">Identifiant du propriétaire</param>
    /// <param name="bookerPerformance">Performance du booker (0-100)</param>
    /// <param name="monthsEmployed">Mois d'emploi du booker</param>
    /// <returns>True si remplacement recommandé</returns>
    bool ShouldReplaceBooker(string ownerId, int bookerPerformance, int monthsEmployed);

    /// <summary>
    /// Récupère le propriétaire d'une compagnie
    /// </summary>
    /// <param name="companyId">Identifiant de la compagnie</param>
    /// <returns>Owner ou null</returns>
    Owner? GetOwnerByCompanyId(string companyId);
}
