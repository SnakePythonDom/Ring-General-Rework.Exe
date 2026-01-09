using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Youth;

/// <summary>
/// ViewModel pour le système youth development.
/// Enrichi dans Phase 6.3 avec structures, programmes, staff et génération.
/// </summary>
public sealed class YouthViewModel : ViewModelBase
{
    private readonly GameRepository? _repository;
    private ShowContext? _context;
    private decimal _budget = 100_000m;
    private int _totalTrainees;
    private string? _coachName;
    private YouthGenerationOptionViewModel? _generationSelection;
    private YouthStructureViewModel? _structureSelection;
    private int _budgetNouveau = 50_000;
    private string? _coachWorkerId;
    private string? _coachRole;
    private string? _actionMessage;

    public YouthViewModel(GameRepository? repository = null)
    {
        _repository = repository;

        // Collections originales
        Trainees = new ObservableCollection<TraineeItemViewModel>();

        // Phase 6.3 - Nouvelles collections
        Structures = new ObservableCollection<YouthStructureViewModel>();
        Programs = new ObservableCollection<YouthProgramViewModel>();
        StaffAssignments = new ObservableCollection<YouthStaffAssignmentViewModel>();

        // Options de génération
        GenerationModes = new List<YouthGenerationOptionViewModel>
        {
            new YouthGenerationOptionViewModel { Mode = "Manual", Label = "Génération Manuelle", Description = "Créer trainees manuellement" },
            new YouthGenerationOptionViewModel { Mode = "Auto", Label = "Génération Auto", Description = "Génération automatique hebdomadaire" },
            new YouthGenerationOptionViewModel { Mode = "Scouting", Label = "Scouting", Description = "Découverte via scouting" },
            new YouthGenerationOptionViewModel { Mode = "Tryout", Label = "Tryout", Description = "Session tryout ouverte" }
        };

        // Phase 6.3 - Commandes
        CreateStructureCommand = ReactiveCommand.Create(CreateStructure);
        AssignCoachCommand = ReactiveCommand.Create(AssignCoach);
        UpdateBudgetCommand = ReactiveCommand.Create<YouthStructureViewModel>(UpdateBudget);
        GenerateTraineesCommand = ReactiveCommand.Create(GenerateTrainees);

        LoadYouthData();
    }

    #region Collections

    public ObservableCollection<TraineeItemViewModel> Trainees { get; }
    public ObservableCollection<YouthStructureViewModel> Structures { get; }
    public ObservableCollection<YouthProgramViewModel> Programs { get; }
    public ObservableCollection<YouthStaffAssignmentViewModel> StaffAssignments { get; }

    #endregion

    #region Properties

    public decimal Budget
    {
        get => _budget;
        set => this.RaiseAndSetIfChanged(ref _budget, value);
    }

    public string BudgetFormatted => $"${Budget:N0}";

    public int TotalTrainees
    {
        get => _totalTrainees;
        set => this.RaiseAndSetIfChanged(ref _totalTrainees, value);
    }

    public string? CoachName
    {
        get => _coachName;
        set => this.RaiseAndSetIfChanged(ref _coachName, value);
    }

    public YouthGenerationOptionViewModel? GenerationSelection
    {
        get => _generationSelection;
        set => this.RaiseAndSetIfChanged(ref _generationSelection, value);
    }

    public YouthStructureViewModel? StructureSelection
    {
        get => _structureSelection;
        set => this.RaiseAndSetIfChanged(ref _structureSelection, value);
    }

    public int BudgetNouveau
    {
        get => _budgetNouveau;
        set => this.RaiseAndSetIfChanged(ref _budgetNouveau, value);
    }

    public string? CoachWorkerId
    {
        get => _coachWorkerId;
        set => this.RaiseAndSetIfChanged(ref _coachWorkerId, value);
    }

    public string? CoachRole
    {
        get => _coachRole;
        set => this.RaiseAndSetIfChanged(ref _coachRole, value);
    }

    public string? ActionMessage
    {
        get => _actionMessage;
        private set => this.RaiseAndSetIfChanged(ref _actionMessage, value);
    }

    public IReadOnlyList<YouthGenerationOptionViewModel> GenerationModes { get; }

