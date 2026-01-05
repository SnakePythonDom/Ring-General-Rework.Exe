using System.Text.Json;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using Xunit;

namespace RingGeneral.Tests;

public sealed class WorkerGenerationServiceTests
{
    [Fact]
    public void GenerateWeekly_ResteDeterministe_AvecMemeSeed()
    {
        var spec = ChargerSpec();
        var state = ConstruireEtat();
        var service = new WorkerGenerationService(new SeededRandomProvider(123), spec);

        var reportA = service.GenerateWeekly(state, 99);
        var reportB = service.GenerateWeekly(state, 99);

        var projectionA = reportA.Workers.Select(worker => new { worker.Prenom, worker.Nom, worker.Age, worker.Specialite }).ToList();
        var projectionB = reportB.Workers.Select(worker => new { worker.Prenom, worker.Nom, worker.Age, worker.Specialite }).ToList();

        Assert.Equal(projectionA, projectionB);
    }

    [Fact]
    public void GenerateWeekly_RespecteCooldown_Annuel()
    {
        var spec = ChargerSpec();
        var options = new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, 1);
        var youth = new YouthStructureState(
            "YOUTH-001",
            "Academy",
            "COMP-001",
            "FR",
            "ACADEMY",
            80000,
            24,
            3,
            12,
            "HYBRIDE",
            true,
            1,
            5);
        var state = new GameState(1, "COMP-001", "FR", options, new[] { youth }, new GenerationCounters(1, 0, new Dictionary<string, int>(), new Dictionary<string, int>(), 0, new Dictionary<string, int>()));

        var service = new WorkerGenerationService(new SeededRandomProvider(7), spec);
        var report = service.GenerateWeekly(state, 7);

        Assert.Empty(report.Workers);
    }

    [Fact]
    public void GenerateWeekly_NeGenerePasHorsSemainePivot()
    {
        var spec = ChargerSpec();
        var options = new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, 1);
        var youth = new YouthStructureState(
            "YOUTH-001",
            "Academy",
            "COMP-001",
            "FR",
            "ACADEMY",
            80000,
            24,
            3,
            12,
            "HYBRIDE",
            true,
            null,
            5);
        var state = new GameState(2, "COMP-001", "FR", options, new[] { youth }, new GenerationCounters(1, 0, new Dictionary<string, int>(), new Dictionary<string, int>(), 0, new Dictionary<string, int>()));

        var service = new WorkerGenerationService(new SeededRandomProvider(7), spec);
        var report = service.GenerateWeekly(state, 7);

        Assert.Empty(report.Workers);
    }

    [Fact]
    public void GenerateWeekly_RespecteCapGlobal()
    {
        var spec = ChargerSpec();
        var options = new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, 1);
        var youth = new YouthStructureState(
            "YOUTH-001",
            "Academy",
            "COMP-001",
            "FR",
            "ACADEMY",
            80000,
            24,
            3,
            12,
            "HYBRIDE",
            true,
            null,
            5);
        var counters = new GenerationCounters(1, spec.Caps.Trainees.GlobalAnnuel, new Dictionary<string, int>(), new Dictionary<string, int>(), 0, new Dictionary<string, int>());
        var state = new GameState(1, "COMP-001", "FR", options, new[] { youth }, counters);

        var service = new WorkerGenerationService(new SeededRandomProvider(7), spec);
        var report = service.GenerateWeekly(state, 7);

        Assert.Empty(report.Workers);
        Assert.Contains(report.ResultatsStructures, result => result.Raison == "Cap mondial atteint.");
    }

    [Fact]
    public void GenerateWeekly_RespecteCapParStructure()
    {
        var spec = ChargerSpec();
        var options = new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, 1);
        var youth = new YouthStructureState(
            "YOUTH-002",
            "Performance Center",
            "COMP-001",
            "FR",
            "PERFORMANCE_CENTER",
            250000,
            24,
            5,
            18,
            "HYBRIDE",
            true,
            null,
            0);
        var state = new GameState(1, "COMP-001", "FR", options, new[] { youth }, new GenerationCounters(1, 0, new Dictionary<string, int>(), new Dictionary<string, int>(), 0, new Dictionary<string, int>()));

        var service = new WorkerGenerationService(new SeededRandomProvider(21), spec);
        var report = service.GenerateWeekly(state, 21);

        var generated = report.ResultatsStructures.FirstOrDefault(result => result.YouthId == youth.YouthId);
        Assert.NotNull(generated);
        Assert.True(generated!.NombreGeneres <= spec.Caps.Trainees.ParStructure.MaxParPeriode);
    }

    [Fact]
    public void EnregistrerGeneration_CreeWorkersEtLiens()
    {
        var spec = ChargerSpec();
        var dbPath = Path.Combine(Path.GetTempPath(), $"ringgeneral-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var repository = new GameRepository(factory);
            repository.Initialiser();

            var options = repository.ChargerParametresGeneration();
            var show = repository.ChargerShowDefinition("SHOW-001");
            var structures = repository.ChargerYouthStructuresPourGeneration();
            var counters = repository.ChargerGenerationCounters(1);
            var state = new GameState(show.Semaine, show.CompagnieId, show.Region, options, structures, counters);

            var service = new WorkerGenerationService(new SeededRandomProvider(42), spec);
            var report = service.GenerateWeekly(state, 42);

            repository.EnregistrerGeneration(report);

            using var connexion = factory.OuvrirConnexion();
            using var workersCommand = connexion.CreateCommand();
            workersCommand.CommandText = "SELECT COUNT(1) FROM workers WHERE type_worker = 'TRAINEE';";
            var workersCount = Convert.ToInt32(workersCommand.ExecuteScalar());

            using var attrsCommand = connexion.CreateCommand();
            attrsCommand.CommandText = "SELECT COUNT(1) FROM worker_attributes;";
            var attrsCount = Convert.ToInt32(attrsCommand.ExecuteScalar());

            using var youthCommand = connexion.CreateCommand();
            youthCommand.CommandText = "SELECT COUNT(1) FROM youth_trainees;";
            var youthCount = Convert.ToInt32(youthCommand.ExecuteScalar());

            Assert.True(workersCount > 0);
            Assert.True(attrsCount > 0);
            Assert.Equal(workersCount, youthCount);
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private static WorkerGenerationSpec ChargerSpec()
    {
        var chemin = Path.Combine(Directory.GetCurrentDirectory(), "specs", "simulation", "worker-generation.fr.json");
        var json = File.ReadAllText(chemin);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<WorkerGenerationSpec>(json, options)
               ?? throw new InvalidOperationException("Spec de génération introuvable.");
    }

    private static GameState ConstruireEtat()
    {
        var options = new WorkerGenerationOptions(YouthGenerationMode.Realiste, WorldGenerationMode.Desactivee, 1);
        var youth = new YouthStructureState(
            "YOUTH-001",
            "Academy",
            "COMP-001",
            "FR",
            "ACADEMY",
            80000,
            24,
            3,
            12,
            "HYBRIDE",
            true,
            null,
            5);
        var counters = new GenerationCounters(1, 0, new Dictionary<string, int>(), new Dictionary<string, int>(), 0, new Dictionary<string, int>());
        return new GameState(1, "COMP-001", "FR", options, new[] { youth }, counters);
    }
}
