using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Enums;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Roster;
using RingGeneral.Core.Models.Trends;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels.Trends;

/// <summary>
/// ViewModel pour la vue des tendances
/// </summary>
public sealed class TrendsViewModel : ViewModelBase
{
    private readonly ITrendRepository? _trendRepository;
    private readonly IRosterAnalysisRepository? _rosterAnalysisRepository;
    private readonly CompatibilityCalculator? _compatibilityCalculator;
    private string _companyId = string.Empty;
    private RosterDNA? _currentDNA;

    public TrendsViewModel(
        ITrendRepository? trendRepository = null,
        IRosterAnalysisRepository? rosterAnalysisRepository = null,
        CompatibilityCalculator? compatibilityCalculator = null)
    {
        _trendRepository = trendRepository;
        _rosterAnalysisRepository = rosterAnalysisRepository;
        _compatibilityCalculator = compatibilityCalculator;

        ActiveTrends = new ObservableCollection<Trend>();
        CompatibilityMatrices = new ObservableCollection<CompatibilityMatrix>();
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshTrendsAsync);
    }

    /// <summary>
    /// Commande pour rafraîchir les tendances
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    /// <summary>
    /// Liste des tendances actives
    /// </summary>
    public ObservableCollection<Trend> ActiveTrends { get; }

    /// <summary>
    /// Matrices de compatibilité avec l'ADN du roster
    /// </summary>
    public ObservableCollection<CompatibilityMatrix> CompatibilityMatrices { get; }

    public string CompanyId
    {
        get => _companyId;
        set => this.RaiseAndSetIfChanged(ref _companyId, value);
    }

    public RosterDNA? CurrentDNA
    {
        get => _currentDNA;
        set => this.RaiseAndSetIfChanged(ref _currentDNA, value);
    }

    /// <summary>
    /// Charge les tendances et calcule les compatibilités
    /// </summary>
    public async Task LoadTrendsAsync(string companyId)
    {
        CompanyId = companyId;

        if (_trendRepository == null || _rosterAnalysisRepository == null)
        {
            Logger.Warning("Repositories non disponibles");
            return;
        }

        try
        {
            // Charger l'ADN du roster
            _currentDNA = await _rosterAnalysisRepository.GetRosterDNAByCompanyIdAsync(companyId);
            CurrentDNA = _currentDNA;

            // Charger les tendances actives
            var trends = await _trendRepository.GetActiveTrendsAsync();
            ActiveTrends.Clear();
            foreach (var trend in trends)
            {
                ActiveTrends.Add(trend);
            }

            // Calculer les matrices de compatibilité
            if (_currentDNA != null && _compatibilityCalculator != null)
            {
                CompatibilityMatrices.Clear();
                foreach (var trend in trends)
                {
                    var matrix = _compatibilityCalculator.CalculateCompatibility(_currentDNA, trend);
                    await _trendRepository.SaveCompatibilityMatrixAsync(matrix);
                    CompatibilityMatrices.Add(matrix);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement des tendances: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rafraîchit les tendances
    /// </summary>
    private async Task RefreshTrendsAsync()
    {
        if (!string.IsNullOrWhiteSpace(CompanyId))
        {
            await LoadTrendsAsync(CompanyId);
        }
    }
}