    public int TotalStructures => Structures.Count;
    public int TotalPrograms => Programs.Count;

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> CreateStructureCommand { get; }
    public ReactiveCommand<Unit, Unit> AssignCoachCommand { get; }
    public ReactiveCommand<YouthStructureViewModel, Unit> UpdateBudgetCommand { get; }
    public ReactiveCommand<Unit, Unit> GenerateTraineesCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Phase 6.3 - Charge le système youth depuis le contexte
    /// </summary>
    public void LoadYouthSystem(ShowContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        Structures.Clear();
        Programs.Clear();
        StaffAssignments.Clear();
        Trainees.Clear();

        try
        {
            if (_repository == null)
            {
                LoadPlaceholderStructures();
                return;
            }

            using var connection = _repository.CreateConnection();

            // Charger les structures youth
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT StructureId, Name, Budget, TraineeCount, Level
                    FROM YouthStructures
                    ORDER BY Name";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Structures.Add(new YouthStructureViewModel
                    {
                        StructureId = reader.GetString(0),
                        Name = reader.GetString(1),
                        Budget = reader.GetInt32(2),
                        TraineeCount = reader.GetInt32(3),
                        Level = reader.GetInt32(4)
                    });
                }
            }

            // Charger les programmes
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ProgramId, Name, Type, Duration, Effectiveness
                    FROM YouthPrograms
                    WHERE IsActive = 1";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Programs.Add(new YouthProgramViewModel
                    {
                        ProgramId = reader.GetString(0),
                        Name = reader.GetString(1),
                        Type = reader.GetString(2),
                        Duration = reader.GetInt32(3),
                        Effectiveness = reader.GetInt32(4)
                    });
                }
            }

            // Charger les assignments staff
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT sa.StaffAssignmentId, w.FullName, sa.Role, sa.StructureId
                    FROM YouthStaffAssignments sa
                    JOIN Workers w ON sa.WorkerId = w.WorkerId
                    WHERE sa.IsActive = 1";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    StaffAssignments.Add(new YouthStaffAssignmentViewModel
                    {
                        AssignmentId = reader.GetString(0),
                        CoachName = reader.GetString(1),
                        Role = reader.GetString(2),
                        StructureId = reader.GetString(3)
                    });
                }
            }

            this.RaisePropertyChanged(nameof(TotalStructures));
            this.RaisePropertyChanged(nameof(TotalPrograms));

            Logger.Info($"Système youth chargé : {Structures.Count} structures, {Programs.Count} programmes");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur lors du chargement du système youth : {ex.Message}");
            LoadPlaceholderStructures();
        }
    }

    /// <summary>
    /// Phase 6.3 - Crée une nouvelle structure youth
    /// </summary>
    public void CreateStructure()
    {
        if (_repository == null)
        {
            ActionMessage = "Repository non disponible";
            return;
        }

        try
        {
            var structureId = $"YS-{Guid.NewGuid():N}".ToUpperInvariant();
            var nom = $"Youth System {Structures.Count + 1}";

            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO YouthStructures (StructureId, Name, Budget, TraineeCount, Level)
                VALUES (@id, @name, @budget, 0, 1)";

            cmd.Parameters.AddWithValue("@id", structureId);
            cmd.Parameters.AddWithValue("@name", nom);
            cmd.Parameters.AddWithValue("@budget", BudgetNouveau);

            cmd.ExecuteNonQuery();

            Structures.Add(new YouthStructureViewModel
            {
                StructureId = structureId,
                Name = nom,
                Budget = BudgetNouveau,
                TraineeCount = 0,
                Level = 1
            });

            ActionMessage = $"Structure '{nom}' créée avec succès";
            this.RaisePropertyChanged(nameof(TotalStructures));

            Logger.Info($"Structure youth créée : {structureId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur création structure : {ex.Message}");
            ActionMessage = $"Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Phase 6.3 - Assigne un coach à une structure
    /// </summary>
    public void AssignCoach()
    {
        if (_repository == null || string.IsNullOrWhiteSpace(CoachWorkerId) ||
            string.IsNullOrWhiteSpace(CoachRole) || StructureSelection == null)
        {
            ActionMessage = "Veuillez sélectionner coach, rôle et structure";
            return;
        }

        try
        {
            var assignmentId = $"YSA-{Guid.NewGuid():N}".ToUpperInvariant();

            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO YouthStaffAssignments (StaffAssignmentId, WorkerId, Role, StructureId, IsActive)
                VALUES (@id, @workerId, @role, @structureId, 1)";

            cmd.Parameters.AddWithValue("@id", assignmentId);
            cmd.Parameters.AddWithValue("@workerId", CoachWorkerId);
            cmd.Parameters.AddWithValue("@role", CoachRole);
            cmd.Parameters.AddWithValue("@structureId", StructureSelection.StructureId);

            cmd.ExecuteNonQuery();

            // Récupérer le nom du coach
            string coachName = "Coach";
            using (var cmdName = connection.CreateCommand())
            {
                cmdName.CommandText = "SELECT FullName FROM Workers WHERE WorkerId = @workerId";
                cmdName.Parameters.AddWithValue("@workerId", CoachWorkerId);
                var result = cmdName.ExecuteScalar();
                if (result != null)
                    coachName = result.ToString() ?? "Coach";
            }

            StaffAssignments.Add(new YouthStaffAssignmentViewModel
            {
                AssignmentId = assignmentId,
                CoachName = coachName,
                Role = CoachRole,
                StructureId = StructureSelection.StructureId
            });

            ActionMessage = $"Coach '{coachName}' assigné comme {CoachRole}";

            Logger.Info($"Coach assigné : {CoachWorkerId} → {StructureSelection.StructureId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur assignment coach : {ex.Message}");
            ActionMessage = $"Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Phase 6.3 - Met à jour le budget d'une structure
    /// </summary>
    public void UpdateBudget(YouthStructureViewModel? structure)
    {
        if (_repository == null || structure == null)
        {
            ActionMessage = "Veuillez sélectionner une structure";
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE YouthStructures
                SET Budget = @budget
                WHERE StructureId = @id";

            cmd.Parameters.AddWithValue("@budget", BudgetNouveau);
            cmd.Parameters.AddWithValue("@id", structure.StructureId);

            cmd.ExecuteNonQuery();

            structure.Budget = BudgetNouveau;

            ActionMessage = $"Budget de '{structure.Name}' mis à jour : ${BudgetNouveau:N0}";

            Logger.Info($"Budget mis à jour : {structure.StructureId} → ${BudgetNouveau}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur mise à jour budget : {ex.Message}");
            ActionMessage = $"Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Phase 6.3 - Génère de nouveaux trainees
    /// </summary>
    public void GenerateTrainees()
    {
        if (GenerationSelection == null)
        {
            ActionMessage = "Veuillez sélectionner un mode de génération";
            return;
        }

        try
        {
            var mode = GenerationSelection.Mode;
            var count = mode switch
            {
                "Manual" => 1,
                "Auto" => 3,
                "Scouting" => 2,
                "Tryout" => 5,
                _ => 1
            };

            for (int i = 0; i < count; i++)
            {
                var trainee = GenerateRandomTrainee(mode);
                Trainees.Add(trainee);

                if (_repository != null)
                {
                    SaveTrainee(trainee);
                }
            }

            TotalTrainees = Trainees.Count;
            ActionMessage = $"{count} trainee(s) généré(s) via {GenerationSelection.Label}";

            Logger.Info($"{count} trainees générés via {mode}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erreur génération trainees : {ex.Message}");
            ActionMessage = $"Erreur : {ex.Message}";
        }
    }

    #endregion

    #region Private Methods

    private void LoadYouthData()
    {
        if (_repository == null)
        {
            LoadPlaceholderData();
            return;
        }

        try
        {
            using var connection = _repository.CreateConnection();

            // Charger le budget youth
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT YouthBudget FROM Companies WHERE IsPlayerControlled = 1 LIMIT 1";
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    Budget = Convert.ToDecimal(result);
                }
            }

            // Charger les trainees
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Youth";
                TotalTrainees = Convert.ToInt32(cmd.ExecuteScalar());
            }

            Logger.Info($"{TotalTrainees} trainees, Budget: ${Budget:N0}");
        }
        catch (Exception ex)
        {
            Logger.Error($"[YouthViewModel] Erreur: {ex.Message}");
            LoadPlaceholderData();
        }
    }

    private void LoadPlaceholderData()
    {
        Trainees.Add(new TraineeItemViewModel
        {
            Name = "John Morrison Jr.",
            Age = 19,
            Potential = 75,
            Progress = 45
        });
        Trainees.Add(new TraineeItemViewModel
        {
            Name = "Sarah Phoenix",
            Age = 21,
            Potential = 82,
            Progress = 68
        });
        Trainees.Add(new TraineeItemViewModel
        {
            Name = "Mike Thunder",
            Age = 20,
            Potential = 70,
            Progress = 52
        });
    }

    private void LoadPlaceholderStructures()
    {
        Structures.Add(new YouthStructureViewModel
        {
            StructureId = "YS001",
            Name = "Performance Center",
            Budget = 250_000,
            TraineeCount = 12,
            Level = 3
        });

        Programs.Add(new YouthProgramViewModel
        {
            ProgramId = "YP001",
            Name = "Wrestling Fundamentals",
            Type = "Technical",
            Duration = 12,
            Effectiveness = 75
        });

        StaffAssignments.Add(new YouthStaffAssignmentViewModel
        {
            AssignmentId = "YSA001",
            CoachName = "William Regal",
            Role = "Head Coach",
            StructureId = "YS001"
        });
    }

    private TraineeItemViewModel GenerateRandomTrainee(string mode)
    {
        var random = new Random();
        var names = new[] { "Alex", "Jordan", "Taylor", "Morgan", "Casey", "Drew", "Sam", "Riley" };
        var surnames = new[] { "Thunder", "Phoenix", "Storm", "Blaze", "Steel", "Shadow", "Knight", "Raven" };

        var basePotential = mode switch
        {
            "Scouting" => random.Next(65, 90),
            "Tryout" => random.Next(50, 75),
            "Auto" => random.Next(55, 80),
            _ => random.Next(60, 85)
        };

        return new TraineeItemViewModel
        {
            Name = $"{names[random.Next(names.Length)]} {surnames[random.Next(surnames.Length)]}",
            Age = random.Next(18, 24),
            Potential = basePotential,
            Progress = random.Next(10, 40)
        };
    }

    private void SaveTrainee(TraineeItemViewModel trainee)
    {
        if (_repository == null) return;

        try
        {
            using var connection = _repository.CreateConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Youth (YouthId, Name, Age, Potential, Progress)
                VALUES (@id, @name, @age, @potential, @progress)";

            cmd.Parameters.AddWithValue("@id", $"Y-{Guid.NewGuid():N}".ToUpperInvariant());
            cmd.Parameters.AddWithValue("@name", trainee.Name);
            cmd.Parameters.AddWithValue("@age", trainee.Age);
            cmd.Parameters.AddWithValue("@potential", trainee.Potential);
            cmd.Parameters.AddWithValue("@progress", trainee.Progress);

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Impossible de sauvegarder trainee : {ex.Message}");
        }
    }

    #endregion
}

public sealed class TraineeItemViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private int _age;
    private int _potential;
    private int _progress;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => this.RaiseAndSetIfChanged(ref _age, value);
    }

    public int Potential
    {
        get => _potential;
        set => this.RaiseAndSetIfChanged(ref _potential, value);
    }

    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public string AgeDisplay => $"{Age} ans";
    public string ProgressDisplay => $"{Progress}%";
}

