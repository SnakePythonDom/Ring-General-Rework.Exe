# 📘 Phases 6.3-6.4 - Guide d'Implémentation

**Date** : 2026-01-08
**Complexité** : Phase 6.3 ⭐⭐⭐⭐ Complexe | Phase 6.4 ⭐⭐ Moyen
**Durée estimée** : Phase 6.3: ~6h | Phase 6.4: ~2h
**Objectif** : Intégrations + Extraction Workers (-1,150 lignes totales)

---

## 🎯 PHASE 6.3 - Intégrations ViewModels Existants

### Objectif

Intégrer la logique actuellement dans GameSessionViewModel avec les ViewModels existants :
- YouthViewModel
- StorylinesViewModel
- CalendarViewModel
- FinanceViewModel
- TitlesViewModel

**Réduction visée** : -850 lignes

---

### 1. Youth System → YouthViewModel

**Code à Déplacer depuis GameSessionViewModel** (~400 lignes):

```csharp
// Collections
public ObservableCollection<YouthStructureViewModel> YouthStructures { get; }
public ObservableCollection<YouthTraineeViewModel> YouthTrainees { get; }
public ObservableCollection<YouthProgramViewModel> YouthPrograms { get; }
public ObservableCollection<YouthStaffAssignmentViewModel> YouthStaffAssignments { get; }

// Propriétés
public YouthGenerationOptionViewModel? YouthGenerationSelection { get; set; }
public YouthStructureViewModel? YouthStructureSelection { get; set; }
public int YouthBudgetNouveau { get; set; }
public string? YouthCoachWorkerId { get; set; }
public string? YouthCoachRole { get; set; }
public string? YouthActionMessage { get; }

// Méthodes (~300 lignes)
private void ChargerYouthSystem()
private void CreerYouthStructure()
private void AssignerYouthCoach()
private void GererYouthBudget()
// etc.
```

**Intégration dans YouthViewModel** :

```csharp
// src/RingGeneral.UI/ViewModels/Youth/YouthViewModel.cs
public sealed class YouthViewModel : ViewModelBase
{
    private readonly GameRepository _repository;

    // Ajouter toutes les collections et propriétés
    public ObservableCollection<YouthStructureViewModel> Structures { get; }
    public ObservableCollection<YouthTraineeViewModel> Trainees { get; }

    // Ajouter méthodes
    public void LoadYouthSystem(ShowContext context)
    {
        // Logique de chargement
    }

    public void CreateStructure()
    {
        // Logique création
    }
}
```

**Bindings XAML** :
```xml
<!-- AVANT -->
<ItemsControl ItemsSource="{Binding YouthStructures}" />

<!-- APRÈS -->
<ItemsControl ItemsSource="{Binding Youth.Structures}" />
```

**Intégration GameSessionViewModel** :
```csharp
public YouthViewModel Youth { get; }

public GameSessionViewModel(...)
{
    Youth = new YouthViewModel(_repository);
    // ...
}

private void ChargerShow()
{
    Youth.LoadYouthSystem(_context);
}
```

---

### 2. Storylines → StorylinesViewModel

**Code à Déplacer** (~200 lignes):

```csharp
// Collections
public ObservableCollection<StorylineOptionViewModel> StorylinesDisponibles { get; }
public IReadOnlyList<StorylinePhaseOptionViewModel> StorylinePhases { get; }
public IReadOnlyList<StorylineStatusOptionViewModel> StorylineStatuts { get; }

// Propriétés
public string? NouveauSegmentStorylineId { get; set; }

// Méthodes (~150 lignes)
private void ChargerStorylines()
private void FiltrerStorylines()
private void AssignerStorylineSegment()
```

**Intégration StorylinesViewModel** :

```csharp
// Ajouter à StorylinesViewModel existant
public void LoadAvailableStorylines(ShowContext context)
{
    // Charger storylines disponibles pour booking
}

public void AssignToSegment(string segmentId, string storylineId)
{
    // Assigner storyline à segment
}
```

---

### 3. Calendar → CalendarViewModel

**Code à Déplacer** (~150 lignes):

