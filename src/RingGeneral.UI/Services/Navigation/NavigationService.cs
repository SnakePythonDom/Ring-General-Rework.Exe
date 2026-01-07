using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using RingGeneral.UI.ViewModels;
using RingGeneral.UI.ViewModels.Core;

namespace RingGeneral.UI.Services.Navigation;

/// <summary>
/// Implémentation du service de navigation avec ReactiveUI
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BehaviorSubject<ViewModelBase?> _currentViewModelSubject;
    private readonly Stack<ViewModelBase> _navigationStack;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _currentViewModelSubject = new BehaviorSubject<ViewModelBase?>(null);
        _navigationStack = new Stack<ViewModelBase>();
        System.Console.WriteLine("[NavigationService] Service initialisé");
    }

    public ViewModelBase? CurrentViewModel => _currentViewModelSubject.Value;

    public IObservable<ViewModelBase?> CurrentViewModelObservable => _currentViewModelSubject.AsObservable();

    public IReadOnlyList<ViewModelBase> NavigationHistory => _navigationStack.Reverse().ToList();

    public bool CanGoBack => _navigationStack.Count > 1;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as TViewModel;
        if (viewModel is null)
        {
            throw new InvalidOperationException($"ViewModel {typeof(TViewModel).Name} non enregistré dans le conteneur DI");
        }

        NavigateToViewModel(viewModel);
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as TViewModel;
        if (viewModel is null)
        {
            throw new InvalidOperationException($"ViewModel {typeof(TViewModel).Name} non enregistré dans le conteneur DI");
        }

        // Si le ViewModel a une méthode Initialize, l'appeler avec le paramètre
        if (viewModel is INavigableViewModel navigable)
        {
            navigable.OnNavigatedTo(parameter);
        }

        NavigateToViewModel(viewModel);
    }

    public bool GoBack()
    {
        if (!CanGoBack)
        {
            return false;
        }

        // Retirer le ViewModel actuel
        _navigationStack.Pop();

        // Obtenir le ViewModel précédent
        var previousViewModel = _navigationStack.Peek();
        _currentViewModelSubject.OnNext(previousViewModel);

        return true;
    }

    private void NavigateToViewModel(ViewModelBase viewModel)
    {
        System.Console.WriteLine($"[NavigationService] Navigation vers {viewModel.GetType().Name}");
        _navigationStack.Push(viewModel);
        _currentViewModelSubject.OnNext(viewModel);
    }
}

/// <summary>
/// Interface optionnelle pour les ViewModels qui veulent recevoir des paramètres de navigation
/// </summary>
public interface INavigableViewModel
{
    void OnNavigatedTo(object? parameter);
}
