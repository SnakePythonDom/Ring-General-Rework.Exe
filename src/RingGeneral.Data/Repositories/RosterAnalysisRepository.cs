using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Roster;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository de l'analyse structurelle du roster
/// </summary>
public class RosterAnalysisRepository : RepositoryBase, IRosterAnalysisRepository
{
    public RosterAnalysisRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    // ====================================================================
    // ROSTER DNA OPERATIONS
    // ====================================================================

    public async Task SaveRosterDNAAsync(RosterDNA dna)
    {
        await Task.Run(() =>
        {
            if (!dna.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"RosterDNA invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO RosterDNAs (
                    DnaId, CompanyId, HardcorePercentage, TechnicalPercentage,
                    LuchaPercentage, EntertainmentPercentage, StrongStylePercentage,
                    DominantStyle, CoherenceScore, CalculatedAt
                ) VALUES (
                    $dnaId, $companyId, $hardcorePercentage, $technicalPercentage,
                    $luchaPercentage, $entertainmentPercentage, $strongStylePercentage,
                    $dominantStyle, $coherenceScore, $calculatedAt
                )
                ON CONFLICT(CompanyId) DO UPDATE SET
                    HardcorePercentage = $hardcorePercentage,
                    TechnicalPercentage = $technicalPercentage,
                    LuchaPercentage = $luchaPercentage,
                    EntertainmentPercentage = $entertainmentPercentage,
                    StrongStylePercentage = $strongStylePercentage,
                    DominantStyle = $dominantStyle,
                    CoherenceScore = $coherenceScore,
                    CalculatedAt = $calculatedAt;";

            AjouterParametre(command, "$dnaId", dna.DnaId);
            AjouterParametre(command, "$companyId", dna.CompanyId);
            AjouterParametre(command, "$hardcorePercentage", dna.HardcorePercentage);
            AjouterParametre(command, "$technicalPercentage", dna.TechnicalPercentage);
            AjouterParametre(command, "$luchaPercentage", dna.LuchaPercentage);
            AjouterParametre(command, "$entertainmentPercentage", dna.EntertainmentPercentage);
            AjouterParametre(command, "$strongStylePercentage", dna.StrongStylePercentage);
            AjouterParametre(command, "$dominantStyle", dna.DominantStyle);
            AjouterParametre(command, "$coherenceScore", dna.CoherenceScore);
            AjouterParametre(command, "$calculatedAt", dna.CalculatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<RosterDNA?> GetRosterDNAByCompanyIdAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT DnaId, CompanyId, HardcorePercentage, TechnicalPercentage,
                       LuchaPercentage, EntertainmentPercentage, StrongStylePercentage,
                       DominantStyle, CoherenceScore, CalculatedAt
                FROM RosterDNAs
                WHERE CompanyId = $companyId
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRosterDNA(reader);
            }

            return null;
        });
    }

    public async Task UpdateRosterDNAAsync(RosterDNA dna)
    {
        await SaveRosterDNAAsync(dna); // Utilise l'upsert
    }

