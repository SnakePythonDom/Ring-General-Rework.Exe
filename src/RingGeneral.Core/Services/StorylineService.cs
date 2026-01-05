using RingGeneral.Core.Models;
using RingGeneral.Core.Simulation;

namespace RingGeneral.Core.Services;

public sealed class StorylineService
{
    private readonly HeatModel _heatModel;

    public StorylineService(HeatModel? heatModel = null)
    {
        _heatModel = heatModel ?? new HeatModel();
    }

    public StorylineInfo Creer(string storylineId, string nom, IReadOnlyList<StorylineParticipant> participants)
    {
        var phase = "BUILD";
        var statut = "ACTIVE";

        return new StorylineInfo(
            storylineId,
            nom,
            phase,
            0,
            statut,
            null,
            participants);
    }

    public StorylineInfo MettreAJour(
        StorylineInfo storyline,
        string? nom = null,
        string? phase = null,
        string? statut = null,
        string? resume = null,
        IReadOnlyList<StorylineParticipant>? participants = null)
    {
        return storyline with
        {
            Nom = nom ?? storyline.Nom,
            Phase = phase ?? storyline.Phase,
            Statut = statut ?? storyline.Statut,
            Resume = resume ?? storyline.Resume,
            Participants = participants ?? storyline.Participants
        };
    }

    public StorylineInfo Avancer(StorylineInfo storyline)
    {
        var phaseSuivante = storyline.Phase switch
        {
            "BUILD" => "PEAK",
            "PEAK" => "BLOWOFF",
            _ => storyline.Phase
        };

        var statut = storyline.Statut;
        if (phaseSuivante == "BLOWOFF" && storyline.Heat >= 80)
        {
            statut = "TERMINEE";
        }

        return storyline with { Phase = phaseSuivante, Statut = statut };
    }

    public int EvaluerDeltaHeatSegment(int noteSegment, int segmentsPrecedents)
    {
        return _heatModel.CalculerDeltaSegment(noteSegment, segmentsPrecedents);
    }
}
