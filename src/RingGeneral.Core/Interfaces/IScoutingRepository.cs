using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IScoutingRepository
{
    ScoutingTarget? ChargerCibleScouting(string workerId);
    IReadOnlyList<ScoutingTarget> ChargerCiblesScouting(int limite);
    bool RapportExiste(string workerId, int semaine);
    void AjouterScoutReport(ScoutReport report);
    IReadOnlyList<ScoutReport> ChargerScoutReports();
    IReadOnlyList<ScoutMission> ChargerMissionsActives();
    IReadOnlyList<ScoutMission> ChargerScoutMissions();
    void MettreAJourMissionProgress(string missionId, int progression, string statut, int semaineMaj);
    void AjouterMission(ScoutMission mission);
}
