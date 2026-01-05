using System.Collections.Generic;
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
    }

    private void OnSimulerShow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.SimulerShow();
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

    private void OnOuvrirArticle(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell && sender is Control control && control.Tag is string articleId)
        {
            shell.Session.Codex.OuvrirArticle(articleId);
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
