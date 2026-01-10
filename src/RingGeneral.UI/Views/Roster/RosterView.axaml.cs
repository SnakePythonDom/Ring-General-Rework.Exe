using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels.Roster;

namespace RingGeneral.UI.Views.Roster;

public sealed partial class RosterView : UserControl
{
    public RosterView()
    {
        InitializeComponent();
        
        // S'abonner à l'événement Loaded pour s'assurer que le DataContext est défini
        this.Loaded += OnLoaded;
        
        // S'abonner aux changements de DataContext
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[RosterView] DataContext changé: {DataContext?.GetType().Name ?? "null"}");
        
        if (DataContext is RosterViewModel viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[RosterView] RosterViewModel détecté, Workers.Count = {viewModel.Workers.Count}");
            
            // S'assurer que les données sont chargées si elles ne le sont pas déjà
            if (viewModel.Workers.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[RosterView] Collection vide, lancement du chargement...");
                // Lancer le chargement si la collection est vide
                _ = viewModel.LoadWorkersCommand.Execute().Subscribe(
                    _ => System.Diagnostics.Debug.WriteLine("[RosterView] Chargement terminé"),
                    ex => System.Diagnostics.Debug.WriteLine($"[RosterView] Erreur lors du chargement: {ex.Message}")
                );
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[RosterView] ATTENTION: DataContext n'est pas un RosterViewModel! Type: {DataContext?.GetType().Name ?? "null"}");
        }
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[RosterView] Loaded event déclenché, DataContext: {DataContext?.GetType().Name ?? "null"}");
        
        // Vérifier que le DataContext est bien défini
        if (DataContext is RosterViewModel viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[RosterView] RosterViewModel détecté au chargement, Workers.Count = {viewModel.Workers.Count}");
            
            // S'assurer que les données sont chargées
            if (viewModel.Workers.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[RosterView] Collection vide au chargement, lancement du chargement...");
                // Lancer le chargement si la collection est vide
                _ = viewModel.LoadWorkersCommand.Execute().Subscribe(
                    _ => System.Diagnostics.Debug.WriteLine("[RosterView] Chargement terminé depuis Loaded"),
                    ex => System.Diagnostics.Debug.WriteLine($"[RosterView] Erreur lors du chargement depuis Loaded: {ex.Message}")
                );
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[RosterView] ATTENTION: DataContext n'est pas un RosterViewModel au chargement! Type: {DataContext?.GetType().Name ?? "null"}");
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
