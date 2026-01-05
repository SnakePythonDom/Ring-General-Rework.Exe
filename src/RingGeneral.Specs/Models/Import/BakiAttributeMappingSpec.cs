namespace RingGeneral.Specs.Models.Import;

public sealed record BakiAttributeMappingSpec(
    string SourceTable,
    BakiAttributeScaleSet SourceScales,
    BakiAttributeScale TargetScale,
    IReadOnlyDictionary<string, BakiAttributeMapping> Mappings,
    BakiAttributeGroupWeights GroupWeights,
    BakiAttributeNormalizationRules Normalization,
    BakiAttributeRandomVariation? RandomVariation,
    BakiAttributeExperienceFactorSpec? ExperienceFactor,
    BakiAttributeReportSpec? Reporting,
    BakiAttributeRoleCoherence? RoleCoherence);

public sealed record BakiAttributeScaleSet(
    BakiAttributeScale Default,
    IReadOnlyDictionary<string, BakiAttributeScale>? ByAttribute);

public sealed record BakiAttributeScale(int Min, int Max);

public sealed record BakiAttributeMapping(
    string? SourceField,
    string MappingMethod,
    int? ClampMin,
    int? ClampMax,
    string Rounding,
    int DefaultIfMissing,
    BakiAttributePiecewiseSpec? Piecewise,
    BakiAttributeFallbackRule? Fallback,
    string? GroupSource);

public sealed record BakiAttributePiecewiseSpec(IReadOnlyList<BakiAttributePiecewiseRange> Ranges);

public sealed record BakiAttributePiecewiseRange(
    int SourceMin,
    int SourceMax,
    int TargetMin,
    int TargetMax);

public sealed record BakiAttributeFallbackRule(
    string Method,
    IReadOnlyList<string> Fields);

public sealed record BakiAttributeGroupWeights(
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> Groups);

public sealed record BakiAttributeNormalizationRules(
    bool Enabled,
    BakiAttributeEliteCap EliteCap,
    BakiAttributeCatastrophicCap CatastrophicCap,
    BakiAttributeVarianceClamp VarianceClamp);

public sealed record BakiAttributeEliteCap(
    bool Enabled,
    int Threshold,
    int MaxCount,
    int ReduceTo,
    IReadOnlyList<string> SignatureAttributes);

public sealed record BakiAttributeCatastrophicCap(
    bool Enabled,
    int Threshold,
    int MaxCount,
    int RaiseTo);

public sealed record BakiAttributeVarianceClamp(
    bool Enabled,
    double MaxStandardDeviation,
    double BlendFactor);

public sealed record BakiAttributeRandomVariation(
    bool Enabled,
    int Seed,
    int MaxDelta,
    IReadOnlyList<string> Attributes);

public sealed record BakiAttributeExperienceFactorSpec(
    string AgeField,
    string ExperienceYearsField,
    int AgeMin,
    int AgeMax,
    int ExperienceMin,
    int ExperienceMax,
    double AgeWeight,
    double ExperienceWeight);

public sealed record BakiAttributeReportSpec(
    string WorkerIdField,
    IReadOnlyList<string> DisplayNameFields,
    int HistogramBins);

public sealed record BakiAttributeRoleCoherence(
    bool Enabled,
    string RoleField,
    IReadOnlyList<string> MainEventerValues,
    IReadOnlyDictionary<string, int> MainEventerMinimums,
    IReadOnlyList<string> RookieValues,
    IReadOnlyDictionary<string, int> RookieMaximums);
