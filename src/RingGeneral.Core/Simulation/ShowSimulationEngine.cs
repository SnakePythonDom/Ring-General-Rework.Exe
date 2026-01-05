using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class ShowSimulationEngine
{
    private readonly IRandomProvider _random;
    private readonly AudienceModel _audienceModel;
    private readonly DealRevenueModel _dealRevenueModel;

    public ShowSimulationEngine(IRandomProvider random, AudienceModel? audienceModel = null, DealRevenueModel? dealRevenueModel = null)
    {
        _random = random;
        _audienceModel = audienceModel ?? new AudienceModel();
        _dealRevenueModel = dealRevenueModel ?? new DealRevenueModel();
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
            var note = Math.Clamp(baseScore + crowdBonus + pacingPenalty + chimieBonus, 0, 100);

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
                var delta = heatModel.CalculerDeltaInactif();
                storylineHeat[storyline.StorylineId] = storylineHeat.TryGetValue(storyline.StorylineId, out var total)
                    ? total + delta
                    : delta;
            }
        }

        var noteShow = segmentsReports.Count == 0 ? 0 : (int)Math.Round(segmentsReports.Average(segment => segment.Note));
        var populariteDeltaCompagnie = (noteShow - 50) / 5;
        populariteCompagnie[context.Compagnie.CompagnieId] = populariteDeltaCompagnie;

        var participantsIds = context.Segments.SelectMany(segment => segment.Participants).Distinct().ToList();
        var participants = context.Workers.Where(worker => participantsIds.Contains(worker.WorkerId)).ToList();
        var stars = _audienceModel.CalculerStars(participants);
        var deal = _dealRevenueModel.TrouverDeal(context.Show.DealTvId);
        var reachBonus = deal?.Reach ?? 0;
        var audienceCap = deal?.AudienceCap ?? 0;
        var audienceDetails = _audienceModel.Calculer(context.Compagnie, noteShow, stars, reachBonus, audienceCap);
        var audience = audienceDetails.Audience;
        var billetterie = Math.Round(1500 + audience * 75 + context.Compagnie.Reach * 20, 2);
        var merch = Math.Round(300 + audience * 20, 2);
        var tv = 0.0;
        if (deal is not null)
        {
            var revenus = _dealRevenueModel.Calculer(deal, audience);
            tv = Math.Round(revenus.RevenueTotale, 2);
        }
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
            $"Impact popularité : {populariteDeltaCompagnie:+#;-#;0}",
            $"Coûts de production : {-financeResult.CoutProduction:#,0}"
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
}
