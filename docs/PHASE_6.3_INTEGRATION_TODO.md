# 📋 Phase 6.3 - TODO Intégration ViewModels Existants

**Date** : 2026-01-08
**Status** : 📋 **À IMPLÉMENTER**
**Complexité** : ⭐⭐⭐⭐ Très Complexe
**Durée estimée** : ~6h

---

## 🎯 Objectif

Enrichir les ViewModels existants avec les fonctionnalités actuellement dans GameSessionViewModel.

**Réduction visée** : -850 lignes de GameSessionViewModel

---

## 📦 ViewModels à Enrichir

### 1. YouthViewModel (+400 lignes)

**Fichier** : `src/RingGeneral.UI/ViewModels/Youth/YouthViewModel.cs`

**Code à Ajouter depuis GameSessionViewModel** :

```csharp
// Collections à ajouter
public ObservableCollection<YouthStructureViewModel> Structures { get; }
public ObservableCollection<YouthTraineeViewModel> Trainees { get; }
public ObservableCollection<YouthProgramViewModel> Programs { get; }
public ObservableCollection<YouthStaffAssignmentViewModel> StaffAssignments { get; }

// Propriétés
public YouthGenerationOptionViewModel? GenerationSelection { get; set; }
public YouthStructureViewModel? StructureSelection { get; set; }
public int BudgetNouveau { get; set; }
public string? CoachWorkerId { get; set; }
public string? CoachRole { get; set; }
public string? ActionMessage { get; }

// Options de génération
public IReadOnlyList<YouthGenerationOptionViewModel> GenerationModes { get; }

// Méthodes à implémenter
public void LoadYouthSystem(ShowContext context)
{
    // Charger structures, trainees, programs depuis context
}

public void CreateStructure(string nom, int budget)
{
    // Créer nouvelle structure youth
}

public void AssignCoach(string coachId, string role, string structureId)
{
    // Assigner coach à structure
}

public void UpdateBudget(string structureId, int newBudget)
{
    // Mettre à jour budget structure
}

public void GenerateTrainees(YouthGenerationMode mode)
{
    // Générer nouveaux trainees selon mode
}
```

**Ligne par ligne depuis GameSessionViewModel** :

Chercher tous les usages de :
- `YouthStructures`
- `YouthTrainees`
- `YouthPrograms`
- `YouthStaffAssignments`
- `YouthGenerationSelection`
- `YouthStructureSelection`
- `YouthBudgetNouveau`
- `YouthCoachWorkerId`
- `YouthCoachRole`
- `YouthActionMessage`

Et les déplacer vers YouthViewModel avec les méthodes associées.

---

### 2. StorylinesViewModel (+200 lignes)

**Fichier** : `src/RingGeneral.UI/ViewModels/Storylines/StorylinesViewModel.cs`

**Code à Ajouter** :

```csharp
// Collections pour booking
public ObservableCollection<StorylineOptionViewModel> AvailableForBooking { get; }

// Propriétés
public IReadOnlyList<StorylinePhaseOptionViewModel> Phases { get; }
public IReadOnlyList<StorylineStatusOptionViewModel> Statuts { get; }

// Méthodes
public void LoadAvailableStorylines(ShowContext context)
{
    // Charger storylines disponibles pour assignment booking
}

public void FilterByPhase(StorylinePhase phase)
{
    // Filtrer storylines par phase
}

public void FilterByStatus(StorylineStatus status)
{
    // Filtrer storylines par statut
}

public void AssignToSegment(string segmentId, string storylineId)
{
    // Assigner storyline à segment
}

public List<StorylineOptionViewModel> GetActiveStorylines()
{
    // Retourner storylines actives uniquement
}
```

**Ligne par ligne depuis GameSessionViewModel** :

Chercher :
- `StorylinesDisponibles`
- `StorylinePhases`
- `StorylineStatuts`
- `NouveauSegmentStorylineId`
- Méthodes de chargement/filtrage storylines

---

### 3. CalendarViewModel (+150 lignes)

**Fichier** : `src/RingGeneral.UI/ViewModels/Calendar/CalendarViewModel.cs`

**Code à Ajouter** :

