using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Attributes;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Services;

public sealed class DatabaseGeneratorService : IDatabaseGeneratorService
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public DatabaseGeneratorService(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task EnsureDatabaseSchemaAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // 1. Run Migrations / Schema Checks
        EnsureSchema(connection);

        // 2. Seed Data
        SeedCompany(connection);
        SeedWorkers(connection);
        SeedTitles(connection);
        SeedYouthStructures(connection);
        SeedSegmentTemplates(connection);

        await Task.CompletedTask;
    }

    public async Task<bool> NeedsRepairAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // Check for specific columns or tables
        if (!TableExists(connection, "YouthStructures")) return true;
        if (!ColumnExists(connection, "WorkerEntertainmentAttributes", "Aura")) return true;
        if (!ColumnExists(connection, "Titles", "CompanyId")) return true;

        // Check data counts
        if (GetCount(connection, "Companies") == 0) return true;
        if (GetCount(connection, "Workers") < 20) return true;
        if (GetCount(connection, "Titles") < 5) return true;
        if (GetCount(connection, "YouthStructures") == 0) return true;

        return false;
    }

    private void EnsureSchema(SqliteConnection connection)
    {
        // Execute the migration script content directly or rely on checks
        // For robustness, we perform explicit checks and fixes here (The "Annealing" process)

        // 1. Check Aura
        if (!ColumnExists(connection, "WorkerEntertainmentAttributes", "Aura"))
        {
            ExecuteSql(connection, "ALTER TABLE WorkerEntertainmentAttributes ADD COLUMN Aura INTEGER NOT NULL DEFAULT 50;");
        }

        // 2. Check YouthStructures
        if (!TableExists(connection, "YouthStructures"))
        {
            ExecuteSql(connection, @"
                CREATE TABLE YouthStructures (
                    YouthId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    CompanyId TEXT NOT NULL,
                    Region TEXT NOT NULL,
                    Type TEXT NOT NULL DEFAULT 'Dojo',
                    BudgetAnnual INTEGER NOT NULL DEFAULT 0,
                    MaxCapacity INTEGER NOT NULL DEFAULT 10,
                    EquipmentLevel INTEGER NOT NULL DEFAULT 50,
                    CoachingQuality INTEGER NOT NULL DEFAULT 50,
                    Philosophy TEXT NOT NULL DEFAULT 'Balanced',
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    LastGraduationWeek INTEGER,
                    Level INTEGER NOT NULL DEFAULT 1,
                    ActiveTraineesCount INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );");
        }

        // 3. Check Titles.CompanyId
        if (!ColumnExists(connection, "Titles", "CompanyId"))
        {
             ExecuteSql(connection, "ALTER TABLE Titles ADD COLUMN CompanyId TEXT REFERENCES Companies(CompanyId);");
        }

        // 4. Check SegmentTemplates
        if (!TableExists(connection, "SegmentTemplates"))
        {
            ExecuteSql(connection, @"
                CREATE TABLE SegmentTemplates (
                    TemplateId TEXT PRIMARY KEY,
                    Nom TEXT NOT NULL,
                    TypeSegment TEXT NOT NULL,
                    DureeMinutes INTEGER NOT NULL,
                    EstMainEvent INTEGER NOT NULL DEFAULT 0,
                    Intensite INTEGER NOT NULL DEFAULT 50,
                    MatchTypeId TEXT
                );");
        }
    }

    private void SeedCompany(SqliteConnection connection)
    {
        var count = GetCount(connection, "Companies");
        if (count == 0)
        {
            ExecuteSql(connection, @"
                INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, FoundedYear)
                VALUES ('COMPANY_001', 'Ring General Wrestling', 'USA', 'NorthAmerica', 50, 1000000, 2024);
            ");
        }
    }

    private void SeedWorkers(SqliteConnection connection)
    {
        var currentCount = GetCount(connection, "Workers");
        var needed = 20 - currentCount;

        if (needed > 0)
        {
            var random = new Random();
            for (int i = 0; i < needed; i++)
            {
                var workerId = Guid.NewGuid().ToString(); // Or integer based on schema.
                // Schema uses TEXT for IDs in some places but Worker model has int Id?
                // Wait, Worker.cs has `public int Id { get; set; }`.
                // DbInitializer schema says `WorkerId TEXT PRIMARY KEY`.
                // Let's stick to the schema: TEXT. But wait, Worker.cs says int.
                // RepositoryBase and existing code seems to mix?
                // Looking at WorkerRepository.GetWorker(int id), it takes int.
                // But `ChargerWorker` takes string workerId.
                // Let's check `001_init.sql` or `DbInitializer` content again.
                // `WorkerId TEXT PRIMARY KEY`.
                // `Worker.cs` mapping might be doing conversion or it's inconsistent.
                // I will use string IDs (numbers as strings) to be safe if it's integer-based logic, or Guids.
                // Given "001_init.sql" defines WorkerId as TEXT.
                // I'll use simple IDs like "WORKER_X" or numbers.

                // Actually, let's create high quality workers as requested.
                // I'll create a mix of generated names.
                var name = $"Worker {currentCount + i + 1}";
                var realName = $"John Doe {currentCount + i + 1}";

                // Use a proper ID format. Existing seed might use ints.
                // Let's generate a string ID.
                var id = (100 + currentCount + i).ToString();

                ExecuteSql(connection, $@"
                    INSERT INTO Workers (WorkerId, Name, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, RoleTv, CreatedAt)
                    VALUES ('{id}', '{name}', 'COMPANY_001', 'USA', 85, 85, 85, 80, 0, 'MidCard', CURRENT_TIMESTAMP);
                ");

                // Populate attributes
                // InRing
                ExecuteSql(connection, $@"
                    INSERT INTO WorkerInRingAttributes (WorkerId, Striking, Grappling, HighFlying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, HardcoreBrawl)
                    VALUES ('{id}', {random.Next(80, 96)}, {random.Next(80, 96)}, {random.Next(70, 90)}, {random.Next(70, 90)}, {random.Next(85, 99)}, {random.Next(85, 99)}, {random.Next(80, 95)}, {random.Next(80, 95)}, {random.Next(90, 100)}, {random.Next(50, 80)});
                ");

                // Entertainment (with Aura)
                ExecuteSql(connection, $@"
                    INSERT INTO WorkerEntertainmentAttributes (WorkerId, Charisma, MicWork, Acting, CrowdConnection, StarPower, Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential, Aura)
                    VALUES ('{id}', {random.Next(80, 96)}, {random.Next(80, 96)}, {random.Next(70, 95)}, {random.Next(85, 99)}, {random.Next(80, 95)}, {random.Next(75, 90)}, {random.Next(80, 95)}, {random.Next(50, 90)}, {random.Next(70, 95)}, {random.Next(60, 90)}, {random.Next(80, 99)});
                ");

                // Story
                ExecuteSql(connection, $@"
                    INSERT INTO WorkerStoryAttributes (WorkerId, CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance, StorytellingLongTerm, EmotionalRange, Adaptability, RivalryChemistry, CreativeInput, MoralAlignment)
                    VALUES ('{id}', {random.Next(70, 95)}, {random.Next(85, 100)}, {random.Next(70, 95)}, {random.Next(70, 95)}, {random.Next(80, 95)}, {random.Next(75, 95)}, {random.Next(70, 90)}, {random.Next(80, 95)}, {random.Next(60, 90)}, {random.Next(50, 100)});
                ");
            }
        }
        else
        {
            // Even if workers exist, ensure they have attribute tables populated
            var workerIds = new List<string>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT WorkerId FROM Workers";
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) workerIds.Add(reader.GetString(0));
            }

            foreach (var wid in workerIds)
            {
                 // Check and Insert InRing
                 if (GetCount(connection, $"WorkerInRingAttributes WHERE WorkerId = '{wid}'") == 0)
                     ExecuteSql(connection, $"INSERT INTO WorkerInRingAttributes (WorkerId) VALUES ('{wid}')");

                 // Check and Insert Entertainment
                 if (GetCount(connection, $"WorkerEntertainmentAttributes WHERE WorkerId = '{wid}'") == 0)
                     ExecuteSql(connection, $"INSERT INTO WorkerEntertainmentAttributes (WorkerId, Aura) VALUES ('{wid}', 50)"); // Ensure Aura has default

                 // Check and Insert Story
                 if (GetCount(connection, $"WorkerStoryAttributes WHERE WorkerId = '{wid}'") == 0)
                     ExecuteSql(connection, $"INSERT INTO WorkerStoryAttributes (WorkerId) VALUES ('{wid}')");
            }
        }
    }

    private void SeedTitles(SqliteConnection connection)
    {
        var count = GetCount(connection, "Titles");
        if (count == 0)
        {
            var titles = new[]
            {
                ("TITLE_001", "World Heavyweight Championship", 100),
                ("TITLE_002", "Intercontinental Championship", 80),
                ("TITLE_003", "Tag Team Championship", 75),
                ("TITLE_004", "Women's Championship", 85),
                ("TITLE_005", "Television Championship", 60)
            };

            foreach (var (id, name, prestige) in titles)
            {
                ExecuteSql(connection, $@"
                    INSERT INTO Titles (TitleId, Name, Prestige, CompanyId)
                    VALUES ('{id}', '{name}', {prestige}, 'COMPANY_001');
                ");
            }
        }
        else
        {
            // Update existing titles to have CompanyId if null
            ExecuteSql(connection, "UPDATE Titles SET CompanyId = 'COMPANY_001' WHERE CompanyId IS NULL OR CompanyId = '';");
        }
    }

    private void SeedYouthStructures(SqliteConnection connection)
    {
        var count = GetCount(connection, "YouthStructures");
        if (count == 0)
        {
            ExecuteSql(connection, @"
                INSERT INTO YouthStructures (YouthId, Name, CompanyId, Region, Type, BudgetAnnual, MaxCapacity, EquipmentLevel, CoachingQuality, Philosophy, Level, ActiveTraineesCount)
                VALUES ('YOUTH_001', 'Dojo Phoenix', 'COMPANY_001', 'NorthAmerica', 'Dojo', 50000, 20, 60, 70, 'Technical', 1, 0);
            ");
        }
    }

    private void SeedSegmentTemplates(SqliteConnection connection)
    {
        var count = GetCount(connection, "SegmentTemplates");
        if (count == 0)
        {
             ExecuteSql(connection, @"
                INSERT INTO SegmentTemplates (TemplateId, Nom, TypeSegment, DureeMinutes, EstMainEvent, Intensite)
                VALUES ('SEG_001', 'Standard Match', 'Match', 15, 0, 70);
            ");
             ExecuteSql(connection, @"
                INSERT INTO SegmentTemplates (TemplateId, Nom, TypeSegment, DureeMinutes, EstMainEvent, Intensite)
                VALUES ('SEG_002', 'Main Event Match', 'Match', 25, 1, 90);
            ");
             ExecuteSql(connection, @"
                INSERT INTO SegmentTemplates (TemplateId, Nom, TypeSegment, DureeMinutes, EstMainEvent, Intensite)
                VALUES ('SEG_003', 'Backstage Interview', 'Angle', 5, 0, 40);
            ");
        }
    }

    // Helpers
    private bool TableExists(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$tableName;";
        command.Parameters.AddWithValue("$tableName", tableName);
        return command.ExecuteScalar() != null;
    }

    private bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({tableName});";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private long GetCount(SqliteConnection connection, string tableName)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
            return (long)command.ExecuteScalar();
        }
        catch
        {
            return 0;
        }
    }

    private void ExecuteSql(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
