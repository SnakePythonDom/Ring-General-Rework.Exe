using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Morale;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

public class RumorEngine : IRumorEngine
{
    private readonly IRumorRepository? _repository;
    private readonly System.Random _random = new();

    public RumorEngine(IRumorRepository? repository = null)
    {
        _repository = repository;
    }

    public bool ShouldTriggerRumor(string companyId, string eventType, int eventSeverity)
    {
        // Un événement déclenche une rumeur si:
        // - Sévérité >= 3 (événement significatif)
        // - OU Sévérité >= 2 avec une chance aléatoire de 40%

        if (eventSeverity >= 3)
            return true;

        if (eventSeverity >= 2)
            return _random.Next(100) < 40; // 40% de chance

        return false;
    }

    public Rumor GenerateRumor(string companyId, string rumorType, string triggerEvent)
    {
        // Générer le texte de rumeur basé sur le type
        var rumorText = GenerateRumorText(rumorType, triggerEvent);

        // Déterminer la sévérité basée sur le type d'événement
        var severity = DetermineSeverity(rumorType);

        var rumor = new Rumor
        {
            CompanyId = companyId,
            RumorType = rumorType,
            RumorText = rumorText,
            Stage = "Emerging",
            Severity = severity,
            AmplificationScore = 10, // Score initial faible
            CreatedAt = DateTime.Now
        };

        // Persister dans la base de données
        if (_repository != null)
        {
            _repository.SaveRumorAsync(rumor).Wait();
        }

        return rumor;
    }

    public void AmplifyRumor(int rumorId, string influencerWorkerId)
    {
        if (_repository == null)
            return;

        var rumor = _repository.GetRumorByIdAsync(rumorId).Result;
        if (rumor == null || !rumor.IsActive)
            return;

        // Augmenter le score d'amplification (influencé par la popularité du worker, ici +10 par défaut)
        rumor.AmplificationScore = Math.Min(rumor.AmplificationScore + 10, 100);

        // Progresser le stage si nécessaire
        UpdateRumorStage(rumor);

        // Sauvegarder les changements
        _repository.UpdateRumorAsync(rumor).Wait();
    }

    public void ProgressRumors(string companyId)
    {
        if (_repository == null)
            return;

        var activeRumors = _repository.GetActiveRumorsAsync(companyId).Result;

        foreach (var rumor in activeRumors)
        {
            // Amplification naturelle chaque semaine (+5 à +15)
            var naturalAmplification = _random.Next(5, 16);
            rumor.AmplificationScore = Math.Min(rumor.AmplificationScore + naturalAmplification, 100);

            // Progresser le stage
            UpdateRumorStage(rumor);

            // Chance de résolution si le stage est Widespread et score > 80
            if (rumor.Stage == "Widespread" && rumor.AmplificationScore >= 80)
            {
                // 30% de chance de résolution automatique (intervention du management)
                if (_random.Next(100) < 30)
                {
                    rumor.Stage = "Resolved";
                }
            }

            // Chance d'être ignorée si le stage est Emerging et score < 20
            if (rumor.Stage == "Emerging" && rumor.AmplificationScore < 20)
            {
                // 20% de chance d'être ignorée
                if (_random.Next(100) < 20)
                {
                    rumor.Stage = "Ignored";
                }
            }

            _repository.UpdateRumorAsync(rumor).Wait();
        }

        // Nettoyer les vieilles rumeurs résolues/ignorées (> 90 jours)
        _repository.CleanupOldRumorsAsync(companyId, daysToKeep: 90).Wait();
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private void UpdateRumorStage(Rumor rumor)
    {
        // Progresser le stage basé sur l'AmplificationScore
        rumor.Stage = rumor.AmplificationScore switch
        {
            >= 70 => "Widespread",
            >= 40 => "Growing",
            >= 20 => "Emerging",
            _ => rumor.Stage // Rester au stage actuel si < 20
        };
    }

    private string GenerateRumorText(string rumorType, string triggerEvent)
    {
        return rumorType switch
        {
            "Nepotism" => $"Des rumeurs circulent sur du favoritisme backstage suite à {triggerEvent}.",
            "UnfairPush" => $"Certains workers se plaignent de pushs injustifiés après {triggerEvent}.",
            "Favoritism" => $"Il y aurait des faveurs accordées à certains talents suite à {triggerEvent}.",
            "Grudge" => $"Des tensions backstage s'intensifient après {triggerEvent}.",
            "Burial" => $"Un worker aurait été enterré volontairement lors de {triggerEvent}.",
            "InjuryIgnored" => $"Le management aurait ignoré une blessure sérieuse durant {triggerEvent}.",
            "UnfairSanction" => $"Une sanction injuste aurait été prononcée suite à {triggerEvent}.",
            _ => $"Des tensions se font sentir backstage concernant {triggerEvent}."
        };
    }

    private int DetermineSeverity(string rumorType)
    {
        return rumorType switch
        {
            "Nepotism" => 4,
            "UnfairPush" => 3,
            "Favoritism" => 4,
            "Grudge" => 2,
            "Burial" => 5,
            "InjuryIgnored" => 5,
            "UnfairSanction" => 4,
            _ => 2
        };
    }

    /// <summary>
    /// Récupère toutes les rumeurs actives (pour UI)
    /// </summary>
    public List<Rumor> GetActiveRumors(string companyId)
    {
        if (_repository == null)
            return new List<Rumor>();

        return _repository.GetActiveRumorsAsync(companyId).Result;
    }

    /// <summary>
    /// Récupère les rumeurs répandues (Widespread)
    /// </summary>
    public List<Rumor> GetWidespreadRumors(string companyId)
    {
        if (_repository == null)
            return new List<Rumor>();

        return _repository.GetWidespreadRumorsAsync(companyId).Result;
    }
}