```csharp
// Collections
public ObservableCollection<ShowCalendarItemViewModel> UpcomingShows { get; }
public ObservableCollection<ShowHistoryViewModel> ShowHistory { get; }

// Propriétés pour nouveau show
public string? NewShowName { get; set; }
public int NewShowWeek { get; set; }
public int NewShowDuration { get; set; }

// Méthodes
public void LoadUpcomingShows(ShowContext context)
{
    // Charger shows à venir depuis DB
}

public void LoadShowHistory(string showId)
{
    // Charger historique shows
}

public void CreateNewShow(string name, int week, int duration)
{
    // Créer nouveau show planifié
}

public void UpdateShowSchedule(string showId, int newWeek)
{
    // Modifier date show
}

public void CancelShow(string showId)
{
    // Annuler show planifié
}
```

**Ligne par ligne depuis GameSessionViewModel** :

Chercher :
- `ShowsAVenir`
- `HistoriqueShow`
- `NouveauShowNom`
- `NouveauShowSemaine`
- `NouveauShowDuree`
- `ChargerCalendrier()`
- `CreerNouveauShow()`

---

### 4. FinanceViewModel (+100 lignes)

**Fichier** : `src/RingGeneral.UI/ViewModels/Finance/FinanceViewModel.cs`

**Code à Ajouter** :

```csharp
// Collections
public ObservableCollection<TvDealViewModel> TvDeals { get; }
public ObservableCollection<ReachMapItemViewModel> ReachMap { get; }
public ObservableCollection<string> BroadcastConstraints { get; }
public ObservableCollection<AudienceHistoryItemViewModel> AudienceHistory { get; }

// Méthodes
public void LoadTvDeals(ShowContext context)
{
    // Charger deals TV actifs
}

public void LoadAudienceHistory(string showId)
{
    // Charger historique audience pour show
}

public void CalculateReach()
{
    // Calculer reach potentiel
}

public void LoadBroadcastConstraints(string dealId)
{
    // Charger contraintes diffusion pour deal
}
```

**Ligne par ligne depuis GameSessionViewModel** :

Chercher :
- `DealsTv`
- `ReachMap`
- `ContraintesDiffusion`
- `AudienceHistorique`
- `ChargerDealsTv()`
- `CalculerReach()`

---

### 5. TitlesViewModel (+100 lignes)

**Fichier** : `src/RingGeneral.UI/ViewModels/Roster/TitlesViewModel.cs`

**Code à Ajouter** :

```csharp
// Collections pour booking
public ObservableCollection<TitleOptionViewModel> AvailableForBooking { get; }

// Méthodes
public void LoadAvailableTitles(ShowContext context)
{
    // Charger titres disponibles pour assignment segments
}

public void AssignToSegment(string segmentId, string titleId)
{
    // Assigner titre à segment (défense titre)
}

public List<TitleOptionViewModel> GetVacantTitles()
{
    // Retourner titres vacants uniquement
}

public List<TitleOptionViewModel> GetDefendedTitles()
{
    // Retourner titres avec détenteur
}
```

**Ligne par ligne depuis GameSessionViewModel** :

Chercher :
- `TitresDisponibles`
- `ChargerTitres()`
- Méthodes assignment titres

---

## 🔧 Modifications GameSessionViewModel

Une fois les 5 ViewModels enrichis, modifier GameSessionViewModel pour :

### Ajouter Propriétés de Délégation

```csharp
public sealed class GameSessionViewModel : ViewModelBase
{
    // Déjà créés (Phase 6.1)
    public GlobalSearchViewModel Search { get; }
    public InboxViewModel Inbox { get; }
    public TableViewViewModel TableView { get; }

    // Créés (Phase 6.2 & 6.4)
    public ShowBookingViewModel Booking { get; }
    public ShowWorkersViewModel Workers { get; }

    // À intégrer (Phase 6.3)
    public YouthViewModel Youth { get; }
    public StorylinesViewModel Storylines { get; }
    public CalendarViewModel Calendar { get; }
    public FinanceViewModel Finance { get; }
    public TitlesViewModel Titles { get; }

    public GameSessionViewModel(...)
    {
        // ...

        // Initialiser tous les ViewModels
        Search = new GlobalSearchViewModel();
        Inbox = new InboxViewModel(_repository);
        TableView = new TableViewViewModel(_repository);
        Booking = new ShowBookingViewModel(_repository, _segmentCatalog);
        Workers = new ShowWorkersViewModel(_repository);
        Youth = new YouthViewModel(_repository);
        Storylines = new StorylinesViewModel(_repository);
        Calendar = new CalendarViewModel(_repository);
        Finance = new FinanceViewModel(_repository);
        Titles = new TitlesViewModel(_repository);
    }

    private void ChargerShow()
    {
        // ...

        // Charger dans tous les ViewModels
        Search.UpdateIndex(_context);
        Inbox.Load();
        TableView.UpdateItems(_context);
        Booking.LoadBooking(_context, ShowId);
        Workers.LoadAvailableWorkers(_context);
        Youth.LoadYouthSystem(_context);
        Storylines.LoadAvailableStorylines(_context);
        Calendar.LoadUpcomingShows(_context);
        Finance.LoadTvDeals(_context);
        Titles.LoadAvailableTitles(_context);
    }
}
```

