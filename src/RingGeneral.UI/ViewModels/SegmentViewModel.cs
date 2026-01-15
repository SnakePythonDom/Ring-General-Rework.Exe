using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.UI.Services.Messaging;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentViewModel : ViewModelBase
{
    private readonly SegmentTypeCatalog _catalog;
    private readonly IEventAggregator _eventAggregator;

    public bool IsMatch => TypeSegment == "Match" || TypeSegment == "match"; // Case insensitive safety

    public ReactiveCommand<Unit, Unit> AddParticipantCommand { get; }
    public ReactiveCommand<ParticipantViewModel, Unit> RemoveParticipantCommand { get; }

    public SegmentViewModel(
        string segmentId,
        string typeSegment,
        int dureeMinutes,
        bool estMainEvent,
        SegmentTypeCatalog catalog,
        IEventAggregator eventAggregator,
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
        _eventAggregator = eventAggregator;
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

        AddParticipantCommand = ReactiveCommand.Create(AddParticipant);
        RemoveParticipantCommand = ReactiveCommand.Create<ParticipantViewModel>(RemoveParticipant);
    }

    /// <summary>
    /// Constructeur à partir d'une SegmentDefinition.
    /// Utilisé pour charger les segments depuis le contexte du show.
    /// </summary>
    public SegmentViewModel(SegmentDefinition segment, SegmentTypeCatalog catalog, IEventAggregator eventAggregator)
        : this(
            segment.SegmentId,
            segment.TypeSegment,
            segment.DureeMinutes,
            segment.EstMainEvent,
            catalog,
            eventAggregator,
            Array.Empty<ParticipantViewModel>(),
            segment.StorylineId,
            segment.TitreId,
            segment.Intensite,
            segment.VainqueurId,
            segment.PerdantId,
            segment.Settings)
    {
    }

    public void AddParticipant(ParticipantViewModel participant)
    {
        Participants.Add(participant);
    }

    private void AddParticipant()
    {
        _eventAggregator.Publish(new RequestWorkerSelectionEvent(this));
    }

    private void RemoveParticipant(ParticipantViewModel participant)
    {
        if (participant != null && Participants.Contains(participant))
        {
            Participants.Remove(participant);
        }
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
            this.RaisePropertyChanged(nameof(IsMatch));
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

    public int? PredictedRating
    {
        get => _predictedRating;
        set => this.RaiseAndSetIfChanged(ref _predictedRating, value);
    }
    private int? _predictedRating;

    public string? PerdantId
    {
        get => _perdantId;
        set => this.RaiseAndSetIfChanged(ref _perdantId, value);
    }
    private string? _perdantId;

    /// <summary>
    /// Snapshots des participants pour le calcul de prédiction
    /// </summary>
    public List<WorkerSnapshot> ParticipantsSnapshots { get; set; } = new();

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
