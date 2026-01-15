namespace RingGeneral.Core.Models;

/// <summary>
/// Gimmick category enumeration
/// </summary>
public enum GimmickCategory
{
    Power,
    Technical,
    HighFlyer,
    Brawler,
    Showman,
    Hardcore,
    AllRounder
}

/// <summary>
/// Represents a wrestling gimmick/character persona
/// </summary>
public class Gimmick
{
    /// <summary>
    /// Unique identifier for the gimmick
    /// </summary>
    public string GimmickId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the gimmick (e.g., "The Undertaker", "The Showstopper")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Flavor text description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Primary category (Power, Technical, HighFlyer, etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// More specific classification (Monster, Technician, Daredevil, etc.)
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Bonus/penalty to entertainment rating (-20 to +20)
    /// </summary>
    public int EntertainmentModifier { get; set; }

    /// <summary>
    /// Bonus/penalty to crowd reactions (-20 to +20)
    /// </summary>
    public int CrowdReactionModifier { get; set; }

    /// <summary>
    /// Preferred alignment (Face, Heel, Tweener, Any)
    /// </summary>
    public string PreferredAlignment { get; set; } = "Any";

    /// <summary>
    /// Era compatibility (Modern, Attitude, Golden, Any)
    /// </summary>
    public string EraCompatibility { get; set; } = "Any";

    /// <summary>
    /// Popularity tier (Jobber, LowerMid, MidCard, UpperMid, MainEvent)
    /// </summary>
    public string PopularityTier { get; set; } = "MidCard";

    /// <summary>
    /// Is this gimmick active/available?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this gimmick was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // ====================================================================
    // HELPER PROPERTIES
    // ====================================================================

    /// <summary>
    /// Get category as enum
    /// </summary>
    public GimmickCategory CategoryEnum
    {
        get
        {
            return Category.ToUpper() switch
            {
                "POWER" => GimmickCategory.Power,
                "TECHNICAL" => GimmickCategory.Technical,
                "HIGHFLYER" => GimmickCategory.HighFlyer,
                "BRAWLER" => GimmickCategory.Brawler,
                "SHOWMAN" => GimmickCategory.Showman,
                "HARDCORE" => GimmickCategory.Hardcore,
                "ALLROUNDER" => GimmickCategory.AllRounder,
                _ => GimmickCategory.AllRounder
            };
        }
    }

    /// <summary>
    /// Get preferred alignment as enum
    /// </summary>
    public Alignment? PreferredAlignmentEnum
    {
        get
        {
            return PreferredAlignment.ToLower() switch
            {
                "face" => Alignment.Face,
                "heel" => Alignment.Heel,
                "tweener" => Alignment.Tweener,
                _ => null // "Any"
            };
        }
    }

    /// <summary>
    /// Get popularity tier as enum
    /// </summary>
    public PushLevel PopularityTierEnum
    {
        get
        {
            return PopularityTier switch
            {
                "MainEvent" => PushLevel.MainEvent,
                "UpperMid" => PushLevel.UpperMid,
                "MidCard" => PushLevel.MidCard,
                "LowerMid" => PushLevel.LowerMid,
                "Jobber" => PushLevel.Jobber,
                _ => PushLevel.MidCard
            };
        }
    }

    /// <summary>
    /// Check if gimmick is compatible with worker's alignment
    /// </summary>
    public bool IsCompatibleWithAlignment(Alignment alignment)
    {
        if (PreferredAlignment == "Any") return true;
        return PreferredAlignmentEnum == alignment;
    }

    /// <summary>
    /// Check if gimmick is compatible with worker's push level
    /// </summary>
    public bool IsCompatibleWithPushLevel(PushLevel pushLevel)
    {
        // Allow gimmicks within one tier of worker's push level
        int tierDifference = Math.Abs((int)PopularityTierEnum - (int)pushLevel);
        return tierDifference <= 1;
    }

    /// <summary>
    /// Get display text for gimmick with modifiers
    /// </summary>
    public string GetDisplayText()
    {
        var parts = new List<string> { Name };

        if (EntertainmentModifier != 0)
            parts.Add($"Entertainment {(EntertainmentModifier > 0 ? "+" : "")}{EntertainmentModifier}");

        if (CrowdReactionModifier != 0)
            parts.Add($"Crowd {(CrowdReactionModifier > 0 ? "+" : "")}{CrowdReactionModifier}");

        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Represents a category of gimmicks
/// </summary>
public class GimmickCategoryInfo
{
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Tracks gimmick changes for a worker over time
/// </summary>
public class GimmickHistory
{
    /// <summary>
    /// Unique identifier for this history entry
    /// </summary>
    public int HistoryId { get; set; }

    /// <summary>
    /// Worker who had this gimmick
    /// </summary>
    public int WorkerId { get; set; }

    /// <summary>
    /// Reference to the gimmick (if from predefined list)
    /// </summary>
    public string? GimmickId { get; set; }

    /// <summary>
    /// Name of the gimmick (stored for historical purposes)
    /// </summary>
    public string GimmickName { get; set; } = string.Empty;

    /// <summary>
    /// When the worker adopted this gimmick
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Now;

    /// <summary>
    /// When the worker dropped this gimmick (NULL if current)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Reason for adopting this gimmick
    /// </summary>
    public string? AdoptionReason { get; set; }

    /// <summary>
    /// How successful was this gimmick (0-100)
    /// </summary>
    public int SuccessRating { get; set; } = 50;

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // ====================================================================
    // NAVIGATION PROPERTIES
    // ====================================================================

    /// <summary>
    /// The worker who had this gimmick
    /// </summary>
    public Worker? Worker { get; set; }

    /// <summary>
    /// The gimmick details (if from predefined list)
    /// </summary>
    public Gimmick? Gimmick { get; set; }

    // ====================================================================
    // HELPER PROPERTIES
    // ====================================================================

    /// <summary>
    /// Is this the current gimmick?
    /// </summary>
    public bool IsCurrent => EndDate == null;

    /// <summary>
    /// How long was/has this gimmick been used?
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            var end = EndDate ?? DateTime.Now;
            return end - StartDate;
        }
    }

    /// <summary>
    /// Duration in days
    /// </summary>
    public int DurationInDays => (int)Duration.TotalDays;

    /// <summary>
    /// Duration in weeks
    /// </summary>
    public int DurationInWeeks => DurationInDays / 7;

    /// <summary>
    /// Get success rating as grade (A-F)
    /// </summary>
    public string SuccessGrade
    {
        get
        {
            return SuccessRating switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }

    /// <summary>
    /// Get display text for this history entry
    /// </summary>
    public string GetDisplayText()
    {
        var duration = IsCurrent ? "Current" : $"{DurationInWeeks} weeks";
        return $"{GimmickName} ({duration}) - {SuccessGrade}";
    }
}
