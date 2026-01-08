using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Services;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Services;

/// <summary>
/// Service de gestion des workers (vieillissement, fatigue, morale, etc.)
/// </summary>
public sealed class WorkerService
{
    private readonly SqliteConnectionFactory _factory;
    private readonly System.Random _random;
    private readonly ILoggingService _logger;

    public WorkerService(SqliteConnectionFactory factory, ILoggingService? logger = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _random = new System.Random();
        _logger = logger ?? ApplicationServices.Logger;
    }

    /// <summary>
    /// Traite le vieillissement hebdomadaire de tous les workers
    /// </summary>
    /// <param name="currentWeek">Semaine courante du jeu</param>
    public void ProcessWeeklyAging(int currentWeek)
    {
        _logger.Info($"Processing weekly aging for week {currentWeek}");

        using var connection = _factory.OuvrirConnexion();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Charger tous les workers actifs
            using (var selectCmd = connection.CreateCommand())
            {
                selectCmd.CommandText = @"
                    SELECT WorkerId, BirthWeek, InRing, Entertainment
                    FROM Workers
                    WHERE CompanyId IS NOT NULL";

                using var reader = selectCmd.ExecuteReader();
                var workersToAge = new List<(string workerId, int age, int inRing, int entertainment)>();

                while (reader.Read())
                {
                    var workerId = reader.GetString(0);
                    var birthWeek = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    var inRing = reader.GetInt32(2);
                    var entertainment = reader.GetInt32(3);

                    // Calculer l'âge en semaines depuis la naissance
                    var ageInWeeks = currentWeek - birthWeek;
                    var ageInYears = ageInWeeks / 52; // ~52 semaines par an

                    workersToAge.Add((workerId, ageInYears, inRing, entertainment));
                }

                reader.Close();

                // Appliquer le vieillissement à chaque worker
                foreach (var worker in workersToAge)
                {
                    ApplyAgingToWorker(connection, worker.workerId, worker.age, worker.inRing, worker.entertainment);
                }

                _logger.Info($"{workersToAge.Count} workers aged");
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.Error($"Error during aging process: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Applique le vieillissement à un worker individuel
    /// </summary>
    private void ApplyAgingToWorker(SqliteConnection connection, string workerId, int ageInYears, int currentInRing, int currentEntertainment)
    {
        // Règle de vieillissement: après 35 ans, 20% de chance de perdre 1 point en InRing chaque année
        if (ageInYears > 35)
        {
            var yearsOver35 = ageInYears - 35;
            var declineChance = 0.20; // 20% de chance par an après 35 ans

            // Chance cumulative de déclin (plus on est vieux, plus c'est probable)
            var totalDeclineChance = Math.Min(declineChance * yearsOver35, 0.80); // Max 80% de chance

            // Lancer le dé pour voir si on applique le déclin
            if (_random.NextDouble() < totalDeclineChance)
            {
                // Réduction de -1 sur InRing (stats physiques)
                var newInRing = Math.Max(1, currentInRing - 1); // Ne peut pas descendre en dessous de 1

                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE Workers
                    SET InRing = @newInRing
                    WHERE WorkerId = @workerId";

                updateCmd.Parameters.AddWithValue("@newInRing", newInRing);
                updateCmd.Parameters.AddWithValue("@workerId", workerId);

                var rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    _logger.Debug($"{workerId} ({ageInYears} years) - Physical decline: InRing {currentInRing} → {newInRing}");
                }
            }
        }
    }

    /// <summary>
    /// Traite la récupération de fatigue hebdomadaire
    /// </summary>
    /// <param name="currentWeek">Semaine courante</param>
    public void ProcessWeeklyFatigueRecovery(int currentWeek)
    {
        _logger.Info($"Processing fatigue recovery for week {currentWeek}");

        using var connection = _factory.OuvrirConnexion();

        try
        {
            // Réduire la fatigue de tous les workers de 5 points par semaine (avec minimum 0)
            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Workers
                SET Fatigue = MAX(0, Fatigue - 5)
                WHERE Fatigue > 0";

            var rowsAffected = updateCmd.ExecuteNonQuery();
            _logger.Info($"{rowsAffected} workers recovered fatigue");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error during fatigue recovery: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Traite les changements de moral hebdomadaires
    /// </summary>
    /// <param name="currentWeek">Semaine courante</param>
    public void ProcessWeeklyMoraleChanges(int currentWeek)
    {
        _logger.Info($"Processing morale changes for week {currentWeek}");

        using var connection = _factory.OuvrirConnexion();

        try
        {
            // Le moral tend naturellement vers 75 (moyenne)
            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Workers
                SET Morale = CASE
                    WHEN Morale < 75 THEN MIN(75, Morale + 2)
                    WHEN Morale > 75 THEN MAX(75, Morale - 2)
                    ELSE Morale
                END
                WHERE Morale != 75";

            var rowsAffected = updateCmd.ExecuteNonQuery();
            _logger.Info($"{rowsAffected} workers had morale adjustment");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error during morale changes: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Traite tous les événements hebdomadaires pour les workers
    /// </summary>
    /// <param name="currentWeek">Semaine courante</param>
    public void ProcessWeeklyTick(int currentWeek)
    {
        _logger.Info($"========== WEEKLY TICK WEEK {currentWeek} ==========");

        ProcessWeeklyAging(currentWeek);
        ProcessWeeklyFatigueRecovery(currentWeek);
        ProcessWeeklyMoraleChanges(currentWeek);

        _logger.Info($"========== END TICK WEEK {currentWeek} ==========");
    }
}
