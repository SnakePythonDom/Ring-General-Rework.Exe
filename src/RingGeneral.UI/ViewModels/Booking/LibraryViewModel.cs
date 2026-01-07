using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la bibliothèque de segments et templates
/// Permet de gérer les templates réutilisables pour le booking
/// </summary>
public sealed class LibraryViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private SegmentTemplateViewModel? _selectedTemplate;
    private string _searchText = string.Empty;
    private string _selectedCategory = "Tous";

    public LibraryViewModel(GameRepository repository)
    {
        _repository = repository;

        // Collections
        Templates = new ObservableCollection<SegmentTemplateViewModel>();
        Categories = new ObservableCollection<string>
        {
            "Tous",
            "Matches",
            "Promos",
            "Angles",
            "Interviews",
            "Backstage"
        };

        // Commands
        CreateTemplateCommand = ReactiveCommand.Create(CreateTemplate);
        EditTemplateCommand = ReactiveCommand.Create<SegmentTemplateViewModel>(EditTemplate);
        DeleteTemplateCommand = ReactiveCommand.Create<SegmentTemplateViewModel>(DeleteTemplate);
        DuplicateTemplateCommand = ReactiveCommand.Create<SegmentTemplateViewModel>(DuplicateTemplate);
        ApplyTemplateCommand = ReactiveCommand.Create<SegmentTemplateViewModel>(ApplyTemplate);
        SearchCommand = ReactiveCommand.Create(ExecuteSearch);
        RefreshCommand = ReactiveCommand.Create(LoadTemplates);

        // Initialisation
        LoadTemplates();
    }

    // ========== COLLECTIONS ==========

    public ObservableCollection<SegmentTemplateViewModel> Templates { get; }
    public ObservableCollection<string> Categories { get; }

    // ========== PROPRIÉTÉS ==========

    public SegmentTemplateViewModel? SelectedTemplate
    {
        get => _selectedTemplate;
        set => this.RaiseAndSetIfChanged(ref _selectedTemplate, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            ExecuteSearch();
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCategory, value);
            ExecuteSearch();
        }
    }

    public int TemplateCount => Templates.Count;

    // ========== COMMANDS ==========

    public ReactiveCommand<Unit, Unit> CreateTemplateCommand { get; }
    public ReactiveCommand<SegmentTemplateViewModel, Unit> EditTemplateCommand { get; }
    public ReactiveCommand<SegmentTemplateViewModel, Unit> DeleteTemplateCommand { get; }
    public ReactiveCommand<SegmentTemplateViewModel, Unit> DuplicateTemplateCommand { get; }
    public ReactiveCommand<SegmentTemplateViewModel, Unit> ApplyTemplateCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    // ========== MÉTHODES PUBLIQUES ==========

    public void LoadTemplates()
    {
        // TODO: Charger depuis le repository
        Templates.Clear();

        // Données de test
        LoadTestData();
        this.RaisePropertyChanged(nameof(TemplateCount));
    }

    // ========== MÉTHODES PRIVÉES ==========

    private void CreateTemplate()
    {
        var newTemplate = new SegmentTemplateViewModel(
            $"TPL-{Guid.NewGuid():N}".ToUpperInvariant(),
            "Nouveau Template",
            "match",
            "Match",
            15,
            false,
            70,
            null,
            null
        );

        Templates.Add(newTemplate);
        SelectedTemplate = newTemplate;
        this.RaisePropertyChanged(nameof(TemplateCount));
    }

    private void EditTemplate(SegmentTemplateViewModel template)
    {
        // TODO: Ouvrir dialogue d'édition
        System.Diagnostics.Debug.WriteLine($"Editing template: {template.Nom}");
    }

    private void DeleteTemplate(SegmentTemplateViewModel template)
    {
        Templates.Remove(template);
        if (SelectedTemplate == template)
        {
            SelectedTemplate = Templates.FirstOrDefault();
        }
        this.RaisePropertyChanged(nameof(TemplateCount));
    }

    private void DuplicateTemplate(SegmentTemplateViewModel template)
    {
        var duplicate = new SegmentTemplateViewModel(
            $"TPL-{Guid.NewGuid():N}".ToUpperInvariant(),
            $"{template.Nom} (copie)",
            template.TypeSegment,
            template.TypeSegmentLibelle,
            template.DureeMinutes,
            template.EstMainEvent,
            template.Intensite,
            template.MatchTypeId,
            template.MatchTypeNom
        );

        Templates.Add(duplicate);
        SelectedTemplate = duplicate;
        this.RaisePropertyChanged(nameof(TemplateCount));
    }

    private void ApplyTemplate(SegmentTemplateViewModel template)
    {
        // TODO: Publier événement pour appliquer le template dans BookingViewModel
        System.Diagnostics.Debug.WriteLine($"Applying template: {template.Nom}");
    }

    private void ExecuteSearch()
    {
        // TODO: Implémenter filtre de recherche
        // Pour l'instant, afficher tous les templates
        LoadTemplates();
    }

    private void LoadTestData()
    {
        // Templates de test
        Templates.Add(new SegmentTemplateViewModel(
            "TPL001",
            "Main Event Championship",
            "match",
            "Match",
            20,
            true,
            90,
            "MATCH001",
            "Singles Match"
        ));

        Templates.Add(new SegmentTemplateViewModel(
            "TPL002",
            "Opening Promo",
            "promo",
            "Promo",
            10,
            false,
            0,
            null,
            null
        ));

        Templates.Add(new SegmentTemplateViewModel(
            "TPL003",
            "Tag Team Match",
            "match",
            "Match",
            15,
            false,
            75,
            "MATCH002",
            "Tag Team Match"
        ));

        Templates.Add(new SegmentTemplateViewModel(
            "TPL004",
            "Backstage Interview",
            "interview",
            "Interview",
            5,
            false,
            0,
            null,
            null
        ));

        Templates.Add(new SegmentTemplateViewModel(
            "TPL005",
            "Angle Setup",
            "angle",
            "Angle",
            8,
            false,
            0,
            null,
            null
        ));
    }
}
