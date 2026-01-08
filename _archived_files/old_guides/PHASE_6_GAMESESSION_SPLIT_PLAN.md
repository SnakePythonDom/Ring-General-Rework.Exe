# 📋 Phase 6 - Plan de Split GameSessionViewModel

**Date** : 2026-01-08
**Status** : 🔄 **PLANIFICATION**
**Fichier cible** : `GameSessionViewModel.cs`
**Taille actuelle** : 2,379 lignes
**Objectif** : ~800 lignes
**Réduction visée** : -1,579 lignes (-66%)

---

## 🎯 Objectifs

1. **Réduire la complexité** : Passer de God Object à architecture modulaire
2. **Améliorer la testabilité** : ViewModels plus petits = tests plus simples
3. **Faciliter la maintenance** : Responsabilités clairement séparées
4. **Respecter SRP** : Single Responsibility Principle

---

## 📊 Analyse des Responsabilités

### 1. **Booking & Segments** (~600 lignes)
**Propriétés** :
- `Segments` (ObservableCollection<SegmentViewModel>)
- `ValidationIssues` (ObservableCollection<BookingIssueViewModel>)
- `Resultats` (ObservableCollection<SegmentResultViewModel>)
- `SegmentTypes`, `SegmentTemplates`, `MatchTypes`
- `SegmentSelectionne` (SegmentViewModel?)

**Méthodes** :
- `AjouterSegment()`, `SupprimerSegment()`, `ModifierSegment()`
- `ValiderBooking()`, `SimulerShow()`
- Template management

**Proposition** : Extraire vers `ShowBookingViewModel`

---

### 2. **Workers & Participants** (~300 lignes)
**Propriétés** :
- `WorkersDisponibles` (ObservableCollection<ParticipantViewModel>)
- `NouveauSegmentParticipants`
- Worker selection logic

**Méthodes** :
- `ChargerWorkers()`, `FiltrerWorkers()`
- Participant assignment

**Proposition** : Extraire vers `ShowWorkersViewModel`

---

### 3. **Youth System** (~400 lignes)
**Propriétés** :
- `YouthStructures`, `YouthTrainees`, `YouthPrograms`
- `YouthStaffAssignments`
- `YouthBudgetNouveau`, `YouthCoachWorkerId`
- `YouthGenerationModes`, `YouthGenerationSelection`

**Méthodes** :
- Youth structure management
- Trainee progression
- Staff assignments

**Proposition** : Extraire vers `YouthManagementViewModel` (déjà existe `YouthViewModel` dans UI)

---

### 4. **Storylines** (~200 lignes)
**Propriétés** :
- `StorylinesDisponibles` (ObservableCollection<StorylineOptionViewModel>)
- `StorylinePhases`, `StorylineStatuts`
- `NouveauSegmentStorylineId`

**Méthodes** :
- Storyline selection
- Phase management

**Proposition** : Intégrer avec `StorylinesViewModel` existant ou créer `ShowStorylineViewModel`

---

### 5. **Calendar & Shows** (~150 lignes)
**Propriétés** :
- `ShowsAVenir` (ObservableCollection<ShowCalendarItemViewModel>)
- `NouveauShowNom`, `NouveauShowSemaine`, `NouveauShowDuree`
- `HistoriqueShow`

**Méthodes** :
- Show scheduling
- Calendar navigation

**Proposition** : Intégrer avec `CalendarViewModel` existant

---

### 6. **Finance & TV Deals** (~100 lignes)
**Propriétés** :
- `DealsTv` (ObservableCollection<TvDealViewModel>)
- `ReachMap`, `ContraintesDiffusion`
- `AudienceHistorique`

**Méthodes** :
- TV deal management
- Audience tracking

**Proposition** : Intégrer avec `FinanceViewModel` existant

---

### 7. **Titles & Championships** (~100 lignes)
**Propriétés** :
- `TitresDisponibles` (ObservableCollection<TitleOptionViewModel>)

**Méthodes** :
- Title assignment in segments

**Proposition** : Intégrer avec `TitlesViewModel` existant ou dans `ShowBookingViewModel`

---

