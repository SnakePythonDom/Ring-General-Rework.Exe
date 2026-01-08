using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Specs.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentViewModel : ReactiveObject
{
    private readonly SegmentTypeCatalog _catalog;

    public SegmentViewModel(
        string segmentId,
        string typeSegment,
        int dureeMinutes,
        bool estMainEvent,
        SegmentTypeCatalog catalog,
        IReadOnlyList<ParticipantViewModel> participants,
        string? storylineId,
        string? titreId,
        int intensite,
        string? vainqueurId,
        string? perdantId,
        IReadOnlyDictionary<string, string>? settings)
    {
        SegmentId = segmentId;
        _catalog = catalog;
        _typeSegment = typeSegment;
        _typeSegmentLibelle = ObtenirLibelle(typeSegment);
        _dureeMinutes = dureeMinutes;
        _estMainEvent = estMainEvent;
        _storylineId = storylineId;
        _titreId = titreId;
        _intensite = intensite;
        _vainqueurId = vainqueurId;
        _perdantId = perdantId;
        Participants = new ObservableCollection<ParticipantViewModel>(participants);
        Consignes = new ObservableCollection<SegmentConsigneViewModel>();
        RechargerConsignes(settings);
    }

    /// <summary>
    /// Constructeur à partir d'une SegmentDefinition.
    /// Utilisé pour charger les segments depuis le contexte du show.
    /// </summary>
    public SegmentViewModel(SegmentDefinition segment)
        : this(
            segment.SegmentId,
            segment.TypeSegment,
            segment.DureeMinutes,
            segment.EstMainEvent,
            new SegmentTypeCatalog(),
            Array.Empty<ParticipantViewModel>(),
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            segment.Settings)
    {
    }

    public string SegmentId { get; }

    public string TypeSegment
    {
        get => _typeSegment;
        set
        {
            this.RaiseAndSetIfChanged(ref _typeSegment, value);
            TypeSegmentLibelle = ObtenirLibelle(value);
            RechargerConsignes(null);
        }
    }
    private string _typeSegment;

    public string TypeSegmentLibelle
    {
        get => _typeSegmentLibelle;
        private set => this.RaiseAndSetIfChanged(ref _typeSegmentLibelle, value);
    }
    private string _typeSegmentLibelle;

    public int DureeMinutes
    {
        get => _dureeMinutes;
        set => this.RaiseAndSetIfChanged(ref _dureeMinutes, value);
    }
    private int _dureeMinutes;

    public bool EstMainEvent
    {
        get => _estMainEvent;
        set => this.RaiseAndSetIfChanged(ref _estMainEvent, value);
    }
    private bool _estMainEvent;

    public string? ParticipantSelectionneeId
    {
        get => _participantSelectionneeId;
        set => this.RaiseAndSetIfChanged(ref _participantSelectionneeId, value);
    }
    private string? _participantSelectionneeId;

    public string? Avertissements
    {
        get => _avertissements;
        set => this.RaiseAndSetIfChanged(ref _avertissements, value);
    }
    private string? _avertissements;

    public ObservableCollection<ParticipantViewModel> Participants { get; }
    public ObservableCollection<SegmentConsigneViewModel> Consignes { get; }

    public string? StorylineId
    {
        get => _storylineId;
        set => this.RaiseAndSetIfChanged(ref _storylineId, value);
    }
    private string? _storylineId;

    public string? TitreId
    {
        get => _titreId;
        set => this.RaiseAndSetIfChanged(ref _titreId, value);
    }
    private string? _titreId;

    public int Intensite
    {
        get => _intensite;
        set => this.RaiseAndSetIfChanged(ref _intensite, value);
    }
    private int _intensite;

    public string? VainqueurId
    {
        get => _vainqueurId;
        set => this.RaiseAndSetIfChanged(ref _vainqueurId, value);
    }
    private string? _vainqueurId;

    public string? PerdantId
    {
        get => _perdantId;
        set => this.RaiseAndSetIfChanged(ref _perdantId, value);
    }
    private string? _perdantId;

    private string ObtenirLibelle(string typeSegment)
        => _catalog.Labels.TryGetValue(typeSegment, out var libelle) ? libelle : typeSegment;

    private void RechargerConsignes(IReadOnlyDictionary<string, string>? settings)
    {
        Consignes.Clear();
        foreach (var consigneId in _catalog.ObtenirConsignesPourType(TypeSegment))
        {
            var options = _catalog.ObtenirOptionsConsigne(consigneId);
            var selection = settings is not null && settings.TryGetValue(consigneId, out var valeur)
                ? valeur
                : options.FirstOrDefault();
            Consignes.Add(new SegmentConsigneViewModel(
                consigneId,
                _catalog.ObtenirLibelleConsigne(consigneId),
                options,
                selection));
        }
    }

    public IReadOnlyDictionary<string, string> ConstruireSettings()
    {
        return Consignes
            .Where(consigne => !string.IsNullOrWhiteSpace(consigne.Selection))
            .ToDictionary(consigne => consigne.Id, consigne => consigne.Selection!, StringComparer.OrdinalIgnoreCase);
    }
}
