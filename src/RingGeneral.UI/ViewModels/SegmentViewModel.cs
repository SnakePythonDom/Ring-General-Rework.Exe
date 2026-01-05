using System.Collections.ObjectModel;
using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class SegmentViewModel : ReactiveObject
{
    private readonly IReadOnlyDictionary<string, string> _segmentLabels;

    public SegmentViewModel(
        string segmentId,
        string typeSegment,
        int dureeMinutes,
        bool estMainEvent,
        IReadOnlyDictionary<string, string> segmentLabels,
        IReadOnlyList<ParticipantViewModel> participants,
        string? storylineId,
        string? titreId,
        int intensite,
        string? vainqueurId,
        string? perdantId)
    {
        SegmentId = segmentId;
        _segmentLabels = segmentLabels;
        _typeSegment = typeSegment;
        _typeSegmentLibelle = ObtenirLibelle(typeSegment);
        _dureeMinutes = dureeMinutes;
        _estMainEvent = estMainEvent;
        _storylineId = storylineId;
        TitreId = titreId;
        Intensite = intensite;
        VainqueurId = vainqueurId;
        PerdantId = perdantId;
        Participants = new ObservableCollection<ParticipantViewModel>(participants);
    }

    public string SegmentId { get; }

    public string TypeSegment
    {
        get => _typeSegment;
        set
        {
            this.RaiseAndSetIfChanged(ref _typeSegment, value);
            TypeSegmentLibelle = ObtenirLibelle(value);
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
    public string? StorylineId
    {
        get => _storylineId;
        set => this.RaiseAndSetIfChanged(ref _storylineId, value);
    }
    private string? _storylineId;
    public string? TitreId { get; }
    public int Intensite { get; }
    public string? VainqueurId { get; }
    public string? PerdantId { get; }

    private string ObtenirLibelle(string typeSegment)
        => _segmentLabels.TryGetValue(typeSegment, out var libelle) ? libelle : typeSegment;
}
