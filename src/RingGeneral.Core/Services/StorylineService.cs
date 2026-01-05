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
        return new StorylineInfo(
            storylineId,
            nom,
            StorylinePhase.Setup,
            0,
            StorylineStatus.Active,
            null,
            participants);
    }

    public StorylineInfo MettreAJour(
        StorylineInfo storyline,
        string? nom = null,
        StorylinePhase? phase = null,
        StorylineStatus? statut = null,
        string? resume = null,
        IReadOnlyList<StorylineParticipant>? participants = null)
    {
        return storyline with
        {
            Nom = nom ?? storyline.Nom,
            Phase = phase ?? storyline.Phase,
            Status = statut ?? storyline.Status,
            Resume = resume ?? storyline.Resume,
            Participants = participants ?? storyline.Participants
        };
    }

    public StorylineInfo Avancer(StorylineInfo storyline)
    {
        var phaseSuivante = storyline.Phase switch
        {
            StorylinePhase.Setup => StorylinePhase.Rising,
            StorylinePhase.Rising => StorylinePhase.Climax,
            StorylinePhase.Climax => StorylinePhase.Fallout,
            _ => storyline.Phase
        };

        var statut = storyline.Status;
        if (phaseSuivante == StorylinePhase.Fallout && storyline.Heat >= 80)
        {
            statut = StorylineStatus.Completed;
        }

        return storyline with { Phase = phaseSuivante, Status = statut };
    }

    public int EvaluerDeltaHeatSegment(int noteSegment, int segmentsPrecedents)
    {
        return _heatModel.CalculerDeltaSegment(noteSegment, segmentsPrecedents);
    }
}
