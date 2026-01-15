# 🎯 Plan Sprint 2 : ProfileView Universel

**Date** : 7 janvier 2026
**Chef de Projet** : Claude DevOps
**Version** : 1.0
**Branche** : `claude/rework-performance-attributes-YBXRx`
**Priorité** : 🔴 HAUTE
**Durée Estimée** : 4-5 jours

---

## 📋 VUE D'ENSEMBLE

### Objectif

Créer la **fiche de profil complète universelle** avec 6 onglets pour afficher et gérer toutes les informations d'un Worker, Staff ou Trainee.

### Périmètre

**ProfileView** avec 6 onglets fonctionnels :
1. 📊 **ATTRIBUTS** - Stats détaillées avec fiche personnage
2. 📝 **CONTRATS** - Termes, historique, actions
3. 🎭 **GIMMICK/PUSH** - Personnage, alignment, push level
4. 👥 **RELATIONS** - Relations 1-à-1 + Factions/Équipes
5. 📖 **HISTORIQUE** - Biographie, matchs, titres, blessures
6. 📌 **NOTES** - Notes personnalisables par catégorie

### Nouveautés v2.0

- ✨ **Fiche personnage** avec photo/avatar dans tab ATTRIBUTS
- ✨ **Système de Factions** (Tag Team, Trio, Faction) dans tab RELATIONS
- ✨ **Spécialisations workers** (Brawler, Technical, High-Flyer, etc.)
- ✨ **Géographie complète** (naissance + résidence)

---

## 🎯 DÉPENDANCES

### Pré-requis

**Sprint 1** : Composants UI Réutilisables ✅ TERMINÉ
- ✅ `AttributeBar` component (pour tab Attributs)
- ✅ `DetailPanel` component (pour context panel)
- ✅ `SortableDataGrid` component (pour listes)
- ✅ `NewsCard` component (non utilisé ici)
- ✅ `RingGeneralTheme.axaml` (thème unifié)

**Données existantes** :
- ✅ `Workers` table avec données seed (BAKI import)
- ✅ `WorkerRepository` fonctionnel
- ✅ Navigation vers ProfileView depuis RosterView

---

## 📐 ARCHITECTURE DÉTAILLÉE

### Structure des Onglets

```
ProfileView (Shell)
├── Header (Photo + Nom + Actions)
└── TabControl (6 tabs)
    ├── Tab 1: ATTRIBUTS
    │   ├── Fiche Personnage (Photo, Identité, Géo)
    │   └── Attributs (Universels, In-Ring, Entertainment, Story)
    ├── Tab 2: CONTRATS
    │   ├── Contrat actuel (termes, dates, salaire)
    │   └── Historique des contrats
    ├── Tab 3: GIMMICK/PUSH
    │   ├── Gimmick actuel + historique
    │   ├── Alignment (Face/Heel/Tweener)
    │   └── Push Level + Booking Intent
    ├── Tab 4: RELATIONS
    │   ├── Relations 1-à-1 (Amitié, Couple, Fraternité, Rivalité)
    │   └── Factions (Tag Team, Trio, Faction)
    ├── Tab 5: HISTORIQUE
    │   ├── Biographie (dates, physique, carrière)
    │   ├── Historique matchs
    │   ├── Historique titres
    │   └── Historique blessures
    └── Tab 6: NOTES
        └── Notes personnalisables avec catégories
```

---

## 🗂️ MODIFICATIONS BASE DE DONNÉES

### Tables à Créer

#### 1. `WorkerRelations` - Relations entre workers

```sql
CREATE TABLE WorkerRelations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId1 INTEGER NOT NULL,
    WorkerId2 INTEGER NOT NULL,
    RelationType TEXT NOT NULL, -- 'Amitie', 'Couple', 'Fraternite', 'Rivalite'
    RelationStrength INTEGER DEFAULT 50, -- 0-100
    Notes TEXT,
    IsPublic INTEGER DEFAULT 1, -- 1 = Kayfabe visible, 0 = Backstage only
    CreatedDate TEXT NOT NULL,
    FOREIGN KEY (WorkerId1) REFERENCES Workers(Id),
    FOREIGN KEY (WorkerId2) REFERENCES Workers(Id),
    UNIQUE(WorkerId1, WorkerId2)
);
```

#### 2. `Factions` - Groupes de wrestlers

```sql
CREATE TABLE Factions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    FactionType TEXT NOT NULL, -- 'TagTeam', 'Trio', 'Faction'
    LeaderId INTEGER, -- Optionnel
    Status TEXT DEFAULT 'Active', -- 'Active', 'Inactive', 'Disbanded'
    CreatedWeek INTEGER NOT NULL,
    CreatedYear INTEGER NOT NULL,
    DisbandedWeek INTEGER,
    DisbandedYear INTEGER,
    FOREIGN KEY (LeaderId) REFERENCES Workers(Id)
);
```

#### 3. `FactionMembers` - Membres des factions

```sql
CREATE TABLE FactionMembers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FactionId INTEGER NOT NULL,
    WorkerId INTEGER NOT NULL,
    JoinedWeek INTEGER NOT NULL,
    JoinedYear INTEGER NOT NULL,
    LeftWeek INTEGER,
    LeftYear INTEGER,
    FOREIGN KEY (FactionId) REFERENCES Factions(Id),
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id),
    UNIQUE(FactionId, WorkerId)
);
```

#### 4. `WorkerNotes` - Notes sur workers

```sql
CREATE TABLE WorkerNotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    Text TEXT NOT NULL,
    Category TEXT DEFAULT 'Other', -- 'BookingIdeas', 'Personal', 'Injury', 'Other'
    CreatedDate TEXT NOT NULL,
    ModifiedDate TEXT,
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id)
);
```

#### 5. `ContractHistory` - Historique des contrats

```sql
CREATE TABLE ContractHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    WeeklySalary REAL NOT NULL,
    SigningBonus REAL DEFAULT 0,
    ContractType TEXT DEFAULT 'Exclusive', -- 'Exclusive', 'PerAppearance', 'Developmental'
    Status TEXT DEFAULT 'Active', -- 'Active', 'Expired', 'Terminated'
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id)
);
```

#### 6. Mises à jour de tables existantes

