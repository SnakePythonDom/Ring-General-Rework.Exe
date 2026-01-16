using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Interfaces;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Contracts;
using System;
using System.Linq;
using System.Reactive;

namespace RingGeneral.UI.ViewModels.Common.Profile
{
    public class WorkerContractViewModel : ViewModelBase
    {
        private readonly Worker _worker;
        private readonly IGameRepository _repository;
        private readonly INavigationService? _navigationService;

        public WorkerContractViewModel(Worker worker, IGameRepository repository, INavigationService? navigationService = null)
        {
            _worker = worker;
            _repository = repository;
            _navigationService = navigationService;

            NegotiateCommand = ReactiveCommand.Create(ExecuteNegotiate);
            FireCommand = ReactiveCommand.Create(ExecuteFire);
            SaveCommand = ReactiveCommand.Create(ExecuteSave);
        }

        public ReactiveCommand<Unit, Unit> NegotiateCommand { get; }
        public ReactiveCommand<Unit, Unit> FireCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        private void ExecuteNegotiate()
        {
            var result = _repository.PersonalityEngine.ProcessInteraction(_worker, InteractionType.Negotiate);
            Logger.Info(result.Message);

            if (_navigationService != null)
            {
                var context = new NegotiationContext
                {
                    WorkerId = _worker.Id.ToString(),
                    CompanyId = _worker.CurrentContract?.CompanyId ?? "C001", // Default if none
                    CurrentWeek = 1 // Placeholder for current week
                };
                _navigationService.NavigateTo<ContractNegotiationViewModel>(context);
            }
        }

        private void ExecuteFire()
        {
            // Simple termination logic for now: mark for contract termination
            if (_worker.HasActiveContract)
            {
                // TODO: Implement proper contract termination via repository
                // Note: CurrentContract is read-only; actual termination should be handled by IContractRepository
                this.RaisePropertyChanged(nameof(Status));
                this.RaisePropertyChanged(nameof(ContractDuration));

                // Persist change
                _repository.UpdateWorker(_worker);
                Logger.Info($"Contrat de {_worker.Name} résilié.");
            }
        }

        private void ExecuteSave()
        {
            try
            {
                _repository.UpdateWorker(_worker);
                Logger.Info($"Modifications du contrat de {_worker.Name} enregistrées.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur lors de la sauvegarde du contrat: {ex.Message}");
            }
        }

        public string Status => _worker.HasActiveContract ? "Active" : "Free Agent";
        public string ContractType => "Written (Exclusive)"; // Placeholder/Enhance later
        public string EmploymentType => "Full Time";      // Placeholder/Enhance later
        public decimal WeeklySalary => _worker.ContractHistory?.FirstOrDefault()?.WeeklySalary ?? 500m;
        public decimal MonthlySalary => WeeklySalary * 4m;
        public string ContractDuration => CalculateDuration();
        public string ContractEndDate => _worker.ContractHistory?.FirstOrDefault()?.EndDate.ToShortDateString() ?? "Unknown";

        // Clauses (Mockup)
        public bool HasCreativeControl { get; set; } = true;
        public bool HasMerchBonus { get; set; } = true;
        public string MerchBonusAmount => "25%";
        public bool HasDepartureClause { get; set; } = false;
        public bool HasWinBonus { get; set; } = true;
        public string WinBonusAmount => "15%";

        // Staff Feedback (Dynamic based on Personality)
        public string FinanceFeedback
        {
            get
            {
                var mental = _worker.MentalAttributes;
                if (mental == null) return "No financial feedback available.";

                if (mental.Loyauté >= 15) return "Worker is highly loyal; potentially willing to take a pay cut for long-term stability.";
                if (mental.Ambition >= 17) return "Worker is extremely ambitious; expect high salary and bonus demands.";
                return "Base salary is standard for this popularity level.";
            }
        }

        public string CreativeFeedback
        {
            get
            {
                var mental = _worker.MentalAttributes;
                if (mental == null) return "No creative feedback available.";

                if (mental.Ambition >= 15 && _worker.PushLevel < PushLevel.UpperMid)
                    return "WARNING: Worker is highly ambitious and expects a much higher push soon.";
                if (mental.Ego >= 17)
                    return "CAUTION: Large ego detected; worker will likely refuse losing streaks or mid-card roles.";

                return "Status aligns well with current creative expectations.";
            }
        }
        public PushLevel PushLevel => _worker.PushLevel;
        public string PushLevelDisplay => _worker.PushLevelDisplayName;

        // Editable Push Level
        public PushLevel SelectedPushLevel
        {
            get => _worker.PushLevel;
            set
            {
                _worker.PushLevel = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(PushLevelDisplay));
            }
        }
        private string CalculateDuration()
        {
            var contract = _worker.ContractHistory?.FirstOrDefault();
            if (contract == null) return "N/A";

            var months = (int)((contract.EndDate - DateTime.Now).TotalDays / 30);
            return $"{months} Months";
        }
    }
}