```csharp
// Collections
public ObservableCollection<ShowCalendarItemViewModel> ShowsAVenir { get; }

// Propriétés
public string? NouveauShowNom { get; set; }
public int NouveauShowSemaine { get; set; }
public int NouveauShowDuree { get; set; }

// Méthodes (~100 lignes)
private void ChargerCalendrier()
private void CreerNouveauShow()
private void PlanifierShow()
private void MettreAJourShowsAVenir()
```

**Intégration CalendarViewModel** :

```csharp
public void LoadUpcomingShows(ShowContext context)
{
    // Charger shows à venir
}

public void CreateNewShow(string nom, int semaine, int duree)
{
    // Créer nouveau show
}
```

---

### 4. Finance → FinanceViewModel

**Code à Déplacer** (~100 lignes):

```csharp
// Collections
public ObservableCollection<TvDealViewModel> DealsTv { get; }
public ObservableCollection<ReachMapItemViewModel> ReachMap { get; }
public ObservableCollection<string> ContraintesDiffusion { get; }
public ObservableCollection<AudienceHistoryItemViewModel> AudienceHistorique { get; }

// Méthodes (~50 lignes)
private void ChargerDealsTv()
private void ChargerAudienceHistorique()
private void CalculerReach()
```

**Intégration FinanceViewModel** :

```csharp
public void LoadTvDeals(ShowContext context)
{
    // Charger deals TV
}

public void LoadAudienceHistory(string showId)
{
    // Charger historique audience
}
```

---

### 5. Titles → TitlesViewModel

**Code à Déplacer** (~100 lignes):

```csharp
// Collections
public ObservableCollection<TitleOptionViewModel> TitresDisponibles { get; }

// Méthodes (~50 lignes)
private void ChargerTitres()
private void AssignerTitreSegment()
```

**Intégration TitlesViewModel** :

```csharp
public void LoadAvailableTitles(ShowContext context)
{
    // Charger titres disponibles
}

public void AssignToSegment(string segmentId, string titreId)
{
    // Assigner titre à segment
}
```

---

### Checklist Phase 6.3

**Youth System**:
- [ ] Déplacer collections vers YouthViewModel
- [ ] Déplacer méthodes youth
- [ ] Intégrer dans GameSessionViewModel
- [ ] Mettre à jour bindings XAML Youth views
- [ ] Tester création/gestion youth structures

**Storylines**:
- [ ] Déplacer collections vers StorylinesViewModel
- [ ] Ajouter méthodes disponibilité
- [ ] Intégrer dans GameSessionViewModel
- [ ] Mettre à jour bindings
- [ ] Tester assignment storylines

**Calendar**:
- [ ] Déplacer vers CalendarViewModel
- [ ] Intégrer dans GameSessionViewModel
- [ ] Mettre à jour bindings
- [ ] Tester création shows

**Finance**:
- [ ] Déplacer vers FinanceViewModel
- [ ] Intégrer dans GameSessionViewModel
- [ ] Mettre à jour bindings
- [ ] Tester affichage deals/audience

**Titles**:
- [ ] Déplacer vers TitlesViewModel
- [ ] Intégrer dans GameSessionViewModel
- [ ] Mettre à jour bindings
- [ ] Tester assignment titres

**Validation**:
- [ ] Tous les tests passent
- [ ] UI fonctionnelle
- [ ] Aucune régression
- [ ] GameSessionViewModel < 1,000 lignes

---

## 🎯 PHASE 6.4 - ShowWorkersViewModel

### Objectif

Extraire la gestion des participants/workers pour le booking.

**Réduction visée** : -300 lignes

---

### Code à Extraire

```csharp
// Collections
public ObservableCollection<ParticipantViewModel> WorkersDisponibles { get; }
public ObservableCollection<ParticipantViewModel> NouveauSegmentParticipants { get; }

// Méthodes (~250 lignes)
private void ChargerWorkersDisponibles()
private void FiltrerWorkers()
private void AjouterParticipantSegment()
private void RetirerParticipantSegment()
private void AssignerRoleWorker()
private void VerifierDisponibiliteWorker()
private void CalculerCompatibiliteWorkers()
```

---

### Architecture ShowWorkersViewModel