```sql
-- Ajouter colonnes à Workers si pas déjà faites dans le rework précédent
ALTER TABLE Workers ADD COLUMN CurrentGimmick TEXT;
ALTER TABLE Workers ADD COLUMN Alignment TEXT DEFAULT 'Face'; -- 'Face', 'Heel', 'Tweener'
ALTER TABLE Workers ADD COLUMN PushLevel TEXT DEFAULT 'MidCard'; -- 'MainEvent', 'UpperMid', 'MidCard', 'LowerMid', 'Jobber'
ALTER TABLE Workers ADD COLUMN TvRole INTEGER DEFAULT 50; -- 0-100 scale
ALTER TABLE Workers ADD COLUMN BookingIntent TEXT; -- Notes du booker
```

---

## 📦 MODELS À CRÉER

### 1. Models de Relations

**Fichier** : `/src/RingGeneral.Core/Models/Relations/WorkerRelation.cs`

```csharp
namespace RingGeneral.Core.Models.Relations
{
    public enum RelationType
    {
        Amitie,      // 🤝 Friendship
        Couple,      // ❤ Romantic
        Fraternite,  // 👊 Brotherhood
        Rivalite     // ⚔ Rivalry
    }

    public class WorkerRelation
    {
        public int Id { get; set; }
        public int WorkerId1 { get; set; }
        public int WorkerId2 { get; set; }
        public RelationType RelationType { get; set; }
        public int RelationStrength { get; set; } = 50; // 0-100
        public string? Notes { get; set; }
        public bool IsPublic { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        // Navigation
        public Worker? Worker1 { get; set; }
        public Worker? Worker2 { get; set; }
    }
}
```

### 2. Models de Factions

**Fichier** : `/src/RingGeneral.Core/Models/Relations/Faction.cs`

```csharp
namespace RingGeneral.Core.Models.Relations
{
    public enum FactionType
    {
        TagTeam,  // 🤜🤛 (2 membres)
        Trio,     // 🎯 (3 membres)
        Faction   // 👊 (3+ membres, généralement 4-6)
    }

    public enum FactionStatus
    {
        Active,
        Inactive,
        Disbanded
    }

    public class Faction
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public FactionType FactionType { get; set; }
        public int? LeaderId { get; set; }
        public FactionStatus Status { get; set; } = FactionStatus.Active;
        public int CreatedWeek { get; set; }
        public int CreatedYear { get; set; }
        public int? DisbandedWeek { get; set; }
        public int? DisbandedYear { get; set; }

        // Navigation
        public Worker? Leader { get; set; }
        public List<FactionMember> Members { get; set; } = new();
    }

    public class FactionMember
    {
        public int Id { get; set; }
        public int FactionId { get; set; }
        public int WorkerId { get; set; }
        public int JoinedWeek { get; set; }
        public int JoinedYear { get; set; }
        public int? LeftWeek { get; set; }
        public int? LeftYear { get; set; }

        // Navigation
        public Faction? Faction { get; set; }
        public Worker? Worker { get; set; }
    }
}
```

### 3. Models de Notes

**Fichier** : `/src/RingGeneral.Core/Models/WorkerNote.cs`

```csharp
namespace RingGeneral.Core.Models
{
    public enum NoteCategory
    {
        BookingIdeas,
        Personal,
        Injury,
        Other
    }

    public class WorkerNote
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public string Text { get; set; } = string.Empty;
        public NoteCategory Category { get; set; } = NoteCategory.Other;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public Worker? Worker { get; set; }
    }
}
```

### 4. Models de Contrats (si pas déjà existants)

**Fichier** : `/src/RingGeneral.Core/Models/ContractHistory.cs`

```csharp
namespace RingGeneral.Core.Models
{
    public class ContractHistory
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal WeeklySalary { get; set; }
        public decimal SigningBonus { get; set; }
        public string ContractType { get; set; } = "Exclusive";
        public string Status { get; set; } = "Active";

        // Navigation
        public Worker? Worker { get; set; }
    }
}
```

---

## 📚 REPOSITORIES À CRÉER

### 1. RelationsRepository

**Interface** : `/src/RingGeneral.Data/Repositories/Interfaces/IRelationsRepository.cs`

```csharp
namespace RingGeneral.Data.Repositories.Interfaces
{
    public interface IRelationsRepository
    {
        // Worker Relations
        List<WorkerRelation> GetRelations(int workerId);
        WorkerRelation? GetRelation(int relationId);
        void AddRelation(WorkerRelation relation);
        void UpdateRelation(WorkerRelation relation);
        void DeleteRelation(int relationId);

        // Factions
        List<Faction> GetFactions(int workerId);
        Faction? GetFaction(int factionId);
        void CreateFaction(Faction faction);
        void UpdateFaction(Faction faction);
        void DisbandFaction(int factionId, int week, int year);

        // Faction Members
        List<FactionMember> GetFactionMembers(int factionId);
        void AddFactionMember(FactionMember member);
        void RemoveFactionMember(int factionId, int workerId, int week, int year);
    }
}
```

**Implémentation** : `/src/RingGeneral.Data/Repositories/RelationsRepository.cs`

### 2. NotesRepository

**Interface** : `/src/RingGeneral.Data/Repositories/Interfaces/INotesRepository.cs`

```csharp
namespace RingGeneral.Data.Repositories.Interfaces
{
    public interface INotesRepository
    {
        List<WorkerNote> GetNotes(int workerId);
        WorkerNote? GetNote(int noteId);
        void AddNote(WorkerNote note);
        void UpdateNote(WorkerNote note);
        void DeleteNote(int noteId);
    }
}
```

**Implémentation** : `/src/RingGeneral.Data/Repositories/NotesRepository.cs`

---

## 🎨 VIEWMODELS À CRÉER

### Structure Hiérarchique

```
ProfileViewModel (Shell)
├── string ProfileType (Worker/Staff/Trainee)
├── string WorkerId
├── Commands (Edit, Release)
└── Tab ViewModels
    ├── AttributesTabViewModel
    ├── ContractsTabViewModel
    ├── GimmickTabViewModel
    ├── RelationsTabViewModel
    ├── HistoryTabViewModel
    └── NotesTabViewModel
```

