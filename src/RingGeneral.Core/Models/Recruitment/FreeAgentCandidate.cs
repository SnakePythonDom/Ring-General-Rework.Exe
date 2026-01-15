namespace RingGeneral.Core.Models.Recruitment;

public enum FreeAgentType
{
    Wrestler,
    Staff
}

/// <summary>
/// Projection model for the Free Agent Market list.
/// Combines common properties of Workers and Staff for unified display and filtering.
/// </summary>
public record FreeAgentCandidate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required FreeAgentType Type { get; init; }
    public int Age { get; init; }
    public string? Nationality { get; init; }
    public string? Gender { get; init; }

    // Financials
    public decimal? WeeklySalaryDemand { get; init; }

    // Primary Attributes
    public int Popularity { get; init; }

    // Specialized Attributes
    // For Wrestler: InRing. For Staff: SkillScore.
    public int PrimarySkill { get; init; }

    // For Wrestler: Entertainment. For Staff: null or PersonalityScore.
    public int? SecondarySkill { get; init; }

    // For Wrestler: Story.
    public int? TertiarySkill { get; init; }

    // Categorization
    public string? RoleDisplay { get; init; } // e.g. "Main Eventer", "Road Agent", "Trainer"
    public string? Specialization { get; init; } // e.g. "Technical", "Creative"

    // Geography
    public string? Region { get; init; }

    // Fit Scores (Contextual to viewing company)
    public double? GeoFitScore { get; set; }
    public double? StrategicFitScore { get; set; }
}