```csharp
using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la gestion des participants d'un show.
/// Responsable de la sélection et assignation des workers aux segments.
/// </summary>
public sealed class ShowWorkersViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private ShowContext? _context;

    public ShowWorkersViewModel(GameRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        AvailableWorkers = new ObservableCollection<ParticipantViewModel>();
        SelectedParticipants = new ObservableCollection<ParticipantViewModel>();

        AddParticipantCommand = ReactiveCommand.Create<ParticipantViewModel>(AddParticipant);
        RemoveParticipantCommand = ReactiveCommand.Create<ParticipantViewModel>(RemoveParticipant);
    }

    /// <summary>
    /// Liste des workers disponibles pour le booking.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> AvailableWorkers { get; }

    /// <summary>
    /// Participants sélectionnés pour le segment en cours.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> SelectedParticipants { get; }

    private string? _searchFilter;
    public string? SearchFilter
    {
        get => _searchFilter;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchFilter, value);
            ApplyFilter();
        }
    }

    public ReactiveCommand<ParticipantViewModel, Unit> AddParticipantCommand { get; }
    public ReactiveCommand<ParticipantViewModel, Unit> RemoveParticipantCommand { get; }

    /// <summary>
    /// Charge les workers disponibles depuis le contexte.
    /// </summary>
    public void LoadAvailableWorkers(ShowContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        AvailableWorkers.Clear();
        foreach (var worker in context.Workers)
        {
            // Vérifier disponibilité (pas blessé gravement, etc.)
            if (IsWorkerAvailable(worker))
            {
                AvailableWorkers.Add(new ParticipantViewModel(worker));
            }
        }

        Logger.Info($"{AvailableWorkers.Count} workers disponibles chargés");
    }

    /// <summary>
    /// Charge les participants d'un segment.
    /// </summary>
    public void LoadSegmentParticipants(SegmentDefinition segment)
    {
        SelectedParticipants.Clear();

        if (_context is null) return;

        foreach (var workerId in segment.Participants)
        {
            var worker = _context.Workers.FirstOrDefault(w => w.WorkerId == workerId);
            if (worker is not null)
            {
                SelectedParticipants.Add(new ParticipantViewModel(worker));
            }
        }
    }

    /// <summary>
    /// Ajoute un participant au segment.
    /// </summary>
    private void AddParticipant(ParticipantViewModel? participant)
    {
        if (participant is null || SelectedParticipants.Contains(participant))
        {
            return;
        }

        SelectedParticipants.Add(participant);
        Logger.Debug($"Participant ajouté : {participant.Nom}");
    }

    /// <summary>
    /// Retire un participant du segment.
    /// </summary>
    private void RemoveParticipant(ParticipantViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        SelectedParticipants.Remove(participant);
        Logger.Debug($"Participant retiré : {participant.Nom}");
    }

    /// <summary>
    /// Vérifie si un worker est disponible pour le booking.
    /// </summary>
    private bool IsWorkerAvailable(Worker worker)
    {
        // Pas blessé gravement
        if (!string.IsNullOrWhiteSpace(worker.Blessure))
        {
            return false;
        }

        // Pas suspendu
        // TODO: Vérifier suspension

        return true;
    }

    private void ApplyFilter()
    {
        // TODO: Implémenter filtrage par SearchFilter
        // Utiliser CollectionView pour filtrage
    }

    /// <summary>
    /// Calcule la compatibilité entre workers pour un match.
    /// </summary>
    public int CalculateCompatibility(ParticipantViewModel worker1, ParticipantViewModel worker2)
    {
        // TODO: Implémenter logique compatibilité
        // Basé sur styles, relation, etc.
        return 75;
    }
}
```

---

### Intégration dans GameSessionViewModel

```csharp
public sealed class GameSessionViewModel : ViewModelBase
{
    public ShowBookingViewModel Booking { get; }
    public ShowWorkersViewModel Workers { get; }

    public GameSessionViewModel(...)
    {
        Booking = new ShowBookingViewModel(_repository, _segmentCatalog);
        Workers = new ShowWorkersViewModel(_repository);
    }

    private void ChargerShow()
    {
        // ...
        Booking.LoadBooking(_context);
        Workers.LoadAvailableWorkers(_context);
    }
}
```

---

### Bindings XAML

