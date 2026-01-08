using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models.Attributes;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository for managing worker performance attributes (30 attributes total).
/// Handles InRing, Entertainment, and Story attributes with full CRUD operations.
/// </summary>
public sealed class WorkerAttributesRepository : RepositoryBase, IWorkerAttributesRepository
{
    public WorkerAttributesRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    // ====================================================================
    // IN-RING ATTRIBUTES (10 attributes)
    // ====================================================================

    public WorkerInRingAttributes? GetInRingAttributes(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT WorkerId, Striking, Grappling, HighFlying, Powerhouse,
                   Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl
            FROM WorkerInRingAttributes
            WHERE WorkerId = $workerId";

        AjouterParametre(command, "$workerId", workerId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerInRingAttributes
            {
                WorkerId = reader.GetInt32(0),
                Striking = reader.GetInt32(1),
                Grappling = reader.GetInt32(2),
                HighFlying = reader.GetInt32(3),
                Powerhouse = reader.GetInt32(4),
                Timing = reader.GetInt32(5),
                Selling = reader.GetInt32(6),
                Psychology = reader.GetInt32(7),
                Stamina = reader.GetInt32(8),
                Safety = reader.GetInt32(9),
                HardcoreBrawl = reader.GetInt32(10)
            };
        }

        return null;
    }

    public void SaveInRingAttributes(WorkerInRingAttributes attributes)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO WorkerInRingAttributes
            (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing,
             Selling, Psychology, Stamina, Safety, HardcoreBrawl)
            VALUES ($workerId, $striking, $grappling, $highFlying, $powerhouse, $timing,
                    $selling, $psychology, $stamina, $safety, $hardcoreBrawl)";

        AjouterParametre(command, "$workerId", attributes.WorkerId);
        AjouterParametre(command, "$striking", attributes.Striking);
        AjouterParametre(command, "$grappling", attributes.Grappling);
        AjouterParametre(command, "$highFlying", attributes.HighFlying);
        AjouterParametre(command, "$powerhouse", attributes.Powerhouse);
        AjouterParametre(command, "$timing", attributes.Timing);
        AjouterParametre(command, "$selling", attributes.Selling);
        AjouterParametre(command, "$psychology", attributes.Psychology);
        AjouterParametre(command, "$stamina", attributes.Stamina);
        AjouterParametre(command, "$safety", attributes.Safety);
        AjouterParametre(command, "$hardcoreBrawl", attributes.HardcoreBrawl);