### 1. ProfileViewModel (Shell)

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/ProfileViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly INavigationService _navigationService;

        // Identity
        public string ProfileType { get; } // "Worker", "Staff", "Trainee"
        public string WorkerId { get; }
        public string FullName { get; }
        public string PhotoPath { get; }
        public string Role { get; }

        // Tab ViewModels
        public AttributesTabViewModel AttributesTab { get; }
        public ContractsTabViewModel ContractsTab { get; }
        public GimmickTabViewModel GimmickTab { get; }
        public RelationsTabViewModel RelationsTab { get; }
        public HistoryTabViewModel HistoryTab { get; }
        public NotesTabViewModel NotesTab { get; }

        // Navigation
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
        }

        // Commands
        public ReactiveCommand<int, Unit> SwitchTabCommand { get; }
        public ReactiveCommand<Unit, Unit> EditCommand { get; }
        public ReactiveCommand<Unit, Unit> ReleaseCommand { get; }

        public ProfileViewModel(
            string workerId,
            IWorkerRepository workerRepository,
            IRelationsRepository relationsRepository,
            INotesRepository notesRepository,
            INavigationService navigationService)
        {
            WorkerId = workerId;
            _workerRepository = workerRepository;
            _navigationService = navigationService;

            // Load worker data
            var worker = _workerRepository.GetById(int.Parse(workerId));
            FullName = worker.Name;
            PhotoPath = worker.PhotoPath ?? "/Assets/default-avatar.png";
            Role = worker.Role;
            ProfileType = "Worker"; // TODO: Detect from worker type

            // Initialize tabs
            AttributesTab = new AttributesTabViewModel(workerId, workerRepository);
            ContractsTab = new ContractsTabViewModel(workerId, workerRepository);
            GimmickTab = new GimmickTabViewModel(workerId, workerRepository);
            RelationsTab = new RelationsTabViewModel(workerId, relationsRepository);
            HistoryTab = new HistoryTabViewModel(workerId, workerRepository);
            NotesTab = new NotesTabViewModel(workerId, notesRepository);

            // Commands
            SwitchTabCommand = ReactiveCommand.Create<int>(index => SelectedTabIndex = index);
            EditCommand = ReactiveCommand.Create(OnEdit);
            ReleaseCommand = ReactiveCommand.Create(OnRelease);
        }

        private void OnEdit()
        {
            // TODO: Open edit dialog
        }

        private void OnRelease()
        {
            // TODO: Confirm and release worker
        }
    }
}
```

---

### 2. AttributesTabViewModel

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/AttributesTabViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class AttributesTabViewModel : ViewModelBase
    {
        // FICHE PERSONNAGE (Header)
        public string PhotoPath { get; set; }
        public bool HasCustomPhoto { get; }
        public string FullName { get; }
        public string RingName { get; }

        // Info Rapide
        public string WorkerType { get; } // Main Eventer, Upper Mid-Carder, etc.
        public string TvRole { get; }
        public ObservableCollection<string> Specializations { get; }

        // Âge et Dates
        public int Age { get; }
        public DateTime BirthDate { get; }
        public string BirthDateFormatted { get; } // "27 avril 1977"

        // Géographie
        public string Birthplace { get; } // "West Newbury, USA"
        public string BirthCountry { get; }
        public string Residence { get; } // "Tampa, Floride, USA"
        public string ResidenceCountry { get; }

        // ATTRIBUTS UNIVERSELS
        public int ConditionPhysique { get; }
        public int Moral { get; }
        public int Popularite { get; }
        public int Fatigue { get; }
        public int Momentum { get; }

        // IN-RING (si Worker)
        public int InRing { get; }
        public int Timing { get; }
        public int Psychology { get; }
        public int Selling { get; }
        public int Stamina { get; }
        public int Safety { get; }

        // ENTERTAINMENT (si Worker)
        public int Entertainment { get; }
        public int Charisma { get; }
        public int Promo { get; }
        public int CrowdConnection { get; }
        public int StarPower { get; }

        // STORY (si Worker)
        public int Story { get; }
        public int Storytelling { get; }
        public int CharacterWork { get; }

        public bool IsWorker { get; }

        // Commands
        public ReactiveCommand<Unit, Unit> ChangePhotoCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateAvatarCommand { get; }

        public AttributesTabViewModel(string workerId, IWorkerRepository repository)
        {
            var worker = repository.GetById(int.Parse(workerId));

            // Load all properties from worker
            FullName = worker.Name;
            PhotoPath = worker.PhotoPath ?? "/Assets/default-avatar.png";
            HasCustomPhoto = !string.IsNullOrEmpty(worker.PhotoPath);

            // Calculate age
            BirthDate = worker.BirthDate;
            Age = DateTime.Now.Year - BirthDate.Year;
            BirthDateFormatted = BirthDate.ToString("dd MMMM yyyy", new CultureInfo("fr-FR"));

            // Geography
            Birthplace = $"{worker.BirthCity}, {worker.BirthCountry}";
            Residence = $"{worker.ResidenceCity}, {worker.ResidenceState}, {worker.ResidenceCountry}";

            // Attributes
            ConditionPhysique = worker.Condition;
            Moral = worker.Morale;
            // ... etc

            IsWorker = true; // TODO: Detect type

            // Commands
            ChangePhotoCommand = ReactiveCommand.Create(OnChangePhoto);
            GenerateAvatarCommand = ReactiveCommand.Create(OnGenerateAvatar);
        }

        private void OnChangePhoto()
        {
            // TODO: Open file dialog
        }

        private void OnGenerateAvatar()
        {
            // TODO: Generate procedural avatar
        }
    }
}
```

---

