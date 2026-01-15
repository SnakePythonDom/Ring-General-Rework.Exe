using ReactiveUI;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentTemplateViewModel : ReactiveObject
{
    public SegmentTemplateViewModel(
        string templateId,
        string nom,
        string? description,
        string typeSegment,
        string typeSegmentLibelle,
        int dureeMinutes,
        bool estMainEvent,
        int intensite,
        string? matchTypeId,
        string? matchTypeNom,
        string? segmentsJson = null)
    {
        TemplateId = templateId;
        Nom = nom;
        Description = description;
        TypeSegment = typeSegment;
        TypeSegmentLibelle = typeSegmentLibelle;
        DureeMinutes = dureeMinutes;
        EstMainEvent = estMainEvent;
        Intensite = intensite;
        MatchTypeId = matchTypeId;
        MatchTypeNom = matchTypeNom;
        SegmentsJson = segmentsJson;
    }

    /// <summary>
    /// Constructeur simplifié prenant un SegmentTemplate.
    /// </summary>
    public SegmentTemplateViewModel(SegmentTemplate template)
        : this(
            template.TemplateId,
            template.Nom,
            template.Description,
            template.TypeSegment,
            template.TypeSegment, // Utiliser le type comme libelle par défaut
            template.DureeMinutes,
            template.EstMainEvent,
            template.Intensite,
            template.MatchTypeId,
            template.MatchTypeId, // Utiliser l'ID comme nom par défaut
            template.SegmentsJson)
    {
    }

    public string TemplateId { get; }
    public string Nom { get; }
    public string? Description { get; }
    public string TypeSegment { get; }
    public string TypeSegmentLibelle { get; }
    public int DureeMinutes { get; }
    public bool EstMainEvent { get; }
    public int Intensite { get; }
    public string? MatchTypeId { get; }
    public string? MatchTypeNom { get; }
    public string? SegmentsJson { get; }

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
