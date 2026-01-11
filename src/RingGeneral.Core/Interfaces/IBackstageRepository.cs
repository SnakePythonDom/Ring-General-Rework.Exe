using System;
using System.Collections.Generic;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository de la gestion du backstage et du moral.
/// </summary>
public interface IBackstageRepository
{
    void AjouterIncident(BackstageIncident incident);
    IReadOnlyList<BackstageIncident> ChargerIncidents();
    void AjouterActionDisciplinaire(DisciplinaryAction action);
    IReadOnlyList<DisciplinaryAction> ChargerActions(string? incidentId = null);
    void AjouterMoraleHistorique(MoraleHistoryEntry entry);
    int ChargerMoraleActuelle(string workerId, int valeurDefaut = 50);
    IReadOnlyList<MoraleHistoryEntry> AppliquerMoraleImpacts(IReadOnlyList<BackstageMoraleImpact> impacts, int week);
}