### 3. ContractsTabViewModel

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/ContractsTabViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class ContractsTabViewModel : ViewModelBase
    {
        // Current Contract
        public DateTime ContractStartDate { get; }
        public DateTime ContractEndDate { get; }
        public int ContractWeeksRemaining { get; }
        public decimal WeeklySalary { get; }
        public decimal SigningBonus { get; }
        public string ContractType { get; }
        public bool AutoRenew { get; }
        public bool HasReleaseClause { get; }

        // Contract History
        public ObservableCollection<ContractHistoryItem> ContractHistory { get; }

        // Commands
        public ReactiveCommand<Unit, Unit> RenegotiateCommand { get; }
        public ReactiveCommand<Unit, Unit> ReleaseCommand { get; }
        public ReactiveCommand<Unit, Unit> ExtendCommand { get; }

        public ContractsTabViewModel(string workerId, IWorkerRepository repository)
        {
            var worker = repository.GetById(int.Parse(workerId));

            // Load contract data
            ContractStartDate = worker.ContractStart;
            ContractEndDate = worker.ContractEnd;
            ContractWeeksRemaining = (int)(ContractEndDate - DateTime.Now).TotalDays / 7;
            WeeklySalary = worker.Salary;

            // Load history
            ContractHistory = new ObservableCollection<ContractHistoryItem>();

            // Commands
            RenegotiateCommand = ReactiveCommand.Create(OnRenegotiate);
            ReleaseCommand = ReactiveCommand.Create(OnRelease);
            ExtendCommand = ReactiveCommand.Create(OnExtend);
        }

        private void OnRenegotiate() { /* TODO */ }
        private void OnRelease() { /* TODO */ }
        private void OnExtend() { /* TODO */ }
    }

    public class ContractHistoryItem : ViewModelBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal WeeklySalary { get; set; }
        public string ContractType { get; set; }
        public string Status { get; set; }
    }
}
```

---

### 4. GimmickTabViewModel

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/GimmickTabViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class GimmickTabViewModel : ViewModelBase
    {
        // Current Gimmick
        private string _currentGimmick;
        public string CurrentGimmick
        {
            get => _currentGimmick;
            set => this.RaiseAndSetIfChanged(ref _currentGimmick, value);
        }

        private string _alignment;
        public string Alignment
        {
            get => _alignment;
            set => this.RaiseAndSetIfChanged(ref _alignment, value);
        }

        private string _pushLevel;
        public string PushLevel
        {
            get => _pushLevel;
            set => this.RaiseAndSetIfChanged(ref _pushLevel, value);
        }

        public int TvRole { get; }
        public string BookingIntent { get; set; }

        // History
        public ObservableCollection<string> GimmickHistory { get; }
        public ObservableCollection<string> FinishingMoves { get; }
        public ObservableCollection<string> Signatures { get; }

        // Commands
        public ReactiveCommand<Unit, Unit> ChangeGimmickCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAlignmentCommand { get; }
        public ReactiveCommand<Unit, Unit> AdjustPushCommand { get; }

        public GimmickTabViewModel(string workerId, IWorkerRepository repository)
        {
            var worker = repository.GetById(int.Parse(workerId));

            _currentGimmick = worker.CurrentGimmick ?? worker.Name;
            _alignment = worker.Alignment ?? "Face";
            _pushLevel = worker.PushLevel ?? "MidCard";
            TvRole = worker.TvRole;
            BookingIntent = worker.BookingIntent ?? "";

            GimmickHistory = new ObservableCollection<string>();
            FinishingMoves = new ObservableCollection<string>();
            Signatures = new ObservableCollection<string>();

            // Commands
            ChangeGimmickCommand = ReactiveCommand.Create(OnChangeGimmick);
            ToggleAlignmentCommand = ReactiveCommand.Create(OnToggleAlignment);
            AdjustPushCommand = ReactiveCommand.Create(OnAdjustPush);
        }

        private void OnChangeGimmick() { /* TODO: Open dialog */ }
        private void OnToggleAlignment()
        {
            Alignment = Alignment switch
            {
                "Face" => "Heel",
                "Heel" => "Tweener",
                _ => "Face"
            };
        }
        private void OnAdjustPush() { /* TODO: Open dialog */ }
    }
}
```

---

