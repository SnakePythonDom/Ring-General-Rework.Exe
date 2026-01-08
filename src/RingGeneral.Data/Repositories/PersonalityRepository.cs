using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Implémentation du repository pour les attributs mentaux et personnalités.
/// </summary>
public class PersonalityRepository : RepositoryBase, IPersonalityRepository
{
    private readonly IPersonalityEngine _personalityEngine;

    public PersonalityRepository(
        SqliteConnectionFactory factory,
        IPersonalityEngine personalityEngine)
        : base(factory)
    {
        _personalityEngine = personalityEngine;
    }

    // === Mental Attributes ===

    public async Task<MentalAttributes?> GetMentalAttributesAsync(string entityId, string entityType)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, EntityId, EntityType,
                       Professionalism, Ambition, Loyalty, Ego, Resilience,
                       Adaptability, Creativity, WorkEthic, SocialSkills, Temperament,
                       LastUpdated
                FROM MentalAttributes
                WHERE EntityId = $entityId AND EntityType = $entityType
                LIMIT 1;";

            AjouterParametre(command, "$entityId", entityId);
            AjouterParametre(command, "$entityType", entityType);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapMentalAttributes(reader);
            }

            return null;
        });
    }

    public async Task SaveMentalAttributesAsync(MentalAttributes attributes)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            // Upsert (INSERT OR REPLACE)
            command.CommandText = @"
                INSERT INTO MentalAttributes (
                    EntityId, EntityType,
                    Professionalism, Ambition, Loyalty, Ego, Resilience,
                    Adaptability, Creativity, WorkEthic, SocialSkills, Temperament,
                    LastUpdated
                ) VALUES (
                    $entityId, $entityType,
                    $professionalism, $ambition, $loyalty, $ego, $resilience,
                    $adaptability, $creativity, $workEthic, $socialSkills, $temperament,
                    $lastUpdated
                )
                ON CONFLICT(EntityId, EntityType) DO UPDATE SET
                    Professionalism = $professionalism,
                    Ambition = $ambition,
                    Loyalty = $loyalty,
                    Ego = $ego,
                    Resilience = $resilience,
                    Adaptability = $adaptability,
                    Creativity = $creativity,
                    WorkEthic = $workEthic,
                    SocialSkills = $socialSkills,
                    Temperament = $temperament,
                    LastUpdated = $lastUpdated;";

            AjouterParametre(command, "$entityId", attributes.EntityId);
            AjouterParametre(command, "$entityType", attributes.EntityType);
            AjouterParametre(command, "$professionalism", attributes.Professionalism);
            AjouterParametre(command, "$ambition", attributes.Ambition);
            AjouterParametre(command, "$loyalty", attributes.Loyalty);
            AjouterParametre(command, "$ego", attributes.Ego);
            AjouterParametre(command, "$resilience", attributes.Resilience);
            AjouterParametre(command, "$adaptability", attributes.Adaptability);
            AjouterParametre(command, "$creativity", attributes.Creativity);
            AjouterParametre(command, "$workEthic", attributes.WorkEthic);
            AjouterParametre(command, "$socialSkills", attributes.SocialSkills);
            AjouterParametre(command, "$temperament", attributes.Temperament);
            AjouterParametre(command, "$lastUpdated", attributes.LastUpdated.ToString("o"));

            command.ExecuteNonQuery();
        });
    }

    public async Task DeleteMentalAttributesAsync(string entityId, string entityType)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                DELETE FROM MentalAttributes
                WHERE EntityId = $entityId AND EntityType = $entityType;";

            AjouterParametre(command, "$entityId", entityId);
            AjouterParametre(command, "$entityType", entityType);

            command.ExecuteNonQuery();
        });
    }

    // === Personality ===

    public async Task<Personality?> GetPersonalityAsync(string entityId, string entityType)
    {
        return await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, EntityId, EntityType,
                       PersonalityLabel, SecondaryTrait1, SecondaryTrait2,
                       PreviousLabel, LabelChangedAt, LastUpdated
                FROM Personalities
                WHERE EntityId = $entityId AND EntityType = $entityType
                LIMIT 1;";

            AjouterParametre(command, "$entityId", entityId);
            AjouterParametre(command, "$entityType", entityType);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapPersonality(reader);
            }

            return null;
        });
    }

    public async Task SavePersonalityAsync(Personality personality)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO Personalities (
                    EntityId, EntityType,
                    PersonalityLabel, SecondaryTrait1, SecondaryTrait2,
                    PreviousLabel, LabelChangedAt, LastUpdated
                ) VALUES (
                    $entityId, $entityType,
                    $personalityLabel, $secondaryTrait1, $secondaryTrait2,
                    $previousLabel, $labelChangedAt, $lastUpdated
                )
                ON CONFLICT(EntityId, EntityType) DO UPDATE SET
                    PersonalityLabel = $personalityLabel,
                    SecondaryTrait1 = $secondaryTrait1,
                    SecondaryTrait2 = $secondaryTrait2,
                    PreviousLabel = $previousLabel,
                    LabelChangedAt = $labelChangedAt,
                    LastUpdated = $lastUpdated;";

            AjouterParametre(command, "$entityId", personality.EntityId);
            AjouterParametre(command, "$entityType", personality.EntityType);
            AjouterParametre(command, "$personalityLabel", personality.PersonalityLabel);
            AjouterParametre(command, "$secondaryTrait1", personality.SecondaryTrait1);
            AjouterParametre(command, "$secondaryTrait2", personality.SecondaryTrait2);
            AjouterParametre(command, "$previousLabel", personality.PreviousLabel);
            AjouterParametre(command, "$labelChangedAt", personality.LabelChangedAt?.ToString("o"));
            AjouterParametre(command, "$lastUpdated", personality.LastUpdated.ToString("o"));

            command.ExecuteNonQuery();
        });
    }

    public async Task LogPersonalityChangeAsync(PersonalityHistory history)
    {
        await Task.Run(() =>
        {
            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                INSERT INTO PersonalityHistory (
                    EntityId, EntityType, OldLabel, NewLabel, ChangeReason, ChangedAt
                ) VALUES (
                    $entityId, $entityType, $oldLabel, $newLabel, $changeReason, $changedAt
                );";

            AjouterParametre(command, "$entityId", history.EntityId);
            AjouterParametre(command, "$entityType", history.EntityType);
            AjouterParametre(command, "$oldLabel", history.OldLabel);
            AjouterParametre(command, "$newLabel", history.NewLabel);
            AjouterParametre(command, "$changeReason", history.ChangeReason);
            AjouterParametre(command, "$changedAt", history.ChangedAt.ToString("o"));

            command.ExecuteNonQuery();
        });
    }

    public async Task<List<PersonalityHistory>> GetPersonalityHistoryAsync(string entityId, string entityType)
    {
        return await Task.Run(() =>
        {
            var history = new List<PersonalityHistory>();

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT Id, EntityId, EntityType, OldLabel, NewLabel, ChangeReason, ChangedAt
                FROM PersonalityHistory
                WHERE EntityId = $entityId AND EntityType = $entityType
                ORDER BY ChangedAt DESC;";

            AjouterParametre(command, "$entityId", entityId);
            AjouterParametre(command, "$entityType", entityType);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                history.Add(MapPersonalityHistory(reader));
            }

            return history;
        });
    }

    // === Batch Operations ===

    public async Task<List<MentalAttributes>> GetAllMentalAttributesByCompanyAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            var results = new List<MentalAttributes>();

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            // Récupérer tous les workers de la compagnie
            command.CommandText = @"
                SELECT ma.Id, ma.EntityId, ma.EntityType,
                       ma.Professionalism, ma.Ambition, ma.Loyalty, ma.Ego, ma.Resilience,
                       ma.Adaptability, ma.Creativity, ma.WorkEthic, ma.SocialSkills, ma.Temperament,
                       ma.LastUpdated
                FROM MentalAttributes ma
                INNER JOIN Workers w ON ma.EntityId = w.Id
                WHERE w.CompanyId = $companyId;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(MapMentalAttributes(reader));
            }

            return results;
        });
    }

    public async Task<List<Personality>> GetAllPersonalitiesByCompanyAsync(string companyId)
    {
        return await Task.Run(() =>
        {
            var results = new List<Personality>();

            using var connexion = OpenConnection();
            using var command = connexion.CreateCommand();

            command.CommandText = @"
                SELECT p.Id, p.EntityId, p.EntityType,
                       p.PersonalityLabel, p.SecondaryTrait1, p.SecondaryTrait2,
                       p.PreviousLabel, p.LabelChangedAt, p.LastUpdated
                FROM Personalities p
                INNER JOIN Workers w ON p.EntityId = w.Id
                WHERE w.CompanyId = $companyId;";

            AjouterParametre(command, "$companyId", companyId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(MapPersonality(reader));
            }

            return results;
        });
    }

    public async Task InitializePersonalitySystemAsync(string entityId, string entityType)
    {
        // Vérifier si déjà initialisé
        var existing = await GetMentalAttributesAsync(entityId, entityType);
        if (existing != null)
            return; // Déjà initialisé

        // Générer attributs mentaux aléatoires
        var mentalAttributes = _personalityEngine.GenerateRandomMentalAttributes();
        mentalAttributes.EntityId = entityId;
        mentalAttributes.EntityType = entityType;

        // Calculer label de personnalité
        var personalityLabel = _personalityEngine.CalculatePersonalityLabel(mentalAttributes);

        // Générer traits secondaires
        var secondaryTraits = _personalityEngine.GenerateSecondaryTraits(mentalAttributes);

        var personality = new Personality
        {
            EntityId = entityId,
            EntityType = entityType,
            PersonalityLabel = personalityLabel,
            SecondaryTrait1 = secondaryTraits.Count > 0 ? secondaryTraits[0] : null,
            SecondaryTrait2 = secondaryTraits.Count > 1 ? secondaryTraits[1] : null,
            LastUpdated = DateTime.Now
        };

        // Sauvegarder
        await SaveMentalAttributesAsync(mentalAttributes);
        await SavePersonalityAsync(personality);
    }

    // === Mappers ===

    private static MentalAttributes MapMentalAttributes(SqliteDataReader reader)
    {
        return new MentalAttributes
        {
            Id = reader.GetInt32(0),
            EntityId = reader.GetString(1),
            EntityType = reader.GetString(2),
            Professionalism = reader.GetInt32(3),
            Ambition = reader.GetInt32(4),
            Loyalty = reader.GetInt32(5),
            Ego = reader.GetInt32(6),
            Resilience = reader.GetInt32(7),
            Adaptability = reader.GetInt32(8),
            Creativity = reader.GetInt32(9),
            WorkEthic = reader.GetInt32(10),
            SocialSkills = reader.GetInt32(11),
            Temperament = reader.GetInt32(12),
            LastUpdated = DateTime.Parse(reader.GetString(13))
        };
    }

    private static Personality MapPersonality(SqliteDataReader reader)
    {
        return new Personality
        {
            Id = reader.GetInt32(0),
            EntityId = reader.GetString(1),
            EntityType = reader.GetString(2),
            PersonalityLabel = reader.GetString(3),
            SecondaryTrait1 = reader.IsDBNull(4) ? null : reader.GetString(4),
            SecondaryTrait2 = reader.IsDBNull(5) ? null : reader.GetString(5),
            PreviousLabel = reader.IsDBNull(6) ? null : reader.GetString(6),
            LabelChangedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
            LastUpdated = DateTime.Parse(reader.GetString(8))
        };
    }

    private static PersonalityHistory MapPersonalityHistory(SqliteDataReader reader)
    {
        return new PersonalityHistory
        {
            Id = reader.GetInt32(0),
            EntityId = reader.GetString(1),
            EntityType = reader.GetString(2),
            OldLabel = reader.GetString(3),
            NewLabel = reader.GetString(4),
            ChangeReason = reader.IsDBNull(5) ? null : reader.GetString(5),
            ChangedAt = DateTime.Parse(reader.GetString(6))
        };
    }
}
