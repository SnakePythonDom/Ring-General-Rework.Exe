using Microsoft.Data.Sqlite;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository des fédérations de niche
/// </summary>
public class NicheFederationRepository : RepositoryBase, INicheFederationRepository
{
    public NicheFederationRepository(SqliteConnectionFactory factory)
        : base(factory)
    {
    }

    public async Task SaveNicheFederationProfileAsync(NicheFederationProfile profile)
    {
        await Task.Run(() =>
        {
            if (!profile.IsValid(out var errorMessage))
            {
                throw new ArgumentException($"NicheFederationProfile invalide: {errorMessage}");
            }

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO NicheFederationProfiles (
                    ProfileId, CompanyId, IsNicheFederation, NicheType,
                    CaptiveAudiencePercentage, TvDependencyReduction, MerchandiseMultiplier,
                    TicketSalesStability, TalentSalaryReduction, TalentLoyaltyBonus,
                    HasGrowthCeiling, MaxSize, EstablishedAt, CeasedAt
                ) VALUES (
                    $profileId, $companyId, $isNicheFederation, $nicheType,
                    $captiveAudiencePercentage, $tvDependencyReduction, $merchandiseMultiplier,
                    $ticketSalesStability, $talentSalaryReduction, $talentLoyaltyBonus,
                    $hasGrowthCeiling, $maxSize, $establishedAt, $ceasedAt
                )
                ON CONFLICT(CompanyId) DO UPDATE SET
                    IsNicheFederation = $isNicheFederation, NicheType = $nicheType,
                    CaptiveAudiencePercentage = $captiveAudiencePercentage,
                    TvDependencyReduction = $tvDependencyReduction,
                    MerchandiseMultiplier = $merchandiseMultiplier,
                    TicketSalesStability = $ticketSalesStability,
                    TalentSalaryReduction = $talentSalaryReduction,
                    TalentLoyaltyBonus = $talentLoyaltyBonus,
                    HasGrowthCeiling = $hasGrowthCeiling, MaxSize = $maxSize,
                    CeasedAt = $ceasedAt;";

            AjouterParametre(command, "$profileId", profile.ProfileId);
            AjouterParametre(command, "$companyId", profile.CompanyId);
            AjouterParametre(command, "$isNicheFederation", profile.IsNicheFederation ? 1 : 0);
            AjouterParametre(command, "$nicheType", profile.NicheType?.ToString());
            AjouterParametre(command, "$captiveAudiencePercentage", profile.CaptiveAudiencePercentage);
            AjouterParametre(command, "$tvDependencyReduction", profile.TvDependencyReduction);
            AjouterParametre(command, "$merchandiseMultiplier", profile.MerchandiseMultiplier);
            AjouterParametre(command, "$ticketSalesStability", profile.TicketSalesStability);
            AjouterParametre(command, "$talentSalaryReduction", profile.TalentSalaryReduction);
            AjouterParametre(command, "$talentLoyaltyBonus", profile.TalentLoyaltyBonus);
            AjouterParametre(command, "$hasGrowthCeiling", profile.HasGrowthCeiling ? 1 : 0);
            AjouterParametre(command, "$maxSize", profile.MaxSize?.ToString());
            AjouterParametre(command, "$establishedAt", profile.EstablishedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            AjouterParametre(command, "$ceasedAt", profile.CeasedAt?.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<NicheFederationProfile?> GetNicheFederationProfileByCompanyIdAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT ProfileId, CompanyId, IsNicheFederation, NicheType,
                       CaptiveAudiencePercentage, TvDependencyReduction, MerchandiseMultiplier,
                       TicketSalesStability, TalentSalaryReduction, TalentLoyaltyBonus,
                       HasGrowthCeiling, MaxSize, EstablishedAt, CeasedAt
                FROM NicheFederationProfiles
                WHERE CompanyId = $companyId
                LIMIT 1;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapNicheFederationProfile(reader);
            }

            return null;
        });
    }

    public async Task<IReadOnlyList<NicheFederationProfile>> GetActiveNicheFederationsAsync()
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT ProfileId, CompanyId, IsNicheFederation, NicheType,
                       CaptiveAudiencePercentage, TvDependencyReduction, MerchandiseMultiplier,
                       TicketSalesStability, TalentSalaryReduction, TalentLoyaltyBonus,
                       HasGrowthCeiling, MaxSize, EstablishedAt, CeasedAt
                FROM NicheFederationProfiles
                WHERE IsNicheFederation = 1 AND CeasedAt IS NULL
                ORDER BY EstablishedAt DESC;";

            using var reader = command.ExecuteReader();
            var results = new List<NicheFederationProfile>();

            while (reader.Read())
            {
                results.Add(MapNicheFederationProfile(reader));
            }

            return results;
        });
    }

    public async Task<IReadOnlyList<NicheFederationProfile>> GetNicheFederationsByTypeAsync(string nicheType)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT ProfileId, CompanyId, IsNicheFederation, NicheType,
                       CaptiveAudiencePercentage, TvDependencyReduction, MerchandiseMultiplier,
                       TicketSalesStability, TalentSalaryReduction, TalentLoyaltyBonus,
                       HasGrowthCeiling, MaxSize, EstablishedAt, CeasedAt
                FROM NicheFederationProfiles
                WHERE NicheType = $nicheType AND IsNicheFederation = 1
                ORDER BY EstablishedAt DESC;";

            AjouterParametre(command, "$nicheType", nicheType);

            using var reader = command.ExecuteReader();
            var results = new List<NicheFederationProfile>();

            while (reader.Read())
            {
                results.Add(MapNicheFederationProfile(reader));
            }

            return results;
        });
    }

    public async Task UpdateNicheFederationProfileAsync(NicheFederationProfile profile)
    {
        await SaveNicheFederationProfileAsync(profile); // Utilise l'upsert
    }

    public async Task DeleteNicheFederationProfileAsync(string companyId)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = "DELETE FROM NicheFederationProfiles WHERE CompanyId = $companyId;";
            AjouterParametre(command, "$companyId", companyId);
            command.ExecuteNonQuery();
        });
    }

    private static NicheFederationProfile MapNicheFederationProfile(SqliteDataReader reader)
    {
        return new NicheFederationProfile
        {
            ProfileId = reader.GetString(0),
            CompanyId = reader.GetString(1),
            IsNicheFederation = reader.GetInt32(2) == 1,
            NicheType = reader.IsDBNull(3) ? null : Enum.Parse<NicheType>(reader.GetString(3)),
            CaptiveAudiencePercentage = reader.GetDouble(4),
            TvDependencyReduction = reader.GetDouble(5),
            MerchandiseMultiplier = reader.GetDouble(6),
            TicketSalesStability = reader.GetDouble(7),
            TalentSalaryReduction = reader.GetDouble(8),
            TalentLoyaltyBonus = reader.GetDouble(9),
            HasGrowthCeiling = reader.GetInt32(10) == 1,
            MaxSize = reader.IsDBNull(11) ? null : Enum.Parse<CompanySize>(reader.GetString(11)),
            EstablishedAt = DateTime.Parse(reader.GetString(12)),
            CeasedAt = reader.IsDBNull(13) ? null : DateTime.Parse(reader.GetString(13))
        };
    }
}