### 5. RelationsTabViewModel

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/RelationsTabViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class RelationsTabViewModel : ViewModelBase
    {
        private readonly IRelationsRepository _relationsRepository;
        private readonly string _workerId;

        // Relations 1-à-1
        public ObservableCollection<WorkerRelationViewModel> Relations { get; }

        // Factions
        public ObservableCollection<FactionViewModel> Factions { get; }

        // Commands
        public ReactiveCommand<Unit, Unit> AddRelationCommand { get; }
        public ReactiveCommand<WorkerRelationViewModel, Unit> EditRelationCommand { get; }
        public ReactiveCommand<WorkerRelationViewModel, Unit> DeleteRelationCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateFactionCommand { get; }

        public RelationsTabViewModel(string workerId, IRelationsRepository repository)
        {
            _workerId = workerId;
            _relationsRepository = repository;

            // Load relations
            Relations = new ObservableCollection<WorkerRelationViewModel>();
            var relations = repository.GetRelations(int.Parse(workerId));
            foreach (var rel in relations)
            {
                Relations.Add(new WorkerRelationViewModel(rel));
            }

            // Load factions
            Factions = new ObservableCollection<FactionViewModel>();
            var factions = repository.GetFactions(int.Parse(workerId));
            foreach (var faction in factions)
            {
                Factions.Add(new FactionViewModel(faction, repository));
            }

            // Commands
            AddRelationCommand = ReactiveCommand.Create(OnAddRelation);
            EditRelationCommand = ReactiveCommand.Create<WorkerRelationViewModel>(OnEditRelation);
            DeleteRelationCommand = ReactiveCommand.Create<WorkerRelationViewModel>(OnDeleteRelation);
            CreateFactionCommand = ReactiveCommand.Create(OnCreateFaction);
        }

        private void OnAddRelation() { /* TODO: Open dialog */ }
        private void OnEditRelation(WorkerRelationViewModel relation) { /* TODO */ }
        private void OnDeleteRelation(WorkerRelationViewModel relation)
        {
            _relationsRepository.DeleteRelation(relation.RelationId);
            Relations.Remove(relation);
        }
        private void OnCreateFaction() { /* TODO: Open dialog */ }
    }

    public class WorkerRelationViewModel : ViewModelBase
    {
        public int RelationId { get; }
        public string RelatedWorkerId { get; }
        public string RelatedWorkerName { get; }
        public RelationType RelationType { get; }
        public string RelationTypeIcon { get; }
        public int RelationStrength { get; set; }
        public string RelationStrengthText { get; }
        public bool IsStrongRelation => RelationStrength >= 70;
        public bool IsMediumRelation => RelationStrength >= 40 && RelationStrength < 70;
        public string Notes { get; set; }
        public bool IsPublic { get; set; }

        public WorkerRelationViewModel(WorkerRelation relation)
        {
            RelationId = relation.Id;
            RelatedWorkerId = relation.WorkerId2.ToString();
            RelatedWorkerName = relation.Worker2?.Name ?? "Unknown";
            RelationType = relation.RelationType;
            RelationTypeIcon = GetIcon(relation.RelationType);
            RelationStrength = relation.RelationStrength;
            RelationStrengthText = GetStrengthText(relation.RelationStrength);
            Notes = relation.Notes ?? "";
            IsPublic = relation.IsPublic;
        }

        private string GetIcon(RelationType type) => type switch
        {
            RelationType.Amitie => "🤝",
            RelationType.Couple => "❤",
            RelationType.Fraternite => "👊",
            RelationType.Rivalite => "⚔",
            _ => "?"
        };

        private string GetStrengthText(int strength) => strength switch
        {
            >= 90 => "Très Fort",
            >= 70 => "Fort",
            >= 40 => "Moyen",
            _ => "Faible"
        };
    }

    public class FactionViewModel : ViewModelBase
    {
        private readonly IRelationsRepository _repository;

        public int FactionId { get; }
        public string FactionName { get; set; }
        public FactionType FactionType { get; }
        public string FactionTypeIcon { get; }
        public ObservableCollection<string> MemberIds { get; }
        public ObservableCollection<string> MemberNames { get; }
        public string MemberNamesText { get; }
        public string LeaderId { get; set; }
        public string LeaderName { get; }
        public bool HasLeader { get; }
        public FactionStatus Status { get; set; }
        public string StatusColor { get; }
        public int CreatedWeek { get; }
        public int CreatedYear { get; }
        public string CreatedDateText { get; }

        // Commands
        public ReactiveCommand<Unit, Unit> EditFactionCommand { get; }
        public ReactiveCommand<Unit, Unit> DisbandFactionCommand { get; }
        public ReactiveCommand<string, Unit> RemoveMemberCommand { get; }
        public ReactiveCommand<Unit, Unit> AddMemberCommand { get; }

        public FactionViewModel(Faction faction, IRelationsRepository repository)
        {
            _repository = repository;
            FactionId = faction.Id;
            FactionName = faction.Name;
            FactionType = faction.FactionType;
            FactionTypeIcon = GetFactionIcon(faction.FactionType);
            Status = faction.Status;
            StatusColor = GetStatusColor(faction.Status);
            CreatedWeek = faction.CreatedWeek;
            CreatedYear = faction.CreatedYear;
            CreatedDateText = $"Semaine {CreatedWeek}/{CreatedYear}";

            // Load members
            MemberIds = new ObservableCollection<string>();
            MemberNames = new ObservableCollection<string>();
            var members = repository.GetFactionMembers(faction.Id);
            foreach (var member in members)
            {
                MemberIds.Add(member.WorkerId.ToString());
                MemberNames.Add(member.Worker?.Name ?? "Unknown");
            }
            MemberNamesText = string.Join(", ", MemberNames);

            // Leader
            HasLeader = faction.LeaderId.HasValue;
            LeaderId = faction.LeaderId?.ToString() ?? "";
            LeaderName = faction.Leader?.Name ?? "";

            // Commands
            EditFactionCommand = ReactiveCommand.Create(OnEdit);
            DisbandFactionCommand = ReactiveCommand.Create(OnDisband);
            RemoveMemberCommand = ReactiveCommand.Create<string>(OnRemoveMember);
            AddMemberCommand = ReactiveCommand.Create(OnAddMember);
        }

        private string GetFactionIcon(FactionType type) => type switch
        {
            FactionType.TagTeam => "🤜🤛",
            FactionType.Trio => "🎯",
            FactionType.Faction => "👊",
            _ => "?"
        };

        private string GetStatusColor(FactionStatus status) => status switch
        {
            FactionStatus.Active => "#10b981",
            FactionStatus.Inactive => "#f59e0b",
            FactionStatus.Disbanded => "#666666",
            _ => "#666666"
        };

        private void OnEdit() { /* TODO */ }
        private void OnDisband() { /* TODO */ }
        private void OnRemoveMember(string memberId) { /* TODO */ }
        private void OnAddMember() { /* TODO */ }
    }
}
```

---

### 6. HistoryTabViewModel

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/HistoryTabViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class HistoryTabViewModel : ViewModelBase
    {
        // Biographie
        public string RealName { get; }
        public DateTime BirthDate { get; }
        public string Hometown { get; }
        public int Height { get; }
        public int Weight { get; }
        public DateTime CareerStart { get; }
        public DateTime CompanyJoinDate { get; }

        // Historique
        public ObservableCollection<TitleReignViewModel> TitleReigns { get; }
        public ObservableCollection<MatchHistoryViewModel> MatchHistory { get; }
        public ObservableCollection<InjuryHistoryViewModel> InjuryHistory { get; }
        public ObservableCollection<StorylineHistoryViewModel> StorylineHistory { get; }

        // Stats
        public int TotalMatches { get; }
        public int Wins { get; }
        public int Losses { get; }
        public int Draws { get; }
        public decimal WinPercentage { get; }
        public int TotalTitleReigns { get; }

        public HistoryTabViewModel(string workerId, IWorkerRepository repository)
        {
            var worker = repository.GetById(int.Parse(workerId));

            // Bio
            RealName = worker.RealName ?? worker.Name;
            BirthDate = worker.BirthDate;
            Hometown = $"{worker.BirthCity}, {worker.BirthCountry}";
            Height = worker.Height;
            Weight = worker.Weight;
            CareerStart = worker.CareerStartDate;
            CompanyJoinDate = worker.HireDate;

            // Collections
            TitleReigns = new ObservableCollection<TitleReignViewModel>();
            MatchHistory = new ObservableCollection<MatchHistoryViewModel>();
            InjuryHistory = new ObservableCollection<InjuryHistoryViewModel>();
            StorylineHistory = new ObservableCollection<StorylineHistoryViewModel>();

            // Stats (TODO: Calculate from match history)
            TotalMatches = 0;
            Wins = 0;
            Losses = 0;
            Draws = 0;
            WinPercentage = 0;
            TotalTitleReigns = TitleReigns.Count;
        }
    }

    public class TitleReignViewModel : ViewModelBase
    {
        public string TitleName { get; set; }
        public DateTime WonDate { get; set; }
        public DateTime? LostDate { get; set; }
        public int DaysHeld { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class MatchHistoryViewModel : ViewModelBase
    {
        public DateTime MatchDate { get; set; }
        public string ShowName { get; set; }
        public string MatchType { get; set; }
        public string Opponent { get; set; }
        public string Result { get; set; } // Win/Loss/Draw
        public int Rating { get; set; } // 0-100
    }

    public class InjuryHistoryViewModel : ViewModelBase
    {
        public DateTime InjuryDate { get; set; }
        public string InjuryType { get; set; }
        public int WeeksOut { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

    public class StorylineHistoryViewModel : ViewModelBase
    {
        public string StorylineName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
```

---

### 7. NotesTabViewModel

**Fichier** : `/src/RingGeneral.UI/ViewModels/Profile/NotesTabViewModel.cs`