        command.ExecuteNonQuery();
    }

    public void UpdateInRingAttribute(int workerId, string attributeName, int value)
    {
        // Validate attribute name to prevent SQL injection
        var validAttributes = new[] { "Striking", "Grappling", "HighFlying", "Powerhouse",
                                      "Timing", "Selling", "Psychology", "Stamina", "Safety", "HardcoreBrawl" };

        if (!validAttributes.Contains(attributeName))
            throw new ArgumentException($"Invalid attribute name: {attributeName}");

        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = $"UPDATE WorkerInRingAttributes SET {attributeName} = $value WHERE WorkerId = $workerId";

        AjouterParametre(command, "$value", Math.Clamp(value, 0, 100));
        AjouterParametre(command, "$workerId", workerId);

        command.ExecuteNonQuery();
    }

    // ====================================================================
    // ENTERTAINMENT ATTRIBUTES (10 attributes)
    // ====================================================================

    public WorkerEntertainmentAttributes? GetEntertainmentAttributes(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower,
                   Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential
            FROM WorkerEntertainmentAttributes
            WHERE WorkerId = $workerId";

        AjouterParametre(command, "$workerId", workerId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerEntertainmentAttributes
            {
                WorkerId = reader.GetInt32(0),
                Charisma = reader.GetInt32(1),
                MicWork = reader.GetInt32(2),
                Acting = reader.GetInt32(3),
                CrowdConnection = reader.GetInt32(4),
                StarPower = reader.GetInt32(5),
                Improvisation = reader.GetInt32(6),
                Entrance = reader.GetInt32(7),
                SexAppeal = reader.GetInt32(8),
                MerchandiseAppeal = reader.GetInt32(9),
                CrossoverPotential = reader.GetInt32(10)
            };
        }

        return null;
    }

    public void SaveEntertainmentAttributes(WorkerEntertainmentAttributes attributes)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO WorkerEntertainmentAttributes
            (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower,
             Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential)
            VALUES ($workerId, $charisma, $micWork, $acting, $crowdConnection, $starPower,
                    $improvisation, $entrance, $sexAppeal, $merchandiseAppeal, $crossoverPotential)";

        AjouterParametre(command, "$workerId", attributes.WorkerId);
        AjouterParametre(command, "$charisma", attributes.Charisma);
        AjouterParametre(command, "$micWork", attributes.MicWork);
        AjouterParametre(command, "$acting", attributes.Acting);
        AjouterParametre(command, "$crowdConnection", attributes.CrowdConnection);
        AjouterParametre(command, "$starPower", attributes.StarPower);
        AjouterParametre(command, "$improvisation", attributes.Improvisation);
        AjouterParametre(command, "$entrance", attributes.Entrance);
        AjouterParametre(command, "$sexAppeal", attributes.SexAppeal);
        AjouterParametre(command, "$merchandiseAppeal", attributes.MerchandiseAppeal);
        AjouterParametre(command, "$crossoverPotential", attributes.CrossoverPotential);

        command.ExecuteNonQuery();
    }

    public void UpdateEntertainmentAttribute(int workerId, string attributeName, int value)
    {
        var validAttributes = new[] { "Charisma", "MicWork", "Acting", "CrowdConnection", "StarPower",
                                      "Improvisation", "Entrance", "SexAppeal", "MerchandiseAppeal", "CrossoverPotential" };

        if (!validAttributes.Contains(attributeName))
            throw new ArgumentException($"Invalid attribute name: {attributeName}");

        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = $"UPDATE WorkerEntertainmentAttributes SET {attributeName} = $value WHERE WorkerId = $workerId";

        AjouterParametre(command, "$value", Math.Clamp(value, 0, 100));
        AjouterParametre(command, "$workerId", workerId);

        command.ExecuteNonQuery();
    }

    // ====================================================================
    // STORY ATTRIBUTES (10 attributes)
    // ====================================================================

    public WorkerStoryAttributes? GetStoryAttributes(int workerId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance,
                   StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry,
                   CreativeInput, MoralAlignment
            FROM WorkerStoryAttributes
            WHERE WorkerId = $workerId";

        AjouterParametre(command, "$workerId", workerId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new WorkerStoryAttributes
            {
                WorkerId = reader.GetInt32(0),
                CharacterDepth = reader.GetInt32(1),
                Consistency = reader.GetInt32(2),
                HeelPerformance = reader.GetInt32(3),
                BabyfacePerformance = reader.GetInt32(4),
                StorytellingLongTerm = reader.GetInt32(5),
                EmotionalRange = reader.GetInt32(6),
                Adaptability = reader.GetInt32(7),
                RivalryChemistry = reader.GetInt32(8),
                CreativeInput = reader.GetInt32(9),
                MoralAlignment = reader.GetInt32(10)
            };
        }

        return null;
    }

    public void SaveStoryAttributes(WorkerStoryAttributes attributes)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO WorkerStoryAttributes
            (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance,
             StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry,
             CreativeInput, MoralAlignment)
            VALUES ($workerId, $characterDepth, $consistency, $heelPerformance, $babyfacePerformance,
                    $storytellingLongTerm, $emotionalRange, $adaptability, $rivalryChemistry,
                    $creativeInput, $moralAlignment)";

        AjouterParametre(command, "$workerId", attributes.WorkerId);
        AjouterParametre(command, "$characterDepth", attributes.CharacterDepth);
        AjouterParametre(command, "$consistency", attributes.Consistency);
        AjouterParametre(command, "$heelPerformance", attributes.HeelPerformance);
        AjouterParametre(command, "$babyfacePerformance", attributes.BabyfacePerformance);
        AjouterParametre(command, "$storytellingLongTerm", attributes.StorytellingLongTerm);
        AjouterParametre(command, "$emotionalRange", attributes.EmotionalRange);
        AjouterParametre(command, "$adaptability", attributes.Adaptability);
        AjouterParametre(command, "$rivalryChemistry", attributes.RivalryChemistry);
        AjouterParametre(command, "$creativeInput", attributes.CreativeInput);
        AjouterParametre(command, "$moralAlignment", attributes.MoralAlignment);

        command.ExecuteNonQuery();
    }

    public void UpdateStoryAttribute(int workerId, string attributeName, int value)
    {
        var validAttributes = new[] { "CharacterDepth", "Consistency", "HeelPerformance", "BabyfacePerformance",
                                      "StorytellingLongTerm", "EmotionalRange", "Adaptability", "RivalryChemistry",
                                      "CreativeInput", "MoralAlignment" };

        if (!validAttributes.Contains(attributeName))
            throw new ArgumentException($"Invalid attribute name: {attributeName}");

        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = $"UPDATE WorkerStoryAttributes SET {attributeName} = $value WHERE WorkerId = $workerId";

        AjouterParametre(command, "$value", Math.Clamp(value, 0, 100));
        AjouterParametre(command, "$workerId", workerId);

        command.ExecuteNonQuery();
    }

    // ====================================================================
    // BULK OPERATIONS
    // ====================================================================

    public (WorkerInRingAttributes? InRing, WorkerEntertainmentAttributes? Entertainment, WorkerStoryAttributes? Story) GetAllAttributes(int workerId)
    {
        return (
            GetInRingAttributes(workerId),
            GetEntertainmentAttributes(workerId),
            GetStoryAttributes(workerId)
        );
    }

    public void InitializeDefaultAttributes(int workerId)
    {
        WithTransaction((conn, trans) =>
        {
            // Initialize InRing attributes (all 50)
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO WorkerInRingAttributes (WorkerId)
                    VALUES ($workerId)";
                AjouterParametre(cmd, "$workerId", workerId);
                cmd.ExecuteNonQuery();
            }

            // Initialize Entertainment attributes (all 50)
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO WorkerEntertainmentAttributes (WorkerId)
                    VALUES ($workerId)";
                AjouterParametre(cmd, "$workerId", workerId);
                cmd.ExecuteNonQuery();
            }

            // Initialize Story attributes (all 50)
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO WorkerStoryAttributes (WorkerId)
                    VALUES ($workerId)";
                AjouterParametre(cmd, "$workerId", workerId);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public void DeleteAllAttributes(int workerId)
    {
        WithTransaction((conn, trans) =>
        {
            // Delete InRing attributes
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = "DELETE FROM WorkerInRingAttributes WHERE WorkerId = $workerId";
                AjouterParametre(cmd, "$workerId", workerId);
                cmd.ExecuteNonQuery();
            }

            // Delete Entertainment attributes
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = "DELETE FROM WorkerEntertainmentAttributes WHERE WorkerId = $workerId";
                AjouterParametre(cmd, "$workerId", workerId);
                cmd.ExecuteNonQuery();
            }

            // Delete Story attributes
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = trans;
                cmd.CommandText = "DELETE FROM WorkerStoryAttributes WHERE WorkerId = $workerId";
                AjouterParametre(cmd, "$workerId", workerId);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public List<int> GetWorkersByInRingAvg(int minAvg)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT WorkerId
            FROM WorkerInRingAttributes
            WHERE InRingAvg >= $minAvg
            ORDER BY InRingAvg DESC";

        AjouterParametre(command, "$minAvg", minAvg);

        var workers = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            workers.Add(reader.GetInt32(0));
        }

        return workers;
    }

    public List<int> GetWorkersByEntertainmentAvg(int minAvg)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT WorkerId
            FROM WorkerEntertainmentAttributes
            WHERE EntertainmentAvg >= $minAvg
            ORDER BY EntertainmentAvg DESC";

        AjouterParametre(command, "$minAvg", minAvg);

        var workers = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            workers.Add(reader.GetInt32(0));
        }

        return workers;
    }

    public List<int> GetWorkersByStoryAvg(int minAvg)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = @"
            SELECT WorkerId
            FROM WorkerStoryAttributes
            WHERE StoryAvg >= $minAvg
            ORDER BY StoryAvg DESC";

        AjouterParametre(command, "$minAvg", minAvg);

        var workers = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            workers.Add(reader.GetInt32(0));
        }

        return workers;
    }
}
