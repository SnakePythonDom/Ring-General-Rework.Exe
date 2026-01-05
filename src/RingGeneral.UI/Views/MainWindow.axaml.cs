using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels;

namespace RingGeneral.UI.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ShellViewModel();
        InitialiserTableView();
    }

    private DataGrid? _tableViewGrid;
    private GameSessionViewModel? _sessionTableView;

    private void InitialiserTableView()
    {
        _tableViewGrid = this.FindControl<DataGrid>("TableViewGrid");
        if (_tableViewGrid is null || DataContext is not ShellViewModel shell)
        {
            return;
        }

        shell.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ShellViewModel.Session))
            {
                AttacherSession(shell.Session);
            }
        };

        AttacherSession(shell.Session);
    }

    private void AttacherSession(GameSessionViewModel session)
    {
        if (_sessionTableView is not null)
        {
            _sessionTableView.TableColumns.CollectionChanged -= OnTableColumnsChanged;
        }

        _sessionTableView = session;
        _sessionTableView.TableColumns.CollectionChanged += OnTableColumnsChanged;
        AppliquerOrdreColonnes();
        AppliquerTriColonnes();
    }

    private void OnTableColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        AppliquerOrdreColonnes();
    }

    private void AppliquerOrdreColonnes()
    {
        if (_tableViewGrid is null || DataContext is not ShellViewModel shell)
        {
            return;
        }

        var colonnesParId = _tableViewGrid.Columns
            .Where(colonne => colonne.Tag is string)
            .ToDictionary(colonne => (string)colonne.Tag!, StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < shell.Session.TableColumns.Count; index++)
        {
            var colonneId = shell.Session.TableColumns[index].Id;
            if (colonnesParId.TryGetValue(colonneId, out var colonne))
            {
                colonne.DisplayIndex = index;
            }
        }
    }

    private void AppliquerTriColonnes()
    {
        if (_tableViewGrid is null || DataContext is not ShellViewModel shell)
        {
            return;
        }
    }

    private void OnSimulerShow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.SimulerShow();
        }
    }

    private void OnAjouterDepuisBibliotheque(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AppliquerTemplateSelectionnee();
        }
    }

    private void OnAppliquerTemplateBibliotheque(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentTemplateViewModel template)
        {
            shell.Session.AppliquerTemplate(template);
        }
    }

    private void OnSemaineSuivante(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.PasserSemaineSuivante();
        }
    }

    private void OnOuvrirPage(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string route)
        {
            shell.OuvrirPage(route);
        }
    }

    private void OnTopbarAction(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell || sender is not Control control || control.Tag is not string actionId)
        {
            return;
        }

        switch (actionId)
        {
            case "topbar.recherche":
                shell.Session.OuvrirRechercheGlobaleCommand.Execute().Subscribe();
                break;
            case "topbar.parametres":
                shell.OuvrirPage("/parametres");
                break;
        }
    }

    private void OnOuvrirAide(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string pageId)
        {
            shell.Session.OuvrirAide(pageId);
        }
    }

    private void OnFermerAide(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.FermerAide();
        }
    }

    private void OnOuvrirImpact(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string pageId)
        {
            shell.Session.SelectionnerImpact(pageId);
        }
    }

    private void OnOuvrirFicheWorker(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control)
        {
            shell.Session.OuvrirFicheWorker(control.Tag as string);
        }
    }

    private void OnOuvrirArticle(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string articleId)
        {
            shell.Session.Codex.OuvrirArticle(articleId);
        }
    }

    private void OnChangerBudgetYouth(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.ChangerBudgetYouth();
        }
    }

    private void OnAffecterCoachYouth(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AffecterCoachYouth();
        }
    }

    private void OnGraduationYouth(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string workerId)
        {
            shell.Session.DiplomerTrainee(workerId);
        }
    }

    private void OnToggleDetailsSimulation(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.DetailsSimulationVisible = !shell.Session.DetailsSimulationVisible;
        }
    }

    private void OnNouvelleSauvegarde(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            var slot = shell.Saves.CreerNouvelleSauvegarde();
            if (slot is not null)
            {
                shell.ChargerSauvegarde(slot);
            }
        }
    }

    private void OnChargerSauvegarde(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            var slot = shell.Saves.SauvegardeSelectionnee ?? shell.Saves.SauvegardeCourante;
            if (slot is not null)
            {
                shell.ChargerSauvegarde(slot);
            }
            else
            {
                shell.Saves.SignalerErreur("Sélectionnez une sauvegarde à charger.");
            }
        }
    }

    private void OnTableSorting(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell || sender is not DataGrid grid)
        {
            return;
        }

        var colonne = grid.CurrentColumn;
        if (colonne is null)
        {
            return;
        }

        var colonneId = colonne.SortMemberPath ?? colonne.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(colonneId))
        {
            return;
        }

        shell.Session.MettreAJourTriTable(colonneId);
        AppliquerTriColonnes();
    }

    private void OnTableColumnUp(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell || sender is not Control control || control.Tag is not TableColumnOrderViewModel colonne)
        {
            return;
        }

        shell.Session.MonterColonne(colonne);
        AppliquerOrdreColonnes();
    }

    private void OnTableColumnDown(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell || sender is not Control control || control.Tag is not TableColumnOrderViewModel colonne)
        {
            return;
        }

        shell.Session.DescendreColonne(colonne);
        AppliquerOrdreColonnes();
    }

    private void OnReinitialiserTriTable(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell)
        {
            return;
        }

        shell.Session.ReinitialiserTriTable();
        AppliquerTriColonnes();
    }

    private async void OnImporterDb(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Importer une base de données",
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Base SQLite", Extensions = { "db", "sqlite" } },
                new() { Name = "Tous les fichiers", Extensions = { "*" } }
            }
        };

        var result = await dialog.ShowAsync(this);
        var chemin = result?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(chemin))
        {
            return;
        }

        var slot = shell.Saves.ImporterBase(chemin);
        if (slot is not null)
        {
            shell.ChargerSauvegarde(slot);
        }
    }

    private async void OnExporterDb(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell)
        {
            return;
        }

        var slot = shell.Saves.SauvegardeSelectionnee ?? shell.Saves.SauvegardeCourante;
        if (slot is null)
        {
            shell.Saves.SignalerErreur("Sélectionnez une sauvegarde à exporter.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Exporter une base de données",
            InitialFileName = $"{slot.Nom}.db",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Base SQLite", Extensions = { "db" } },
                new() { Name = "Tous les fichiers", Extensions = { "*" } }
            }
        };

        var chemin = await dialog.ShowAsync(this);
        if (string.IsNullOrWhiteSpace(chemin))
        {
            return;
        }

        shell.Saves.ExporterBase(chemin, slot);
    }

    private async void OnExporterPack(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell)
        {
            return;
        }

        var slot = shell.Saves.SauvegardeSelectionnee ?? shell.Saves.SauvegardeCourante;
        if (slot is null)
        {
            shell.Saves.SignalerErreur("Sélectionnez une sauvegarde à exporter.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Exporter un pack de sauvegarde",
            InitialFileName = $"{slot.Nom}.zip",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Pack Ring General", Extensions = { "zip" } }
            }
        };

        var chemin = await dialog.ShowAsync(this);
        if (string.IsNullOrWhiteSpace(chemin))
        {
            return;
        }

        shell.Saves.ExporterPack(chemin, slot);
    }

    private void OnAjouterSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AjouterSegment();
        }
    }

    private void OnCopierSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentViewModel segment)
        {
            shell.Session.CopierSegment(segment);
        }
    }

    private void OnDupliquerMatch(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentViewModel segment)
        {
            shell.Session.DupliquerMatch(segment);
        }
    }

    private void OnSupprimerSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentViewModel segment)
        {
            shell.Session.SupprimerSegment(segment);
        }
    }

    private void OnEnregistrerSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentViewModel segment)
        {
            shell.Session.EnregistrerSegment(segment);
        }
    }

    private void OnDeplacerSegmentHaut(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentViewModel segment)
        {
            shell.Session.DeplacerSegment(segment, -1);
        }
    }

    private void OnDeplacerSegmentBas(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is SegmentViewModel segment)
        {
            shell.Session.DeplacerSegment(segment, 1);
        }
    }

    private void OnAjouterParticipant(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is SegmentViewModel segment)
        {
            shell.Session.AjouterParticipant(segment);
        }
    }

    private void OnRetirerParticipant(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is ParticipantViewModel participant &&
            control.Tag is SegmentViewModel segment)
        {
            shell.Session.RetirerParticipant(segment, participant);
        }
    }

    private void OnAjouterParticipantNouveauSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AjouterParticipantNouveauSegment();
        }
    }

    private void OnRetirerParticipantNouveauSegment(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.DataContext is ParticipantViewModel participant)
        {
            shell.Session.RetirerParticipantNouveauSegment(participant);
        }
    }

    private void OnCorrigerIssue(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is BookingIssueViewModel issue)
        {
            shell.Session.CorrigerIssue(issue);
        }
    }

    private async void OnImporterPack(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Importer un pack de sauvegarde",
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Pack Ring General", Extensions = { "zip" } },
                new() { Name = "Tous les fichiers", Extensions = { "*" } }
            }
        };

        var result = await dialog.ShowAsync(this);
        var chemin = result?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(chemin))
        {
            return;
        }

        var slot = shell.Saves.ImporterPack(chemin);
        if (slot is not null)
        {
            shell.ChargerSauvegarde(slot);
        }
    }

    private void OnAppliquerTemplate(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AppliquerTemplateSelectionnee();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
