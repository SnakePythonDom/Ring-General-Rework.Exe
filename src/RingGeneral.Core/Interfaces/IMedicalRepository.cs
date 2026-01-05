using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IMedicalRepository
{
    int AjouterBlessure(Injury injury);
    Injury? ChargerBlessure(int injuryId);
    void MettreAJourBlessure(Injury injury);
    int AjouterPlan(RecoveryPlan plan);
    RecoveryPlan? ChargerPlanPourBlessure(int injuryId);
    void MettreAJourPlanStatut(int injuryId, string statut, int? completedWeek);
    void AjouterNote(MedicalNote note);
    void MettreAJourStatutBlessureWorker(string workerId, string statut);
    string? ChargerStatutBlessureWorker(string workerId);
}
