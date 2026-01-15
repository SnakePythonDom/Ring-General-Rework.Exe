using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Contracts;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Data.Repositories;
using RingGeneral.UI.Services.Navigation;

namespace RingGeneral.UI.ViewModels.Contracts;

/// <summary>
/// Phase 3.1 - ViewModel pour la négociation de contrats avec sélection de template
/// </summary>
public sealed class ContractNegotiationViewModel : ViewModelBase, INavigableViewModel
{
    private readonly ContractNegotiationService _negotiationService;
    private readonly TemplateService _templateService;
    private readonly GameRepository _repository;
    private string _workerId;
    private string _companyId;
    private int _currentWeek;

    private ContractTemplate? _selectedTemplate;
    private decimal _monthlyWage;
    private decimal _appearanceFee;
    private int _durationMonths = 12;
    private bool _isExclusive = true;
    private bool _hasRenewalOption = false;
    private string _statusMessage = string.Empty;

    public ContractNegotiationViewModel(
        ContractNegotiationService negotiationService,
        TemplateService templateService,
        GameRepository repository)
    {
        _negotiationService = negotiationService ?? throw new ArgumentNullException(nameof(negotiationService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        // Initialize with default/safe values, real values come from OnNavigatedTo
        _workerId = string.Empty;
        _companyId = string.Empty;
        _currentWeek = 1;

        ContractTemplates = new ObservableCollection<ContractTemplate>();
        LoadTemplates();

        ApplyTemplateCommand = ReactiveCommand.Create(ApplyTemplate);
        CreateOfferCommand = ReactiveCommand.Create(CreateOffer, this.WhenAnyValue(x => x.CanCreateOffer));
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is NegotiationContext context)
        {
            _workerId = context.WorkerId;
            _companyId = context.CompanyId;
            _currentWeek = context.CurrentWeek;

            Logger.Info($"Negotiation started for Worker: {_workerId}, Company: {_companyId}, Week: {_currentWeek}");
        }
    }

    public ObservableCollection<ContractTemplate> ContractTemplates { get; }

    public ContractTemplate? SelectedTemplate
    {
        get => _selectedTemplate;
        set => this.RaiseAndSetIfChanged(ref _selectedTemplate, value);
    }

    public decimal MonthlyWage
    {
        get => _monthlyWage;
        set => this.RaiseAndSetIfChanged(ref _monthlyWage, value);
    }

    public decimal AppearanceFee
    {
        get => _appearanceFee;
        set => this.RaiseAndSetIfChanged(ref _appearanceFee, value);
    }

    public int DurationMonths
    {
        get => _durationMonths;
        set => this.RaiseAndSetIfChanged(ref _durationMonths, value);
    }

    public bool IsExclusive
    {
        get => _isExclusive;
        set => this.RaiseAndSetIfChanged(ref _isExclusive, value);
    }

    public bool HasRenewalOption
    {
        get => _hasRenewalOption;
        set => this.RaiseAndSetIfChanged(ref _hasRenewalOption, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool CanCreateOffer => MonthlyWage > 0 && DurationMonths > 0;

    public ReactiveCommand<Unit, Unit> ApplyTemplateCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateOfferCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private void LoadTemplates()
    {
        ContractTemplates.Clear();
        var templates = _templateService.LoadContractTemplates();
        foreach (var template in templates)
        {
            ContractTemplates.Add(template);
        }
    }

    /// <summary>
    /// Phase 3.1 - Applique un template pour pré-remplir les champs
    /// </summary>
    private void ApplyTemplate()
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Veuillez sélectionner un template";
            return;
        }

        MonthlyWage = SelectedTemplate.MonthlyWage;
        AppearanceFee = SelectedTemplate.AppearanceFee;
        DurationMonths = SelectedTemplate.DurationMonths;
        IsExclusive = SelectedTemplate.IsExclusive;
        HasRenewalOption = SelectedTemplate.HasRenewalOption;

        StatusMessage = $"Template '{SelectedTemplate.Name}' appliqué. Vous pouvez modifier les valeurs si nécessaire.";
        Logger.Info($"Template appliqué: {SelectedTemplate.TemplateId}");
    }

    private void CreateOffer()
    {
        try
        {
            // Convertir MonthlyWage en SalaireHebdo (approximation: 4.33 semaines/mois)
            var salaireHebdo = MonthlyWage / 4.33m;
            var startWeek = _currentWeek;
            var endWeek = startWeek + (DurationMonths * 4); // Approximation: 4 semaines/mois

            var draft = new ContractOfferDraft(
                _workerId,
                _companyId,
                "HYBRIDE", // TypeContrat hybride (MonthlyWage + AppearanceFee)
                startWeek,
                endWeek,
                salaireHebdo,
                AppearanceFee, // BonusShow utilisé pour AppearanceFee
                0m, // Buyout
                0, // NonCompeteWeeks
                HasRenewalOption,
                IsExclusive,
                2); // ExpirationDelaiSemaines

            var offer = _negotiationService.CreerOffre(draft, _currentWeek, false);

            StatusMessage = $"✅ Offre créée avec succès (ID: {offer.OfferId.Substring(0, 8)}...)";
            Logger.Info($"Offre de contrat créée: {offer.OfferId} pour worker {_workerId}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur: {ex.Message}";
            Logger.Error($"Erreur création offre: {ex.Message}");
        }
    }

    private void Cancel()
    {
        // La fenêtre sera fermée par le code appelant
        StatusMessage = "Négociation annulée";
    }
}

public class NegotiationContext
{
    public string WorkerId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public int CurrentWeek { get; set; }
}
