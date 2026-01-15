namespace RingGeneral.Core.Models;

public sealed record ScoutingTarget(
    string WorkerId,
    string NomComplet,
    string Region,
    int InRing,
    int Entertainment,
    int Story,
    int Popularite);

public sealed record ScoutReport(
    string ReportId,
    string WorkerId,
    string Nom,
    string Region,
    int Potentiel,
    int InRing,
    int Entertainment,
    int Story,
    string Resume,
    string Notes,
    int Semaine,
    string Source);

public sealed record ShortlistEntry(
    string ShortlistId,
    string WorkerId,
    string Nom,
    string Note,
    int Semaine);

public sealed record ScoutMission(
    string MissionId,
    string Titre,
    string Region,
    string Focus,
    int Progression,
    int Objectif,
    string Statut,
    int SemaineDebut,
    int SemaineMaj);

public sealed record ScoutingWeeklyRefresh(
    int Semaine,
    int RapportsCrees,
    int MissionsAvancees,
    int MissionsTerminees);
