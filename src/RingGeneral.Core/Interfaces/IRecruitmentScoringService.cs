using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IRecruitmentScoringService
{
    /// <summary>
    /// Calcule un score de pertinence (0-100) pour un worker vis-à-vis d'une compagnie.
    /// Incorpore le Geo-Fit (Region) et le Strategic-Fit (DNA).
    /// </summary>
    double CalculateScore(string companyId, Worker worker);

    /// <summary>
    /// Calcule le Geo-Fit (0-100) basé sur la localisation.
    /// </summary>
    double GetGeoFit(string companyId, Worker worker);

    /// <summary>
    /// Calcule le Strategic-Fit (0-100) basé sur le style/DNA.
    /// </summary>
    double GetStrategicFit(string companyId, Worker worker);
}
