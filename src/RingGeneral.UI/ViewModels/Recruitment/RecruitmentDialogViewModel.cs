using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Recruitment;
using RingGeneral.UI.Services.Messaging;

namespace RingGeneral.UI.ViewModels.Recruitment;

public sealed class RecruitmentDialogViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRecruitmentService _recruitmentService;
    private readonly FreeAgentCandidate _agent;
    private readonly string _companyId;

    private string _selectedRecruitmentType = "Main Roster";
    private decimal _salaryOffer;
    private string? _message;
    private bool _isBusy;

    public RecruitmentDialogViewModel(
        FreeAgentCandidate agent,
        string companyId,
        IRecruitmentService recruitmentService,
        IEventAggregator eventAggregator)
    {
        _agent = agent;
        _companyId = companyId;
        _recruitmentService = recruitmentService;
        _eventAggregator = eventAggregator;

        _salaryOffer = agent.WeeklySalaryDemand ?? 1000;

        RecruitmentTypes = new ObservableCollection<string> { "Main Roster", "Child Company" };

        if (agent.Age < 25 && agent.Type == FreeAgentType.Wrestler)
        {
            RecruitmentTypes.Add("Youth Structure");
        }

        if (agent.Age >= 45 && agent.Type == FreeAgentType.Wrestler)
        {
            RecruitmentTypes.Add("Reconversion (Staff)");
        }

        ConfirmCommand = ReactiveCommand.CreateFromTask(ConfirmRecruitmentAsync);
        CancelCommand = ReactiveCommand.Create(() => { /* Logic to close */ });
    }

    public FreeAgentCandidate Agent => _agent;
    public ObservableCollection<string> RecruitmentTypes { get; }

    public string SelectedRecruitmentType
    {
        get => _selectedRecruitmentType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRecruitmentType, value);
            UpdateDefaultSalary();
        }
    }

    public decimal SalaryOffer
    {
        get => _salaryOffer;
        set => this.RaiseAndSetIfChanged(ref _salaryOffer, value);
    }

    public string? Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private void UpdateDefaultSalary()
    {
        if (_selectedRecruitmentType == "Youth Structure")
            SalaryOffer = 100; // Low trainee allowance
        else if (_selectedRecruitmentType == "Reconversion (Staff)")
            SalaryOffer = _agent.WeeklySalaryDemand * 1.3m ?? 1500; // Premium for ego
        else
            SalaryOffer = _agent.WeeklySalaryDemand ?? 1000;
    }

    private async Task ConfirmRecruitmentAsync()
    {
        IsBusy = true;
        Message = "Négociation en cours...";

        try
        {
            RecruitmentResult? result = null;

            switch (_selectedRecruitmentType)
            {
                case "Main Roster":
                    result = await _recruitmentService.SignToMainRosterAsync(_agent.Id, _companyId, _salaryOffer);
                    break;
                case "Child Company":
                    result = await _recruitmentService.SignToChildCompanyAsync(_agent.Id, _companyId, "CHILD_001", _salaryOffer);
                    break;
                case "Youth Structure":
                    result = await _recruitmentService.SignToYouthStructureAsync(_agent.Id, _companyId, "YOUTH_001");
                    break;
                case "Reconversion (Staff)":
                    var reconversion = await _recruitmentService.NegotiateReconversionAsync(_agent.Id, _companyId, StaffRole.WrestlingTrainer, _salaryOffer);
                    Message = reconversion.Message;
                    if (reconversion.Status == ReconversionStatus.Accepted)
                    {
                        // Success
                    }
                    return;
            }

            if (result != null)
            {
                Message = result.Message;
                if (result.Status == RecruitmentStatus.Success)
                {
                    // Success logic
                }
            }
        }
        catch (Exception ex)
        {
            Message = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
