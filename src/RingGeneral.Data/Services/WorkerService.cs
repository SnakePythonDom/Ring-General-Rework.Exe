using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Services;

/// <summary>
/// Service de gestion des workers (vieillissement, fatigue, morale, etc.)
/// </summary>
public sealed class WorkerService
{
    private readonly SqliteConnectionFactory _factory;
    private readonly System.Random _random;

    public WorkerService(SqliteConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _random = new System.Random();
    }

    /// <summary>
    /// Traite le vieillissement hebdomadaire de tous les workers
    /// </summary>
    /// <param name="currentWeek">Semaine courante du jeu</param>
    public void ProcessWeeklyAging(int currentWeek)
    {
        Console.WriteLine($"[WorkerService] Traitement du vieillissement pour la semaine {currentWeek}");

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

                Console.WriteLine($"[WorkerService] {workersToAge.Count} workers vieillis");
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Error.WriteLine($"[WorkerService] Erreur lors du vieillissement: {ex.Message}");
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
                    Console.WriteLine($"[WorkerService] {workerId} ({ageInYears} ans) - Déclin physique: InRing {currentInRing} → {newInRing}");
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
        Console.WriteLine($"[WorkerService] Traitement de la récupération de fatigue pour la semaine {currentWeek}");

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
            Console.WriteLine($"[WorkerService] {rowsAffected} workers ont récupéré de la fatigue");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WorkerService] Erreur lors de la récupération de fatigue: {ex.Message}");
        }
    }

    /// <summary>
    /// Traite les changements de moral hebdomadaires
    /// </summary>
    /// <param name="currentWeek">Semaine courante</param>
    public void ProcessWeeklyMoraleChanges(int currentWeek)
    {
        Console.WriteLine($"[WorkerService] Traitement des changements de moral pour la semaine {currentWeek}");

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
            Console.WriteLine($"[WorkerService] {rowsAffected} workers ont eu un ajustement de moral");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WorkerService] Erreur lors des changements de moral: {ex.Message}");
        }
    }

    /// <summary>
    /// Traite tous les événements hebdomadaires pour les workers
    /// </summary>
    /// <param name="currentWeek">Semaine courante</param>
    public void ProcessWeeklyTick(int currentWeek)
    {
        Console.WriteLine($"[WorkerService] ========== TICK HEBDOMADAIRE SEMAINE {currentWeek} ==========");

        ProcessWeeklyAging(currentWeek);
        ProcessWeeklyFatigueRecovery(currentWeek);
        ProcessWeeklyMoraleChanges(currentWeek);

        Console.WriteLine($"[WorkerService] ========== FIN TICK SEMAINE {currentWeek} ==========");
    }
}
