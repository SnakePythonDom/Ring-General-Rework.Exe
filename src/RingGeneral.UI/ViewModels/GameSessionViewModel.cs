using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Random;
using RingGeneral.Core.Simulation;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using RingGeneral.Specs.Models;
using RingGeneral.Specs.Services;

namespace RingGeneral.UI.ViewModels;

public sealed class GameSessionViewModel : ViewModelBase
{
    private const string ShowId = "SHOW-001";
    private readonly GameRepository _repository;
    private readonly BookingValidator _validator = new();
    private readonly IReadOnlyDictionary<string, string> _segmentLabels;
    private ShowContext? _context;

    public GameSessionViewModel()
    {
        var cheminDb = Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db");
        var factory = new SqliteConnectionFactory($"Data Source={cheminDb}");
        _repository = new GameRepository(factory);
        _repository.Initialiser();
        _segmentLabels = ChargerSegmentTypes();

        Segments = new ObservableCollection<SegmentViewModel>();
        Resultats = new ObservableCollection<SegmentResultViewModel>();
        Inbox = new ObservableCollection<InboxItemViewModel>();

        ChargerShow();
        ChargerInbox();
    }

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<SegmentResultViewModel> Resultats { get; }
    public ObservableCollection<InboxItemViewModel> Inbox { get; }

    public string? ValidationErreurs
    {
        get => _validationErreurs;
        private set => this.RaiseAndSetIfChanged(ref _validationErreurs, value);
    }
    private string? _validationErreurs;

    public string? ValidationAvertissements
    {
        get => _validationAvertissements;
        private set => this.RaiseAndSetIfChanged(ref _validationAvertissements, value);
    }
    private string? _validationAvertissements;

    public string? ResumeShow
    {
        get => _resumeShow;
        private set => this.RaiseAndSetIfChanged(ref _resumeShow, value);
    }
    private string? _resumeShow;

    public void SimulerShow()
    {
        if (_context is null)
        {
            return;
        }

        var booking = _repository.ChargerBookingPlan(_context);
        var validation = _validator.ValiderBooking(booking);
        ValidationErreurs = validation.EstValide ? null : string.Join("\n", validation.Erreurs);
        ValidationAvertissements = validation.Avertissements.Count == 0 ? null : string.Join("\n", validation.Avertissements);

        if (!validation.EstValide)
        {
            return;
        }

        var seed = HashCode.Combine(_context.Show.ShowId, _context.Show.Semaine);
        var engine = new ShowSimulationEngine(new SeededRandomProvider(seed));
        var resultat = engine.Simuler(_context);

        Resultats.Clear();
        foreach (var segment in resultat.RapportShow.Segments)
        {
            var libelle = _segmentLabels.TryGetValue(segment.TypeSegment, out var label) ? label : segment.TypeSegment;
            Resultats.Add(new SegmentResultViewModel(segment, libelle));
        }

        ResumeShow = $"Note {resultat.RapportShow.NoteGlobale} • Audience {resultat.RapportShow.Audience} • Billetterie {resultat.RapportShow.Billetterie:C}";

        var impactApplier = new ImpactApplier(_repository);
        var segmentResults = resultat.RapportShow.Segments
            .Select(segment => new SegmentResult(segment.SegmentId, segment.Note, $"Segment {segment.TypeSegment}", segment))
            .ToList();
        var impactContext = new ImpactContext(
            _context.Show.ShowId,
            segmentResults,
            _context.Storylines.Select(storyline => storyline.StorylineId).ToList(),
            resultat.RapportShow,
            resultat.Delta);
        impactApplier.AppliquerImpacts(impactContext);

        ChargerShow();
    }

    public void PasserSemaineSuivante()
    {
        var weekly = new WeeklyLoopService(_repository);
        weekly.PasserSemaineSuivante(ShowId);
        ChargerInbox();
        ChargerShow();
    }

    private void ChargerShow()
    {
        _context = _repository.ChargerShowContext(ShowId);
        Segments.Clear();

        foreach (var segment in _context.Segments)
        {
            var participants = _context.Workers.Where(worker => segment.Participants.Contains(worker.WorkerId))
                .Select(worker => worker.NomComplet)
                .ToList();
            var libelle = _segmentLabels.TryGetValue(segment.TypeSegment, out var label) ? label : segment.TypeSegment;
            Segments.Add(new SegmentViewModel(
                segment.SegmentId,
                segment.TypeSegment,
                libelle,
                segment.DureeMinutes,
                string.Join(", ", participants),
                segment.EstMainEvent));
        }
    }

    private void ChargerInbox()
    {
        Inbox.Clear();
        foreach (var item in _repository.ChargerInbox())
        {
            Inbox.Add(new InboxItemViewModel(item));
        }
    }

    private static IReadOnlyDictionary<string, string> ChargerSegmentTypes()
    {
        var reader = new SpecsReader();
        var chemins = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "specs", "booking", "segment-types.fr.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "specs", "booking", "segment-types.fr.json")
        };

        var chemin = chemins.FirstOrDefault(File.Exists);
        if (chemin is null)
        {
            return new Dictionary<string, string>();
        }

        var spec = reader.Charger<SegmentTypesSpec>(chemin);
        return spec.Types.ToDictionary(type => type.Id, type => type.Libelle);
    }
}