```xml
<!-- Liste workers disponibles -->
<DataGrid ItemsSource="{Binding Workers.AvailableWorkers}"
          AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Nom" Binding="{Binding Nom}" />
        <DataGridTextColumn Header="Popularité" Binding="{Binding Popularite}" />
    </DataGrid.Columns>
</DataGrid>

<!-- Participants sélectionnés -->
<ItemsControl ItemsSource="{Binding Workers.SelectedParticipants}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding Nom}" />
                <Button Command="{Binding DataContext.Workers.RemoveParticipantCommand,
                                          RelativeSource={RelativeSource AncestorType=Window}}"
                        CommandParameter="{Binding}"
                        Content="Retirer" />
            </StackPanel>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

---

### Checklist Phase 6.4

- [ ] Créer ShowWorkersViewModel.cs
- [ ] Implémenter collections (AvailableWorkers, SelectedParticipants)
- [ ] Implémenter LoadAvailableWorkers()
- [ ] Implémenter LoadSegmentParticipants()
- [ ] Implémenter AddParticipant() / RemoveParticipant()
- [ ] Implémenter IsWorkerAvailable()
- [ ] Implémenter filtrage
- [ ] Intégrer dans GameSessionViewModel
- [ ] Mettre à jour bindings XAML
- [ ] Tester ajout/retrait participants
- [ ] Tester filtrage workers
- [ ] Valider aucune régression

---

## 📊 Métriques Finales Attendues

### GameSessionViewModel - Évolution

| Phase | Lignes | Delta |
|-------|--------|-------|
| **Début** | 2,379 | - |
| **Après 6.1** | 2,379 | 0 (ViewModels créés séparément) |
| **Après 6.2** | ~1,779 | -600 |
| **Après 6.3** | ~929 | -850 |
| **Après 6.4** | ~629 | -300 |
| **Objectif** | <800 | ✅ |

### Nouveaux ViewModels Créés

| ViewModel | Lignes | Responsabilité |
|-----------|--------|----------------|
| GlobalSearchViewModel | 228 | Recherche globale |
| InboxViewModel | 181 | Notifications |
| TableViewViewModel | 436 | Tables génériques |
| ShowBookingViewModel | ~600 | Booking complet |
| ShowWorkersViewModel | ~300 | Participants |
| **TOTAL** | **~1,745** | - |

### Intégrations

| ViewModel Existant | Ajout Lignes | Responsabilité Ajoutée |
|-------------------|--------------|------------------------|
| YouthViewModel | ~400 | Gestion complète youth |
| StorylinesViewModel | ~200 | Disponibilité booking |
| CalendarViewModel | ~150 | Planning shows |
| FinanceViewModel | ~100 | TV deals, audience |
| TitlesViewModel | ~100 | Titres disponibles |
| **TOTAL** | **~950** | - |

---

## ⏱️ Timeline Globale

| Phase | Durée | Complexité |
|-------|-------|------------|
| 6.1 | ✅ 4h | ⭐ Facile |
| 6.2 | ~4h | ⭐⭐⭐ Difficile |
| 6.3 | ~6h | ⭐⭐⭐⭐ Complexe |
| 6.4 | ~2h | ⭐⭐ Moyen |
| Tests | ~2h | - |
| **TOTAL** | **~18h** | - |

---

## 🎯 Résultat Final

### Architecture Finale

```
GameSessionViewModel (Coordinateur ~629 lignes)
├── Booking: ShowBookingViewModel (600 lignes)
├── Workers: ShowWorkersViewModel (300 lignes)
├── Search: GlobalSearchViewModel (228 lignes)
├── Inbox: InboxViewModel (181 lignes)
├── TableView: TableViewViewModel (436 lignes)
└── Intégrations:
    ├── Youth: YouthViewModel (+400 lignes)
    ├── Storylines: StorylinesViewModel (+200 lignes)
    ├── Calendar: CalendarViewModel (+150 lignes)
    ├── Finance: FinanceViewModel (+100 lignes)
    └── Titles: TitlesViewModel (+100 lignes)
```

### Bénéfices

✅ **Single Responsibility Principle** : Chaque ViewModel a UNE responsabilité
✅ **Testabilité** : ViewModels testables indépendamment
✅ **Maintenabilité** : Code plus facile à comprendre et modifier
✅ **Réutilisabilité** : ViewModels génériques réutilisables
✅ **Performance** : Moins de code chargé en mémoire par ViewModel
✅ **Collaboration** : Équipe peut travailler en parallèle sur différents ViewModels

---

**STATUS** : Guides d'implémentation complets pour Phases 6.3-6.4
