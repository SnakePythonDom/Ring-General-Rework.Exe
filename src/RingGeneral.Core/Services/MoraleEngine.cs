using RingGeneral.Core.Models.Morale;
using RingGeneral.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RingGeneral.Core.Services;

public class MoraleEngine : IMoraleEngine
{
    private readonly IMoraleRepository? _repository;

    public MoraleEngine(IMoraleRepository? repository = null)
    {
        _repository = repository;
    }

    public void UpdateMorale(string entityId, string eventType, int impact)
    {
        if (_repository == null)
            return;

        // Récupérer le moral actuel (par défaut companyId = "1" pour l'instant)
        var currentMorale = _repository.GetBackstageMoraleAsync(entityId, companyId: "1").Result;

        if (currentMorale == null)
        {
            // Créer un nouveau moral si n'existe pas
            currentMorale = new BackstageMorale
            {
                EntityId = entityId,
                EntityType = "Worker", // Par défaut, à améliorer
                CompanyId = "1",
                MoraleScore = 70,
                PreviousMoraleScore = 70,
                LastUpdated = DateTime.Now
            };
        }

        // Sauvegarder le score précédent
        currentMorale.PreviousMoraleScore = currentMorale.MoraleScore;

        // Appliquer l'impact basé sur le type d'événement
        int moraleChange = CalculateMoraleChange(eventType, impact);
        currentMorale.MoraleScore = Math.Clamp(currentMorale.MoraleScore + moraleChange, 0, 100);
        currentMorale.LastUpdated = DateTime.Now;

        // Sauvegarder
        _repository.SaveBackstageMoraleAsync(currentMorale).Wait();

        // Recalculer le moral de compagnie
        _repository.RecalculateCompanyMoraleAsync("1").Wait();
    }

    public CompanyMorale CalculateCompanyMorale(string companyId)
    {
        if (_repository == null)
            return new CompanyMorale { CompanyId = companyId, GlobalMoraleScore = 70 };

        // Déclencher le recalcul
        _repository.RecalculateCompanyMoraleAsync(companyId).Wait();

        // Récupérer le résultat
        var companyMorale = _repository.GetCompanyMoraleAsync(companyId).Result;

        return companyMorale ?? new CompanyMorale { CompanyId = companyId, GlobalMoraleScore = 70 };
    }

    public List<string> DetectWeakSignals(string companyId)
    {
        if (_repository == null)
            return new List<string>();

        var signals = new List<string>();

        // Récupérer les morales faibles et critiques
        var lowMorale = _repository.GetLowMoraleEntitiesAsync(companyId, threshold: 40).Result;
        var criticalMorale = _repository.GetCriticalMoraleEntitiesAsync(companyId, threshold: 20).Result;

        // Générer des signaux faibles basés sur les morales
        if (criticalMorale.Count >= 3)
        {
            signals.Add($"⚠️ {criticalMorale.Count} workers ont un moral critique (< 20)");
        }
        else if (criticalMorale.Any())
        {
            foreach (var morale in criticalMorale)
            {
                signals.Add($"⚠️ Worker {morale.EntityId} a un moral critique: {morale.MoraleScore}");
            }
        }

        if (lowMorale.Count >= 5)
        {
            signals.Add($"ℹ️ {lowMorale.Count} workers ont un moral faible (< 40)");
        }

        // Détecter les tendances négatives
        var allMorale = _repository.GetAllBackstageMoraleAsync(companyId).Result;
        var declining = allMorale.Where(m => !m.IsImproving && m.MoraleScore < m.PreviousMoraleScore).ToList();

        if (declining.Count >= allMorale.Count / 2 && allMorale.Any())
        {
            signals.Add($"📉 Le moral est en baisse générale ({declining.Count}/{allMorale.Count} workers)");
        }

        return signals;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private int CalculateMoraleChange(string eventType, int baseImpact)
    {
        // Mapper les types d'événements à des changements de moral
        return eventType switch
        {
            "MainEventPush" => +baseImpact * 2, // +6 à +10
            "SuccessfulMatch" => +baseImpact, // +3 à +5
            "TitleWin" => +baseImpact * 3, // +9 à +15
            "PushFailed" => -baseImpact * 2, // -6 à -10
            "Buried" => -baseImpact * 3, // -9 à -15
            "Jobber" => -baseImpact, // -3 à -5
            "InjuryIgnored" => -baseImpact * 2, // -6 à -10
            "UnfairSanction" => -baseImpact * 3, // -9 à -15
            "Nepotism" => -baseImpact, // -3 à -5
            "Favoritism" => -baseImpact * 2, // -6 à -10
            _ => baseImpact // Impact par défaut
        };
    }
}
