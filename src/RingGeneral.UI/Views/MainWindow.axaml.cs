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

    private void OnCreerStoryline(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.CreerStoryline();
        }
    }

    private void OnMettreAJourStoryline(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.MettreAJourStoryline();
        }
    }

    private void OnAvancerStoryline(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AvancerStoryline();
        }
    }

    private void OnSupprimerStoryline(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.SupprimerStoryline();
        }
    }

    private void OnAjouterParticipantStoryline(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            shell.Session.AjouterParticipantStoryline();
        }
    }

    private void OnRetirerParticipantStoryline(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ShellViewModel shell || sender is not Control control)
        {
            return;
        }

        if (control.DataContext is StorylineParticipantViewModel participant)
        {
            shell.Session.RetirerParticipantStoryline(participant);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
