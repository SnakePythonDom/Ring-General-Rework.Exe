using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Staff;
using RingGeneral.Core.Services;

namespace RingGeneral.UI.ViewModels;

/// <summary>
/// ViewModel pour l'affichage d'un membre du staff dans la liste.
/// </summary>
public sealed class StaffMemberListItemViewModel : ViewModelBase
{
    private int _skillScore;
    private string _employmentStatus;

    public StaffMemberListItemViewModel(
        string staffId,
        string fullName,
        string role,
        string department,
        int skillScore,
        int yearsOfExperience,
        string employmentStatus,
        decimal monthlySalary,
        DateTime hireDate,
        DateTime? contractEndDate)
    {
        StaffId = staffId;
        FullName = fullName;
        Role = role;
        Department = department;
        _skillScore = skillScore;
        YearsOfExperience = yearsOfExperience;
        _employmentStatus = employmentStatus;
        MonthlySalary = monthlySalary;
        HireDate = hireDate;
        ContractEndDate = contractEndDate;

        UpdateDerivedProperties();
    }

    public string StaffId { get; }
    public string FullName { get; }
    public string Role { get; }
    public string Department { get; }
    public int YearsOfExperience { get; }
    public decimal MonthlySalary { get; }
    public DateTime HireDate { get; }
    public DateTime? ContractEndDate { get; }

    public int SkillScore
    {
        get => _skillScore;
        set
        {
            this.RaiseAndSetIfChanged(ref _skillScore, value);
            UpdateDerivedProperties();
        }
    }

    public string EmploymentStatus
    {
        get => _employmentStatus;
        set
        {
            this.RaiseAndSetIfChanged(ref _employmentStatus, value);
            UpdateDerivedProperties();
        }
    }

    private string? _skillLabel;
    public string? SkillLabel
    {
        get => _skillLabel;
        private set => this.RaiseAndSetIfChanged(ref _skillLabel, value);
    }

    private string? _statusLabel;
    public string? StatusLabel
    {
        get => _statusLabel;
        private set => this.RaiseAndSetIfChanged(ref _statusLabel, value);
    }

    private bool _isContractExpiringSoon;
    public bool IsContractExpiringSoon
    {
        get => _isContractExpiringSoon;
        private set => this.RaiseAndSetIfChanged(ref _isContractExpiringSoon, value);
    }

    private string? _summary;
    public string? Summary
    {
        get => _summary;
        private set => this.RaiseAndSetIfChanged(ref _summary, value);
    }

    private void UpdateDerivedProperties()
    {
        SkillLabel = SkillScore >= 80 ? "Expert" :
                    SkillScore >= 60 ? "Compétent" :
                    SkillScore >= 40 ? "Moyen" : "Débutant";

        StatusLabel = EmploymentStatus == "Active" ? "✓ Actif" :
                     EmploymentStatus == "OnLeave" ? "En congé" : "Inactif";

        IsContractExpiringSoon = ContractEndDate.HasValue &&
                                (ContractEndDate.Value - DateTime.Now).TotalDays <= 90;

        var contractWarning = IsContractExpiringSoon ? " ⚠️ Contrat expire bientôt" : "";
        Summary = $"{Role} • {Department} • Skill: {SkillLabel} ({SkillScore}/100) • ${MonthlySalary:N0}/mois{contractWarning}";
    }
}

/// <summary>
/// ViewModel pour l'affichage d'un staff créatif avec ses attributs spécifiques.
/// </summary>
public sealed class CreativeStaffViewModel : ViewModelBase
{
    private int _bookerCompatibilityScore;

    public CreativeStaffViewModel(
        StaffMemberListItemViewModel baseMember,
        int creativityScore,
        int consistencyScore,
        string workerBias,
        int bookerCompatibilityScore,
        bool canRuinStorylines)
    {
        BaseMember = baseMember;
        CreativityScore = creativityScore;
        ConsistencyScore = consistencyScore;
        WorkerBias = workerBias;
        _bookerCompatibilityScore = bookerCompatibilityScore;
        CanRuinStorylines = canRuinStorylines;

        UpdateCompatibilityLabel();
    }

