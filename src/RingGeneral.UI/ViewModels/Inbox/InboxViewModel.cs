using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private InboxItemViewModel? _selectedItem;
    private InboxTab _selectedTab = InboxTab.All;

    public InboxViewModel()
    {
        Items = new ObservableCollection<InboxItemViewModel>();
        FilteredItems = new ObservableCollection<InboxItemViewModel>();
        Items.CollectionChanged += (_, _) => UpdateFilter();
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
    /// Liste filtrée selon l'onglet sélectionné.
    /// </summary>
    public ObservableCollection<InboxItemViewModel> FilteredItems { get; }

    /// <summary>
    /// Élément sélectionné.
    /// </summary>
    public InboxItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    /// <summary>
    /// Onglet sélectionné dans l'inbox.
    /// </summary>
    public InboxTab SelectedTab
    {
        get => _selectedTab;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTab, value);
            this.RaisePropertyChanged(nameof(IsAllTabSelected));
            this.RaisePropertyChanged(nameof(IsContractsTabSelected));
            this.RaisePropertyChanged(nameof(IsMedicalTabSelected));
            UpdateFilter();
        }
    }

    public bool IsAllTabSelected
    {
        get => SelectedTab == InboxTab.All;
        set
        {
            if (value)
            {
                SelectedTab = InboxTab.All;
            }
        }
    }

    public bool IsContractsTabSelected
    {
        get => SelectedTab == InboxTab.Contracts;
        set
        {
            if (value)
            {
                SelectedTab = InboxTab.Contracts;
            }
        }
    }

    public bool IsMedicalTabSelected
    {
        get => SelectedTab == InboxTab.Medical;
        set
        {
            if (value)
            {
                SelectedTab = InboxTab.Medical;
            }
        }
    }

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
            UpdateFilter();
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
        UpdateFilter();
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
        UpdateFilter();
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
        UpdateFilter();
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
        UpdateFilter();
    }

    private void UpdateFilter()
    {
        FilteredItems.Clear();

        foreach (var item in Items.Where(ShouldIncludeItem))
        {
            FilteredItems.Add(item);
        }

        if (SelectedItem is not null && !FilteredItems.Contains(SelectedItem))
        {
            SelectedItem = null;
        }
    }

    private bool ShouldIncludeItem(InboxItemViewModel item)
    {
        return SelectedTab switch
        {
            InboxTab.Contracts => item.Type.Equals("Contract", StringComparison.OrdinalIgnoreCase),
            InboxTab.Medical => item.Type.Equals("Injury", StringComparison.OrdinalIgnoreCase)
                || item.Type.Equals("Medical", StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }
}

public enum InboxTab
{
    All,
    Contracts,
    Medical
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

    public string BadgeLabel => Type switch
    {
        "Injury" => "Médical",
        "Contract" => "Contrat",
        "Backstage" => "Backstage",
        "Rivalry" => "Rivalité",
        "Achievement" => "Succès",
        _ => "Système"
    };

    public string BadgeBackground => Type switch
    {
        "Injury" => "#065f46",
        "Contract" => "#1e3a8a",
        "Backstage" => "#4b5563",
        "Rivalry" => "#7f1d1d",
        "Achievement" => "#7c2d12",
        _ => "#1e3a8a"
    };

    public string BadgeForeground => Type switch
    {
        "Injury" => "#6ee7b7",
        "Contract" => "#93c5fd",
        "Backstage" => "#e5e7eb",
        "Rivalry" => "#fecaca",
        "Achievement" => "#fde68a",
        _ => "#93c5fd"
    };

    public string AccentColor => Type switch
    {
        "Injury" => "#10b981",
        "Contract" => "#3b82f6",
        "Backstage" => "#6b7280",
        "Rivalry" => "#ef4444",
        "Achievement" => "#f59e0b",
        _ => "#3b82f6"
    };
}