    public async Task DeleteRosterDNAAsync(string companyId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = "DELETE FROM RosterDNAs WHERE CompanyId = $companyId;";
            AjouterParametre(command, "$companyId", companyId);
            command.ExecuteNonQuery();
        });
    }

    // ====================================================================
    // STRUCTURAL ANALYSIS OPERATIONS
    // ====================================================================

    public async Task SaveStructuralAnalysisAsync(RosterStructuralAnalysis analysis)
    {
        await Task.Run(() =>
        {
            if (!analysis.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"RosterStructuralAnalysis invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO RosterStructuralAnalyses (
                    AnalysisId, CompanyId, StarPowerMoyen, WorkrateMoyen,
                    SpecialisationDominante, Profondeur, IndiceDependance,
                    Polyvalence, CalculatedAt, WeekNumber, Year
                ) VALUES (
                    $analysisId, $companyId, $starPowerMoyen, $workrateMoyen,
                    $specialisationDominante, $profondeur, $indiceDependance,
                    $polyvalence, $calculatedAt, $weekNumber, $year
                );";

            AjouterParametre(command, "$analysisId", analysis.AnalysisId);
            AjouterParametre(command, "$companyId", analysis.CompanyId);
            AjouterParametre(command, "$starPowerMoyen", analysis.StarPowerMoyen);
            AjouterParametre(command, "$workrateMoyen", analysis.WorkrateMoyen);
            AjouterParametre(command, "$specialisationDominante", analysis.SpecialisationDominante);
            AjouterParametre(command, "$profondeur", analysis.Profondeur);
            AjouterParametre(command, "$indiceDependance", analysis.IndiceDependance);
            AjouterParametre(command, "$polyvalence", analysis.Polyvalence);
            AjouterParametre(command, "$calculatedAt", analysis.CalculatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$weekNumber", analysis.WeekNumber);
            AjouterParametre(command, "$year", analysis.Year);

            command.ExecuteNonQuery();
        });
    }

    public async Task<RosterStructuralAnalysis?> GetLatestAnalysisByCompanyIdAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT AnalysisId, CompanyId, StarPowerMoyen, WorkrateMoyen,
                       SpecialisationDominante, Profondeur, IndiceDependance,
                       Polyvalence, CalculatedAt, WeekNumber, Year
                FROM RosterStructuralAnalyses
                WHERE CompanyId = $companyId
                ORDER BY Year DESC, WeekNumber DESC
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRosterStructuralAnalysis(reader);
            }

            return null;
        });
    }

    public async Task<IReadOnlyList<RosterStructuralAnalysis>> GetAnalysesByCompanyIdAsync(string companyId, int? limit = null)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            var limitClause = limit.HasValue ? $"LIMIT {limit.Value}" : "";
            command.CommandText = $@"
                SELECT AnalysisId, CompanyId, StarPowerMoyen, WorkrateMoyen,
                       SpecialisationDominante, Profondeur, IndiceDependance,
                       Polyvalence, CalculatedAt, WeekNumber, Year
                FROM RosterStructuralAnalyses
                WHERE CompanyId = $companyId
                ORDER BY Year DESC, WeekNumber DESC
                {limitClause};";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            var results = new List<RosterStructuralAnalysis>();

            while (reader.Read())
            {
                results.Add(MapRosterStructuralAnalysis(reader));
            }

            return results;
        });
    }

    public async Task<RosterStructuralAnalysis?> GetAnalysisByDateAsync(string companyId, int year, int weekNumber)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT AnalysisId, CompanyId, StarPowerMoyen, WorkrateMoyen,
                       SpecialisationDominante, Profondeur, IndiceDependance,
                       Polyvalence, CalculatedAt, WeekNumber, Year
                FROM RosterStructuralAnalyses
                WHERE CompanyId = $companyId AND Year = $year AND WeekNumber = $weekNumber
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);
            AjouterParametre(command, "$year", year);
            AjouterParametre(command, "$weekNumber", weekNumber);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRosterStructuralAnalysis(reader);
            }

            return null;
        });
    }

    public async Task DeleteOldAnalysesAsync(string companyId, int keepLastN = 10)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                DELETE FROM RosterStructuralAnalyses
                WHERE CompanyId = $companyId
                  AND AnalysisId NOT IN (
                      SELECT AnalysisId
                      FROM RosterStructuralAnalyses
                      WHERE CompanyId = $companyId
                      ORDER BY Year DESC, WeekNumber DESC
                      LIMIT $keepLastN
                  );";

            AjouterParametre(command, "$companyId", companyId);
            AjouterParametre(command, "$keepLastN", keepLastN);

            command.ExecuteNonQuery();
        });
    }

    // ====================================================================
    // MAPPING METHODS
    // ====================================================================

    private static RosterDNA MapRosterDNA(SqliteDataReader reader)
    {
        return new RosterDNA
        {
            DnaId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            HardcorePercentage = reader.GetDouble(2),
            TechnicalPercentage = reader.GetDouble(3),
            LuchaPercentage = reader.GetDouble(4),
            EntertainmentPercentage = reader.GetDouble(5),
            StrongStylePercentage = reader.GetDouble(6),
            DominantStyle = reader.GetString(7),
            CoherenceScore = reader.GetDouble(8),
            CalculatedAt = DateTime.Parse(reader.GetString(9))
        };
    }

    private static RosterStructuralAnalysis MapRosterStructuralAnalysis(SqliteDataReader reader)
    {
        return new RosterStructuralAnalysis
        {
            AnalysisId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            StarPowerMoyen = reader.GetDouble(2),
            WorkrateMoyen = reader.GetDouble(3),
            SpecialisationDominante = reader.GetString(4),
            Profondeur = reader.GetInt32(5),
            IndiceDependance = reader.GetDouble(6),
            Polyvalence = reader.GetDouble(7),
            CalculatedAt = DateTime.Parse(reader.GetString(8)),
            WeekNumber = reader.GetInt32(9),
            Year = reader.GetInt32(10),
            Dna = null // Sera chargé séparément si nécessaire
        };
    }
}