    public StaffMemberListItemViewModel BaseMember { get; }
    public int CreativityScore { get; }
    public int ConsistencyScore { get; }
    public string WorkerBias { get; }
    public bool CanRuinStorylines { get; }

    public int BookerCompatibilityScore
    {
        get => _bookerCompatibilityScore;
        set
        {
            this.RaiseAndSetIfChanged(ref _bookerCompatibilityScore, value);
            UpdateCompatibilityLabel();
        }
    }

    private string? _compatibilityLabel;
    public string? CompatibilityLabel
    {
        get => _compatibilityLabel;
        private set => this.RaiseAndSetIfChanged(ref _compatibilityLabel, value);
    }

    private bool _isDangerous;
    public bool IsDangerous
    {
        get => _isDangerous;
        private set => this.RaiseAndSetIfChanged(ref _isDangerous, value);
    }

    public string CreativeProfile => $"Créativité: {CreativityScore}/100 • Consistance: {ConsistencyScore}/100 • Biais: {WorkerBias}";

    private void UpdateCompatibilityLabel()
    {
        CompatibilityLabel = BookerCompatibilityScore >= 80 ? "Excellente" :
                            BookerCompatibilityScore >= 60 ? "Bonne" :
                            BookerCompatibilityScore >= 40 ? "Moyenne" :
                            BookerCompatibilityScore >= 20 ? "Faible" : "Dangereuse";

        IsDangerous = BookerCompatibilityScore <= 30 && CanRuinStorylines;
    }
}

/// <summary>
/// ViewModel pour l'affichage d'un staff structurel avec ses attributs spécifiques.
/// </summary>
public sealed class StructuralStaffViewModel : ViewModelBase
{
    public StructuralStaffViewModel(
        StaffMemberListItemViewModel baseMember,
        string expertiseDomain,
        int expertiseLevel,
        int totalInterventions,
        int successfulInterventions)
    {
        BaseMember = baseMember;
        ExpertiseDomain = expertiseDomain;
        ExpertiseLevel = expertiseLevel;
        TotalInterventions = totalInterventions;
        SuccessfulInterventions = successfulInterventions;

        SuccessRate = totalInterventions > 0
            ? (int)((successfulInterventions / (double)totalInterventions) * 100)
            : 0;

        ExpertiseProfile = $"{ExpertiseDomain} • Niveau: {ExpertiseLevel}/100 • Succès: {SuccessRate}%";
    }

    public StaffMemberListItemViewModel BaseMember { get; }
    public string ExpertiseDomain { get; }
    public int ExpertiseLevel { get; }
    public int TotalInterventions { get; }
    public int SuccessfulInterventions { get; }
    public int SuccessRate { get; }
    public string ExpertiseProfile { get; }
}

/// <summary>
/// ViewModel pour l'affichage d'un trainer avec ses attributs spécifiques.
/// </summary>
public sealed class TrainerViewModel : ViewModelBase
{
    private int _currentStudents;

    public TrainerViewModel(
        StaffMemberListItemViewModel baseMember,
        string infrastructureId,
        string trainingSpecialization,
        int progressionBonus,
        int currentStudents,
        int maxStudentCapacity,
        int graduatedStudents,
        int failedStudents)
    {
        BaseMember = baseMember;
        InfrastructureId = infrastructureId;
        TrainingSpecialization = trainingSpecialization;
        ProgressionBonus = progressionBonus;
        _currentStudents = currentStudents;
        MaxStudentCapacity = maxStudentCapacity;
        GraduatedStudents = graduatedStudents;
        FailedStudents = failedStudents;

        UpdateDerivedProperties();
    }

    public StaffMemberListItemViewModel BaseMember { get; }
    public string InfrastructureId { get; }
    public string TrainingSpecialization { get; }
    public int ProgressionBonus { get; }
    public int MaxStudentCapacity { get; }
    public int GraduatedStudents { get; }
    public int FailedStudents { get; }