```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class NotesTabViewModel : ViewModelBase
    {
        private readonly INotesRepository _notesRepository;
        private readonly string _workerId;

        public ObservableCollection<NoteViewModel> Notes { get; }

        private string _newNoteText;
        public string NewNoteText
        {
            get => _newNoteText;
            set => this.RaiseAndSetIfChanged(ref _newNoteText, value);
        }

        // Commands
        public ReactiveCommand<Unit, Unit> AddNoteCommand { get; }
        public ReactiveCommand<NoteViewModel, Unit> EditNoteCommand { get; }
        public ReactiveCommand<NoteViewModel, Unit> DeleteNoteCommand { get; }

        public NotesTabViewModel(string workerId, INotesRepository repository)
        {
            _workerId = workerId;
            _notesRepository = repository;
            _newNoteText = "";

            // Load notes
            Notes = new ObservableCollection<NoteViewModel>();
            var notes = repository.GetNotes(int.Parse(workerId));
            foreach (var note in notes.OrderByDescending(n => n.CreatedDate))
            {
                Notes.Add(new NoteViewModel(note));
            }

            // Commands
            AddNoteCommand = ReactiveCommand.Create(OnAddNote);
            EditNoteCommand = ReactiveCommand.Create<NoteViewModel>(OnEditNote);
            DeleteNoteCommand = ReactiveCommand.Create<NoteViewModel>(OnDeleteNote);
        }

        private void OnAddNote()
        {
            if (string.IsNullOrWhiteSpace(NewNoteText)) return;

            var note = new WorkerNote
            {
                WorkerId = int.Parse(_workerId),
                Text = NewNoteText,
                Category = NoteCategory.Other,
                CreatedDate = DateTime.Now
            };

            _notesRepository.AddNote(note);
            Notes.Insert(0, new NoteViewModel(note));
            NewNoteText = "";
        }

        private void OnEditNote(NoteViewModel note) { /* TODO */ }

        private void OnDeleteNote(NoteViewModel note)
        {
            _notesRepository.DeleteNote(note.NoteId);
            Notes.Remove(note);
        }
    }

    public class NoteViewModel : ViewModelBase
    {
        public int NoteId { get; }

        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public DateTime CreatedDate { get; }
        public DateTime? ModifiedDate { get; set; }

        private string _category;
        public string Category
        {
            get => _category;
            set => this.RaiseAndSetIfChanged(ref _category, value);
        }

        public string CreatedDateText => CreatedDate.ToString("dd/MM/yyyy HH:mm");

        public NoteViewModel(WorkerNote note)
        {
            NoteId = note.Id;
            _text = note.Text;
            CreatedDate = note.CreatedDate;
            ModifiedDate = note.ModifiedDate;
            _category = note.Category.ToString();
        }
    }
}
```

---

## 🎨 VIEWS À CRÉER

### Structure des Fichiers

```
/src/RingGeneral.UI/Views/Profile/
├── ProfileView.axaml (Shell avec Header + TabControl)
├── ProfileView.axaml.cs
├── Tabs/
    ├── AttributesTabView.axaml
    ├── AttributesTabView.axaml.cs
    ├── ContractsTabView.axaml
    ├── ContractsTabView.axaml.cs
    ├── GimmickTabView.axaml
    ├── GimmickTabView.axaml.cs
    ├── RelationsTabView.axaml
    ├── RelationsTabView.axaml.cs
    ├── HistoryTabView.axaml
    ├── HistoryTabView.axaml.cs
    ├── NotesTabView.axaml
    └── NotesTabView.axaml.cs
```

### ProfileView (Shell)

**Fichier** : `/src/RingGeneral.UI/Views/Profile/ProfileView.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:RingGeneral.UI.ViewModels.Profile"
             x:Class="RingGeneral.UI.Views.Profile.ProfileView"
             x:DataType="vm:ProfileViewModel">

    <Grid RowDefinitions="Auto,*">
        <!-- HEADER: Photo + Nom + Actions -->
        <Border Grid.Row="0" Classes="panel" Padding="16" Margin="0,0,0,8">
            <Grid ColumnDefinitions="Auto,*,Auto">
                <!-- Photo -->
                <Border Grid.Column="0" Width="80" Height="80"
                        CornerRadius="40" ClipToBounds="True" Margin="0,0,16,0">
                    <Image Source="{Binding PhotoPath}"
                           Stretch="UniformToFill"/>
                </Border>

                <!-- Infos -->
                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                    <TextBlock Classes="h2" Text="{Binding FullName}"/>
                    <TextBlock Classes="body muted" Text="{Binding Role}"/>
                    <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,4,0,0">
                        <TextBlock Classes="caption" Text="{Binding ProfileType}"/>
                        <TextBlock Classes="caption" Text="•"/>
                        <TextBlock Classes="caption" Text="{Binding ContractStatus}"/>
                    </StackPanel>
                </StackPanel>

                <!-- Actions -->
                <StackPanel Grid.Column="2" Spacing="8">
                    <Button Classes="secondary" Content="✏ Éditer"
                            Command="{Binding EditCommand}"/>
                    <Button Classes="danger" Content="🚫 Libérer"
                            Command="{Binding ReleaseCommand}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- TABS -->
        <TabControl Grid.Row="1" SelectedIndex="{Binding SelectedTabIndex}">
            <TabItem Header="📊 ATTRIBUTS">
                <ScrollViewer>
                    <ContentControl Content="{Binding AttributesTab}"/>
                </ScrollViewer>
            </TabItem>

            <TabItem Header="📝 CONTRATS">
                <ScrollViewer>
                    <ContentControl Content="{Binding ContractsTab}"/>
                </ScrollViewer>
            </TabItem>

            <TabItem Header="🎭 GIMMICK/PUSH">
                <ScrollViewer>
                    <ContentControl Content="{Binding GimmickTab}"/>
                </ScrollViewer>
            </TabItem>

            <TabItem Header="👥 RELATIONS">
                <ScrollViewer>
                    <ContentControl Content="{Binding RelationsTab}"/>
                </ScrollViewer>
            </TabItem>

            <TabItem Header="📖 HISTORIQUE">
                <ScrollViewer>
                    <ContentControl Content="{Binding HistoryTab}"/>
                </ScrollViewer>
            </TabItem>

            <TabItem Header="📌 NOTES">
                <ScrollViewer>
                    <ContentControl Content="{Binding NotesTab}"/>
                </ScrollViewer>
            </TabItem>
        </TabControl>
    </Grid>
</UserControl>
```

### AttributesTabView

**Fichier** : `/src/RingGeneral.UI/Views/Profile/Tabs/AttributesTabView.axaml`

Voir `SPRINT_2_DESIGN.md` pour le layout complet avec fiche personnage.

### RelationsTabView

