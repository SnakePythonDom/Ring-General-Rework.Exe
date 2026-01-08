using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Tables;

/// <summary>
/// ViewModel générique pour l'affichage et la gestion de tables de données.
/// Supporte le tri, le filtrage, la recherche et la personnalisation des colonnes.
/// </summary>
public sealed class TableViewViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private readonly List<TableSortSetting> _sortSettings = new();
    private bool _suspendPreferences;
    private TableViewItemViewModel? _selection;
    private string? _searchText;
    private TableFilterOptionViewModel? _selectedTypeFilter;
    private TableFilterOptionViewModel? _selectedStatusFilter;
    private string? _resultsResume;

    public TableViewViewModel()
    {
        Items = new ObservableCollection<TableViewItemViewModel>();
        ItemsView = new DataGridCollectionView(Items)
        {
            Filter = FilterItems
        };
        Configuration = new TableViewConfigurationViewModel();
        Columns = new ObservableCollection<TableColumnOrderViewModel>
        {
            new("Type", "Type"),
            new("Compagnie", "Compagnie"),
            new("Role", "Rôle"),
            new("Statut", "Statut"),
            new("Popularite", "Popularité"),
            new("Momentum", "Momentum"),
            new("Note", "Note")
        };
        TypeFilters = new ObservableCollection<TableFilterOptionViewModel>
        {
            new("Tous", "Tous"),
            new("Worker", "Workers"),
            new("Titre", "Titres"),
            new("Storyline", "Storylines")
        };
        StatusFilters = new ObservableCollection<TableFilterOptionViewModel>
        {
            new("Tous", "Tous"),
            new("Actif", "Actif"),
            new("Inactif", "Inactif"),
            new("Blesse", "Blessé"),
            new("Vacant", "Vacant")
        };

        SelectedTypeFilter = TypeFilters[0];
        SelectedStatusFilter = StatusFilters[0];

        _suspendPreferences = false;
        Configuration.PropertyChanged += (_, _) => SavePreferences();
        Columns.CollectionChanged += (_, _) => SavePreferences();
    }

    public TableViewViewModel(GameRepository repository) : this()
    {
        _repository = repository;
        LoadPreferences();
    }

    /// <summary>
    /// Liste des éléments de la table.
    /// </summary>
    public ObservableCollection<TableViewItemViewModel> Items { get; }

    /// <summary>
    /// Vue de la collection avec tri/filtrage appliqués.
    /// </summary>
    public DataGridCollectionView ItemsView { get; }

    /// <summary>
    /// Configuration de l'affichage des colonnes.
    /// </summary>
    public TableViewConfigurationViewModel Configuration { get; }

    /// <summary>
    /// Liste des colonnes et leur ordre.
    /// </summary>
    public ObservableCollection<TableColumnOrderViewModel> Columns { get; }

    /// <summary>
    /// Filtres par type (Workers, Titres, etc.).
    /// </summary>
    public ObservableCollection<TableFilterOptionViewModel> TypeFilters { get; }

    /// <summary>
    /// Filtres par statut (Actif, Blessé, etc.).
    /// </summary>
    public ObservableCollection<TableFilterOptionViewModel> StatusFilters { get; }

    /// <summary>
    /// Élément sélectionné dans la table.
    /// </summary>
    public TableViewItemViewModel? Selection
    {
        get => _selection;
        set
        {
            this.RaiseAndSetIfChanged(ref _selection, value);
            this.RaisePropertyChanged(nameof(HasSelection));
            this.RaisePropertyChanged(nameof(SelectionContext));
        }
    }

    /// <summary>
    /// Texte de recherche.
    /// </summary>
    public string? SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            ApplyFilter();
            SavePreferences();
        }
    }

    /// <summary>
    /// Filtre de type sélectionné.
    /// </summary>
    public TableFilterOptionViewModel? SelectedTypeFilter
    {
        get => _selectedTypeFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTypeFilter, value);
            ApplyFilter();
            SavePreferences();
        }
    }

    /// <summary>
    /// Filtre de statut sélectionné.
    /// </summary>
    public TableFilterOptionViewModel? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedStatusFilter, value);
            ApplyFilter();
            SavePreferences();
        }
    }

    /// <summary>
    /// Résumé des résultats (ex: "Résultats : 10 / 50").
    /// </summary>
    public string? ResultsResume
    {
        get => _resultsResume;
        private set => this.RaiseAndSetIfChanged(ref _resultsResume, value);
    }

    public IReadOnlyList<TableSortSetting> SortSettings => _sortSettings;
    public bool HasSelection => Selection is not null;
    public string SelectionContext => Selection is null
        ? "Aucune fiche sélectionnée"
        : $"{Selection.Nom} · {Selection.Type}";

    /// <summary>
    /// Met à jour les éléments de la table depuis le contexte du show.
    /// </summary>
    public void UpdateItems(ShowContext context)
    {
        Items.Clear();

        if (context is null)
        {
            return;
        }

        // Ajouter la compagnie
        Items.Add(new TableViewItemViewModel(
            context.Compagnie.CompagnieId,
            context.Compagnie.Nom,
            "Compagnie",
            context.Compagnie.Nom,
            "-",
            $"Prestige {context.Compagnie.Prestige}",
            0, // CompanyState n'a pas de Popularite, utiliser 0
            0,
            0,
            $"Prestige {context.Compagnie.Prestige}",
            Array.Empty<string>()));

        // Ajouter les workers
        foreach (var worker in context.Workers)
        {
            var note = (int)Math.Round((worker.InRing + worker.Entertainment + worker.Story) / 3.0);
            Items.Add(new TableViewItemViewModel(
                worker.WorkerId,
                worker.NomComplet,
                "Worker",
                context.Compagnie.Nom,
                worker.RoleTv,
                string.IsNullOrWhiteSpace(worker.Blessure) ? "Actif" : "Blessé",
                worker.Popularite,
                worker.Momentum,
                note,
                $"{worker.RoleTv} • Pop {worker.Popularite} • Note {note}",
                Array.Empty<string>()));
        }

        // Ajouter les titres
        foreach (var titre in context.Titres)
        {
            var detenteur = context.Workers.FirstOrDefault(w => w.WorkerId == titre.DetenteurId);
            var statut = detenteur is null ? "Vacant" : "Défendu";
            var detenteurNom = detenteur?.NomComplet ?? "Vacant";
            Items.Add(new TableViewItemViewModel(
                titre.TitreId,
                titre.Nom,
                "Titre",
                "-",
                "-",
                statut,
                titre.Prestige,
                0,
                0,
                $"Détenteur: {detenteurNom}",
                Array.Empty<string>()));
        }

        // Ajouter les storylines
        foreach (var storyline in context.Storylines)
        {
            var statut = GetStatusLabel(storyline.Status);
            var participantsCount = storyline.Participants.Count;
            Items.Add(new TableViewItemViewModel(
                storyline.StorylineId,
                storyline.Nom,
                "Storyline",
                "-",
                "-",
                statut,
                0,
                0,
                0,
                $"{participantsCount} participant(s) • {statut}",
                Array.Empty<string>()));
        }

        UpdateResultsResume();
    }

    /// <summary>
    /// Filtre les éléments selon les critères.
    /// </summary>
    private bool FilterItems(object? item)
    {
        if (item is not TableViewItemViewModel vm)
        {
            return false;
        }

        // Filtre par type
        if (SelectedTypeFilter?.Id != "Tous" && vm.Type != SelectedTypeFilter?.Id)
        {
            return false;
        }

        // Filtre par statut
        if (SelectedStatusFilter?.Id != "Tous" && vm.Statut != SelectedStatusFilter?.Id)
        {
            return false;
        }

        // Filtre par recherche
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            return vm.Nom.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                   vm.Type.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    /// <summary>
    /// Applique les filtres et met à jour la vue.
    /// </summary>
    public void ApplyFilter()
    {
        ItemsView.Refresh();
        UpdateResultsResume();
    }

    private void UpdateResultsResume()
    {
        ResultsResume = $"Résultats : {ItemsView.Count} / {Items.Count}";
    }

    public void MoveColumnUp(TableColumnOrderViewModel column)
    {
        var index = Columns.IndexOf(column);
        if (index <= 0)
        {
            return;
        }

        Columns.Move(index, index - 1);
        SavePreferences();
    }

    public void MoveColumnDown(TableColumnOrderViewModel column)
    {
        var index = Columns.IndexOf(column);
        if (index < 0 || index >= Columns.Count - 1)
        {
            return;
        }

        Columns.Move(index, index + 1);
        SavePreferences();
    }

    public void SortByColumn(string columnId, bool ascending)
    {
        _sortSettings.Clear();
        _sortSettings.Add(new TableSortSetting(columnId, ascending ? TableSortDirection.Ascending : TableSortDirection.Descending));
        ApplySort();
        SavePreferences();
    }

    public void AddSecondarySort(string columnId, bool ascending)
    {
        _sortSettings.Add(new TableSortSetting(columnId, ascending ? TableSortDirection.Ascending : TableSortDirection.Descending));
        ApplySort();
        SavePreferences();
    }

    private void ApplySort()
    {
        ItemsView.Refresh();
        // TODO: Implement actual sorting logic with _sortSettings
    }

    private void LoadPreferences()
    {
        if (_repository is null)
        {
            return;
        }

        _suspendPreferences = true;

        try
        {
            var settings = _repository.ChargerTableUiSettings();
            if (settings is null)
            {
                _suspendPreferences = false;
                return;
            }

            SearchText = settings.Recherche;
            SelectedTypeFilter = TypeFilters.FirstOrDefault(f => f.Id.Equals(settings.FiltreType, StringComparison.OrdinalIgnoreCase))
                ?? TypeFilters[0];
            SelectedStatusFilter = StatusFilters.FirstOrDefault(f => f.Id.Equals(settings.FiltreStatut, StringComparison.OrdinalIgnoreCase))
                ?? StatusFilters[0];

            // Load column visibility
            foreach (var kvp in settings.ColonnesVisibles)
            {
                if (kvp.Key == "Type") Configuration.AfficherType = kvp.Value;
                else if (kvp.Key == "Compagnie") Configuration.AfficherCompagnie = kvp.Value;
                else if (kvp.Key == "Role") Configuration.AfficherRole = kvp.Value;
                else if (kvp.Key == "Statut") Configuration.AfficherStatut = kvp.Value;
                else if (kvp.Key == "Popularite") Configuration.AfficherPopularite = kvp.Value;
                else if (kvp.Key == "Momentum") Configuration.AfficherMomentum = kvp.Value;
                else if (kvp.Key == "Note") Configuration.AfficherNote = kvp.Value;
            }

            // Load column order
            if (settings.ColonnesOrdre?.Any() == true)
            {
                var mapping = Columns.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
                var newOrder = settings.ColonnesOrdre
                    .Select(id => mapping.TryGetValue(id, out var col) ? col : null)
                    .Where(col => col is not null)
                    .Select(col => col!)
                    .ToList();

                foreach (var col in Columns)
                {
                    if (!newOrder.Contains(col))
                    {
                        newOrder.Add(col);
                    }
                }

                Columns.Clear();
                foreach (var col in newOrder)
                {
                    Columns.Add(col);
                }
            }
        }
        finally
        {
            _suspendPreferences = false;
        }
    }

    private void SavePreferences()
    {
        if (_suspendPreferences || _repository is null)
        {
            return;
        }

        var colonnesVisibles = new Dictionary<string, bool>
        {
            ["Type"] = Configuration.AfficherType,
            ["Compagnie"] = Configuration.AfficherCompagnie,
            ["Role"] = Configuration.AfficherRole,
            ["Statut"] = Configuration.AfficherStatut,
            ["Popularite"] = Configuration.AfficherPopularite,
            ["Momentum"] = Configuration.AfficherMomentum,
            ["Note"] = Configuration.AfficherNote
        };

        var settings = new TableUiSettings(
            SearchText ?? string.Empty,
            SelectedTypeFilter?.Id ?? "Tous",
            SelectedStatusFilter?.Id ?? "Tous",
            colonnesVisibles,
            Columns.Select(c => c.Id).ToList(),
            _sortSettings.ToList());

        _repository.SauvegarderTableUiSettings(settings);
    }

    private static string GetStatusLabel(StorylineStatus status)
    {
        return status switch
        {
            StorylineStatus.Active => "Actif",
            StorylineStatus.Suspended => "En pause",
            StorylineStatus.Completed => "Terminée",
            _ => "Inconnue"
        };
    }
}

/// <summary>
/// Wrapper pour ObservableCollection avec support du filtrage.
/// Fournit une vue filtrée d'une collection source.
/// </summary>
public sealed class DataGridCollectionView
{
    private readonly ObservableCollection<TableViewItemViewModel> _source;
    private int _cachedCount;

    public DataGridCollectionView(ObservableCollection<TableViewItemViewModel> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        Filter = null;
        _cachedCount = _source.Count;
    }

    /// <summary>
    /// Prédicat de filtrage appliqué aux éléments.
    /// </summary>
    public Predicate<object?>? Filter { get; set; }

    /// <summary>
    /// Nombre d'éléments après application du filtre.
    /// </summary>
    public int Count => _cachedCount;

    /// <summary>
    /// Recalcule le filtre et met à jour le compteur.
    /// </summary>
    public void Refresh()
    {
        if (Filter is null)
        {
            _cachedCount = _source.Count;
        }
        else
        {
            _cachedCount = _source.Count(item => Filter(item));
        }
    }
}
