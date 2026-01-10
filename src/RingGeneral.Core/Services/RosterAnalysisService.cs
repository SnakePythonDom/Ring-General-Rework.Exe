using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Roster;
using RingGeneral.Data.Database;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Service pour calculer l'analyse structurelle du roster
/// </summary>
public class RosterAnalysisService
{
    private readonly IRosterAnalysisRepository _rosterAnalysisRepository;
    private readonly SqliteConnectionFactory _connectionFactory;

    public RosterAnalysisService(
        IRosterAnalysisRepository rosterAnalysisRepository,
        SqliteConnectionFactory connectionFactory)
    {
        _rosterAnalysisRepository = rosterAnalysisRepository ?? throw new ArgumentNullException(nameof(rosterAnalysisRepository));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Calcule l'analyse structurelle complète pour une compagnie
    /// </summary>
    public async Task<RosterStructuralAnalysis> CalculateStructuralAnalysisAsync(
        string companyId,
        int weekNumber,
        int year)
    {
        // Charger tous les workers de la compagnie avec leurs attributs
        var workers = await LoadWorkersWithAttributesAsync(companyId);

        if (workers.Count == 0)
        {
            throw new InvalidOperationException($"Aucun worker trouvé pour la compagnie {companyId}");
        }

        // Calculer les indicateurs globaux
        var starPowerMoyen = CalculateStarPowerMoyen(workers);
        var workrateMoyen = CalculateWorkrateMoyen(workers);
        var specialisationDominante = DetermineSpecialisationDominante(workers);
        var profondeur = CalculateProfondeur(workers);
        var indiceDependance = CalculateDependencyIndex(workers);
        var polyvalence = CalculatePolyvalence(workers);

        // Calculer l'ADN du roster
        var dna = await CalculateRosterDNAAsync(companyId, workers);

        // Créer l'analyse
        var analysisId = Guid.NewGuid().ToString("N");
        var analysis = new RosterStructuralAnalysis
        {
            AnalysisId = analysisId,
            CompanyId = companyId,
            StarPowerMoyen = starPowerMoyen,
            WorkrateMoyen = workrateMoyen,
            SpecialisationDominante = specialisationDominante,
            Profondeur = profondeur,
            IndiceDependance = indiceDependance,
            Polyvalence = polyvalence,
            Dna = dna,
            CalculatedAt = DateTime.Now,
            WeekNumber = weekNumber,
            Year = year
        };

        // Sauvegarder
        await _rosterAnalysisRepository.SaveStructuralAnalysisAsync(analysis);

        return analysis;
    }

    /// <summary>
    /// Calcule l'ADN du roster (composition stylistique)
    /// </summary>
    public async Task<RosterDNA> CalculateRosterDNAAsync(string companyId, List<WorkerWithAttributes>? workers = null)
    {
        workers ??= await LoadWorkersWithAttributesAsync(companyId);

        if (workers.Count == 0)
        {
            throw new InvalidOperationException($"Aucun worker trouvé pour la compagnie {companyId}");
        }

        // Calculer les pourcentages par style basés sur les spécialisations
        var styleCounts = new Dictionary<string, int>
        {
            { "Hardcore", 0 },
            { "Technical", 0 },
            { "Lucha", 0 },
            { "Entertainment", 0 },
            { "StrongStyle", 0 }
        };

        foreach (var worker in workers)
        {
            // Déterminer le style principal du worker basé sur ses attributs et spécialisations
            var primaryStyle = DetermineWorkerStyle(worker);
            if (styleCounts.ContainsKey(primaryStyle))
            {
                styleCounts[primaryStyle]++;
            }
        }

        var total = workers.Count;
        var hardcorePercentage = (styleCounts["Hardcore"] / (double)total) * 100;
        var technicalPercentage = (styleCounts["Technical"] / (double)total) * 100;
        var luchaPercentage = (styleCounts["Lucha"] / (double)total) * 100;
        var entertainmentPercentage = (styleCounts["Entertainment"] / (double)total) * 100;
        var strongStylePercentage = (styleCounts["StrongStyle"] / (double)total) * 100;

        // Déterminer le style dominant
        var dominantStyle = styleCounts.OrderByDescending(kvp => kvp.Value).First().Key;

        // Calculer la cohérence (écart-type des pourcentages, inversé)
        var percentages = new[] { hardcorePercentage, technicalPercentage, luchaPercentage, entertainmentPercentage, strongStylePercentage };
        var mean = percentages.Average();
        var variance = percentages.Sum(p => Math.Pow(p - mean, 2)) / percentages.Length;
        var stdDev = Math.Sqrt(variance);
        var coherenceScore = Math.Max(0, 100 - (stdDev * 2)); // Plus l'écart-type est faible, plus la cohérence est élevée

        var dnaId = Guid.NewGuid().ToString("N");
        var dna = new RosterDNA
        {
            DnaId = dnaId,
            CompanyId = companyId,
            HardcorePercentage = hardcorePercentage,
            TechnicalPercentage = technicalPercentage,
            LuchaPercentage = luchaPercentage,
            EntertainmentPercentage = entertainmentPercentage,
            StrongStylePercentage = strongStylePercentage,
            DominantStyle = dominantStyle,
            CoherenceScore = coherenceScore,
            CalculatedAt = DateTime.Now
        };

        // Sauvegarder
        await _rosterAnalysisRepository.SaveRosterDNAAsync(dna);

        return dna;
    }

    /// <summary>
    /// Obtient les workers capables de main event
    /// </summary>
    public async Task<List<string>> GetMainEventCapableWorkersAsync(string companyId)
    {
        var workers = await LoadWorkersWithAttributesAsync(companyId);

        // Un worker est capable de main event si :
        // - Star Power >= 70 OU
        // - (InRingAvg >= 75 ET EntertainmentAvg >= 70) OU
        // - Popularité >= 80
        return workers
            .Where(w => 
                w.StarPower >= 70 ||
                (w.InRingAvg >= 75 && w.EntertainmentAvg >= 70) ||
                w.Popularity >= 80)
            .Select(w => w.WorkerId)
            .ToList();
    }

    // ====================================================================
    // PRIVATE HELPER METHODS
    // ====================================================================

    private async Task<List<WorkerWithAttributes>> LoadWorkersWithAttributesAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = _connectionFactory.OuvrirConnexion();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT 
                    w.WorkerId,
                    w.Popularity,
                    COALESCE(ir.InRingAvg, 0) as InRingAvg,
                    COALESCE(e.EntertainmentAvg, 0) as EntertainmentAvg,
                    COALESCE(s.StoryAvg, 0) as StoryAvg,
                    COALESCE(e.StarPower, 0) as StarPower,
                    GROUP_CONCAT(ws.Specialization) as Specializations
                FROM Workers w
                LEFT JOIN (
                    SELECT WorkerId, 
                           (Striking + Grappling + HighFlying + Powerhouse + Timing + 
                            Selling + Psychology + Stamina + Safety + HardcoreBrawl) / 10.0 as InRingAvg
                    FROM WorkerInRingAttributes
                ) ir ON w.WorkerId = ir.WorkerId
                LEFT JOIN (
                    SELECT WorkerId,
                           (Charisma + MicWork + Acting + CrowdConnection + StarPower + 
                            Improvisation + Entrance + SexAppeal + MerchandiseAppeal + CrossoverPotential) / 10.0 as EntertainmentAvg,
                           StarPower
                    FROM WorkerEntertainmentAttributes
                ) e ON w.WorkerId = e.WorkerId
                LEFT JOIN (
                    SELECT WorkerId,
                           (CharacterDepth + Consistency + HeelPerformance + BabyfacePerformance + 
                            StorytellingLongTerm + EmotionalRange + Adaptability + RivalryChemistry + 
                            CreativeInput + MoralAlignment) / 10.0 as StoryAvg
                    FROM WorkerStoryAttributes
                ) s ON w.WorkerId = s.WorkerId
                LEFT JOIN WorkerSpecializations ws ON w.WorkerId = ws.WorkerId AND ws.Level = 1
                WHERE w.CompanyId = $companyId
                GROUP BY w.WorkerId;";

            command.Parameters.AddWithValue("$companyId", companyId);

            using var reader = command.ExecuteReader();
            var workers = new List<WorkerWithAttributes>();

            while (reader.Read())
            {
                var specializationsStr = reader.IsDBNull(6) ? "" : reader.GetString(6);
                var specializations = string.IsNullOrWhiteSpace(specializationsStr) 
                    ? new List<string>() 
                    : specializationsStr.Split(',').ToList();

                workers.Add(new WorkerWithAttributes
                {
                    WorkerId = reader.GetString(0),
                    Popularity = reader.GetInt32(1),
                    InRingAvg = (int)reader.GetDouble(2),
                    EntertainmentAvg = (int)reader.GetDouble(3),
                    StoryAvg = (int)reader.GetDouble(4),
                    StarPower = (int)reader.GetDouble(5),
                    Specializations = specializations
                });
            }

            return workers;
        });
    }

    private double CalculateStarPowerMoyen(List<WorkerWithAttributes> workers)
    {
        if (workers.Count == 0) return 0;
        return workers.Average(w => (double)w.StarPower);
    }

    private double CalculateWorkrateMoyen(List<WorkerWithAttributes> workers)
    {
        if (workers.Count == 0) return 0;
        return workers.Average(w => (double)w.InRingAvg);
    }

    private string DetermineSpecialisationDominante(List<WorkerWithAttributes> workers)
    {
        var styleCounts = new Dictionary<string, int>
        {
            { "Hardcore", 0 },
            { "Technical", 0 },
            { "Lucha", 0 },
            { "Entertainment", 0 },
            { "StrongStyle", 0 }
        };

        foreach (var worker in workers)
        {
            var style = DetermineWorkerStyle(worker);
            if (styleCounts.ContainsKey(style))
            {
                styleCounts[style]++;
            }
        }

        return styleCounts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private int CalculateProfondeur(List<WorkerWithAttributes> workers)
    {
        return workers.Count(w => 
            w.StarPower >= 70 ||
            (w.InRingAvg >= 75 && w.EntertainmentAvg >= 70) ||
            w.Popularity >= 80);
    }

    private double CalculateDependencyIndex(List<WorkerWithAttributes> workers)
    {
        if (workers.Count == 0) return 0;

        // Trier par star power + popularité
        var sortedWorkers = workers
            .OrderByDescending(w => w.StarPower + w.Popularity)
            .ToList();

        if (sortedWorkers.Count < 2) return 100; // Dépendance totale si moins de 2 workers

        // Calculer le pourcentage de "star power" concentré dans les 2 meilleurs
        var top2StarPower = sortedWorkers.Take(2).Sum(w => w.StarPower + w.Popularity);
        var totalStarPower = workers.Sum(w => w.StarPower + w.Popularity);

        if (totalStarPower == 0) return 0;

        return (top2StarPower / totalStarPower) * 100;
    }

    private double CalculatePolyvalence(List<WorkerWithAttributes> workers)
    {
        if (workers.Count == 0) return 0;

        // La polyvalence est basée sur la diversité des spécialisations
        var allSpecializations = workers
            .SelectMany(w => w.Specializations)
            .Distinct()
            .ToList();

        // Plus il y a de spécialisations différentes, plus la polyvalence est élevée
        var maxPossibleSpecializations = 8; // Nombre maximum de spécialisations possibles
        return Math.Min(100, (allSpecializations.Count / (double)maxPossibleSpecializations) * 100);
    }

    private string DetermineWorkerStyle(WorkerWithAttributes worker)
    {
        // Déterminer le style basé sur les spécialisations et attributs
        if (worker.Specializations.Contains("Hardcore"))
            return "Hardcore";
        if (worker.Specializations.Contains("Technical"))
            return "Technical";
        if (worker.Specializations.Contains("HighFlyer"))
            return "Lucha";
        if (worker.Specializations.Contains("Showman"))
            return "Entertainment";
        if (worker.Specializations.Contains("Power"))
            return "StrongStyle";

        // Par défaut, basé sur les attributs
        if (worker.InRingAvg > worker.EntertainmentAvg + 10)
            return "Technical";
        if (worker.EntertainmentAvg > worker.InRingAvg + 10)
            return "Entertainment";

        return "Hybrid";
    }

    private class WorkerWithAttributes
    {
        public string WorkerId { get; set; } = string.Empty;
        public int Popularity { get; set; }
        public int InRingAvg { get; set; }
        public int EntertainmentAvg { get; set; }
        public int StoryAvg { get; set; }
        public int StarPower { get; set; }
        public List<string> Specializations { get; set; } = new();
    }
}
