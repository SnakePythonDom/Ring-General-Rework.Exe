using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class AudienceModel
{
    public AudienceDetails Evaluer(AudienceInputs inputs)
    {
        var reachContribution = (int)Math.Round(inputs.Reach * 0.45);
        var showContribution = (int)Math.Round(inputs.ShowScore * 0.35);
        var starsContribution = (int)Math.Round(inputs.Stars * 0.25);
        var saturationPenalty = (int)Math.Round(inputs.Saturation * 0.3);

        var audience = Math.Clamp(
            reachContribution + showContribution + starsContribution - saturationPenalty,
            0,
            100);

        return new AudienceDetails
        {
            Audience = audience,
            Reach = inputs.Reach,
            ShowScore = inputs.ShowScore,
            Stars = inputs.Stars,
            Saturation = inputs.Saturation,
            ReachContribution = reachContribution,
            ShowScoreContribution = showContribution,
            StarsContribution = starsContribution,
            SaturationPenalty = saturationPenalty
        };
    }
}