    public int CurrentStudents
    {
        get => _currentStudents;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentStudents, value);
            UpdateDerivedProperties();
        }
    }

    private bool _isOverloaded;
    public bool IsOverloaded
    {
        get => _isOverloaded;
        private set => this.RaiseAndSetIfChanged(ref _isOverloaded, value);
    }

    private int _graduationRate;
    public int GraduationRate
    {
        get => _graduationRate;
        private set => this.RaiseAndSetIfChanged(ref _graduationRate, value);
    }

    private string? _trainerProfile;
    public string? TrainerProfile
    {
        get => _trainerProfile;
        private set => this.RaiseAndSetIfChanged(ref _trainerProfile, value);
    }

    private void UpdateDerivedProperties()
    {
        IsOverloaded = (CurrentStudents / (double)MaxStudentCapacity) >= 0.8;

        var totalStudents = GraduatedStudents + FailedStudents;
        GraduationRate = totalStudents > 0
            ? (int)((GraduatedStudents / (double)totalStudents) * 100)
            : 0;

        var overloadWarning = IsOverloaded ? " ⚠️ Surchargé" : "";
        TrainerProfile = $"{TrainingSpecialization} • Étudiants: {CurrentStudents}/{MaxStudentCapacity} • " +
                        $"Bonus: +{ProgressionBonus}% • Taux réussite: {GraduationRate}%{overloadWarning}";
    }
}

/// <summary>
/// ViewModel pour la compatibilité staff-booker.
/// </summary>
public sealed class StaffCompatibilityViewModel : ViewModelBase
{
    public StaffCompatibilityViewModel(
        string compatibilityId,
        string staffName,
        int overallScore,
        int creativeVisionScore,
        int bookingStyleScore,
        int biasAlignmentScore,
        int riskToleranceScore,
        int personalChemistryScore,
        int collaborations,
        int successfulCollaborations,
        DateTime lastCalculated)
    {
        CompatibilityId = compatibilityId;
        StaffName = staffName;
        OverallScore = overallScore;
        CreativeVisionScore = creativeVisionScore;
        BookingStyleScore = bookingStyleScore;
        BiasAlignmentScore = biasAlignmentScore;
        RiskToleranceScore = riskToleranceScore;
        PersonalChemistryScore = personalChemistryScore;
        Collaborations = collaborations;
        SuccessfulCollaborations = successfulCollaborations;
        LastCalculated = lastCalculated;

        OverallLabel = overallScore >= 80 ? "Excellente synergie" :
                      overallScore >= 60 ? "Bonne compatibilité" :
                      overallScore >= 40 ? "Compatibilité moyenne" :
                      overallScore >= 20 ? "Faible compatibilité" : "⚠️ DANGEREUX";

        SuccessRate = collaborations > 0
            ? (int)((successfulCollaborations / (double)collaborations) * 100)
            : 0;

        NeedsRecalculation = (DateTime.Now - lastCalculated).TotalDays > 30;
    }

    public string CompatibilityId { get; }
    public string StaffName { get; }
    public int OverallScore { get; }
    public int CreativeVisionScore { get; }
    public int BookingStyleScore { get; }
    public int BiasAlignmentScore { get; }
    public int RiskToleranceScore { get; }
    public int PersonalChemistryScore { get; }
    public int Collaborations { get; }
    public int SuccessfulCollaborations { get; }
    public DateTime LastCalculated { get; }
    public string OverallLabel { get; }
    public int SuccessRate { get; }
    public bool NeedsRecalculation { get; }

    public string CompatibilityBreakdown =>
        $"Vision créative: {CreativeVisionScore}/100 • Style booking: {BookingStyleScore}/100 • " +
        $"Alignement biais: {BiasAlignmentScore}/100 • Tolérance risque: {RiskToleranceScore}/100 • " +
        $"Chimie: {PersonalChemistryScore}/100";
}

