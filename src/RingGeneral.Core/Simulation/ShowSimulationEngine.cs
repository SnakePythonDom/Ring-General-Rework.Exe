using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class ShowSimulationEngine
{
    private readonly IRandomProvider _random;
    private readonly AudienceModel _audienceModel;
    private readonly DealRevenueModel _dealRevenueModel;
    
    // Services pour l'analyse structurelle et les tendances
    private readonly RingGeneral.Core.Interfaces.ITrendRepository? _trendRepository;
    private readonly RingGeneral.Core.Interfaces.IRosterAnalysisRepository? _rosterAnalysisRepository;
    private readonly RingGeneral.Core.Interfaces.INicheFederationRepository? _nicheRepository;

    public ShowSimulationEngine(
        IRandomProvider random,
        AudienceModel? audienceModel = null,
        DealRevenueModel? dealRevenueModel = null,
        RingGeneral.Core.Interfaces.ITrendRepository? trendRepository = null,
        RingGeneral.Core.Interfaces.IRosterAnalysisRepository? rosterAnalysisRepository = null,
        RingGeneral.Core.Interfaces.INicheFederationRepository? nicheRepository = null)
    {
        _random = random;
        _audienceModel = audienceModel ?? new AudienceModel();
        _dealRevenueModel = dealRevenueModel ?? new DealRevenueModel();
        _trendRepository = trendRepository;
        _rosterAnalysisRepository = rosterAnalysisRepository;
        _nicheRepository = nicheRepository;
    }

    public ShowSimulationResult Simuler(ShowContext context)
    {
        var heatModel = new HeatModel();
        var fatigueDelta = new Dictionary<string, int>();
        var blessures = new Dictionary<string, string>();
        var momentumDelta = new Dictionary<string, int>();
        var populariteWorkers = new Dictionary<string, int>();
        var populariteCompagnie = new Dictionary<string, int>();
        var storylineHeat = new Dictionary<string, int>();
        var titrePrestige = new Dictionary<string, int>();
        var finances = new List<FinanceTransaction>();
        var segmentsReports = new List<SegmentReport>();
        var storylinesUtilisees = new HashSet<string>();
        var storylineSegmentsCount = new Dictionary<string, int>();

        var crowdHeat = Math.Clamp((context.Compagnie.Prestige + context.Compagnie.AudienceMoyenne) / 2, 0, 100);
        var promoStreak = 0;
        var segmentsLents = 0;

        foreach (var segment in context.Segments)
        {
            var participants = context.Workers.Where(worker => segment.Participants.Contains(worker.WorkerId)).ToList();
            var inRing = participants.Count == 0 ? 0 : (int)Math.Round(participants.Average(worker => worker.InRing));
            var entertainment = participants.Count == 0 ? 0 : (int)Math.Round(participants.Average(worker => worker.Entertainment));
            var story = participants.Count == 0 ? 0 : (int)Math.Round(participants.Average(worker => worker.Story));

            if (segment.TypeSegment is not "match")
            {
                inRing = (int)Math.Round(inRing * 0.4);
                entertainment = (int)Math.Round(entertainment * 1.1);
            }

            if (!string.IsNullOrWhiteSpace(segment.StorylineId))
            {
                storylinesUtilisees.Add(segment.StorylineId);
                storylineSegmentsCount[segment.StorylineId] = storylineSegmentsCount.TryGetValue(segment.StorylineId, out var count)
                    ? count + 1
                    : 1;
                story += 6;
            }

            var chimieBonus = CalculerChimie(context, participants.Select(worker => worker.WorkerId).ToList());
            var pacingPenalty = 0;

            if (segment.TypeSegment is "promo" or "angle_backstage")
            {
                promoStreak++;
                if (promoStreak >= 2)
                {
                    pacingPenalty -= 5 * (promoStreak - 1);
                }
            }
            else
            {
                promoStreak = 0;
            }

            if (segment.Intensite <= 35)
            {
                segmentsLents++;
                if (segmentsLents >= 2)
                {
                    pacingPenalty -= 4;
                }
            }
            else
            {
                segmentsLents = 0;
            }

            var baseScore = (int)Math.Round((inRing + entertainment + story) / 3.0);
            var crowdBonus = (crowdHeat - 50) / 10;
            var moraleBonus = participants.Count == 0
                ? 0
                : (int)Math.Round((participants.Average(worker => worker.Morale) - 50) / 10.0);
            var note = Math.Clamp(baseScore + crowdBonus + pacingPenalty + chimieBonus, 0, 100);
            note = Math.Clamp(note + moraleBonus, 0, 100);

            // Bonus de compatibilité avec les tendances (Phase 6)
            var compatibilityBonus = CalculateCompatibilityBonus(context.Compagnie.CompagnieId);
            note = Math.Clamp(note + (int)compatibilityBonus, 0, 100);

            var events = new List<string>();
            if (segment.TypeSegment == "match")
            {
                var botchChance = Math.Clamp(0.08 + (segment.Intensite / 120.0) - (inRing / 200.0), 0.02, 0.25);
                if (_random.NextDouble() < botchChance)
                {
                    events.Add("Botch");
                    note = Math.Max(0, note - 6);
                }
            }
            else if (segment.TypeSegment is "promo" or "angle_backstage" or "interview")
            {
                var incidentChance = Math.Clamp(0.04 + (segment.Intensite / 180.0) - (entertainment / 220.0), 0.02, 0.15);
                if (_random.NextDouble() < incidentChance)
                {
                    events.Add("Incident backstage");
                    note = Math.Max(0, note - 4);
                }
            }

            var fatigueImpact = AppliquerFatigue(segment, participants, fatigueDelta);
            var blessuresSegment = DeterminerBlessures(segment, participants, fatigueImpact, blessures, events);
            var momentumImpact = AppliquerMomentum(segment, note, momentumDelta);
            var populariteImpact = AppliquerPopularite(participants, segment, note, populariteWorkers);
            var storylineImpact = AppliquerStorylineHeat(segment, note, storylineHeat, storylineSegmentsCount, heatModel);
            var titreImpact = AppliquerTitrePrestige(segment, note, titrePrestige);

            var facteurs = new List<SegmentBreakdownItem>
            {
                new("Chaleur du public", crowdBonus),
                new("Pacing", pacingPenalty),
                new("Chimie", chimieBonus),
                new("Morale", moraleBonus),
                new("Storyline", segment.StorylineId is null ? 0 : 4)
            };

            var impact = new SegmentImpact(
                new Dictionary<string, int>(fatigueImpact),
                new Dictionary<string, int>(momentumImpact),
                new Dictionary<string, int>(populariteImpact),
                new Dictionary<string, int>(storylineImpact),
                new Dictionary<string, int>(titreImpact),
                blessuresSegment);

            var report = new SegmentReport(
                segment.SegmentId,
                segment.TypeSegment,
                note,
                Math.Clamp(inRing, 0, 100),
                Math.Clamp(entertainment, 0, 100),
                Math.Clamp(story, 0, 100),
                crowdHeat,
                Math.Clamp(crowdHeat + (note - 50) / 2, 0, 100),
                pacingPenalty,
                chimieBonus,
                events,
                facteurs,
                impact);

            segmentsReports.Add(report);
            crowdHeat = report.CrowdHeatApres;
        }

        foreach (var storyline in context.Storylines)
        {
            if (!storylinesUtilisees.Contains(storyline.StorylineId))
            {
                var inactiveDelta = heatModel.CalculerDeltaInactif();
                storylineHeat[storyline.StorylineId] = storylineHeat.TryGetValue(storyline.StorylineId, out var total)
                    ? total + inactiveDelta
                    : inactiveDelta;
            }
        }

        var noteShow = segmentsReports.Count == 0 ? 0 : (int)Math.Round(segmentsReports.Average(segment => segment.Note));
        var populariteDeltaCompagnie = (noteShow - 50) / 5;
        populariteCompagnie[context.Compagnie.CompagnieId] = populariteDeltaCompagnie;

        var stars = CalculerStarPower(context);
        var saturation = CalculerSaturation(context, segmentsReports.Count);
        var reach = Math.Clamp(context.Compagnie.Reach + (context.DealTv?.ReachBonus ?? 0), 0, 100);
        var audienceDetails = _audienceModel.Evaluer(new AudienceInputs(reach, noteShow, stars, saturation));
        var audience = audienceDetails.Audience;

        var billetterie = Math.Round(1500.0 + audience * 75 + reach * 20, 2);
        var merch = Math.Round(300.0 + audience * 20, 2);
        var tv = 0.0;
        if (context.DealTv is not null)
        {
            var revenue = _dealRevenueModel.Calculer(context.DealTv, audienceDetails);
            tv = Math.Round(revenue.Revenue, 2);
        }
        else if (context.Show.DealTvId is not null)
        {
            tv = Math.Round(5000.0 + audience * 40, 2);
        }

        // Appliquer les bénéfices de niche (Phase 6)
        var nicheBenefits = CalculateNicheBenefits(context.Compagnie.CompagnieId);
        billetterie = Math.Round(billetterie * nicheBenefits.TicketStabilityMultiplier, 2);
        merch = Math.Round(merch * nicheBenefits.MerchandiseMultiplier, 2);
        tv = Math.Round(tv * (1.0 - nicheBenefits.TvDependencyReduction / 100.0), 2);

        finances.Add(new FinanceTransaction("billetterie", billetterie, "Billetterie"));
        finances.Add(new FinanceTransaction("merch", merch, "Merchandising"));
        if (tv > 0)
        {
            finances.Add(new FinanceTransaction("tv", tv, "Droits TV"));
        }

        var totalFinances = billetterie + merch + tv;
        var pointsCles = new List<string>
        {
            $"Note globale : {noteShow}",
            $"Audience estimée : {audience}",
            $"Star power : {stars} • Saturation {saturation}",
            $"Impact popularité : {populariteDeltaCompagnie:+#;-#;0}"
        };

        var rapportShow = new ShowReport(
            context.Show.ShowId,
            noteShow,
            audience,
            audienceDetails,
            billetterie,
            merch,
            tv,
            populariteDeltaCompagnie,
            segmentsReports,
            pointsCles);

        var delta = new GameStateDelta(
            new Dictionary<string, int>(fatigueDelta),
            new Dictionary<string, string>(blessures),
            new Dictionary<string, int>(momentumDelta),
            new Dictionary<string, int>(populariteWorkers),
            new Dictionary<string, int>(populariteCompagnie),
            new Dictionary<string, int>(storylineHeat),
            new Dictionary<string, int>(titrePrestige),
            finances);

        return new ShowSimulationResult(rapportShow, delta);
    }

    private static int CalculerStarPower(ShowContext context)
    {
        if (context.Workers.Count == 0)
        {
            return context.Compagnie.Prestige;
        }

        return (int)Math.Round(
            context.Workers
                .OrderByDescending(worker => worker.Popularite)
                .Take(3)
                .Average(worker => worker.Popularite));
    }

    private static int CalculerSaturation(ShowContext context, int segmentsCount)
    {
        var baseSaturation = (int)Math.Round(context.Compagnie.AudienceMoyenne * 0.6);
        var dureeImpact = (int)Math.Round(context.Show.DureeMinutes / 4.0);
        var segmentsImpact = segmentsCount * 2;
        return Math.Clamp(baseSaturation + dureeImpact + segmentsImpact, 0, 100);
    }

    private static int CalculerChimie(ShowContext context, IReadOnlyList<string> participants)
    {
        if (participants.Count < 2)
        {
            return 0;
        }

        var bonus = 0;
        for (var i = 0; i < participants.Count; i++)
        {
            for (var j = i + 1; j < participants.Count; j++)
            {
                var key = $"{participants[i]}|{participants[j]}";
                if (!context.Chimies.TryGetValue(key, out var chimie))
                {
                    key = $"{participants[j]}|{participants[i]}";
                }

                if (context.Chimies.TryGetValue(key, out chimie))
                {
                    bonus += chimie / 10;
                }
            }
        }

        return Math.Clamp(bonus, -8, 8);
    }

    private static Dictionary<string, int> AppliquerFatigue(
        SegmentDefinition segment,
        IReadOnlyList<WorkerSnapshot> participants,
        IDictionary<string, int> fatigueDelta)
    {
        var intensite = Math.Clamp(segment.Intensite, 10, 100);
        var multiplicateur = segment.TypeSegment == "match" ? 1.0 : 0.6;
        var deltaLocal = new Dictionary<string, int>();

        foreach (var participant in participants)
        {
            var delta = (int)Math.Round(segment.DureeMinutes * (intensite / 50.0) * multiplicateur);
            deltaLocal[participant.WorkerId] = delta;
            fatigueDelta[participant.WorkerId] = fatigueDelta.TryGetValue(participant.WorkerId, out var total) ? total + delta : delta;
        }

        return deltaLocal;
    }

    private List<string> DeterminerBlessures(
        SegmentDefinition segment,
        IReadOnlyList<WorkerSnapshot> participants,
        IReadOnlyDictionary<string, int> fatigueImpact,
        IDictionary<string, string> blessures,
        ICollection<string> events)
    {
        var blessuresSegment = new List<string>();
        var baseRisk = segment.TypeSegment == "match" ? 0.06 : 0.03;

        foreach (var participant in participants)
        {
            var fatigue = participant.Fatigue + (fatigueImpact.TryGetValue(participant.WorkerId, out var delta) ? delta : 0);
            var risque = baseRisk + (segment.Intensite / 150.0) + (fatigue / 250.0);
            if (_random.NextDouble() < risque)
            {
                var blessure = fatigue > 80 ? "MOYENNE" : "LEGERE";
                blessures[participant.WorkerId] = blessure;
                blessuresSegment.Add(participant.WorkerId);
                events.Add($"Blessure {participant.NomComplet}");
            }
        }

        return blessuresSegment;
    }

    private static Dictionary<string, int> AppliquerMomentum(
        SegmentDefinition segment,
        int note,
        IDictionary<string, int> momentumDelta)
    {
        var deltaLocal = new Dictionary<string, int>();
        if (segment.VainqueurId is null)
        {
            return deltaLocal;
        }

        var delta = 2 + (note - 50) / 20;
        momentumDelta[segment.VainqueurId] = momentumDelta.TryGetValue(segment.VainqueurId, out var total) ? total + delta : delta;
        deltaLocal[segment.VainqueurId] = delta;

        if (segment.PerdantId is not null)
        {
            var malus = -2 + (note - 50) / 25;
            momentumDelta[segment.PerdantId] = momentumDelta.TryGetValue(segment.PerdantId, out var totalLoss) ? totalLoss + malus : malus;
            deltaLocal[segment.PerdantId] = malus;
        }

        return deltaLocal;
    }

    private static Dictionary<string, int> AppliquerPopularite(
        IReadOnlyList<WorkerSnapshot> participants,
        SegmentDefinition segment,
        int note,
        IDictionary<string, int> populariteDelta)
    {
        var deltaLocal = new Dictionary<string, int>();
        if (participants.Count == 0)
        {
            return deltaLocal;
        }

        var deltaBase = (note - 50) / 10;
        foreach (var participant in participants)
        {
            var delta = deltaBase;
            if (segment.VainqueurId == participant.WorkerId)
            {
                delta += 2;
            }
            else if (segment.PerdantId == participant.WorkerId)
            {
                delta -= 2;
            }

            populariteDelta[participant.WorkerId] = populariteDelta.TryGetValue(participant.WorkerId, out var total)
                ? total + delta
                : delta;
            deltaLocal[participant.WorkerId] = delta;
        }

        return deltaLocal;
    }

    private static Dictionary<string, int> AppliquerStorylineHeat(
        SegmentDefinition segment,
        int noteSegment,
        IDictionary<string, int> storylineHeatDelta,
        IReadOnlyDictionary<string, int> storylineSegmentsCount,
        HeatModel heatModel)
    {
        var deltaLocal = new Dictionary<string, int>();
        if (segment.StorylineId is null)
        {
            return deltaLocal;
        }

        var segmentsPrecedents = storylineSegmentsCount.TryGetValue(segment.StorylineId, out var count)
            ? Math.Max(0, count - 1)
            : 0;
        var delta = heatModel.CalculerDeltaSegment(noteSegment, segmentsPrecedents);

        storylineHeatDelta[segment.StorylineId] = storylineHeatDelta.TryGetValue(segment.StorylineId, out var total)
            ? total + delta
            : delta;
        deltaLocal[segment.StorylineId] = delta;
        return deltaLocal;
    }

    private static Dictionary<string, int> AppliquerTitrePrestige(
        SegmentDefinition segment,
        int note,
        IDictionary<string, int> titrePrestigeDelta)
    {
        var deltaLocal = new Dictionary<string, int>();
        if (segment.TypeSegment != "match" || segment.TitreId is null)
        {
            return deltaLocal;
        }

        var delta = Math.Clamp((note - 50) / 15, -3, 4);
        titrePrestigeDelta[segment.TitreId] = titrePrestigeDelta.TryGetValue(segment.TitreId, out var total)
            ? total + delta
            : delta;
        deltaLocal[segment.TitreId] = delta;
        return deltaLocal;
    }

    /// <summary>
    /// Calcule le bonus de compatibilité avec les tendances actives
    /// </summary>
    private double CalculateCompatibilityBonus(string companyId)
    {
        if (_trendRepository == null || _rosterAnalysisRepository == null)
        {
            return 0;
        }

        try
        {
            var dna = _rosterAnalysisRepository.GetRosterDNAByCompanyIdAsync(companyId).Result;
            if (dna == null) return 0;

            var activeTrends = _trendRepository.GetActiveTrendsAsync().Result;
            if (activeTrends.Count == 0) return 0;

            // Calculer le bonus moyen de toutes les tendances actives
            double totalBonus = 0;
            int count = 0;

            foreach (var trend in activeTrends)
            {
                var matrix = _trendRepository.GetCompatibilityMatrixAsync(companyId, trend.TrendId).Result;
                if (matrix != null && matrix.Level == RingGeneral.Core.Enums.CompatibilityLevel.Alignment)
                {
                    totalBonus += matrix.QualityBonus;
                    count++;
                }
            }

            return count > 0 ? totalBonus / count : 0;
        }
        catch
        {
            // En cas d'erreur, retourner 0 pour ne pas bloquer la simulation
            return 0;
        }
    }

    /// <summary>
    /// Calcule les bénéfices de niche pour une compagnie
    /// </summary>
    private (double TicketStabilityMultiplier, double MerchandiseMultiplier, double TvDependencyReduction) CalculateNicheBenefits(string companyId)
    {
        if (_nicheRepository == null)
        {
            return (1.0, 1.0, 0.0);
        }

        try
        {
            var nicheProfile = _nicheRepository.GetNicheFederationProfileByCompanyIdAsync(companyId).Result;
            if (nicheProfile == null || !nicheProfile.IsNicheFederation)
            {
                return (1.0, 1.0, 0.0);
            }

            // Appliquer les multiplicateurs de niche
            // La stabilité de billetterie réduit la variance mais maintient le niveau moyen
            var ticketMultiplier = 1.0 + (nicheProfile.TicketSalesStability / 200.0); // Légère augmentation de stabilité
            
            return (
                TicketStabilityMultiplier: ticketMultiplier,
                MerchandiseMultiplier: nicheProfile.MerchandiseMultiplier,
                TvDependencyReduction: nicheProfile.TvDependencyReduction
            );
        }
        catch
        {
            // En cas d'erreur, retourner les valeurs par défaut
            return (1.0, 1.0, 0.0);
        }
    }
}
