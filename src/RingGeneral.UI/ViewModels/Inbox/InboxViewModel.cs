using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Inbox;

/// <summary>
/// ViewModel pour la gestion de la boîte de réception (inbox).
/// Affiche les notifications et événements importants du jeu.
/// </summary>
public sealed class InboxViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;

    public InboxViewModel()
    {
        Items = new ObservableCollection<InboxItemViewModel>();
    }

    public InboxViewModel(GameRepository repository) : this()
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Liste des éléments de la boîte de réception.
    /// </summary>
    public ObservableCollection<InboxItemViewModel> Items { get; }

    /// <summary>
    /// Nombre total d'éléments dans l'inbox.
    /// </summary>
    public int TotalItems => Items.Count;

    /// <summary>
    /// Nombre d'éléments non lus.
    /// </summary>
    public int UnreadItems => Items.Count(item => !item.IsRead);

    /// <summary>
    /// Indique s'il y a des éléments non lus.
    /// </summary>
    public bool HasUnreadItems => UnreadItems > 0;

    /// <summary>
    /// Charge les éléments de l'inbox depuis le repository.
    /// </summary>
    public void Load()
    {
        if (_repository is null)
        {
            Logger.Warning("Impossible de charger l'inbox : repository non initialisé");
            return;
        }

        Items.Clear();

        try
        {
            foreach (var item in _repository.ChargerInbox())
            {
                Items.Add(new InboxItemViewModel(item));
            }

            Logger.Debug($"Inbox chargé : {Items.Count} éléments");
            this.RaisePropertyChanged(nameof(TotalItems));
            this.RaisePropertyChanged(nameof(UnreadItems));
            this.RaisePropertyChanged(nameof(HasUnreadItems));
        }
        catch (Exception ex)
        {
            Logger.Error("Erreur lors du chargement de l'inbox", ex);
        }
    }

    /// <summary>
    /// Marque un élément comme lu.
    /// </summary>
    public void MarkAsRead(InboxItemViewModel item)
    {
        if (item is null)
        {
            return;
        }

        item.IsRead = true;
        this.RaisePropertyChanged(nameof(UnreadItems));
        this.RaisePropertyChanged(nameof(HasUnreadItems));
    }

    /// <summary>
    /// Marque tous les éléments comme lus.
    /// </summary>
    public void MarkAllAsRead()
    {
        foreach (var item in Items)
        {
            item.IsRead = true;
        }

        this.RaisePropertyChanged(nameof(UnreadItems));
        this.RaisePropertyChanged(nameof(HasUnreadItems));
    }

    /// <summary>
    /// Supprime un élément de l'inbox.
    /// </summary>
    public void Remove(InboxItemViewModel item)
    {
        if (item is null)
        {
            return;
        }

        Items.Remove(item);
        this.RaisePropertyChanged(nameof(TotalItems));
        this.RaisePropertyChanged(nameof(UnreadItems));
        this.RaisePropertyChanged(nameof(HasUnreadItems));
    }

    /// <summary>
    /// Vide complètement l'inbox.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        this.RaisePropertyChanged(nameof(TotalItems));
        this.RaisePropertyChanged(nameof(UnreadItems));
        this.RaisePropertyChanged(nameof(HasUnreadItems));
    }
}

/// <summary>
/// ViewModel pour un élément individuel de l'inbox.
/// </summary>
public sealed class InboxItemViewModel : ViewModelBase
{
    private bool _isRead;

    public InboxItemViewModel(InboxItem item)
    {
        Id = $"{item.Type}_{item.Semaine}_{Guid.NewGuid():N}";
        Type = item.Type;
        Titre = item.Titre;
        Message = item.Contenu;
        Semaine = item.Semaine;
        _isRead = false;
    }

    public string Id { get; }
    public string Type { get; }
    public string Titre { get; }
    public string Message { get; }
    public int Semaine { get; }

    public bool IsRead
    {
        get => _isRead;
        set
        {
            if (_isRead != value)
            {
                _isRead = value;
                this.RaisePropertyChanged(nameof(IsRead));
            }
        }
    }

    /// <summary>
    /// Date formatée pour l'affichage.
    /// </summary>
    public string FormattedDate => $"Semaine {Semaine}";

    /// <summary>
    /// Icône basée sur le type d'élément.
    /// </summary>
    public string Icon => Type switch
    {
        "Injury" => "🏥",
        "Contract" => "📝",
        "Backstage" => "🎭",
        "Rivalry" => "⚔️",
        "Achievement" => "🏆",
        _ => "📬"
    };
}