**Fichier** : `/src/RingGeneral.UI/Views/Profile/Tabs/RelationsTabView.axaml`

Voir `SPRINT_2_DESIGN.md` pour le layout complet avec relations et factions.

---

## 🗓️ PLAN D'EXÉCUTION (4-5 JOURS)

### JOUR 1 : Base de Données + Models

**Agent** : Systems Architect

#### Matin (4h)
- [ ] Créer migration SQL avec 6 nouvelles tables
- [ ] Ajouter colonnes à Workers (CurrentGimmick, Alignment, PushLevel, etc.)
- [ ] Tester la migration sur copie de la DB

#### Après-midi (4h)
- [ ] Créer 4 Models (WorkerRelation, Faction, FactionMember, WorkerNote)
- [ ] Mettre à jour Worker.cs avec navigation properties
- [ ] Tests unitaires des Models

**Livrables Jour 1** :
- ✅ Migration SQL complète
- ✅ 4 Models créés
- ✅ Tests passants

---

### JOUR 2 : Repositories + ViewModels (Part 1)

**Agent** : Systems Architect

#### Matin (4h)
- [ ] Créer IRelationsRepository + implémentation
- [ ] Créer INotesRepository + implémentation
- [ ] Enregistrer dans DI (App.axaml.cs)
- [ ] Tests repositories

#### Après-midi (4h)
- [ ] Créer ProfileViewModel (shell)
- [ ] Créer AttributesTabViewModel
- [ ] Créer ContractsTabViewModel

**Livrables Jour 2** :
- ✅ 2 Repositories fonctionnels
- ✅ 3 ViewModels créés
- ✅ DI configuré

---

### JOUR 3 : ViewModels (Part 2)

**Agent** : Systems Architect

#### Matin (4h)
- [ ] Créer GimmickTabViewModel
- [ ] Créer RelationsTabViewModel (avec WorkerRelationViewModel + FactionViewModel)

#### Après-midi (4h)
- [ ] Créer HistoryTabViewModel
- [ ] Créer NotesTabViewModel
- [ ] Tests de binding

**Livrables Jour 3** :
- ✅ 4 ViewModels restants créés
- ✅ 7 ViewModels au total
- ✅ Tests de binding

---

### JOUR 4 : Views (Part 1)

**Agent** : UI Specialist

#### Matin (4h)
- [ ] Créer ProfileView.axaml (Shell avec Header + TabControl)
- [ ] Créer AttributesTabView.axaml (avec fiche personnage)
- [ ] Créer ContractsTabView.axaml

#### Après-midi (4h)
- [ ] Créer GimmickTabView.axaml
- [ ] Créer RelationsTabView.axaml (relations + factions)

**Livrables Jour 4** :
- ✅ 5 Views créées (Shell + 4 tabs)
- ✅ Layout complet pour 4 premiers tabs

---

### JOUR 5 : Views (Part 2) + Integration

**Agent** : UI Specialist + Systems Architect

#### Matin (4h)
- [ ] Créer HistoryTabView.axaml
- [ ] Créer NotesTabView.axaml
- [ ] Enregistrer DataTemplates dans MainWindow.axaml

#### Après-midi (4h)
- [ ] Tests d'intégration complets
- [ ] Navigation depuis RosterView → ProfileView
- [ ] Tests avec données réelles
- [ ] Corrections de bugs

**Livrables Jour 5** :
- ✅ 7 Views complètes (Shell + 6 tabs)
- ✅ Navigation fonctionnelle
- ✅ Tests validés
- ✅ Sprint 2 TERMINÉ

---

## 📊 RÉCAPITULATIF DES LIVRABLES

### Base de Données (Jour 1)
- ✅ 6 nouvelles tables
- ✅ Colonnes ajoutées à Workers
- ✅ Migration SQL testée

### Models (Jour 1)
- ✅ WorkerRelation.cs
- ✅ Faction.cs + FactionMember.cs
- ✅ WorkerNote.cs
- ✅ ContractHistory.cs (si pas déjà existant)

### Repositories (Jour 2)
- ✅ IRelationsRepository + implémentation
- ✅ INotesRepository + implémentation
- ✅ DI configuré

### ViewModels (Jours 2-3)
1. ✅ ProfileViewModel (shell)
2. ✅ AttributesTabViewModel
3. ✅ ContractsTabViewModel
4. ✅ GimmickTabViewModel
5. ✅ RelationsTabViewModel
   - ✅ WorkerRelationViewModel
   - ✅ FactionViewModel
6. ✅ HistoryTabViewModel
7. ✅ NotesTabViewModel

**Total** : 7 ViewModels principaux + 2 ViewModels imbriqués = **9 ViewModels**

### Views (Jours 4-5)
1. ✅ ProfileView.axaml (Shell)
2. ✅ AttributesTabView.axaml
3. ✅ ContractsTabView.axaml
4. ✅ GimmickTabView.axaml
5. ✅ RelationsTabView.axaml
6. ✅ HistoryTabView.axaml
7. ✅ NotesTabView.axaml

**Total** : 7 Views (1 shell + 6 tabs)

### Tests
- ✅ Tests unitaires Models
- ✅ Tests unitaires Repositories
- ✅ Tests de binding ViewModels
- ✅ Tests d'intégration Navigation

---

## ✅ CRITÈRES DE VALIDATION

### Critères Techniques

- [ ] 6 tables créées et migration réussie
- [ ] 4 nouveaux Models créés
- [ ] 2 nouveaux Repositories fonctionnels
- [ ] 9 ViewModels créés et fonctionnels
- [ ] 7 Views créées (1 shell + 6 tabs)
- [ ] DataTemplates enregistrés
- [ ] Navigation ProfileView fonctionnelle
- [ ] Tous les tests passent
- [ ] Compilation réussie sans warnings

### Critères Fonctionnels

- [ ] Utilisateur peut voir les 6 onglets d'un profil
- [ ] Tab Attributs affiche fiche personnage + attributs avec AttributeBar
- [ ] Tab Contrats affiche contrat actuel + historique
- [ ] Tab Gimmick permet d'éditer gimmick, alignment, push
- [ ] Tab Relations affiche relations 1-à-1 et factions
- [ ] Tab Historique affiche bio + matchs + titres
- [ ] Tab Notes permet d'ajouter/éditer/supprimer des notes
- [ ] Navigation depuis RosterView fonctionne
- [ ] Support Worker/Staff/Trainee (détection de type)

### Critères Qualité

