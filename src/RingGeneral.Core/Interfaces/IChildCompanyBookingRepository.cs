using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;

namespace RingGeneral.Core.Interfaces;

/// <summary>
/// Interface pour le repository de contrôle de booking des child companies
/// </summary>
public interface IChildCompanyBookingRepository
{
    /// <summary>
    /// Crée ou met à jour le contrôle de booking pour une child company
    /// </summary>
    void SauvegarderControle(string childCompanyId, ChildCompanyBookingControl control);

    /// <summary>
    /// Charge le contrôle de booking pour une child company
    /// </summary>
    ChildCompanyBookingControl? ChargerControle(string childCompanyId);

    /// <summary>
    /// Met à jour le niveau de contrôle
    /// </summary>
    void MettreAJourNiveauControle(string childCompanyId, BookingControlLevel level);

    /// <summary>
    /// Active ou désactive la planification automatique
    /// </summary>
    void MettreAJourPlanificationAuto(string childCompanyId, bool enabled);

    /// <summary>
    /// Supprime le contrôle de booking (retour aux valeurs par défaut)
    /// </summary>
    void SupprimerControle(string childCompanyId);
}
