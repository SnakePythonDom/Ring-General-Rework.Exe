using ReactiveUI;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentTemplateViewModel : ReactiveObject
{
    public SegmentTemplateViewModel(
        string templateId,
        string nom,
        string typeSegment,
        string typeSegmentLibelle,
        int dureeMinutes,
        bool estMainEvent,
        int intensite,
        string? matchTypeId,
        string? matchTypeNom)
    {
        TemplateId = templateId;
        Nom = nom;
        TypeSegment = typeSegment;
        TypeSegmentLibelle = typeSegmentLibelle;
        DureeMinutes = dureeMinutes;
        EstMainEvent = estMainEvent;
        Intensite = intensite;
        MatchTypeId = matchTypeId;
        MatchTypeNom = matchTypeNom;
    }

    /// <summary>
    /// Constructeur simplifié prenant un SegmentTemplate.
    /// </summary>
    public SegmentTemplateViewModel(SegmentTemplate template)
        : this(
            template.TemplateId,
            template.Nom,
            template.TypeSegment,
            template.TypeSegment, // Utiliser le type comme libelle par défaut
            template.DureeMinutes,
            template.EstMainEvent,
            template.Intensite,
            template.MatchTypeId,
            template.MatchTypeId) // Utiliser l'ID comme nom par défaut
    {
    }

    public string TemplateId { get; }
    public string Nom { get; }
    public string TypeSegment { get; }
    public string TypeSegmentLibelle { get; }
    public int DureeMinutes { get; }
    public bool EstMainEvent { get; }
    public int Intensite { get; }
    public string? MatchTypeId { get; }
    public string? MatchTypeNom { get; }

    public string Resume
    {
        get
        {
            var details = new List<string>
            {
                TypeSegmentLibelle,
                $"{DureeMinutes} min"
            };

            if (!string.IsNullOrWhiteSpace(MatchTypeNom))
            {
                details.Add(MatchTypeNom);
            }

            if (EstMainEvent)
            {
                details.Add("Main event");
            }

            return string.Join(" • ", details);
        }
    }
}