- [ ] Code respecte MVVM
- [ ] Namespaces corrects
- [ ] Pas de code dupliqué
- [ ] UI responsive et fluide
- [ ] Thème cohérent avec RingGeneralTheme
- [ ] Tooltips sur tous les éléments importants

---

## 📁 FICHIERS IMPACTÉS

### Nouveaux Fichiers (37 fichiers)

**Base de Données** (1) :
1. `/src/RingGeneral.Data/Migrations/Migration_Sprint2_ProfileView.sql`

**Models** (4) :
2. `/src/RingGeneral.Core/Models/Relations/WorkerRelation.cs`
3. `/src/RingGeneral.Core/Models/Relations/Faction.cs`
4. `/src/RingGeneral.Core/Models/Relations/FactionMember.cs`
5. `/src/RingGeneral.Core/Models/WorkerNote.cs`

**Repositories** (4) :
6. `/src/RingGeneral.Data/Repositories/Interfaces/IRelationsRepository.cs`
7. `/src/RingGeneral.Data/Repositories/RelationsRepository.cs`
8. `/src/RingGeneral.Data/Repositories/Interfaces/INotesRepository.cs`
9. `/src/RingGeneral.Data/Repositories/NotesRepository.cs`

**ViewModels** (9) :
10. `/src/RingGeneral.UI/ViewModels/Profile/ProfileViewModel.cs` (refonte si existe)
11. `/src/RingGeneral.UI/ViewModels/Profile/AttributesTabViewModel.cs`
12. `/src/RingGeneral.UI/ViewModels/Profile/ContractsTabViewModel.cs`
13. `/src/RingGeneral.UI/ViewModels/Profile/GimmickTabViewModel.cs`
14. `/src/RingGeneral.UI/ViewModels/Profile/RelationsTabViewModel.cs`
15. `/src/RingGeneral.UI/ViewModels/Profile/WorkerRelationViewModel.cs`
16. `/src/RingGeneral.UI/ViewModels/Profile/FactionViewModel.cs`
17. `/src/RingGeneral.UI/ViewModels/Profile/HistoryTabViewModel.cs`
18. `/src/RingGeneral.UI/ViewModels/Profile/NotesTabViewModel.cs`

**Views** (14) :
19. `/src/RingGeneral.UI/Views/Profile/ProfileView.axaml`
20. `/src/RingGeneral.UI/Views/Profile/ProfileView.axaml.cs`
21. `/src/RingGeneral.UI/Views/Profile/Tabs/AttributesTabView.axaml`
22. `/src/RingGeneral.UI/Views/Profile/Tabs/AttributesTabView.axaml.cs`
23. `/src/RingGeneral.UI/Views/Profile/Tabs/ContractsTabView.axaml`
24. `/src/RingGeneral.UI/Views/Profile/Tabs/ContractsTabView.axaml.cs`
25. `/src/RingGeneral.UI/Views/Profile/Tabs/GimmickTabView.axaml`
26. `/src/RingGeneral.UI/Views/Profile/Tabs/GimmickTabView.axaml.cs`
27. `/src/RingGeneral.UI/Views/Profile/Tabs/RelationsTabView.axaml`
28. `/src/RingGeneral.UI/Views/Profile/Tabs/RelationsTabView.axaml.cs`
29. `/src/RingGeneral.UI/Views/Profile/Tabs/HistoryTabView.axaml`
30. `/src/RingGeneral.UI/Views/Profile/Tabs/HistoryTabView.axaml.cs`
31. `/src/RingGeneral.UI/Views/Profile/Tabs/NotesTabView.axaml`
32. `/src/RingGeneral.UI/Views/Profile/Tabs/NotesTabView.axaml.cs`

**Tests** (4) :
33. `/tests/RingGeneral.Tests/Repositories/RelationsRepositoryTests.cs`
34. `/tests/RingGeneral.Tests/Repositories/NotesRepositoryTests.cs`
35. `/tests/RingGeneral.Tests/ViewModels/ProfileViewModelTests.cs`
36. `/tests/RingGeneral.Tests/Integration/ProfileNavigationTests.cs`

**Documentation** (1) :
37. `/docs/SPRINT_2_IMPLEMENTATION_REPORT.md`

### Fichiers Modifiés (3 fichiers)

1. `/src/RingGeneral.Core/Models/Worker.cs` - Ajout navigation properties
2. `/src/RingGeneral.UI/App.axaml.cs` - Enregistrement DI
3. `/src/RingGeneral.UI/Views/MainWindow.axaml` - DataTemplates

**Total** : 37 nouveaux + 3 modifiés = **40 fichiers**

---

## ⚠️ RISQUES ET MITIGATION

### Risque 1 : Complexité des Relations

**Impact** : Relations bidirectionnelles difficiles à gérer

**Mitigation** :
- Unique constraint sur (WorkerId1, WorkerId2)
- Validation côté application
- Tests exhaustifs des edge cases

### Risque 2 : Performance des Factions

**Impact** : Chargement lent si beaucoup de factions

**Mitigation** :
- Lazy loading des membres
- Index sur FactionId et WorkerId
- Cache si nécessaire

### Risque 3 : UI Surchargée

**Impact** : 6 tabs = beaucoup de contenu

**Mitigation** :
- Lazy loading des tabs (charger au clic)
- Expanders collapsibles
- Pagination si listes > 50 items

---

## 🔗 DÉPENDANCES AVEC AUTRES SPRINTS

### Après Sprint 2

**Sprint 3** : Résultats de Simulation
- Pourra afficher historique des matchs dans tab Historique

**Sprint 4** : Inbox & Actualités
- Pourra générer messages pour fins de contrat (tab Contrats)

**Sprint 6** : Boucle de Jeu
- ProfileView utilisé après chaque simulation pour voir impacts

---

## ✅ CHECKLIST DE DÉMARRAGE

- [ ] Sprint 1 (Composants UI) terminé ✅
- [ ] AttributeBar component fonctionnel ✅
- [ ] RingGeneralTheme.axaml disponible ✅
- [ ] Backup de la base de données
- [ ] Lire SPRINT_2_DESIGN.md pour mockups UI
- [ ] Valider ce plan avec l'équipe
- [ ] Assigner Systems Architect (Jours 1-3)
- [ ] Assigner UI Specialist (Jours 4-5)

---

**Version** : 1.0
**Auteur** : Chef de Projet DevOps (Claude)
**Date de création** : 7 janvier 2026
**Statut** : ✅ PRÊT POUR VALIDATION

---

**Prochaine Action** : Attendre validation avant de démarrer Jour 1 (Base de Données + Models).
