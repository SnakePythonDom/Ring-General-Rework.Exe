using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Owner;
using RingGeneral.Data.Repositories;
using System;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de décisions stratégiques du propriétaire.
/// Influence les décisions à long terme de la compagnie.
/// </summary>
public sealed class OwnerDecisionEngine : IOwnerDecisionEngine
{
    private readonly IOwnerRepository? _ownerRepository;

    public OwnerDecisionEngine(IOwnerRepository? ownerRepository = null)
    {
        _ownerRepository = ownerRepository;
    }

    public bool ApprovesBudget(string ownerId, decimal proposedBudget, decimal companyTreasury)
    {
        if (_ownerRepository == null)
            return false;

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return false;

        // Calculer le pourcentage du trésor que représente le budget
        var budgetPercentage = companyTreasury > 0 ? (proposedBudget / companyTreasury) * 100 : 100;

        // Owners avec haute priorité financière sont plus conservateurs
        var maxBudgetPercentage = owner.FinancialPriority >= 70 ? 20 :
                                  owner.FinancialPriority >= 40 ? 35 :
                                  50;

        return budgetPercentage <= maxBudgetPercentage;
    }

    public int GetOptimalShowFrequency(string ownerId, int currentWeek)
    {
        if (_ownerRepository == null)
            return 4; // Default: monthly

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return 4;

        return owner.ShowFrequencyPreference switch
        {
            "Weekly" => 1,
            "BiWeekly" => 2,
            "Monthly" => 4,
            _ => 4
        };
    }

    public bool WouldAcceptRisk(string ownerId, int riskLevel, decimal potentialReward)
    {
        if (_ownerRepository == null)
            return false;

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return false;

        // Utiliser la méthode du modèle Owner
        var baseAcceptance = owner.WouldAcceptRisk(riskLevel);

        // Ajuster basé sur la récompense potentielle
        // Si la récompense est très haute, même un owner conservateur peut accepter
        if (potentialReward > 100000 && owner.VisionType == "Business")
        {
            return riskLevel <= owner.RiskTolerance + 20; // Bonus tolérance
        }

        return baseAcceptance;
    }

    public string GetCurrentPriority(
        string ownerId,
        decimal companyFinances,
        int fanSatisfaction,
        int rosterMorale)
    {
        if (_ownerRepository == null)
            return "Financial";

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return "Financial";

        // Si finances critiques (< $50k), priorité financière forcée
        if (companyFinances < 50000)
        {
            return "Financial";
        }

        // Si moral roster critique (< 30), priorité talent development
        if (rosterMorale < 30)
        {
            return "TalentDevelopment";
        }

        // Sinon, utiliser la priorité dominante de l'owner
        return owner.GetDominantPriority();
    }

    public bool ShouldHireTalent(
        string ownerId,
        decimal talentCost,
        int talentPopularity,
        int talentSkill,
        int currentRosterSize)
    {
        if (_ownerRepository == null)
            return false;

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return false;

        // Vérifier si le roster n'est pas déjà trop grand
        var maxRosterSize = 50; // TODO: rendre configurable
        if (currentRosterSize >= maxRosterSize)
        {
            return false;
        }

        // Owner avec haute priorité TalentDevelopment préfère développer
        // plutôt qu'acheter des stars
        if (owner.TalentDevelopmentFocus >= 70)
        {
            // Accepte seulement les talents à haut potentiel (skill >= 70) et peu chers
            return talentSkill >= 70 && talentCost < 10000;
        }

        // Owner Business accepte les stars populaires
        if (owner.VisionType == "Business")
        {
            // Star populaire qui peut générer revenus
            return talentPopularity >= 70 || talentSkill >= 80;
        }

        // Owner Creative accepte les talents techniques
        if (owner.VisionType == "Creative")
        {
            return talentSkill >= 75;
        }

        // Owner Balanced: mix
        return (talentPopularity >= 60 && talentSkill >= 60);
    }

    public int CalculateOwnerSatisfaction(
        string ownerId,
        int financialPerformance,
        int creativePerformance,
        int fanGrowth)
    {
        if (_ownerRepository == null)
            return 50;

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return 50;

        // Calculer la satisfaction basée sur les priorités de l'owner
        var financialWeight = owner.FinancialPriority / 100.0;
        var fanWeight = owner.FanSatisfactionPriority / 100.0;
        var creativeWeight = 1.0 - (financialWeight + fanWeight); // Reste pour créativité

        // Normaliser financialPerformance et fanGrowth (-100/+100 → 0-100)
        var normalizedFinancial = (financialPerformance + 100) / 2.0;
        var normalizedFanGrowth = (fanGrowth + 100) / 2.0;

        var satisfaction = (normalizedFinancial * financialWeight) +
                          (creativePerformance * creativeWeight) +
                          (normalizedFanGrowth * fanWeight);

        // Ajuster basé sur le VisionType
        satisfaction = owner.VisionType switch
        {
            "Creative" => satisfaction + (creativePerformance >= 70 ? 10 : 0),
            "Business" => satisfaction + (financialPerformance >= 50 ? 10 : 0),
            _ => satisfaction
        };

        return Math.Clamp((int)satisfaction, 0, 100);
    }

    public bool ShouldReplaceBooker(string ownerId, int bookerPerformance, int monthsEmployed)
    {
        if (_ownerRepository == null)
            return false;

        var owner = _ownerRepository.GetOwnerByIdAsync(ownerId).Result;
        if (owner == null)
            return false;

        // Période de grâce (3 mois minimum)
        if (monthsEmployed < 3)
        {
            return false;
        }

        // Owner avec haute tolérance au risque garde plus longtemps
        var performanceThreshold = owner.RiskTolerance >= 70 ? 30 :
                                  owner.RiskTolerance >= 40 ? 40 :
                                  50;

        // Remplacer si performance sous le seuil ET employé depuis au moins 6 mois
        if (bookerPerformance < performanceThreshold && monthsEmployed >= 6)
        {
            return true;
        }

        // Ou si performance catastrophique (< 20) peu importe durée
        if (bookerPerformance < 20 && monthsEmployed >= 3)
        {
            return true;
        }

        return false;
    }

    public Owner? GetOwnerByCompanyId(string companyId)
    {
        if (_ownerRepository == null)
            return null;

        return _ownerRepository.GetOwnerByCompanyIdAsync(companyId).Result;
    }
}
