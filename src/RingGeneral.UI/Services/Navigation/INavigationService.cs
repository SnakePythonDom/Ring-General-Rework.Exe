using RingGeneral.UI.ViewModels;
using RingGeneral.UI.ViewModels.Core;

namespace RingGeneral.UI.Services.Navigation;

/// <summary>
/// Service de navigation pour gérer les transitions entre ViewModels
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// ViewModel actuellement affiché
    /// </summary>
    ViewModelBase? CurrentViewModel { get; }

    /// <summary>
    /// Observable du ViewModel actuel pour binding XAML
    /// </summary>
    IObservable<ViewModelBase?> CurrentViewModelObservable { get; }

    /// <summary>
    /// Navigue vers un ViewModel spécifique
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigue vers un ViewModel avec un paramètre
    /// </summary>
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;

    /// <summary>
    /// Retour à la page précédente
    /// </summary>
    bool GoBack();

    /// <summary>
    /// Vérifie si on peut revenir en arrière
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Historique de navigation
    /// </summary>
    IReadOnlyList<ViewModelBase> NavigationHistory { get; }
}