### 8. **Inbox & Notifications** (~100 lignes)
**Propriétés** :
- `Inbox` (ObservableCollection<InboxItemViewModel>)

**Méthodes** :
- Notification management
- Inbox filtering

**Proposition** : Extraire vers `InboxViewModel`

---

### 9. **Help & Codex** (~150 lignes)
**Propriétés** :
- `AidePanel` (HelpPanelViewModel)
- `Codex` (CodexViewModel)
- `ImpactPages`, `Tooltips`
- `PourquoiNote`, `Conseils`

**Méthodes** :
- Help content display
- Tooltips management

**Proposition** : Garder dans GameSessionViewModel (support infrastructure)

---

### 10. **Table View** (~200 lignes)
**Propriétés** :
- `TableItems`, `TableItemsView`
- `TableConfiguration`, `TableColumns`
- `TableTypeFilters`, `TableStatusFilters`

**Méthodes** :
- Table sorting, filtering
- Column configuration

**Proposition** : Extraire vers `TableViewViewModel` (générique réutilisable)

---

### 11. **Global Search** (~100 lignes)
**Propriétés** :
- `RechercheGlobaleResultats` (ObservableCollection<GlobalSearchResultViewModel>)
- `OuvrirRechercheGlobaleCommand`, `FermerRechercheGlobaleCommand`

**Méthodes** :
- Global search indexing
- Search execution

**Proposition** : Extraire vers `GlobalSearchViewModel`

---

### 12. **Core Session** (~180 lignes restantes)
**Propriétés** :
- `_repository`, `_scoutingRepository`, etc.
- `_context` (ShowContext)
- Database path, initialization

**Méthodes** :
- Session initialization
- Database loading
- Coordination entre sous-ViewModels

**Proposition** : **Garder** dans GameSessionViewModel (coordinateur)

---

## 🛠️ Plan d'Extraction par Phases

### Phase 6.1 - Extractions Simples ✅ Priorité Haute
1. **GlobalSearchViewModel** (~100 lignes)
   - Peu de dépendances
   - Responsabilité claire

2. **InboxViewModel** (~100 lignes)
   - Indépendant du reste
   - Simple à extraire

3. **TableViewViewModel** (~200 lignes)
   - Générique, réutilisable
   - Pas de logique métier

**Gain** : -400 lignes (2,379 → 1,979)

---

### Phase 6.2 - ShowBookingViewModel ✅ Priorité Haute
**Extraction** : Booking & Segments (~600 lignes)

**Nouveau fichier** : `ShowBookingViewModel.cs`

**Responsabilités** :
- Gestion complète des segments
- Validation booking
- Templates et types
- Simulation show

**Dépendances** :
- GameRepository
- BookingValidator
- SegmentTypeCatalog

**Gain** : -600 lignes (1,979 → 1,379)

---

### Phase 6.3 - Intégrations avec ViewModels Existants ⚠️ Priorité Moyenne
1. **YouthViewModel** : Intégrer youth management (~400 lignes)
2. **StorylinesViewModel** : Intégrer storyline selection (~200 lignes)
3. **CalendarViewModel** : Intégrer show scheduling (~150 lignes)
4. **FinanceViewModel** : Intégrer TV deals (~100 lignes)

**Gain** : -850 lignes (1,379 → 529)

---

### Phase 6.4 - ShowWorkersViewModel ⚠️ Priorité Basse
**Extraction** : Workers & Participants (~300 lignes)

**Nouveau fichier** : `ShowWorkersViewModel.cs`

**Gain** : -300 lignes (529 → 229)

---

## 📐 Architecture Cible

```
GameSessionViewModel (Coordinateur ~800 lignes)
├── ShowBookingViewModel (Booking & Segments)
│   ├── Segments management
│   ├── Validation
│   └── Simulation
├── ShowWorkersViewModel (Participants)
│   ├── Worker selection
│   └── Assignment logic
├── GlobalSearchViewModel (Search)
├── InboxViewModel (Notifications)
├── TableViewViewModel (Generic tables)
└── Intégrations :
    ├── YouthViewModel (Youth system)
    ├── StorylinesViewModel (Storylines)
    ├── CalendarViewModel (Shows scheduling)
    ├── FinanceViewModel (TV Deals)
    └── TitlesViewModel (Championships)
```