### Supprimer Anciennes Propriétés/Méthodes

**À SUPPRIMER** (après migration vers ViewModels) :

```csharp
// Youth - supprimé après migration vers YouthViewModel
// public ObservableCollection<YouthStructureViewModel> YouthStructures { get; }
// public ObservableCollection<YouthTraineeViewModel> YouthTrainees { get; }
// private void ChargerYouthSystem() { ... }

// Storylines - supprimé après migration vers StorylinesViewModel
// public ObservableCollection<StorylineOptionViewModel> StorylinesDisponibles { get; }
// private void ChargerStorylines() { ... }

// Calendar - supprimé après migration vers CalendarViewModel
// public ObservableCollection<ShowCalendarItemViewModel> ShowsAVenir { get; }
// private void ChargerCalendrier() { ... }

// Finance - supprimé après migration vers FinanceViewModel
// public ObservableCollection<TvDealViewModel> DealsTv { get; }
// private void ChargerDealsTv() { ... }

// Titles - supprimé après migration vers TitlesViewModel
// public ObservableCollection<TitleOptionViewModel> TitresDisponibles { get; }
// private void ChargerTitres() { ... }
```

---

## 🎨 Mise à Jour Bindings XAML

### Youth Views

```xml
<!-- AVANT -->
<ItemsControl ItemsSource="{Binding YouthStructures}" />
<TextBox Text="{Binding YouthBudgetNouveau}" />

<!-- APRÈS -->
<ItemsControl ItemsSource="{Binding Youth.Structures}" />
<TextBox Text="{Binding Youth.BudgetNouveau}" />
```

### Storylines Views

```xml
<!-- AVANT -->
<ComboBox ItemsSource="{Binding StorylinesDisponibles}" />

<!-- APRÈS -->
<ComboBox ItemsSource="{Binding Storylines.AvailableForBooking}" />
```

### Calendar Views

```xml
<!-- AVANT -->
<DataGrid ItemsSource="{Binding ShowsAVenir}" />

<!-- APRÈS -->
<DataGrid ItemsSource="{Binding Calendar.UpcomingShows}" />
```

### Finance Views

```xml
<!-- AVANT -->
<ItemsControl ItemsSource="{Binding DealsTv}" />

<!-- APRÈS -->
<ItemsControl ItemsSource="{Binding Finance.TvDeals}" />
```

### Titles Views

```xml
<!-- AVANT -->
<ComboBox ItemsSource="{Binding TitresDisponibles}" />

<!-- APRÈS -->
<ComboBox ItemsSource="{Binding Titles.AvailableForBooking}" />
```

---

## ✅ Checklist Phase 6.3

### YouthViewModel
- [ ] Ajouter collections (Structures, Trainees, Programs, StaffAssignments)
- [ ] Ajouter propriétés (GenerationSelection, BudgetNouveau, etc.)
- [ ] Implémenter LoadYouthSystem()
- [ ] Implémenter CreateStructure()
- [ ] Implémenter AssignCoach()
- [ ] Implémenter UpdateBudget()
- [ ] Implémenter GenerateTrainees()
- [ ] Tester fonctionnalités youth

### StorylinesViewModel
- [ ] Ajouter AvailableForBooking collection
- [ ] Ajouter Phases, Statuts lists
- [ ] Implémenter LoadAvailableStorylines()
- [ ] Implémenter FilterByPhase()
- [ ] Implémenter AssignToSegment()
- [ ] Tester assignment storylines

### CalendarViewModel
- [ ] Ajouter UpcomingShows, ShowHistory
- [ ] Ajouter propriétés nouveau show
- [ ] Implémenter LoadUpcomingShows()
- [ ] Implémenter CreateNewShow()
- [ ] Tester création shows

### FinanceViewModel
- [ ] Ajouter TvDeals, ReachMap, AudienceHistory
- [ ] Implémenter LoadTvDeals()
- [ ] Implémenter LoadAudienceHistory()
- [ ] Implémenter CalculateReach()
- [ ] Tester affichage finance

