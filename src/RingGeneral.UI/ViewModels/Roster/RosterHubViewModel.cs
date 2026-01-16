using ReactiveUI;
using RingGeneral.UI.Services.Navigation;
using System;

namespace RingGeneral.UI.ViewModels.Roster
{
    public class RosterHubViewModel : ViewModelBase, INavigableViewModel
    {
        private readonly INavigationService _navigationService;
        
        public RosterViewModel WorkersVM { get; }
        public FactionsViewModel FactionsVM { get; }
        public TitlesViewModel TitlesVM { get; }

        public RosterHubViewModel(
            INavigationService navigationService,
            RosterViewModel workersVm,
            FactionsViewModel factionsVm,
            TitlesViewModel titlesVm)
        {
            _navigationService = navigationService;
            WorkersVM = workersVm;
            FactionsVM = factionsVm;
            TitlesVM = titlesVm;
        }

        public void OnNavigatedTo(object? parameter)
        {
            WorkersVM.OnNavigatedTo(parameter);
            // FactionsVM.RefreshCommand.Execute().Subscribe(); // If needed
        }
    }
}