/// <summary>
/// Phase 6.3 - ViewModel pour une structure youth
/// </summary>
public sealed class YouthStructureViewModel : ViewModelBase
{
    private string _structureId = string.Empty;
    private string _name = string.Empty;
    private int _budget;
    private int _traineeCount;
    private int _level;

    public string StructureId
    {
        get => _structureId;
        set => this.RaiseAndSetIfChanged(ref _structureId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Budget
    {
        get => _budget;
        set => this.RaiseAndSetIfChanged(ref _budget, value);
    }

    public int TraineeCount
    {
        get => _traineeCount;
        set => this.RaiseAndSetIfChanged(ref _traineeCount, value);
    }

    public int Level
    {
        get => _level;
        set => this.RaiseAndSetIfChanged(ref _level, value);
    }

    public string BudgetDisplay => $"${Budget:N0}";
    public string TraineeCountDisplay => $"{TraineeCount} trainees";
    public string LevelDisplay => $"Level {Level}";
}

/// <summary>
/// Phase 6.3 - ViewModel pour un programme youth
/// </summary>
public sealed class YouthProgramViewModel : ViewModelBase
{
    private string _programId = string.Empty;
    private string _name = string.Empty;
    private string _type = string.Empty;
    private int _duration;
    private int _effectiveness;

    public string ProgramId
    {
        get => _programId;
        set => this.RaiseAndSetIfChanged(ref _programId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    public int Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    public int Effectiveness
    {
        get => _effectiveness;
        set => this.RaiseAndSetIfChanged(ref _effectiveness, value);
    }

    public string DurationDisplay => $"{Duration} semaines";
    public string EffectivenessDisplay => $"{Effectiveness}%";
}

/// <summary>
/// Phase 6.3 - ViewModel pour un assignment de coach youth
/// </summary>
public sealed class YouthStaffAssignmentViewModel : ViewModelBase
{
    private string _assignmentId = string.Empty;
    private string _coachName = string.Empty;
    private string _role = string.Empty;
    private string _structureId = string.Empty;

    public string AssignmentId
    {
        get => _assignmentId;
        set => this.RaiseAndSetIfChanged(ref _assignmentId, value);
    }

    public string CoachName
    {
        get => _coachName;
        set => this.RaiseAndSetIfChanged(ref _coachName, value);
    }

    public string Role
    {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }

    public string StructureId
    {
        get => _structureId;
        set => this.RaiseAndSetIfChanged(ref _structureId, value);
    }

    public string Display => $"{CoachName} - {Role}";
}

/// <summary>
/// Phase 6.3 - Option de génération de trainees
/// </summary>
public sealed class YouthGenerationOptionViewModel : ViewModelBase
{
    private string _mode = string.Empty;
    private string _label = string.Empty;
    private string _description = string.Empty;

    public string Mode
    {
        get => _mode;
        set => this.RaiseAndSetIfChanged(ref _mode, value);
    }

    public string Label
    {
        get => _label;
        set => this.RaiseAndSetIfChanged(ref _label, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
}
