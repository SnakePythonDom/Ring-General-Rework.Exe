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
using RingGeneral.UI.Services;

namespace RingGeneral.UI.ViewModels;

public sealed class GameSessionViewModel : ViewModelBase
{
    private const string ShowId = "SHOW-001";
    private readonly GameRepository _repository;
    private readonly BookingValidator _validator = new();
    private readonly IReadOnlyDictionary<string, string> _segmentLabels;
    private readonly HelpContentProvider _helpProvider = new();
    private readonly IReadOnlyDictionary<string, HelpPageEntry> _helpPages;
    private readonly IReadOnlyDictionary<string, HelpPageEntry> _impactPages;
    private readonly TooltipHelper _tooltipHelper;
    private ShowContext? _context;

    public GameSessionViewModel()
    {
        var cheminDb = Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db");
        var factory = new SqliteConnectionFactory($"Data Source={cheminDb}");
        _repository = new GameRepository(factory);
        _repository.Initialiser();
        _segmentLabels = ChargerSegmentTypes();
        _tooltipHelper = new TooltipHelper(_helpProvider);
        _helpPages = ChargerPages();
        _impactPages = _helpPages
            .Where(page => page.Key.StartsWith("impacts.", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        Segments = new ObservableCollection<SegmentViewModel>();
        Resultats = new ObservableCollection<SegmentResultViewModel>();
        Inbox = new ObservableCollection<InboxItemViewModel>();
        AttributsPrincipaux = new ObservableCollection<AttributeViewModel>();
        PourquoiNote = new ObservableCollection<string>();
        Conseils = new ObservableCollection<string>();
        ImpactPages = new ObservableCollection<ImpactPageViewModel>();
        AidePanel = new HelpPanelViewModel();
        Codex = ChargerCodex();
        YouthGenerationModes = new[]
        {
            new YouthGenerationOptionViewModel("Désactivée", YouthGenerationMode.Desactivee),
            new YouthGenerationOptionViewModel("Réaliste", YouthGenerationMode.Realiste),
            new YouthGenerationOptionViewModel("Abondante", YouthGenerationMode.Abondante)
        };
        WorldGenerationModes = new[]
        {
            new WorldGenerationOptionViewModel("Désactivée", WorldGenerationMode.Desactivee),
            new WorldGenerationOptionViewModel("Faible", WorldGenerationMode.Faible)
        };

        ChargerShow();
        ChargerInbox();
        ChargerImpactsInitial();
        ChargerParametresGeneration();
    }

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<SegmentResultViewModel> Resultats { get; }
    public ObservableCollection<InboxItemViewModel> Inbox { get; }
    public ObservableCollection<AttributeViewModel> AttributsPrincipaux { get; }
    public ObservableCollection<string> PourquoiNote { get; }
    public ObservableCollection<string> Conseils { get; }
    public ObservableCollection<ImpactPageViewModel> ImpactPages { get; }
    public HelpPanelViewModel AidePanel { get; }
    public CodexViewModel Codex { get; }

    public IReadOnlyList<YouthGenerationOptionViewModel> YouthGenerationModes { get; }
    public IReadOnlyList<WorldGenerationOptionViewModel> WorldGenerationModes { get; }

    public YouthGenerationOptionViewModel? YouthGenerationSelection
    {
        get => _youthGenerationSelection;
        set => this.RaiseAndSetIfChanged(ref _youthGenerationSelection, value);
    }
    private YouthGenerationOptionViewModel? _youthGenerationSelection;

    public WorldGenerationOptionViewModel? WorldGenerationSelection
    {
        get => _worldGenerationSelection;
        set => this.RaiseAndSetIfChanged(ref _worldGenerationSelection, value);
    }
    private WorldGenerationOptionViewModel? _worldGenerationSelection;

    public int SemainePivotAnnuelle
    {
        get => _semainePivotAnnuelle;
        set => this.RaiseAndSetIfChanged(ref _semainePivotAnnuelle, value);
    }
    private int _semainePivotAnnuelle = 1;

    public string? ParametresGenerationMessage
    {
        get => _parametresGenerationMessage;
        private set => this.RaiseAndSetIfChanged(ref _parametresGenerationMessage, value);
    }
    private string? _parametresGenerationMessage;

    public IReadOnlyDictionary<string, string> Tooltips => _tooltipHelper.Tooltips;

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

    public string? DetailsSimulation
    {
        get => _detailsSimulation;
        private set => this.RaiseAndSetIfChanged(ref _detailsSimulation, value);
    }
    private string? _detailsSimulation;

    public bool DetailsSimulationVisible
    {
        get => _detailsSimulationVisible;
        set => this.RaiseAndSetIfChanged(ref _detailsSimulationVisible, value);
    }
    private bool _detailsSimulationVisible;

    public bool AideOuverte
    {
        get => _aideOuverte;
        private set => this.RaiseAndSetIfChanged(ref _aideOuverte, value);
    }
    private bool _aideOuverte;

    public ImpactPageViewModel? ImpactSelectionnee
    {
        get => _impactSelectionnee;
        private set => this.RaiseAndSetIfChanged(ref _impactSelectionnee, value);
    }
    private ImpactPageViewModel? _impactSelectionnee;

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
        MettreAJourAnalyseShow(resultat);
        MettreAJourImpacts(resultat);

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

    public void EnregistrerParametresGeneration()
    {
        var youthMode = YouthGenerationSelection?.Mode ?? YouthGenerationMode.Realiste;
        var worldMode = WorldGenerationSelection?.Mode ?? WorldGenerationMode.Desactivee;
        var pivot = SemainePivotAnnuelle > 0 ? SemainePivotAnnuelle : null;
        _repository.SauvegarderParametresGeneration(new WorkerGenerationOptions(youthMode, worldMode, pivot));
        ParametresGenerationMessage = "Paramètres de génération enregistrés.";
    }

    public void OuvrirAide(string pageId)
    {
        if (_helpPages.TryGetValue(pageId, out var page))
        {
            AidePanel.Titre = page.Titre;
            AidePanel.Resume = page.Resume;
            AidePanel.Sections.Clear();
            foreach (var section in page.Sections)
            {
                AidePanel.Sections.Add(new HelpSectionViewModel(section.Titre, section.Contenu));
            }

            AidePanel.ErreursFrequentes.Clear();
            foreach (var erreur in page.ErreursFrequentes)
            {
                AidePanel.ErreursFrequentes.Add(erreur);
            }

            AideOuverte = true;
        }
    }

    public void FermerAide()
    {
        AideOuverte = false;
    }

    public void SelectionnerImpact(string pageId)
    {
        ImpactSelectionnee = ImpactPages.FirstOrDefault(page =>
            page.Id.Equals(pageId, StringComparison.OrdinalIgnoreCase));
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

        MettreAJourAttributs();
    }

    private void ChargerInbox()
    {
        Inbox.Clear();
        foreach (var item in _repository.ChargerInbox())
        {
            Inbox.Add(new InboxItemViewModel(item));
        }
    }

    private void ChargerParametresGeneration()
    {
        var options = _repository.ChargerParametresGeneration();
        YouthGenerationSelection = YouthGenerationModes.FirstOrDefault(mode => mode.Mode == options.YouthMode);
        WorldGenerationSelection = WorldGenerationModes.FirstOrDefault(mode => mode.Mode == options.WorldMode);
        SemainePivotAnnuelle = options.SemainePivotAnnuelle ?? 1;
    }

    private void MettreAJourAttributs()
    {
        AttributsPrincipaux.Clear();
        if (_context is null || _context.Workers.Count == 0)
        {
            return;
        }

        var worker = _context.Workers[0];
        AttributsPrincipaux.Add(new AttributeViewModel("In-Ring", worker.InRing, _tooltipHelper.Obtenir("attr.inring")));
        AttributsPrincipaux.Add(new AttributeViewModel("Entertainment", worker.Entertainment, _tooltipHelper.Obtenir("attr.entertainment")));
        AttributsPrincipaux.Add(new AttributeViewModel("Story", worker.Story, _tooltipHelper.Obtenir("attr.story")));
        AttributsPrincipaux.Add(new AttributeViewModel("Popularité", worker.Popularite, _tooltipHelper.Obtenir("attr.popularite")));
        AttributsPrincipaux.Add(new AttributeViewModel("Fatigue", worker.Fatigue, _tooltipHelper.Obtenir("attr.fatigue")));
        AttributsPrincipaux.Add(new AttributeViewModel("Momentum", worker.Momentum, _tooltipHelper.Obtenir("attr.momentum")));
    }

    private void MettreAJourAnalyseShow(ShowSimulationResult resultat)
    {
        PourquoiNote.Clear();
        Conseils.Clear();

        var facteurs = resultat.RapportShow.Segments
            .SelectMany(segment => segment.Facteurs)
            .OrderByDescending(facteur => Math.Abs(facteur.Impact))
            .Take(5)
            .Select(facteur => $"{facteur.Libelle} {facteur.Impact:+#;-#;0}")
            .ToList();

        foreach (var facteur in facteurs)
        {
            PourquoiNote.Add(facteur);
        }

        var conseils = GenererConseils(resultat);
        foreach (var conseil in conseils)
        {
            Conseils.Add(conseil);
        }

        DetailsSimulation =
            "La note combine performance in-ring, divertissement, heat du public et rythme global. " +
            "Les bonus/malus de chimie, d'intensité et de pacing sont appliqués avant les impacts.";
    }

    private void ChargerImpactsInitial()
    {
        ImpactPages.Clear();
        foreach (var page in _impactPages.Values)
        {
            var pourquoi = page.Sections.FirstOrDefault(section => section.Titre.StartsWith("Pourquoi", StringComparison.OrdinalIgnoreCase))?.Contenu ?? string.Empty;
            var comment = page.Sections.FirstOrDefault(section => section.Titre.StartsWith("Comment", StringComparison.OrdinalIgnoreCase))?.Contenu ?? string.Empty;
            ImpactPages.Add(new ImpactPageViewModel(page.Id, page.Titre, pourquoi, comment));
        }

        ImpactSelectionnee = ImpactPages.FirstOrDefault();
    }

    private void MettreAJourImpacts(ShowSimulationResult resultat)
    {
        var delta = resultat.Delta;
        if (delta is null)
        {
            return;
        }

        foreach (var page in ImpactPages)
        {
            page.Deltas.Clear();
        }

        AjouterDeltas("impacts.popularite",
            delta.PopulariteCompagnieDelta.Select(kv => $"Compagnie {kv.Value:+#;-#;0}"),
            delta.PopulariteWorkersDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}"));

        AjouterDeltas("impacts.finances",
            delta.Finances.Select(tx => $"{tx.Libelle} {tx.Montant:+#;-#;0}"));

        AjouterDeltas("impacts.fatigue",
            delta.FatigueDelta.Select(kv => $"{kv.Key} +{kv.Value}"),
            delta.Blessures.Select(kv => $"{kv.Key} : {kv.Value}"));

        AjouterDeltas("impacts.storylines",
            delta.StorylineHeatDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}"));

        AjouterDeltas("impacts.titres",
            delta.TitrePrestigeDelta.Select(kv => $"{kv.Key} {kv.Value:+#;-#;0}"));

        foreach (var page in ImpactPages)
        {
            if (page.Deltas.Count == 0)
            {
                page.Deltas.Add("Aucun changement récent.");
            }
        }
    }

    private void AjouterDeltas(string pageId, params IEnumerable<string>[] lignes)
    {
        var page = ImpactPages.FirstOrDefault(p => p.Id.Equals(pageId, StringComparison.OrdinalIgnoreCase));
        if (page is null)
        {
            return;
        }

        foreach (var ligne in lignes.SelectMany(ligne => ligne))
        {
            page.Deltas.Add(ligne);
        }
    }

    private IReadOnlyList<string> GenererConseils(ShowSimulationResult resultat)
    {
        var conseils = new List<string>();
        if (resultat.RapportShow.NoteGlobale < 60)
        {
            conseils.Add("Renforcez le main event et limitez les segments faibles.");
        }

        if (resultat.RapportShow.Segments.Any(segment => segment.PacingPenalty < 0))
        {
            conseils.Add("Alternez match et promo pour éviter les pénalités de rythme.");
        }

        if (resultat.RapportShow.Segments.Any(segment => segment.Impact.FatigueDelta.Values.Any(delta => delta > 6)))
        {
            conseils.Add("Réduisez la durée des matchs les plus fatigants.");
        }

        if (resultat.RapportShow.Segments.All(segment => segment.Impact.StorylineHeatDelta.Count == 0))
        {
            conseils.Add("Liez davantage de segments aux storylines actives.");
        }

        if (conseils.Count == 0)
        {
            conseils.Add("Continuez sur ce rythme et consolidez vos points forts.");
        }

        return conseils;
    }

    private CodexViewModel ChargerCodex()
    {
        var glossaire = _helpProvider.ChargerGlossaire();
        var systemes = _helpProvider.ChargerSystemes();
        var tutoriel = _helpProvider.ChargerTutorial();

        var articles = new List<CodexArticleViewModel>();
        articles.AddRange(glossaire.Entrees.Select(entree =>
            new CodexArticleViewModel(entree.Id, entree.Terme, "Glossaire", entree.Definition, entree.Liens)));

        articles.AddRange(systemes.Systemes.Select(systeme =>
            new CodexArticleViewModel(systeme.Id, systeme.Titre, "Système",
                $"{systeme.Resume}\n- {string.Join("\n- ", systeme.Points)}", systeme.Liens)));

        foreach (var etape in tutoriel.Etapes)
        {
            articles.Add(new CodexArticleViewModel(etape.Id, etape.Titre, "Guide",
                $"{etape.Contenu}\nObjectif : {etape.Objectif}", Array.Empty<string>()));
        }

        return new CodexViewModel(articles);
    }

    private IReadOnlyDictionary<string, HelpPageEntry> ChargerPages()
    {
        var spec = _helpProvider.ChargerPages();
        return spec.Pages.ToDictionary(page => page.Id, page => page, StringComparer.OrdinalIgnoreCase);
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

public sealed record YouthGenerationOptionViewModel(string Libelle, YouthGenerationMode Mode);

public sealed record WorldGenerationOptionViewModel(string Libelle, WorldGenerationMode Mode);
