using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;

namespace RingGeneral.Core.Services;

/// <summary>
/// Phase 1.2 - Service pour gérer les niveaux de contrôle du booking
/// </summary>
public sealed class BookingControlService : IBookingControlService
{
    private readonly IBookerAIEngine _bookerAIEngine;

    public BookingControlService(IBookerAIEngine bookerAIEngine)
    {
        _bookerAIEngine = bookerAIEngine ?? throw new ArgumentNullException(nameof(bookerAIEngine));
    }

    public List<SegmentDefinition> GenerateShowWithControlLevel(
        BookingControlLevel controlLevel,
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments = null,
        AutoBookingConstraints? constraints = null)
    {
        return controlLevel switch
        {
            BookingControlLevel.Spectator => GenerateSpectatorShow(bookerId, showContext, existingSegments, constraints),
            BookingControlLevel.Producer => GenerateProducerShow(bookerId, showContext, existingSegments, constraints),
            BookingControlLevel.CoBooker => GenerateCoBookerShow(bookerId, showContext, existingSegments, constraints),
            BookingControlLevel.Dictator => existingSegments ?? new List<SegmentDefinition>(), // Pas d'IA, retourner segments existants
            _ => existingSegments ?? new List<SegmentDefinition>()
        };
    }

    /// <summary>
    /// Spectator : IA contrôle 100% des décisions
    /// </summary>
    private List<SegmentDefinition> GenerateSpectatorShow(
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments,
        AutoBookingConstraints? constraints)
    {
        // Générer un show complet automatiquement
        return _bookerAIEngine.GenerateAutoBooking(bookerId, showContext, existingSegments, constraints);
    }

    /// <summary>
    /// Producer : IA propose, joueur valide (pour l'instant, même comportement que Spectator)
    /// TODO: Implémenter système de validation
    /// </summary>
    private List<SegmentDefinition> GenerateProducerShow(
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments,
        AutoBookingConstraints? constraints)
    {
        // Pour l'instant, même comportement que Spectator
        // TODO: Ajouter système de validation où le joueur peut veto certaines décisions
        return _bookerAIEngine.GenerateAutoBooking(bookerId, showContext, existingSegments, constraints);
    }

    /// <summary>
    /// CoBooker : Partage des responsabilités
    /// Joueur gère titres majeurs, IA développe midcard
    /// </summary>
    private List<SegmentDefinition> GenerateCoBookerShow(
        string bookerId,
        ShowContext showContext,
        List<SegmentDefinition>? existingSegments,
        AutoBookingConstraints? constraints)
    {
        existingSegments ??= new List<SegmentDefinition>();

        // Vérifier si le joueur a déjà créé des segments pour titres majeurs
        var playerMainEventSegments = existingSegments.Where(s => s.EstMainEvent || s.TitreId != null).ToList();
        var playerMidcardSegments = existingSegments.Except(playerMainEventSegments).ToList();

        // Si le joueur a déjà créé le main event, l'IA développe seulement la midcard
        if (playerMainEventSegments.Any())
        {
            var midcardConstraints = constraints ?? new AutoBookingConstraints();
            midcardConstraints = midcardConstraints with
            {
                RequireMainEvent = false, // Pas de main event nécessaire
                MaxSegments = Math.Max(4, midcardConstraints.MaxSegments - playerMainEventSegments.Count)
            };

            var aiSegments = _bookerAIEngine.GenerateAutoBooking(
                bookerId,
                showContext,
                playerMidcardSegments,
                midcardConstraints);

            return playerMainEventSegments.Concat(aiSegments).ToList();
        }

        // Si le joueur n'a pas créé de main event, l'IA peut le proposer
        return _bookerAIEngine.GenerateAutoBooking(bookerId, showContext, existingSegments, constraints);
    }
}
