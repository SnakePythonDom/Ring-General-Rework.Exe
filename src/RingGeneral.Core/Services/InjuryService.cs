using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class InjuryService
{
    private readonly IMedicalRepository _repository;
    private readonly MedicalRecommendations _recommendations;

    public InjuryService(IMedicalRepository repository, MedicalRecommendations recommendations)
    {
        _repository = repository;
        _recommendations = recommendations;
    }

    public InjuryApplicationResult AppliquerBlessure(
        string workerId,
        string type,
        int severite,
        int semaine,
        int fatigue,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(workerId))
        {
            throw new ArgumentException("WorkerId manquant.", nameof(workerId));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type de blessure manquant.", nameof(type));
        }

        var recommendation = _recommendations.Recommander(fatigue, severite);
        var retourEstime = recommendation.ReposSemaines == 0 ? (int?)null : semaine + recommendation.ReposSemaines;

        var injury = new Injury(
            0,
            workerId,
            type,
            Math.Clamp(severite, 0, 100),
            semaine,
            retourEstime,
            true,
            note,
            recommendation.Risque);

        var injuryId = _repository.AjouterBlessure(injury);
        var injuryPersisted = injury with { InjuryId = injuryId };

        _repository.MettreAJourStatutBlessureWorker(workerId, type);

        var plan = new RecoveryPlan(
            0,
            injuryId,
            workerId,
            semaine,
            retourEstime ?? semaine,
            "EN_COURS",
            recommendation.Conseil,
            null);
        var planId = _repository.AjouterPlan(plan);
        var planPersisted = plan with { RecoveryPlanId = planId };

        if (!string.IsNullOrWhiteSpace(note))
        {
            _repository.AjouterNote(new MedicalNote(0, workerId, injuryId, semaine, note));
        }

        return new InjuryApplicationResult(injuryPersisted, planPersisted, recommendation);
    }

    public InjuryRecoveryResult RecupererBlessure(int injuryId, int semaine, string? note = null)
    {
        var injury = _repository.ChargerBlessure(injuryId)
            ?? throw new InvalidOperationException($"Blessure introuvable: {injuryId}.");

        var updated = injury with { EndWeek = semaine, IsActive = false };
        _repository.MettreAJourBlessure(updated);
        _repository.MettreAJourPlanStatut(injuryId, "TERMINE", semaine);
        _repository.MettreAJourStatutBlessureWorker(injury.WorkerId, "AUCUNE");

        if (!string.IsNullOrWhiteSpace(note))
        {
            _repository.AjouterNote(new MedicalNote(0, injury.WorkerId, injuryId, semaine, note));
        }

        return new InjuryRecoveryResult(injuryId, injury.WorkerId, semaine, "TERMINE");
    }

    public double CalculerRisque(int fatigue, int severite, bool retourAnticipe = false)
    {
        var recommendation = _recommendations.Recommander(fatigue, severite);
        var risque = recommendation.Risque;
        if (retourAnticipe)
        {
            risque = Math.Min(1.0, risque + 0.15);
        }

        return risque;
    }
}
