using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Search;

/// <summary>
/// ViewModel pour la recherche globale dans l'application.
/// Permet de rechercher workers, storylines, titres, etc.
/// </summary>
public sealed class GlobalSearchViewModel : ViewModelBase
{
    private readonly List<GlobalSearchResultViewModel> _searchIndex = new();
    private bool _isVisible;
    private string? _query;
    private bool _hasNoResults;

    public GlobalSearchViewModel()
    {
        Results = new ObservableCollection<GlobalSearchResultViewModel>();
        OpenCommand = ReactiveCommand.Create(Open);
        CloseCommand = ReactiveCommand.Create(Close);
    }

    /// <summary>
    /// Résultats de recherche affichés.
    /// </summary>
    public ObservableCollection<GlobalSearchResultViewModel> Results { get; }

    /// <summary>
    /// Indique si le panneau de recherche est visible.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        private set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    /// <summary>
    /// Requête de recherche entrée par l'utilisateur.
    /// </summary>
    public string? Query
    {
        get => _query;
        set
        {
            this.RaiseAndSetIfChanged(ref _query, value);
            UpdateResults();
        }
    }

    /// <summary>
    /// Indique si aucun résultat n'a été trouvé.
    /// </summary>
    public bool HasNoResults
    {
        get => _hasNoResults;
        private set => this.RaiseAndSetIfChanged(ref _hasNoResults, value);
    }

    /// <summary>
    /// Commande pour ouvrir le panneau de recherche.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenCommand { get; }

    /// <summary>
    /// Commande pour fermer le panneau de recherche.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    /// <summary>
    /// Ouvre le panneau de recherche globale.
    /// </summary>
    public void Open()
    {
        IsVisible = true;
        Query ??= string.Empty;
        UpdateResults();
    }

    /// <summary>
    /// Ferme le panneau de recherche globale.
    /// </summary>
    public void Close()
    {
        IsVisible = false;
    }

    /// <summary>
    /// Ouvre la recherche avec une requête pré-remplie (ex: nom d'un worker).
    /// </summary>
    public void OpenWithQuery(string query)
    {
        Open();
        Query = query;
    }

    /// <summary>
    /// Met à jour l'index de recherche depuis le contexte du show.
    /// </summary>
    /// <param name="context">Contexte du show contenant workers, titres, storylines, etc.</param>
    public void UpdateIndex(ShowContext context)
    {
        _searchIndex.Clear();

        if (context is null)
        {
            return;
        }

        // Indexer les workers
        foreach (var worker in context.Workers)
        {
            _searchIndex.Add(new GlobalSearchResultViewModel(
                "Worker",
                worker.NomComplet,
                $"{worker.RoleTv} • Popularité {worker.Popularite}",
                string.IsNullOrWhiteSpace(worker.Blessure) ? "Actif" : "Blessé"));
        }

        // Indexer la compagnie
        _searchIndex.Add(new GlobalSearchResultViewModel(
            "Compagnie",
            context.Compagnie.Nom,
            $"{context.Compagnie.Region} • Prestige {context.Compagnie.Prestige}",
            "Info"));

        // Indexer les titres
        foreach (var titre in context.Titres)
        {
            var detenteur = context.Workers.FirstOrDefault(w => w.WorkerId == titre.DetenteurId)?.NomComplet ?? "Vacant";
            _searchIndex.Add(new GlobalSearchResultViewModel(
                "Titre",
                titre.Nom,
                $"Détenteur {detenteur}",
                "Info"));
        }

        // Indexer les storylines
        foreach (var storyline in context.Storylines)
        {
            var participants = storyline.Participants
                .Select(p => context.Workers.FirstOrDefault(w => w.WorkerId == p.WorkerId)?.NomComplet)
                .Where(nom => nom is not null)
                .Select(nom => nom!)
                .Take(3);
            var phase = GetPhaseLabel(storyline.Phase);
            _searchIndex.Add(new GlobalSearchResultViewModel(
                "Storyline",
                storyline.Nom,
                $"Participants {string.Join(", ", participants)} • Phase {phase}",
                "Info"));
        }

        UpdateResults();
    }

    /// <summary>
    /// Met à jour les résultats de recherche basés sur la requête.
    /// </summary>
    private void UpdateResults()
    {
        Results.Clear();
        var recherche = Query?.Trim();

        var resultats = string.IsNullOrWhiteSpace(recherche)
            ? _searchIndex
            : _searchIndex.Where(item =>
                item.Nom.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(recherche, StringComparison.OrdinalIgnoreCase));

        var liste = resultats.Take(12).ToList();
        foreach (var resultat in liste)
        {
            Results.Add(resultat);
        }

        HasNoResults = liste.Count == 0;
    }

    private static string GetPhaseLabel(StorylinePhase phase)
    {
        return phase switch
        {
            StorylinePhase.Setup => "Configuration",
            StorylinePhase.Rising => "Développement",
            StorylinePhase.Climax => "Climax",
            StorylinePhase.Fallout => "Résolution",
            _ => "Inconnue"
        };
    }
}

/// <summary>
/// ViewModel pour un résultat de recherche globale.
/// </summary>
public sealed class GlobalSearchResultViewModel
{
    public GlobalSearchResultViewModel(string type, string nom, string description, string statut)
    {
        Type = type;
        Nom = nom;
        Description = description;
        Statut = statut;
    }

    public string Type { get; }
    public string Nom { get; }
    public string Description { get; }
    public string Statut { get; }
}
