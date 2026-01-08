using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Crisis;
using RingGeneral.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de gestion de crises backstage.
/// Détecte, escalade, et résout les crises selon un pipeline de 5 stages.
/// </summary>
public sealed class CrisisEngine : ICrisisEngine
{
    private readonly ICrisisRepository? _crisisRepository;
    private readonly Random _random = new();

    public CrisisEngine(ICrisisRepository? crisisRepository = null)
    {
        _crisisRepository = crisisRepository;
    }

    public bool ShouldTriggerCrisis(string companyId, int companyMoraleScore, int activeRumorsCount)
    {
        // Déclenchement basé sur:
        // - Moral critique (< 30): 80% chance
        // - Moral bas (30-50) + rumeurs actives (>= 3): 50% chance
        // - Rumeurs nombreuses (>= 5): 40% chance

        if (companyMoraleScore < 30)
        {
            return _random.Next(100) < 80; // 80% chance
        }

        if (companyMoraleScore < 50 && activeRumorsCount >= 3)
        {
            return _random.Next(100) < 50; // 50% chance
        }

        if (activeRumorsCount >= 5)
        {
            return _random.Next(100) < 40; // 40% chance
        }

        return false;
    }

    public Crisis CreateCrisis(string companyId, string triggerReason, int severity)
    {
        // Déterminer le type de crise basé sur la raison
        var crisisType = DetermineCrisisType(triggerReason, severity);

        var crisis = new Crisis
        {
            CompanyId = companyId,
            CrisisType = crisisType,
            Stage = "WeakSignals", // Toutes les crises commencent par signaux faibles
            Severity = Math.Clamp(severity, 1, 5),
            Description = GenerateCrisisDescription(crisisType, triggerReason),
            EscalationScore = 10, // Score initial faible
            ResolutionAttempts = 0,
            CreatedAt = DateTime.Now
        };

        // Sauvegarder
        if (_crisisRepository != null)
        {
            _crisisRepository.SaveCrisisAsync(crisis).Wait();
        }

        return crisis;
    }

    public void ProgressCrises(string companyId)
    {
        if (_crisisRepository == null)
            return;

        var activeCrises = _crisisRepository.GetActiveCrisesAsync(companyId).Result;

        foreach (var crisis in activeCrises)
        {
            // Augmentation naturelle du score d'escalade chaque semaine (+10 à +25)
            var naturalEscalation = _random.Next(10, 26);
            var updatedCrisis = crisis.IncreaseEscalation(naturalEscalation);

            // Vérifier si doit escalader au stage suivant
            if (ShouldEscalate(updatedCrisis))
            {
                updatedCrisis = updatedCrisis.Escalate();
            }

            // Vérifier si doit être ignorée (dissipée)
            if (ShouldIgnoreCrisis(updatedCrisis))
            {
                updatedCrisis = updatedCrisis.Ignore();
            }

            // Sauvegarder les changements
            _crisisRepository.UpdateCrisisAsync(updatedCrisis).Wait();
        }
    }

    public Crisis? EscalateCrisis(int crisisId)
    {
        if (_crisisRepository == null)
            return null;

        var crisis = _crisisRepository.GetCrisisByIdAsync(crisisId).Result;
        if (crisis == null || !crisis.IsActive())
            return null;

        var escalated = crisis.Escalate();
        _crisisRepository.UpdateCrisisAsync(escalated).Wait();

        return escalated;
    }

    public bool AttemptResolution(int crisisId, int interventionQuality)
    {
        if (_crisisRepository == null)
            return false;

        var crisis = _crisisRepository.GetCrisisByIdAsync(crisisId).Result;
        if (crisis == null || !crisis.IsActive())
            return false;

        // Incrémenter tentatives de résolution
        var updatedCrisis = crisis.IncrementResolutionAttempts();

        // Calculer chance de succès basée sur:
        // - Qualité intervention (0-100)
        // - Sévérité de la crise (plus sévère = plus difficile)
        // - Nombre de tentatives (fatigue)
        var baseChance = interventionQuality;
        var severityPenalty = crisis.Severity * 10; // -10% par niveau sévérité
        var attemptPenalty = crisis.ResolutionAttempts * 5; // -5% par tentative

        var successChance = Math.Clamp(baseChance - severityPenalty - attemptPenalty, 10, 90);

        var success = _random.Next(100) < successChance;

        if (success)
        {
            // Résolution réussie
            updatedCrisis = updatedCrisis.Resolve();
        }
        else
        {
            // Échec: réduction modérée de l'escalade (-10 à -20)
            var reduction = _random.Next(10, 21);
            updatedCrisis = updatedCrisis.DecreaseEscalation(reduction);
        }

        _crisisRepository.UpdateCrisisAsync(updatedCrisis).Wait();
        return success;
    }

