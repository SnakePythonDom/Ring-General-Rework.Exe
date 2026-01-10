using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Trends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de génération et gestion des tendances
/// </summary>
public class TrendEngine
{
    private readonly ITrendRepository _trendRepository;
    private readonly System.Random _random;

    public TrendEngine(ITrendRepository trendRepository)
    {
        _trendRepository = trendRepository ?? throw new ArgumentNullException(nameof(trendRepository));
        _random = new System.Random();
    }

    /// <summary>
    /// Génère une nouvelle tendance procédurale
    /// </summary>
    public Trend GenerateTrend(TrendCategory category, TrendType type, string? region = null)
    {
        var trendId = Guid.NewGuid().ToString("N");
        var name = GenerateTrendName(category, type);
        var description = GenerateTrendDescription(category, type, name);

        // Générer les affinités stylistiques basées sur la catégorie
        var (hardcore, technical, lucha, entertainment, strongStyle) = GenerateStyleAffinities(category);

        // Durée et intensité aléatoires mais réalistes
        var intensity = _random.Next(40, 100);
        var durationWeeks = _random.Next(12, 52); // 3 mois à 1 an

        // Pénétration du marché selon le type
        var marketPenetration = type switch
        {
            TrendType.Global => _random.Next(60, 100),
            TrendType.Regional => _random.Next(30, 70),
            TrendType.Local => _random.Next(10, 40),
            _ => 50
        };

        var affectedRegions = type == TrendType.Global 
            ? new[] { "Global" }
            : new[] { region ?? "Unknown" };

        return new Trend
        {
            TrendId = trendId,
            Name = name,
            Type = type,
            Category = category,
            Description = description,
            HardcoreAffinity = hardcore,
            TechnicalAffinity = technical,
            LuchaAffinity = lucha,
            EntertainmentAffinity = entertainment,
            StrongStyleAffinity = strongStyle,
            StartDate = DateTime.Now,
            EndDate = null,
            Intensity = intensity,
            DurationWeeks = durationWeeks,
            MarketPenetration = marketPenetration,
            AffectedRegions = string.Join(",", affectedRegions),
            IsActive = true,
            CreatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Fait progresser les tendances actives (appelé chaque semaine)
    /// </summary>
    public async Task ProgressTrendsAsync()
    {
        var activeTrends = await _trendRepository.GetActiveTrendsAsync();

        foreach (var trend in activeTrends)
        {
            var progress = trend.GetProgress(DateTime.Now);

            // Si la tendance est terminée, la désactiver
            if (progress >= 100)
            {
                var updatedTrend = new Trend
                {
                    TrendId = trend.TrendId,
                    Name = trend.Name,
                    Type = trend.Type,
                    Category = trend.Category,
                    Description = trend.Description,
                    HardcoreAffinity = trend.HardcoreAffinity,
                    TechnicalAffinity = trend.TechnicalAffinity,
                    LuchaAffinity = trend.LuchaAffinity,
                    EntertainmentAffinity = trend.EntertainmentAffinity,
                    StrongStyleAffinity = trend.StrongStyleAffinity,
                    StartDate = trend.StartDate,
                    EndDate = DateTime.Now,
                    Intensity = trend.Intensity,
                    DurationWeeks = trend.DurationWeeks,
                    MarketPenetration = trend.MarketPenetration,
                    AffectedRegions = trend.AffectedRegions,
                    IsActive = false,
                    CreatedAt = trend.CreatedAt
                };

                await _trendRepository.UpdateTrendAsync(updatedTrend);
            }
        }
    }

    /// <summary>
    /// Génère des tendances aléatoires si nécessaire (appelé périodiquement)
    /// </summary>
    public async Task GenerateRandomTrendsIfNeededAsync()
    {
        var activeTrends = await _trendRepository.GetActiveTrendsAsync();

        // Générer une nouvelle tendance globale si moins de 2 actives
        if (activeTrends.Count(t => t.Type == TrendType.Global) < 2)
        {
            var newTrend = GenerateTrend(TrendCategory.Style, TrendType.Global);
            await _trendRepository.SaveTrendAsync(newTrend);
        }

        // 30% de chance de générer une tendance régionale
        if (_random.Next(100) < 30)
        {
            var regionalTrend = GenerateTrend(TrendCategory.Format, TrendType.Regional);
            await _trendRepository.SaveTrendAsync(regionalTrend);
        }
    }

    // ====================================================================
    // PRIVATE HELPER METHODS
    // ====================================================================

    private string GenerateTrendName(TrendCategory category, TrendType type)
    {
        var names = category switch
        {
            TrendCategory.Style => new[] { "Lucha Boom", "Strong Style Era", "Technical Revolution", "Hardcore Revival", "Entertainment Wave" },
            TrendCategory.Format => new[] { "Tournament Era", "PPV Focus", "Weekly Wars", "Streaming Revolution", "Crowd Interaction" },
            TrendCategory.Audience => new[] { "Family-Friendly Boom", "Hardcore Revival", "Mainstream Push", "Indie Explosion", "International Expansion" },
            _ => new[] { "New Trend", "Market Shift", "Industry Change" }
        };

        return names[_random.Next(names.Length)];
    }

    private string GenerateTrendDescription(TrendCategory category, TrendType type, string name)
    {
        var typeDesc = type switch
        {
            TrendType.Global => "mondiale",
            TrendType.Regional => "régionale",
            TrendType.Local => "locale",
            _ => ""
        };

        return $"Tendance {typeDesc} : {name}. Cette tendance affecte l'industrie du catch et influence les préférences du public.";
    }

    private (double hardcore, double technical, double lucha, double entertainment, double strongStyle) 
        GenerateStyleAffinities(TrendCategory category)
    {
        return category switch
        {
            TrendCategory.Style => GenerateStyleBasedAffinities(),
            TrendCategory.Format => (20, 30, 20, 30, 0), // Format neutre
            TrendCategory.Audience => (10, 20, 15, 55, 0), // Focus entertainment
            _ => (20, 20, 20, 20, 20) // Équilibré
        };
    }

    private (double hardcore, double technical, double lucha, double entertainment, double strongStyle) 
        GenerateStyleBasedAffinities()
    {
        // Choisir un style dominant aléatoirement
        var dominantStyle = _random.Next(5);
        
        return dominantStyle switch
        {
            0 => (80 + _random.Next(20), 10 + _random.Next(10), 5, 5, 0), // Hardcore
            1 => (5, 80 + _random.Next(20), 5, 10 + _random.Next(10), 0), // Technical
            2 => (5, 10, 80 + _random.Next(20), 5, 0), // Lucha
            3 => (5, 10, 5, 80 + _random.Next(20), 0), // Entertainment
            4 => (10, 30, 5, 5, 50 + _random.Next(30)), // StrongStyle
            _ => (20, 20, 20, 20, 20)
        };
    }
}