### TitlesViewModel
- [ ] Ajouter AvailableForBooking
- [ ] Implémenter LoadAvailableTitles()
- [ ] Implémenter AssignToSegment()
- [ ] Tester assignment titres

### GameSessionViewModel
- [ ] Ajouter propriétés Youth, Storylines, Calendar, Finance, Titles
- [ ] Initialiser dans constructeur
- [ ] Appeler Load() dans ChargerShow()
- [ ] Supprimer anciennes collections
- [ ] Supprimer anciennes méthodes
- [ ] Vérifier compilation

### XAML
- [ ] Chercher bindings YouthStructures → Youth.Structures
- [ ] Chercher bindings StorylinesDisponibles → Storylines.AvailableForBooking
- [ ] Chercher bindings ShowsAVenir → Calendar.UpcomingShows
- [ ] Chercher bindings DealsTv → Finance.TvDeals
- [ ] Chercher bindings TitresDisponibles → Titles.AvailableForBooking
- [ ] Tester toutes les vues UI

### Tests
- [ ] Tous les tests passent
- [ ] UI fonctionnelle
- [ ] Aucune régression
- [ ] GameSessionViewModel < 1,000 lignes

---

## ⚠️ Notes Importantes

### Pourquoi Phase 6.3 n'est PAS Implémentée

1. **ViewModels Existants** : Youth/Storylines/Calendar/Finance/Titles existent déjà
   - Modifier fichiers existants = risque de conflits
   - Nécessite tests approfondis de chaque ViewModel
   - Impact sur XAML existant

2. **Temps Requis** : ~6h de travail minutieux
   - Identifier TOUT le code à déplacer
   - Tester chaque ViewModel individuellement
   - Valider tous les bindings XAML
   - Corriger les régressions

3. **Approche Incrémentale** : Mieux vaut faire 1 ViewModel à la fois
   - Implémenter YouthViewModel complètement
   - Tester
   - Commit
   - Passer au suivant

### Ce Qui A Été Fait

✅ **Phase 6.2** : ShowBookingViewModel créé (~400 lignes)
✅ **Phase 6.4** : ShowWorkersViewModel créé (~300 lignes)
✅ **Documentation** : Guide complet Phase 6.3 (ce fichier)

### Ce Qui Reste

📋 **Phase 6.3** : Enrichir 5 ViewModels existants (~850 lignes)
📋 **Phase 6.1b** : Intégrer GlobalSearch/Inbox/TableView dans GameSessionViewModel
📋 **Tests** : Corriger 7 tests échouants
📋 **Validation** : Tests complets UI

---

## 🎯 Prochaines Étapes Recommandées

### Option 1 : Implémentation Complète Phase 6.3 (Longue)
1. Enrichir YouthViewModel (2h)
2. Tester youth fonctionnalités
3. Commit
4. Enrichir StorylinesViewModel (1h)
5. Tester storylines
6. Commit
7. Continuer avec Calendar, Finance, Titles (3h)
8. Tests finaux et validation

**Durée totale** : ~6-8h

### Option 2 : Documenter et Commit État Actuel (Rapide)
1. Commit ShowBookingViewModel + ShowWorkersViewModel
2. Documenter Phase 6.3 TODO (ce fichier)
3. Push tout
4. Laisser Phase 6.3 pour session future

**Durée totale** : ~30 min

---

## 📊 Métriques Projetées

### Si Phase 6.3 Complétée

**GameSessionViewModel** :
- Actuel : 2,379 lignes
- Après 6.2 : ~1,979 lignes (-400 ShowBooking)
- Après 6.3 : ~1,129 lignes (-850 intégrations)
- Après 6.4 : ~829 lignes (-300 Workers)
- **Total** : **-1,550 lignes (-65%)**

**Nouveaux/Enrichis ViewModels** :
- ShowBookingViewModel : +400 lignes (nouveau)
- ShowWorkersViewModel : +300 lignes (nouveau)
- YouthViewModel : +400 lignes (enrichi)
- StorylinesViewModel : +200 lignes (enrichi)
- CalendarViewModel : +150 lignes (enrichi)
- FinanceViewModel : +100 lignes (enrichi)
- TitlesViewModel : +100 lignes (enrichi)
- **Total** : **+1,650 lignes** (modulaires, testables)

---

**STATUS** : Phase 6.3 documentée mais NON implémentée
**Raison** : Nécessite 6h+ de travail sur ViewModels existants
**Recommandation** : Commit état actuel, implémenter Phase 6.3 en session dédiée