    public int CalculateMoraleImpact(Crisis crisis)
    {
        // Impact négatif basé sur sévérité et stage
        var baseImpact = crisis.Severity * -5; // -5 à -25

        // Multiplicateur selon le stage
        var stageMultiplier = crisis.Stage switch
        {
            "WeakSignals" => 0.5,
            "Rumors" => 1.0,
            "Declared" => 1.5,
            "InResolution" => 1.2,
            _ => 0.0
        };

        var totalImpact = (int)(baseImpact * stageMultiplier);

        // Bonus négatif pour score d'escalade élevé
        if (crisis.EscalationScore >= 80)
        {
            totalImpact -= 10; // Pénalité supplémentaire
        }

        return Math.Clamp(totalImpact, -50, 0);
    }

    public bool ShouldIgnoreCrisis(Crisis crisis)
    {
        // Une crise est ignorée si:
        // - Stage = WeakSignals ET EscalationScore < 15 ET 30% chance
        // - Stage = Rumors ET EscalationScore < 25 ET 20% chance

        if (crisis.Stage == "WeakSignals" && crisis.EscalationScore < 15)
        {
            return _random.Next(100) < 30; // 30% chance
        }

        if (crisis.Stage == "Rumors" && crisis.EscalationScore < 25)
        {
            return _random.Next(100) < 20; // 20% chance
        }

        return false;
    }

    public List<Crisis> GetActiveCrises(string companyId)
    {
        if (_crisisRepository == null)
            return new List<Crisis>();

        return _crisisRepository.GetActiveCrisesAsync(companyId).Result;
    }

    public List<Crisis> GetCriticalCrises(string companyId)
    {
        if (_crisisRepository == null)
            return new List<Crisis>();

        return _crisisRepository.GetCriticalCrisesAsync(companyId).Result;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    /// <summary>
    /// Détermine si une crise doit escalader au stage suivant
    /// </summary>
    private bool ShouldEscalate(Crisis crisis)
    {
        // Escalade basée sur EscalationScore
        return crisis.Stage switch
        {
            "WeakSignals" => crisis.EscalationScore >= 40,  // 40+ → Rumors
            "Rumors" => crisis.EscalationScore >= 60,       // 60+ → Declared
            "Declared" => crisis.EscalationScore >= 80,     // 80+ → InResolution
            _ => false
        };
    }

    /// <summary>
    /// Détermine le type de crise basé sur la raison déclenchante
    /// </summary>
    private string DetermineCrisisType(string triggerReason, int severity)
    {
        // Logique simplifiée - en production, analyserait triggerReason
        if (triggerReason.Contains("moral", StringComparison.OrdinalIgnoreCase))
            return "MoraleCollapse";

        if (triggerReason.Contains("rumor", StringComparison.OrdinalIgnoreCase) ||
            triggerReason.Contains("rumeur", StringComparison.OrdinalIgnoreCase))
            return "RumorEscalation";

        if (triggerReason.Contains("plainte", StringComparison.OrdinalIgnoreCase) ||
            triggerReason.Contains("grievance", StringComparison.OrdinalIgnoreCase))
            return "WorkerGrievance";

        if (triggerReason.Contains("public", StringComparison.OrdinalIgnoreCase) ||
            triggerReason.Contains("scandal", StringComparison.OrdinalIgnoreCase))
            return "PublicScandal";

        if (triggerReason.Contains("financial", StringComparison.OrdinalIgnoreCase) ||
            triggerReason.Contains("budget", StringComparison.OrdinalIgnoreCase))
            return "FinancialCrisis";

        if (triggerReason.Contains("exodus", StringComparison.OrdinalIgnoreCase) ||
            triggerReason.Contains("démission", StringComparison.OrdinalIgnoreCase))
            return "TalentExodus";

        // Par défaut: MoraleCollapse si sévérité élevée, sinon WorkerGrievance
        return severity >= 4 ? "MoraleCollapse" : "WorkerGrievance";
    }

    /// <summary>
    /// Génère une description de crise
    /// </summary>
    private string GenerateCrisisDescription(string crisisType, string triggerReason)
    {
        return crisisType switch
        {
            "MoraleCollapse" => $"Le moral backstage s'effondre suite à {triggerReason}.",
            "RumorEscalation" => $"Des rumeurs incontrôlables se répandent: {triggerReason}.",
            "WorkerGrievance" => $"Des workers ont formulé des plaintes formelles: {triggerReason}.",
            "PublicScandal" => $"Un scandale public éclate: {triggerReason}.",
            "FinancialCrisis" => $"Une crise financière menace la compagnie: {triggerReason}.",
            "TalentExodus" => $"Des talents clés menacent de partir: {triggerReason}.",
            _ => $"Une crise backstage se développe: {triggerReason}."
        };
    }
}
