using System;
using System.Collections.Generic;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository des paramètres du jeu et de l'interface.
/// </summary>
public interface ISettingsRepository
{
    WorkerGenerationOptions ChargerParametresGeneration();
    void SauvegarderParametresGeneration(WorkerGenerationOptions options);
    TableUiSettings ChargerTableUiSettings();
    void SauvegarderTableUiSettings(TableUiSettings settings);
    string ChargerBookingControlLevel();
    void SauvegarderBookingControlLevel(string controlLevel);
}
