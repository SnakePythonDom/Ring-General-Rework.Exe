using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class StorylineService
{
    public StorylineDefinition Creer(string compagnieId, string nom, IReadOnlyList<string> participants)
    {
        if (string.IsNullOrWhiteSpace(compagnieId))
        {
            throw new ArgumentException("CompagnieId requis.", nameof(compagnieId));
        }

        if (string.IsNullOrWhiteSpace(nom))
        {
            throw new ArgumentException("Nom de storyline requis.", nameof(nom));
        }

        var storylineId = $"STORY-{Guid.NewGuid():N}".ToUpperInvariant();
        return new StorylineDefinition(
            storylineId,
            compagnieId,
            nom.Trim(),
            50,
            StorylinePhase.Setup,
            StorylineStatus.Active,
            participants);
    }

    public StorylineDefinition MettreAJour(
        StorylineDefinition storyline,
        string? nom = null,
        int? heat = null,
        StorylinePhase? phase = null,
        StorylineStatus? status = null,
        IReadOnlyList<string>? participants = null)
    {
        return storyline with
        {
            Nom = string.IsNullOrWhiteSpace(nom) ? storyline.Nom : nom.Trim(),
            Heat = heat ?? storyline.Heat,
            Phase = phase ?? storyline.Phase,
            Status = status ?? storyline.Status,
            Participants = participants ?? storyline.Participants
        };
    }

    public StorylineDefinition Avancer(StorylineDefinition storyline)
    {
        if (storyline.Status == StorylineStatus.Completed)
        {
            return storyline with { Heat = Math.Clamp(storyline.Heat, 0, 100) };
        }

        var heat = Math.Clamp(storyline.Heat, 0, 100);
        var status = storyline.Status;
        var phase = storyline.Phase;

        if (heat <= 10)
        {
            status = StorylineStatus.Suspended;
            phase = StorylinePhase.Setup;
        }
        else
        {
            status = StorylineStatus.Active;
            phase = heat switch
            {
                < 40 => StorylinePhase.Setup,
                < 70 => StorylinePhase.Rising,
                < 90 => StorylinePhase.Climax,
                _ => StorylinePhase.Fallout
            };
        }

        if (phase == StorylinePhase.Fallout && heat < 50)
        {
            status = StorylineStatus.Completed;
        }

        return storyline with
        {
            Heat = heat,
            Status = status,
            Phase = phase
        };
    }
}
