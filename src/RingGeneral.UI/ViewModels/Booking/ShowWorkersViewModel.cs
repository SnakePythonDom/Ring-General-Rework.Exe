using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Collections;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la gestion des participants d'un show.
/// Responsable de la sélection et assignation des workers aux segments.
/// Extrait depuis GameSessionViewModel (Phase 6.4).
/// </summary>
public sealed class ShowWorkersViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private ShowContext? _context;
    private string? _searchFilter;

    public ShowWorkersViewModel(GameRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        AvailableWorkers = new ObservableCollection<ParticipantViewModel>();
        AvailableWorkersView = new DataGridCollectionView(AvailableWorkers)
        {
            Filter = FilterWorkers
        };
        SelectedParticipants = new ObservableCollection<ParticipantViewModel>();

        AddParticipantCommand = ReactiveCommand.Create<ParticipantViewModel>(AddParticipant);
        RemoveParticipantCommand = ReactiveCommand.Create<ParticipantViewModel>(RemoveParticipant);
        ClearParticipantsCommand = ReactiveCommand.Create(ClearParticipants);
    }

    #region Collections

    /// <summary>
    /// Liste des workers disponibles pour le booking.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> AvailableWorkers { get; }

    /// <summary>
    /// Vue filtrée des workers disponibles.
    /// </summary>
    public DataGridCollectionView AvailableWorkersView { get; }

    /// <summary>
    /// Participants sélectionnés pour le segment en cours.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> SelectedParticipants { get; }

    #endregion

    #region Properties

    /// <summary>
    /// Filtre de recherche pour les workers.
    /// </summary>
    public string? SearchFilter
    {
        get => _searchFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchFilter, value);
            AvailableWorkersView.Refresh();
            Logger.Debug($"Filtre workers appliqué : '{value}'");
        }
    }

    /// <summary>
    /// Nombre de workers disponibles (après filtrage).
    /// </summary>
    public int AvailableCount => AvailableWorkersView.Count;

    /// <summary>
    /// Nombre de participants sélectionnés.
    /// </summary>
    public int SelectedCount => SelectedParticipants.Count;

    /// <summary>
    /// Indique si des participants sont sélectionnés.
    /// </summary>
    public bool HasParticipants => SelectedParticipants.Any();

    #endregion

    #region Commands

    public ReactiveCommand<ParticipantViewModel, Unit> AddParticipantCommand { get; }
    public ReactiveCommand<ParticipantViewModel, Unit> RemoveParticipantCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearParticipantsCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Charge les workers disponibles depuis le contexte.
    /// </summary>
    public void LoadAvailableWorkers(ShowContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        AvailableWorkers.Clear();

        foreach (var worker in context.Workers.OrderBy(w => w.NomComplet))
        {
            // Vérifier disponibilité (pas blessé gravement, etc.)
            if (IsWorkerAvailable(worker))
            {
                AvailableWorkers.Add(new ParticipantViewModel(worker));
            }
        }

        this.RaisePropertyChanged(nameof(AvailableCount));
        Logger.Info($"{AvailableWorkers.Count} workers disponibles chargés");
    }

    /// <summary>
    /// Charge les participants d'un segment.
    /// </summary>
    public void LoadSegmentParticipants(SegmentDefinition segment)
    {
        SelectedParticipants.Clear();

        if (_context is null || segment is null)
        {
            return;
        }

        foreach (var workerId in segment.Participants)
        {
            var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == workerId);
            if (worker is not null)
            {
                SelectedParticipants.Add(new ParticipantViewModel(worker));
            }
        }

        this.RaisePropertyChanged(nameof(SelectedCount));
        this.RaisePropertyChanged(nameof(HasParticipants));
        Logger.Debug($"Segment participants chargés : {SelectedParticipants.Count}");
    }

    /// <summary>
    /// Charge les participants depuis une liste d'IDs.
    /// </summary>
    public void LoadParticipants(IEnumerable<string> workerIds)
    {
        SelectedParticipants.Clear();

        if (_context is null || workerIds is null)
        {
            return;
        }

        foreach (var workerId in workerIds)
        {
            var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == workerId);
            if (worker is not null)
            {
                SelectedParticipants.Add(new ParticipantViewModel(worker));
            }
        }

        this.RaisePropertyChanged(nameof(SelectedCount));
        this.RaisePropertyChanged(nameof(HasParticipants));
    }

    /// <summary>
    /// Obtient les IDs des participants sélectionnés.
    /// </summary>
    public List<string> GetSelectedWorkerIds()
    {
        return SelectedParticipants.Select(p => p.WorkerId).ToList();
    }

    /// <summary>
    /// Ajoute un participant à la sélection.
    /// </summary>
    public void AddParticipant(ParticipantViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        // Vérifier si déjà présent
        if (SelectedParticipants.Any(p => p.WorkerId == participant.WorkerId))
        {
            Logger.Debug($"Participant déjà sélectionné : {participant.Nom}");
            return;
        }

        SelectedParticipants.Add(participant);

        this.RaisePropertyChanged(nameof(SelectedCount));
        this.RaisePropertyChanged(nameof(HasParticipants));

        Logger.Debug($"Participant ajouté : {participant.Nom}");
    }

    /// <summary>
    /// Retire un participant de la sélection.
    /// </summary>
    public void RemoveParticipant(ParticipantViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        SelectedParticipants.Remove(participant);

        this.RaisePropertyChanged(nameof(SelectedCount));
        this.RaisePropertyChanged(nameof(HasParticipants));

        Logger.Debug($"Participant retiré : {participant.Nom}");
    }

    /// <summary>
    /// Vide la sélection de participants.
    /// </summary>
    public void ClearParticipants()
    {
        var count = SelectedParticipants.Count;
        SelectedParticipants.Clear();

        this.RaisePropertyChanged(nameof(SelectedCount));
        this.RaisePropertyChanged(nameof(HasParticipants));

        Logger.Debug($"{count} participants retirés");
    }

    /// <summary>
    /// Calcule la compatibilité entre deux workers pour un match.
    /// </summary>
    public int CalculateCompatibility(ParticipantViewModel worker1, ParticipantViewModel worker2)
    {
        if (worker1 is null || worker2 is null)
        {
            return 0;
        }

        // Calcul basique de compatibilité
        // TODO: Implémenter logique avancée (styles, relation, etc.)

        var compatibility = 50; // Base

        // Bonus si styles similaires
        if (Math.Abs(worker1.InRing - worker2.InRing) < 20)
        {
            compatibility += 10;
        }

        // Bonus si popularité similaire
        if (Math.Abs(worker1.Popularite - worker2.Popularite) < 20)
        {
            compatibility += 10;
        }

        // Malus si trop de différence de niveau
        if (Math.Abs(worker1.InRing - worker2.InRing) > 40)
        {
            compatibility -= 15;
        }

        return Math.Clamp(compatibility, 0, 100);
    }

    /// <summary>
    /// Obtient les meilleurs matchups pour un worker donné.
    /// </summary>
    public List<(ParticipantViewModel Worker, int Compatibility)> GetBestMatchups(ParticipantViewModel worker, int count = 5)
    {
        if (worker is null || _context is null)
        {
            return new List<(ParticipantViewModel, int)>();
        }

        var matchups = AvailableWorkers
            .Where(w => w.WorkerId != worker.WorkerId)
            .Select(w => (Worker: w, Compatibility: CalculateCompatibility(worker, w)))
            .OrderByDescending(m => m.Compatibility)
            .Take(count)
            .ToList();

        return matchups;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Filtre les workers selon le critère de recherche.
    /// </summary>
    private bool FilterWorkers(object? item)
    {
        if (item is not ParticipantViewModel worker)
        {
            return false;
        }

        // Pas de filtre = tout afficher
        if (string.IsNullOrWhiteSpace(SearchFilter))
        {
            return true;
        }

        // Recherche case-insensitive dans le nom
        return worker.Nom.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
               worker.RoleTv.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Vérifie si un worker est disponible pour le booking.
    /// </summary>
    private bool IsWorkerAvailable(Worker worker)
    {
        if (worker is null)
        {
            return false;
        }

        // Pas blessé gravement
        if (!string.IsNullOrWhiteSpace(worker.Blessure))
        {
            // TODO: Vérifier gravité blessure
            // Pour l'instant, on exclut tous les blessés
            return false;
        }

        // Pas suspendu
        // TODO: Vérifier suspension dans système disciplinaire

        // A un contrat actif
        // TODO: Vérifier statut contrat

        return true;
    }

    #endregion
}
