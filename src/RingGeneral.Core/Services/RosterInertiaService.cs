using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Roster;
using System;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour gérer l'inertie du roster et les transitions progressives
/// </summary>
public class RosterInertiaService
{
    private readonly IRosterAnalysisRepository _rosterAnalysisRepository;
    private readonly IDNATransitionRepository _transitionRepository;

    public RosterInertiaService(
        IRosterAnalysisRepository rosterAnalysisRepository,
        IDNATransitionRepository transitionRepository)
    {
        _rosterAnalysisRepository = rosterAnalysisRepository ?? throw new ArgumentNullException(nameof(rosterAnalysisRepository));
        _transitionRepository = transitionRepository ?? throw new ArgumentNullException(nameof(transitionRepository));
    }

    /// <summary>
    /// Calcule l'inertie du roster (résistance au changement)
    /// Formule: Inertie = (Cohérence * 0.4) + (Profondeur * 0.3) + (Dépendance * 0.3)
    /// </summary>
    public async Task<double> CalculateRosterInertiaAsync(string companyId)
    {
        var analysis = await _rosterAnalysisRepository.GetLatestAnalysisByCompanyIdAsync(companyId);
        if (analysis == null) return 50; // Inertie moyenne par défaut

        var dna = await _rosterAnalysisRepository.GetRosterDNAByCompanyIdAsync(companyId);
        if (dna == null) return 50;

        // Normaliser la profondeur (0-100)
        var normalizedProfondeur = Math.Min(100, (analysis.Profondeur / 10.0) * 100);

        // Calculer l'inertie
        var inertia = (dna.CoherenceScore * 0.4) +
                     (normalizedProfondeur * 0.3) +
                     (analysis.IndiceDependance * 0.3);

        return Math.Clamp(inertia, 0, 100);
    }

    /// <summary>
    /// Démarre une transition d'ADN
    /// </summary>
    public async Task<DNATransition> TransitionDNAAsync(
        string companyId,
        RosterDNA targetDNA,
        int? durationWeeks = null)
    {
        var currentDNA = await _rosterAnalysisRepository.GetRosterDNAByCompanyIdAsync(companyId);
        if (currentDNA == null)
        {
            throw new InvalidOperationException($"Aucun ADN trouvé pour la compagnie {companyId}");
        }

        // Vérifier qu'il n'y a pas déjà une transition active
        var activeTransition = await _transitionRepository.GetActiveTransitionByCompanyIdAsync(companyId);
        if (activeTransition != null)
        {
            throw new InvalidOperationException($"Une transition est déjà en cours pour la compagnie {companyId}");
        }

        // Calculer la durée basée sur l'inertie
        var inertia = await CalculateRosterInertiaAsync(companyId);
        var baseDurationWeeks = durationWeeks ?? CalculateDurationFromInertia(inertia);

        var transitionId = Guid.NewGuid().ToString("N");
        var transition = new DNATransition
        {
            TransitionId = transitionId,
            CompanyId = companyId,
            StartDNAId = currentDNA.DnaId,
            TargetDNAId = targetDNA.DnaId,
            CurrentWeek = 0,
            TotalWeeks = baseDurationWeeks,
            ProgressPercentage = 0,
            InertiaScore = inertia,
            StartedAt = DateTime.Now,
            CompletedAt = null,
            IsActive = true
        };

        await _transitionRepository.SaveDNATransitionAsync(transition);
        return transition;
    }

    /// <summary>
    /// Fait progresser les transitions actives (appelé chaque semaine)
    /// </summary>
    public async Task ProgressTransitionsAsync()
    {
        // Note: Cette méthode devrait être appelée depuis WeeklyLoopService
        // Pour l'instant, c'est une structure de base
    }

    private int CalculateDurationFromInertia(double inertia)
    {
        // Plus l'inertie est élevée, plus la transition est longue
        // Inertie 0-30: 26 semaines (6 mois)
        // Inertie 30-60: 52 semaines (1 an)
        // Inertie 60-100: 104 semaines (2 ans)
        return inertia switch
        {
            < 30 => 26,
            < 60 => 52,
            _ => 104
        };
    }
}