/// <summary>
/// ViewModel principal pour la gestion du staff.
/// </summary>
public sealed class StaffManagementViewModel : ViewModelBase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IStaffCompatibilityRepository _compatibilityRepository;
    private readonly StaffCompatibilityCalculator _compatibilityCalculator;
    private readonly string _companyId;

    private StaffMemberListItemViewModel? _selectedStaff;
    private string _selectedDepartment = "All";

    public sealed record StaffHireRequest(
        StaffMember Member,
        CreativeStaff? CreativeStaff,
        StructuralStaff? StructuralStaff,
        Trainer? Trainer);

    public StaffManagementViewModel(
        string companyId,
        IStaffRepository staffRepository,
        IStaffCompatibilityRepository compatibilityRepository,
        StaffCompatibilityCalculator compatibilityCalculator)
    {
        _companyId = companyId;
        _staffRepository = staffRepository;
        _compatibilityRepository = compatibilityRepository;
        _compatibilityCalculator = compatibilityCalculator;

        AllStaff = new ObservableCollection<StaffMemberListItemViewModel>();
        CreativeStaff = new ObservableCollection<CreativeStaffViewModel>();
        StructuralStaff = new ObservableCollection<StructuralStaffViewModel>();
        Trainers = new ObservableCollection<TrainerViewModel>();
        Compatibilities = new ObservableCollection<StaffCompatibilityViewModel>();
        DangerousStaff = new ObservableCollection<CreativeStaffViewModel>();

        HireStaffCommand = ReactiveCommand.CreateFromTask<StaffHireRequest>(HireStaffAsync);
        TerminateStaffCommand = ReactiveCommand.CreateFromTask<string>(TerminateStaffAsync);
        CalculateCompatibilityCommand = ReactiveCommand.CreateFromTask<(string StaffId, string BookerId)>(CalculateCompatibilityAsync);
        RecalculateAllCompatibilitiesCommand = ReactiveCommand.CreateFromTask(RecalculateAllCompatibilitiesAsync);
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public ObservableCollection<StaffMemberListItemViewModel> AllStaff { get; }
    public ObservableCollection<CreativeStaffViewModel> CreativeStaff { get; }
    public ObservableCollection<StructuralStaffViewModel> StructuralStaff { get; }
    public ObservableCollection<TrainerViewModel> Trainers { get; }
    public ObservableCollection<StaffCompatibilityViewModel> Compatibilities { get; }
    public ObservableCollection<CreativeStaffViewModel> DangerousStaff { get; }

    public StaffMemberListItemViewModel? SelectedStaff
    {
        get => _selectedStaff;
        set => this.RaiseAndSetIfChanged(ref _selectedStaff, value);
    }

    public string SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDepartment, value);
            FilterStaffByDepartment();
        }
    }

    private int _totalStaffCount;
    public int TotalStaffCount
    {
        get => _totalStaffCount;
        private set => this.RaiseAndSetIfChanged(ref _totalStaffCount, value);
    }

    private double _monthlyStaffCost;
    public double MonthlyStaffCost
    {
        get => _monthlyStaffCost;
        private set => this.RaiseAndSetIfChanged(ref _monthlyStaffCost, value);
    }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<StaffHireRequest, Unit> HireStaffCommand { get; }
    public ReactiveCommand<string, Unit> TerminateStaffCommand { get; }
    public ReactiveCommand<(string StaffId, string BookerId), Unit> CalculateCompatibilityCommand { get; }
    public ReactiveCommand<Unit, Unit> RecalculateAllCompatibilitiesCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public async Task LoadDataAsync()
    {
        try
        {
            Logger.Info("Loading staff management data...");

            // Charger tout le staff
            var allStaffMembers = await _staffRepository.GetStaffByCompanyIdAsync(_companyId);
            AllStaff.Clear();
            foreach (var staff in allStaffMembers)
            {
                AllStaff.Add(MapStaffMemberToViewModel(staff));
            }

            // Charger les staff créatifs
            var creativeStaffMembers = await _staffRepository.GetCreativeStaffByCompanyIdAsync(_companyId);
            CreativeStaff.Clear();
            foreach (var creative in creativeStaffMembers)
            {
                var baseVm = AllStaff.FirstOrDefault(s => s.StaffId == creative.StaffId);
                if (baseVm != null)
                {
                    CreativeStaff.Add(MapCreativeStaffToViewModel(creative, baseVm));
                }
            }

            // Charger les staff structurels
            var structuralStaffMembers = await _staffRepository.GetStructuralStaffByCompanyIdAsync(_companyId);
            StructuralStaff.Clear();
            foreach (var structural in structuralStaffMembers)
            {
                var baseVm = AllStaff.FirstOrDefault(s => s.StaffId == structural.StaffId);
                if (baseVm != null)
                {
                    StructuralStaff.Add(MapStructuralStaffToViewModel(structural, baseVm));
                }
            }

            // Charger les trainers
            var trainerMembers = await _staffRepository.GetTrainersByCompanyIdAsync(_companyId);
            Trainers.Clear();
            foreach (var trainer in trainerMembers)
            {
                var baseVm = AllStaff.FirstOrDefault(s => s.StaffId == trainer.StaffId);
                if (baseVm != null)
                {
                    Trainers.Add(MapTrainerToViewModel(trainer, baseVm));
                }
            }

            // Identifier le staff créatif dangereux
            DangerousStaff.Clear();
            var dangerousCreatives = await _staffRepository.GetDangerousCreativeStaffAsync(_companyId);
            foreach (var dangerous in dangerousCreatives)
            {
                var baseVm = AllStaff.FirstOrDefault(s => s.StaffId == dangerous.StaffId);
                if (baseVm != null)
                {
                    DangerousStaff.Add(MapCreativeStaffToViewModel(dangerous, baseVm));
                }
            }

            // Calculer les statistiques
            TotalStaffCount = allStaffMembers.Count;
            MonthlyStaffCost = await _staffRepository.CalculateMonthlyStaffCostAsync(_companyId);

            Logger.Info($"Loaded {TotalStaffCount} staff members (Creative: {CreativeStaff.Count}, " +
                      $"Structural: {StructuralStaff.Count}, Trainers: {Trainers.Count})");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading staff data: {ex.Message}", ex);
        }
    }

    public async Task LoadCompatibilitiesAsync(string bookerId)
    {
        try
        {
            Logger.Info($"Loading compatibilities for booker {bookerId}...");

            var compatibilityModels = await _compatibilityRepository.GetCompatibilitiesByBookerIdAsync(bookerId);
            Compatibilities.Clear();

            foreach (var compat in compatibilityModels.OrderByDescending(c => c.OverallScore))
            {
                var staff = await _staffRepository.GetStaffMemberByIdAsync(compat.StaffId);
                if (staff != null)
                {
                    Compatibilities.Add(MapCompatibilityToViewModel(compat, staff.Name));
                }
            }

            Logger.Info($"Loaded {Compatibilities.Count} compatibilities");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading compatibilities: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // PRIVATE COMMAND HANDLERS
    // ====================================================================

    private async Task HireStaffAsync(StaffHireRequest parameters)
    {
        try
        {
            Logger.Info($"Hiring new staff: {parameters.Member.Name}...");

            await _staffRepository.SaveStaffMemberAsync(parameters.Member);

            if (parameters.CreativeStaff != null)
            {
                await _staffRepository.SaveCreativeStaffAsync(parameters.CreativeStaff);
            }

            if (parameters.StructuralStaff != null)
            {
                await _staffRepository.SaveStructuralStaffAsync(parameters.StructuralStaff);
            }

            if (parameters.Trainer != null)
            {
                await _staffRepository.SaveTrainerAsync(parameters.Trainer);
            }

            await LoadDataAsync();

            Logger.Info($"Staff hired: {parameters.Member.Name} ({parameters.Member.Role})");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error hiring staff: {ex.Message}", ex);
        }
    }

    private async Task TerminateStaffAsync(string staffId)
    {
        try
        {
            Logger.Info($"Terminating staff: {staffId}...");

            await _staffRepository.DeleteStaffMemberAsync(staffId);

            await LoadDataAsync();

            Logger.Info("Staff terminated successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error terminating staff: {ex.Message}", ex);
        }
    }

    private async Task CalculateCompatibilityAsync((string StaffId, string BookerId) parameters)
    {
        try
        {
            Logger.Info($"Calculating compatibility between staff {parameters.StaffId} and booker {parameters.BookerId}...");

            // Note: Cette méthode nécessiterait d'avoir accès au Booker
            // Pour l'instant, elle est un placeholder
            // TODO: Implémenter le calcul complet avec IBookerRepository

            Logger.Info("Compatibility calculated");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error calculating compatibility: {ex.Message}", ex);
        }
    }

    private async Task RecalculateAllCompatibilitiesAsync()
    {
        try
        {
            Logger.Info("Recalculating all compatibilities...");

            var needingRecalc = await _compatibilityRepository.GetCompatibilitiesNeedingRecalculationAsync();

            Logger.Info($"Found {needingRecalc.Count} compatibilities needing recalculation");

            // TODO: Implémenter le recalcul en batch

        }
        catch (Exception ex)
        {
            Logger.Error($"Error recalculating compatibilities: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private void FilterStaffByDepartment()
    {
        // Cette méthode serait utilisée pour filtrer la vue en fonction du département sélectionné
        // L'implémentation complète nécessiterait un système de filtre plus sophistiqué
        Logger.Info($"Filtering staff by department: {SelectedDepartment}");
    }

    private static StaffMemberListItemViewModel MapStaffMemberToViewModel(StaffMember staff)
    {
        var monthlySalary = (decimal)(staff.AnnualSalary / 12.0);
        return new StaffMemberListItemViewModel(
            staff.StaffId,
            staff.Name,
            staff.Role.ToString(),
            staff.Department.ToString(),
            staff.SkillScore,
            staff.YearsOfExperience,
            staff.EmploymentStatus,
            monthlySalary,
            staff.HireDate,
            staff.ContractEndDate);
    }

    private static CreativeStaffViewModel MapCreativeStaffToViewModel(
        CreativeStaff creative,
        StaffMemberListItemViewModel baseVm)
    {
        return new CreativeStaffViewModel(
            baseVm,
            creative.CreativityScore,
            creative.ConsistencyScore,
            creative.WorkerBias.ToString(),
            creative.BookerCompatibilityScore,
            creative.CanRuinStorylines);
    }

    private static StructuralStaffViewModel MapStructuralStaffToViewModel(
        StructuralStaff structural,
        StaffMemberListItemViewModel baseVm)
    {
        return new StructuralStaffViewModel(
            baseVm,
            structural.ExpertiseDomain,
            structural.EfficiencyScore,
            structural.TotalInterventions,
            structural.SuccessfulInterventions);
    }

    private static TrainerViewModel MapTrainerToViewModel(
        Trainer trainer,
        StaffMemberListItemViewModel baseVm)
    {
        return new TrainerViewModel(
            baseVm,
            trainer.InfrastructureId,
            trainer.TrainingSpecialization,
            trainer.ProgressionBonus,
            trainer.CurrentStudents,
            trainer.MaxStudentCapacity,
            trainer.GraduatedStudents,
            trainer.FailedStudents);
    }

    private static StaffCompatibilityViewModel MapCompatibilityToViewModel(
        StaffCompatibility compatibility,
        string staffName)
    {
        return new StaffCompatibilityViewModel(
            compatibility.CompatibilityId,
            staffName,
            compatibility.OverallScore,
            compatibility.CreativeVisionScore,
            compatibility.BookingStyleScore,
            compatibility.BiasAlignmentScore,
            compatibility.RiskToleranceScore,
            compatibility.PersonalChemistryScore,
            compatibility.SuccessfulCollaborations + compatibility.FailedCollaborations,
            compatibility.SuccessfulCollaborations,
            compatibility.LastCalculatedAt);
    }
}