---

## ⚙️ Stratégie de Migration

### 1. **Créer le ViewModel enfant**
```csharp
public sealed class ShowBookingViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly BookingValidator _validator;

    public ShowBookingViewModel(GameRepository repository)
    {
        _repository = repository;
        _validator = new BookingValidator();
        Segments = new ObservableCollection<SegmentViewModel>();
    }
}
```

### 2. **Déplacer les propriétés**
- Copier les ObservableCollections
- Copier les propriétés liées
- Copier les commandes ReactiveCommand

### 3. **Déplacer les méthodes**
- Méthodes privées → méthodes publiques/internal
- Ajuster les appels de dépendances

### 4. **Intégrer dans GameSessionViewModel**
```csharp
public sealed class GameSessionViewModel : ViewModelBase
{
    public ShowBookingViewModel Booking { get; }
    public GlobalSearchViewModel Search { get; }
    public InboxViewModel Inbox { get; }

    public GameSessionViewModel(...)
    {
        Booking = new ShowBookingViewModel(_repository);
        Search = new GlobalSearchViewModel();
        Inbox = new InboxViewModel(_repository);
    }
}
```

### 5. **Mettre à jour les bindings XAML**
```xml
<!-- Avant -->
<DataGrid ItemsSource="{Binding Segments}" />

<!-- Après -->
<DataGrid ItemsSource="{Binding Booking.Segments}" />
```

---

## ⚠️ Risques et Précautions

### Risques Identifiés
1. **Breaking Changes UI** : Bindings XAML à mettre à jour
2. **État Partagé** : Certaines propriétés partagées entre responsabilités
3. **Tests** : Risque de casser des tests existants
4. **Dépendances Circulaires** : ViewModels qui se référencent mutuellement

### Précautions
1. **Tests unitaires** : Créer des tests AVANT extraction
2. **Commits incrémentaux** : Un ViewModel extrait = un commit
3. **Backward compatibility** : Propriétés obsolètes temporaires
4. **Code review** : Valider chaque extraction

---

## 📅 Timeline Estimée

| Phase | Tâche | Lignes | Complexité | Durée estimée |
|-------|-------|--------|------------|---------------|
| 6.1 | GlobalSearchViewModel | -100 | ⭐ Facile | 1h |
| 6.1 | InboxViewModel | -100 | ⭐ Facile | 1h |
| 6.1 | TableViewViewModel | -200 | ⭐⭐ Moyen | 2h |
| 6.2 | ShowBookingViewModel | -600 | ⭐⭐⭐ Difficile | 4h |
| 6.3 | Intégrations existantes | -850 | ⭐⭐⭐⭐ Complexe | 6h |
| 6.4 | ShowWorkersViewModel | -300 | ⭐⭐ Moyen | 2h |
| - | Tests & Validation | - | - | 2h |
| **TOTAL** | | **-2,150** | | **~18h** |

---

## ✅ Critères de Succès

1. **Taille** : GameSessionViewModel < 800 lignes
2. **Tests** : 100% des tests passent
3. **Compilation** : Aucune erreur
4. **UI** : Toutes les fonctionnalités opérationnelles
5. **Performance** : Pas de régression
6. **Documentation** : Chaque ViewModel documenté

---

## 📝 Notes

- **Phase 6 est VOLUMINEUSE** : Nécessite plusieurs sessions de travail
- **Approche incrémentale** : Commencer par les extractions simples (Phase 6.1)
- **Validation continue** : Tests après chaque extraction
- **Pas de Big Bang** : Éviter de tout refactor en une seule fois

---

## 🚀 Prochaines Étapes

1. ✅ Créer ce document de planification
2. 🔄 Corriger les tests échouants (priorité immédiate)
3. 🔲 Commencer Phase 6.1 (GlobalSearch + Inbox + TableView)
4. 🔲 Phase 6.2 (ShowBookingViewModel)
5. 🔲 Phases 6.3 et 6.4 selon feedback

---

**Status** : Planification complète - Prêt pour exécution par phases

