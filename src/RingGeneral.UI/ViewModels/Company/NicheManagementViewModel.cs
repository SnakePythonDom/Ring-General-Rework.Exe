using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Company;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels.Company;

/// <summary>
/// ViewModel pour la gestion des fédérations de niche
/// </summary>
public sealed class NicheManagementViewModel : ViewModelBase
{
    private readonly INicheFederationRepository? _nicheRepository;
    private readonly NicheFederationService? _nicheService;
    private readonly IOwnerDecisionEngine? _ownerDecisionEngine;
    private string _companyId = string.Empty;
    private NicheFederationProfile? _currentProfile;
    private bool _isNicheFederation;
    private NicheType? _nicheType;

    public NicheManagementViewModel(
        INicheFederationRepository? nicheRepository = null,
        NicheFederationService? nicheService = null,
        IOwnerDecisionEngine? ownerDecisionEngine = null)
    {
        _nicheRepository = nicheRepository;
        _nicheService = nicheService;
        _ownerDecisionEngine = ownerDecisionEngine;

        EstablishNicheCommand = ReactiveCommand.CreateFromTask<string>(EstablishNicheAsync);
        AbandonNicheCommand = ReactiveCommand.CreateFromTask(AbandonNicheAsync);
        EvaluateNicheCommand = ReactiveCommand.CreateFromTask<NicheType>(EvaluateNicheAsync);
    }

    /// <summary>
    /// Commande pour établir une niche
    /// </summary>
    public ReactiveCommand<string, Unit> EstablishNicheCommand { get; }

    /// <summary>
    /// Commande pour abandonner le statut de niche
    /// </summary>
    public ReactiveCommand<Unit, Unit> AbandonNicheCommand { get; }

    /// <summary>
    /// Commande pour évaluer la viabilité d'une niche
    /// </summary>
    public ReactiveCommand<NicheType, Unit> EvaluateNicheCommand { get; }

    public string CompanyId
    {
        get => _companyId;
        set => this.RaiseAndSetIfChanged(ref _companyId, value);
    }

    public bool IsNicheFederation
    {
        get => _isNicheFederation;
        set => this.RaiseAndSetIfChanged(ref _isNicheFederation, value);
    }

    public NicheType? NicheType
    {
        get => _nicheType;
        set => this.RaiseAndSetIfChanged(ref _nicheType, value);
    }

    public NicheFederationProfile? CurrentProfile
    {
        get => _currentProfile;
        set => this.RaiseAndSetIfChanged(ref _currentProfile, value);
    }

    /// <summary>
    /// Charge le profil de niche pour une compagnie
    /// </summary>
    public async Task LoadNicheProfileAsync(string companyId)
    {
        CompanyId = companyId;

        if (_nicheRepository == null)
        {
            return;
        }

        try
        {
            var profile = await _nicheRepository.GetNicheFederationProfileByCompanyIdAsync(companyId);
            CurrentProfile = profile;
            IsNicheFederation = profile?.IsNicheFederation ?? false;
            NicheType = profile?.NicheType;
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement du profil de niche: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Établit une fédération comme niche
    /// </summary>
    private async Task EstablishNicheAsync(string nicheTypeStr)
    {
        if (_nicheService == null || !Enum.TryParse<NicheType>(nicheTypeStr, out var nicheType))
        {
            return;
        }

        try
        {
            var profile = await _nicheService.EstablishNicheAsync(CompanyId, nicheType);
            CurrentProfile = profile;
            IsNicheFederation = true;
            NicheType = nicheType;
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors de l'établissement de la niche: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Abandonne le statut de niche
    /// </summary>
    private async Task AbandonNicheAsync()
    {
        if (_nicheService == null)
        {
            return;
        }

        try
        {
            await _nicheService.AbandonNicheAsync(CompanyId);
            IsNicheFederation = false;
            NicheType = null;
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors de l'abandon de la niche: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Évalue la viabilité d'une niche
    /// </summary>
    private async Task EvaluateNicheAsync(NicheType nicheType)
    {
        if (_ownerDecisionEngine == null)
        {
            return;
        }

        try
        {
            var report = _ownerDecisionEngine.EvaluateNicheViability(CompanyId, nicheType);
            // Afficher le rapport (sera implémenté dans la vue)
            Logger.Info($"Évaluation de niche: {report.Recommendation}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors de l'évaluation de la niche: {ex.Message}", ex);
        }
    }
}
