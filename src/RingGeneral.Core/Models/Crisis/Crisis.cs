using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace RingGeneral.Core.Models.Crisis;

/// <summary>
/// Représente une crise backstage avec pipeline de 5 stages.
/// Les crises escaladent de WeakSignals → Rumors → Declared → InResolution → Resolved/Ignored
/// </summary>
public sealed record Crisis
{
    /// <summary>
    /// Identifiant unique de la crise
    /// </summary>
    public int CrisisId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Type de crise:
    /// - MoraleCollapse: Effondrement du moral collectif
    /// - RumorEscalation: Rumeur devenue incontrôlable
    /// - WorkerGrievance: Plainte formelle de workers
    /// - PublicScandal: Scandale public (médias)
    /// - FinancialCrisis: Crise financière (dettes, salaires)
    /// - TalentExodus: Fuite de talents (démissions en masse)
    /// </summary>
    [Required]
    public required string CrisisType { get; init; }

    /// <summary>
    /// Stage actuel dans le pipeline:
    /// - WeakSignals: Signaux faibles détectés
    /// - Rumors: Rumeurs répandues
    /// - Declared: Crise déclarée officiellement
    /// - InResolution: En cours de résolution
    /// - Resolved: Résolue
    /// - Ignored: Ignorée (dissipée naturellement)
    /// </summary>
    [Required]
    public required string Stage { get; init; }

    /// <summary>
    /// Sévérité de la crise (1-5)
    /// - 1: Mineure
    /// - 2: Modérée
    /// - 3: Sérieuse
    /// - 4: Majeure
    /// - 5: Critique
    /// </summary>
    [Range(1, 5)]
    public required int Severity { get; init; }

    /// <summary>
    /// Description de la crise
    /// </summary>
    [Required]
    [StringLength(1000)]
    public required string Description { get; init; }

    /// <summary>
    /// Workers affectés (JSON array de WorkerIds)
    /// </summary>
    public string? AffectedWorkers { get; init; }

    /// <summary>
    /// Score d'escalade (0-100)
    /// - Augmente chaque semaine sans intervention
    /// - Détermine quand la crise passe au stage suivant
    /// </summary>
    [Range(0, 100)]
    public int EscalationScore { get; init; }

    /// <summary>
    /// Nombre de tentatives de résolution
    /// </summary>
    public int ResolutionAttempts { get; init; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Date de résolution (NULL si non résolue)
    /// </summary>
    public DateTime? ResolvedAt { get; init; }

    /// <summary>
    /// Valide que la crise respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        var validTypes = new[]
        {
            "MoraleCollapse", "RumorEscalation", "WorkerGrievance",
            "PublicScandal", "FinancialCrisis", "TalentExodus"
        };

        if (!validTypes.Contains(CrisisType))
        {
            errorMessage = $"CrisisType doit être: {string.Join(", ", validTypes)}";
            return false;
        }

        var validStages = new[] { "WeakSignals", "Rumors", "Declared", "InResolution", "Resolved", "Ignored" };
        if (!validStages.Contains(Stage))
        {
            errorMessage = $"Stage doit être: {string.Join(", ", validStages)}";
            return false;
        }

        if (Severity is < 1 or > 5)
        {
            errorMessage = "Severity doit être entre 1 et 5";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Description) || Description.Length > 1000)
        {
            errorMessage = "Description doit être entre 1 et 1000 caractères";
            return false;
        }

        if (EscalationScore is < 0 or > 100)
        {
            errorMessage = "EscalationScore doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si la crise est active (non résolue/ignorée)
    /// </summary>
    public bool IsActive() => Stage is not "Resolved" and not "Ignored";

    /// <summary>
    /// Détermine si la crise est au stade critique
    /// </summary>
    public bool IsCritical() => Severity >= 4 || Stage == "Declared";

    /// <summary>
    /// Récupère la liste des workers affectés depuis le JSON
    /// </summary>
    public List<string> GetAffectedWorkerIds()
    {
        if (string.IsNullOrWhiteSpace(AffectedWorkers))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(AffectedWorkers) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Crée une copie avec workers affectés
    /// </summary>
    public Crisis WithAffectedWorkers(List<string> workerIds)
    {
        var json = JsonSerializer.Serialize(workerIds);
        return this with { AffectedWorkers = json };
    }

    /// <summary>
    /// Escalade la crise au stage suivant
    /// </summary>
    public Crisis Escalate()
    {
        var nextStage = Stage switch
        {
            "WeakSignals" => "Rumors",
            "Rumors" => "Declared",
            "Declared" => "InResolution",
            "InResolution" => "InResolution", // Reste bloqué
            _ => Stage
        };

        return this with { Stage = nextStage, EscalationScore = Math.Min(EscalationScore + 20, 100) };
    }

    /// <summary>
    /// Marque la crise comme résolue
    /// </summary>
    public Crisis Resolve()
    {
        return this with
        {
            Stage = "Resolved",
            ResolvedAt = DateTime.Now,
            EscalationScore = 0
        };
    }

    /// <summary>
    /// Marque la crise comme ignorée (dissipée)
    /// </summary>
    public Crisis Ignore()
    {
        return this with
        {
            Stage = "Ignored",
            ResolvedAt = DateTime.Now,
            EscalationScore = 0
        };
    }

    /// <summary>
    /// Augmente le score d'escalade
    /// </summary>
    public Crisis IncreaseEscalation(int amount)
    {
        return this with { EscalationScore = Math.Min(EscalationScore + amount, 100) };
    }

    /// <summary>
    /// Réduit le score d'escalade (intervention réussie)
    /// </summary>
    public Crisis DecreaseEscalation(int amount)
    {
        return this with { EscalationScore = Math.Max(EscalationScore - amount, 0) };
    }

    /// <summary>
    /// Incrémente le nombre de tentatives de résolution
    /// </summary>
    public Crisis IncrementResolutionAttempts()
    {
        return this with { ResolutionAttempts = ResolutionAttempts + 1 };
    }

    /// <summary>
    /// Retourne le label de sévérité
    /// </summary>
    public string GetSeverityLabel()
    {
        return Severity switch
        {
            1 => "Mineure",
            2 => "Modérée",
            3 => "Sérieuse",
            4 => "Majeure",
            5 => "Critique",
            _ => "Inconnue"
        };
    }
}
