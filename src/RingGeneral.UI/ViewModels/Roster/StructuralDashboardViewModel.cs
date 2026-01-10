using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Roster;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels.Roster;

/// <summary>
/// ViewModel pour le Tableau de Bord Structurel
/// Affiche l'analyse agrégée du roster
/// </summary>
public sealed class StructuralDashboardViewModel : ViewModelBase
{
    private readonly IRosterAnalysisRepository? _rosterAnalysisRepository;
    private readonly RosterAnalysisService? _rosterAnalysisService;
    private string _companyId = string.Empty;
    private RosterStructuralAnalysis? _currentAnalysis;
    private RosterDNA? _currentDNA;

    // Indicateurs globaux
    private double _starPowerMoyen;
    private double _workrateMoyen;
    private string _specialisationDominante = "Hybrid";
    private int _profondeur;
    private double _indiceDependance;
    private double _polyvalence;

    public StructuralDashboardViewModel(
        IRosterAnalysisRepository? rosterAnalysisRepository = null,
        RosterAnalysisService? rosterAnalysisService = null)
    {
        _rosterAnalysisRepository = rosterAnalysisRepository;
        _rosterAnalysisService = rosterAnalysisService;

        MainEventCapableWorkers = new ObservableCollection<string>();
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAnalysisAsync);
    }

    /// <summary>
    /// Commande pour rafraîchir l'analyse
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    /// <summary>
    /// Liste des workers capables de main event
    /// </summary>
    public ObservableCollection<string> MainEventCapableWorkers { get; }

    public string CompanyId
    {
        get => _companyId;
        set => this.RaiseAndSetIfChanged(ref _companyId, value);
    }

    public double StarPowerMoyen
    {
        get => _starPowerMoyen;
        set => this.RaiseAndSetIfChanged(ref _starPowerMoyen, value);
    }

    public double WorkrateMoyen
    {
        get => _workrateMoyen;
        set => this.RaiseAndSetIfChanged(ref _workrateMoyen, value);
    }

    public string SpecialisationDominante
    {
        get => _specialisationDominante;
        set => this.RaiseAndSetIfChanged(ref _specialisationDominante, value);
    }

    public int Profondeur
    {
        get => _profondeur;
        set => this.RaiseAndSetIfChanged(ref _profondeur, value);
    }

    public double IndiceDependance
    {
        get => _indiceDependance;
        set => this.RaiseAndSetIfChanged(ref _indiceDependance, value);
    }

    public double Polyvalence
    {
        get => _polyvalence;
        set => this.RaiseAndSetIfChanged(ref _polyvalence, value);
    }

    public RosterDNA? CurrentDNA
    {
        get => _currentDNA;
        set => this.RaiseAndSetIfChanged(ref _currentDNA, value);
    }

    /// <summary>
    /// Charge l'analyse structurelle pour une compagnie
    /// </summary>
    public async Task LoadAnalysisAsync(string companyId)
    {
        CompanyId = companyId;

        if (_rosterAnalysisRepository == null)
        {
            Logger.Warning("RosterAnalysisRepository non disponible");
            return;
        }

        try
        {
            var analysis = await _rosterAnalysisRepository.GetLatestAnalysisByCompanyIdAsync(companyId);
            if (analysis != null)
            {
                _currentAnalysis = analysis;
                StarPowerMoyen = analysis.StarPowerMoyen;
                WorkrateMoyen = analysis.WorkrateMoyen;
                SpecialisationDominante = analysis.SpecialisationDominante;
                Profondeur = analysis.Profondeur;
                IndiceDependance = analysis.IndiceDependance;
                Polyvalence = analysis.Polyvalence;
                CurrentDNA = analysis.Dna;
            }

            // Charger l'ADN si pas déjà chargé
            if (CurrentDNA == null)
            {
                CurrentDNA = await _rosterAnalysisRepository.GetRosterDNAByCompanyIdAsync(companyId);
            }

            // Charger les workers capables de main event
            if (_rosterAnalysisService != null)
            {
                var mainEventWorkers = await _rosterAnalysisService.GetMainEventCapableWorkersAsync(companyId);
                MainEventCapableWorkers.Clear();
                foreach (var workerId in mainEventWorkers)
                {
                    MainEventCapableWorkers.Add(workerId);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement de l'analyse structurelle: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rafraîchit l'analyse structurelle
    /// </summary>
    private async Task RefreshAnalysisAsync()
    {
        if (string.IsNullOrWhiteSpace(CompanyId) || _rosterAnalysisService == null)
        {
            return;
        }

        try
        {
            // Calculer la semaine et l'année actuelles (approximation)
            var weekNumber = DateTime.Now.DayOfYear / 7;
            var year = DateTime.Now.Year;

            await _rosterAnalysisService.CalculateStructuralAnalysisAsync(CompanyId, weekNumber, year);
            await LoadAnalysisAsync(CompanyId);
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du rafraîchissement de l'analyse: {ex.Message}", ex);
        }
    }
}
