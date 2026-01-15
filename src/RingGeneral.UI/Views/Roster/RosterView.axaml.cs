using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RingGeneral.UI.ViewModels.Roster;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

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
                _ = viewModel.LoadWorkersCommand.Execute(Unit.Default)
                    .SelectMany(task => System.Reactive.Linq.Observable.FromAsync(() => task))
                    .Subscribe(
                        _ => 
                        {
                            System.Diagnostics.Debug.WriteLine("[RosterView] Chargement terminé");
                            // Forcer le rafraîchissement du DataGrid
                            RefreshDataGrid();
                        },
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
                _ = viewModel.LoadWorkersCommand.Execute(Unit.Default)
                    .SelectMany(task => System.Reactive.Linq.Observable.FromAsync(() => task))
                    .Subscribe(
                        _ => 
                        {
                            System.Diagnostics.Debug.WriteLine("[RosterView] Chargement terminé depuis Loaded");
                            // Forcer le rafraîchissement du DataGrid
                            RefreshDataGrid();
                        },
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

    private void RefreshDataGrid()
    {
        // Forcer le rafraîchissement du DataGrid après le chargement
        // Utiliser Dispatcher pour s'assurer qu'on est sur le thread UI
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<DataGrid>("WorkersDataGrid") is DataGrid dataGrid)
            {
                var itemsCount = dataGrid.ItemsSource?.Cast<object>().Count() ?? 0;
                System.Diagnostics.Debug.WriteLine($"[RosterView] Rafraîchissement du DataGrid, ItemsSource count: {itemsCount}");
                
                if (DataContext is RosterViewModel viewModel)
                {
                    System.Diagnostics.Debug.WriteLine($"[RosterView] ViewModel Workers.Count: {viewModel.Workers.Count}");
                    
                    // Log des premiers workers pour vérifier les données
                    if (viewModel.Workers.Count > 0)
                    {
                        var firstWorker = viewModel.Workers[0];
                        System.Diagnostics.Debug.WriteLine($"[RosterView] Premier worker: {firstWorker.Name}, Company: {firstWorker.Company}, Role: {firstWorker.Role}");
                    }
                    
                    // Forcer la mise à jour en réassignant temporairement l'ItemsSource
                    var currentSource = dataGrid.ItemsSource;
                    dataGrid.ItemsSource = null;
                    dataGrid.ItemsSource = currentSource;
                    
                    // Forcer la mise à jour visuelle
                    dataGrid.InvalidateVisual();
                    dataGrid.InvalidateArrange();
                    
                    // Vérifier après rafraîchissement
                    var newCount = dataGrid.ItemsSource?.Cast<object>().Count() ?? 0;
                    System.Diagnostics.Debug.WriteLine($"[RosterView] DataGrid rafraîchi, nouveau count: {newCount}");
                    
                    // Vérifier si le DataGrid est visible
                    System.Diagnostics.Debug.WriteLine($"[RosterView] DataGrid IsVisible: {dataGrid.IsVisible}, Opacity: {dataGrid.Opacity}, Height: {dataGrid.Height}, ActualHeight: {dataGrid.Bounds.Height}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[RosterView] WorkersDataGrid non trouvé !");
            }
        }, Avalonia.Threading.DispatcherPriority.Loaded);
    }
}
