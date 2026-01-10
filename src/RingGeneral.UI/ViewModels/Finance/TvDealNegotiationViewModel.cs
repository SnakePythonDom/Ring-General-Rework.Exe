using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Finance;

/// <summary>
/// Phase 2.1 - ViewModel pour la négociation TV Deal
/// </summary>
public sealed class TvDealNegotiationViewModel : ViewModelBase
{
    private readonly ITvDealNegotiationService _negotiationService;
    private readonly string _companyId;
    private int _currentStep = 1;
    private AvailableNetwork? _selectedNetwork;
    private TvDealTerms _terms = new TvDealTerms { DurationYears = 2, IsExclusive = false, ShowsPerYear = 52 };
    private TvDealOffer? _currentOffer;
    private string _negotiationMessage = string.Empty;
    private int _showsPerYearIndex = 2; // 52 shows/year par défaut

    public TvDealNegotiationViewModel(
        ITvDealNegotiationService negotiationService,
        string companyId)
    {
        _negotiationService = negotiationService ?? throw new ArgumentNullException(nameof(negotiationService));
        _companyId = companyId ?? throw new ArgumentNullException(nameof(companyId));

        AvailableNetworks = new ObservableCollection<AvailableNetwork>();
        LoadAvailableNetworks();

        NextStepCommand = ReactiveCommand.Create(NextStep, this.WhenAnyValue(x => x.CanProceedToNextStep));
        PreviousStepCommand = ReactiveCommand.Create(PreviousStep, this.WhenAnyValue(x => x.CanGoBack));
        NegotiateCommand = ReactiveCommand.Create<decimal>(Negotiate);
        SignDealCommand = ReactiveCommand.Create(SignDeal, this.WhenAnyValue(x => x.CanSignDeal));
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    public ObservableCollection<AvailableNetwork> AvailableNetworks { get; }

    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentStep, value);
            this.RaisePropertyChanged(nameof(IsStep1));
            this.RaisePropertyChanged(nameof(IsStep2));
            this.RaisePropertyChanged(nameof(IsStep3));
            this.RaisePropertyChanged(nameof(IsStep4));
        }
    }

    public AvailableNetwork? SelectedNetwork
    {
        get => _selectedNetwork;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedNetwork, value);
            if (value != null && CurrentStep == 2)
            {
                CalculateInitialOffer();
            }
        }
    }

    public TvDealTerms Terms
    {
        get => _terms;
        set
        {
            this.RaiseAndSetIfChanged(ref _terms, value);
            if (SelectedNetwork != null && CurrentStep == 2)
            {
                CalculateInitialOffer();
            }
        }
    }

    public TvDealOffer? CurrentOffer
    {
        get => _currentOffer;
        set => this.RaiseAndSetIfChanged(ref _currentOffer, value);
    }

    public string NegotiationMessage
    {
        get => _negotiationMessage;
        set => this.RaiseAndSetIfChanged(ref _negotiationMessage, value);
    }

    public bool CanProceedToNextStep => CurrentStep switch
    {
        1 => SelectedNetwork != null,
        2 => CurrentOffer != null,
        3 => CurrentOffer != null,
        _ => false
    };

    public bool CanGoBack => CurrentStep > 1;

    public bool CanSignDeal => CurrentStep == 4 && CurrentOffer != null;

    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;
    public bool IsStep4 => CurrentStep == 4;

    public int ShowsPerYearIndex
    {
        get => _showsPerYearIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _showsPerYearIndex, value);
            var showsPerYear = value switch
            {
                0 => 12,
                1 => 24,
                2 => 52,
                3 => 104,
                _ => 52
            };
            Terms = Terms with { ShowsPerYear = showsPerYear };
        }
    }

    public ReactiveCommand<Unit, Unit> NextStepCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousStepCommand { get; }
    public ReactiveCommand<decimal, Unit> NegotiateCommand { get; }
    public ReactiveCommand<Unit, Unit> SignDealCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private void LoadAvailableNetworks()
    {
        AvailableNetworks.Clear();
        var networks = _negotiationService.GetAvailableNetworks(_companyId);
        foreach (var network in networks)
        {
            AvailableNetworks.Add(network);
        }
    }

    private void CalculateInitialOffer()
    {
        if (SelectedNetwork == null) return;

        try
        {
            CurrentOffer = _negotiationService.CalculateInitialOffer(
                SelectedNetwork.NetworkId,
                _companyId,
                Terms);
            NegotiationMessage = string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur calcul offre initiale: {ex.Message}");
            NegotiationMessage = $"Erreur: {ex.Message}";
        }
    }

    private void NextStep()
    {
        if (CurrentStep == 1 && SelectedNetwork != null)
        {
            CurrentStep = 2;
            CalculateInitialOffer();
        }
        else if (CurrentStep == 2 && CurrentOffer != null)
        {
            CurrentStep = 3;
        }
        else if (CurrentStep == 3)
        {
            CurrentStep = 4;
        }
    }

    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    private void Negotiate(decimal increasePercent)
    {
        if (SelectedNetwork == null || CurrentOffer == null) return;

        try
        {
            var result = _negotiationService.NegotiateDeal(
                SelectedNetwork.NetworkId,
                _companyId,
                CurrentOffer,
                increasePercent);

            NegotiationMessage = result.Message;

            if (result.IsAccepted && result.CounterOffer != null)
            {
                CurrentOffer = result.CounterOffer;
            }
            else if (!result.IsAccepted && result.CounterOffer != null)
            {
                // Proposer la contre-offre
                CurrentOffer = result.CounterOffer;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur négociation: {ex.Message}");
            NegotiationMessage = $"Erreur: {ex.Message}";
        }
    }

    private void SignDeal()
    {
        if (SelectedNetwork == null || CurrentOffer == null) return;

        try
        {
            var deal = _negotiationService.SignDeal(
                SelectedNetwork.NetworkId,
                _companyId,
                CurrentOffer,
                Terms);

            Logger.Info($"TV Deal signé: {deal.NetworkName}");
            NegotiationMessage = $"Deal signé avec succès! {deal.NetworkName}";
            
            // TODO: Fermer la fenêtre ou retourner au FinanceView
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur signature deal: {ex.Message}");
            NegotiationMessage = $"Erreur: {ex.Message}";
        }
    }

    private void Cancel()
    {
        // TODO: Fermer la fenêtre
        Logger.Info("Négociation annulée");
    }
}
