# 📘 Phase 6.2 - Guide d'Implémentation ShowBookingViewModel

**Date** : 2026-01-08
**Complexité** : ⭐⭐⭐ Difficile
**Durée estimée** : ~4h
**Objectif** : Extraire ~600 lignes de code booking depuis GameSessionViewModel

---

## 🎯 Objectif

Extraire toute la logique de booking/segments depuis GameSessionViewModel vers un nouveau ShowBookingViewModel dédié.

**Réduction visée** : -600 lignes (GameSessionViewModel 2,379 → 1,779)

---

## 📋 Inventaire du Code Booking

### Collections à Extraire

```csharp
// Dans GameSessionViewModel.cs
public ObservableCollection<SegmentViewModel> Segments { get; }
public ObservableCollection<BookingIssueViewModel> ValidationIssues { get; }
public ObservableCollection<SegmentResultViewModel> Resultats { get; }
public ObservableCollection<SegmentTypeOptionViewModel> SegmentTypes { get; }
public ObservableCollection<SegmentTemplateViewModel> SegmentTemplates { get; }
public ObservableCollection<MatchTypeViewModel> MatchTypes { get; }
public ObservableCollection<string> PourquoiNote { get; }
public ObservableCollection<string> Conseils { get; }
public ObservableCollection<string> ConsignesBooking { get; }
```

### Propriétés à Extraire

```csharp
public SegmentViewModel? SegmentSelectionne { get; set; }
public SegmentResultViewModel? ResultatSelectionne { get; set; }
public string? ValidationErreurs { get; }
public string? ValidationAvertissements { get; }
```

### Services/Helpers Privés à Extraire

```csharp
private readonly BookingValidator _validator;
private readonly SegmentTypeCatalog _segmentCatalog;
private readonly BookingBuilderService _bookingBuilder;
private readonly TemplateService _templateService;
```

### Méthodes à Extraire

**Gestion Segments** (~200 lignes):
- `AjouterSegment()`
- `SupprimerSegment()`
- `ModifierSegment()`
- `DeplacerSegmentHaut()`
- `DeplacerSegmentBas()`
- `DupliquerSegment()`

**Validation** (~100 lignes):
- `ValiderBooking()`
- `MettreAJourValidation()`
- `MettreAJourAvertissements()`

**Simulation** (~150 lignes):
- `SimulerShow()`
- `MettreAJourAnalyseShow()`
- `MettreAJourResultats()`

**Templates** (~100 lignes):
- `ChargerTemplates()`
- `AppliquerTemplate()`
- `SauvegarderTemplate()`

**Helpers** (~50 lignes):
- `CalculerDureeShow()`
- `VerifierContraintes()`
- Autres utilitaires booking

**Total estimé** : ~600 lignes

---

## 🏗️ Architecture Cible

### ShowBookingViewModel.cs

```csharp
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Core.Validation;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Booking;

/// <summary>
/// ViewModel pour la gestion complète du booking d'un show.
/// Responsable des segments, validation, simulation et templates.
/// </summary>
public sealed class ShowBookingViewModel : ViewModelBase
{
    private readonly GameRepository _repository;
    private readonly BookingValidator _validator;
    private readonly SegmentTypeCatalog _catalog;
    private readonly BookingBuilderService _builder;
    private readonly TemplateService _templateService;
    private ShowContext? _context;

    public ShowBookingViewModel(
        GameRepository repository,
        SegmentTypeCatalog catalog)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _validator = new BookingValidator();
        _builder = new BookingBuilderService();
        _templateService = new TemplateService();

        // Collections
        Segments = new ObservableCollection<SegmentViewModel>();
        ValidationIssues = new ObservableCollection<BookingIssueViewModel>();
        Results = new ObservableCollection<SegmentResultViewModel>();
        SegmentTypes = new ObservableCollection<SegmentTypeOptionViewModel>();
        Templates = new ObservableCollection<SegmentTemplateViewModel>();
        MatchTypes = new ObservableCollection<MatchTypeViewModel>();
        WhyNote = new ObservableCollection<string>();
        Tips = new ObservableCollection<string>();
        BookingGuidelines = new ObservableCollection<string>();

        // Commandes
        AddSegmentCommand = ReactiveCommand.Create(AddSegment);
        RemoveSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(RemoveSegment);
        MoveSegmentUpCommand = ReactiveCommand.Create<SegmentViewModel>(MoveSegmentUp);
        MoveSegmentDownCommand = ReactiveCommand.Create<SegmentViewModel>(MoveSegmentDown);
        SimulateShowCommand = ReactiveCommand.Create(SimulateShow);
        ValidateBookingCommand = ReactiveCommand.Create(ValidateBooking);
    }

    #region Collections

    public ObservableCollection<SegmentViewModel> Segments { get; }
    public ObservableCollection<BookingIssueViewModel> ValidationIssues { get; }
    public ObservableCollection<SegmentResultViewModel> Results { get; }
    public ObservableCollection<SegmentTypeOptionViewModel> SegmentTypes { get; }
    public ObservableCollection<SegmentTemplateViewModel> Templates { get; }
    public ObservableCollection<MatchTypeViewModel> MatchTypes { get; }
    public ObservableCollection<string> WhyNote { get; }
    public ObservableCollection<string> Tips { get; }
    public ObservableCollection<string> BookingGuidelines { get; }

    #endregion

    #region Properties

    private SegmentViewModel? _selectedSegment;
    public SegmentViewModel? SelectedSegment
    {
        get => _selectedSegment;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSegment, value);
            this.RaisePropertyChanged(nameof(HasSelectedSegment));
        }
    }

    public bool HasSelectedSegment => SelectedSegment is not null;

    private SegmentResultViewModel? _selectedResult;
    public SegmentResultViewModel? SelectedResult
    {
        get => _selectedResult;
        set => this.RaiseAndSetIfChanged(ref _selectedResult, value);
    }

    private string? _validationErrors;
    public string? ValidationErrors
    {
        get => _validationErrors;
        private set => this.RaiseAndSetIfChanged(ref _validationErrors, value);
    }

    private string? _validationWarnings;
    public string? ValidationWarnings
    {
        get => _validationWarnings;
        private set => this.RaiseAndSetIfChanged(ref _validationWarnings, value);
    }

    public int TotalDuration => Segments.Sum(s => s.DureeMinutes);
    public int SegmentCount => Segments.Count;

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> AddSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> RemoveSegmentCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentUpCommand { get; }
    public ReactiveCommand<SegmentViewModel, Unit> MoveSegmentDownCommand { get; }
    public ReactiveCommand<Unit, Unit> SimulateShowCommand { get; }
    public ReactiveCommand<Unit, Unit> ValidateBookingCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Charge le booking depuis le contexte du show.
    /// </summary>
    public void LoadBooking(ShowContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        Segments.Clear();
        foreach (var segment in context.Segments)
        {
            Segments.Add(new SegmentViewModel(segment));
        }

        LoadSegmentTypes();
        LoadTemplates();
        LoadMatchTypes();
        ValidateBooking();

        Logger.Info($"Booking chargé : {Segments.Count} segments");
    }

    /// <summary>
    /// Ajoute un nouveau segment au booking.
    /// </summary>
    public void AddSegment()
    {
        if (_context is null)
        {
            Logger.Warning("Impossible d'ajouter un segment : contexte non chargé");
            return;
        }

        var newSegment = new SegmentDefinition(
            $"SEG-{Guid.NewGuid():N}".ToUpperInvariant(),
            "promo",
            new List<string>(),
            10,
            false,
            null,
            null,
            0,
            null,
            null,
            new Dictionary<string, string>());

        _repository.AjouterSegment(_context.Show.ShowId, newSegment, Segments.Count + 1);
        Segments.Add(new SegmentViewModel(newSegment));

        ValidateBooking();
        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(SegmentCount));

        Logger.Debug($"Segment ajouté : {newSegment.SegmentId}");
    }

    /// <summary>
    /// Supprime un segment du booking.
    /// </summary>
    public void RemoveSegment(SegmentViewModel? segment)
    {
        if (segment is null || _context is null)
        {
            return;
        }

        _repository.SupprimerSegment(segment.SegmentId);
        Segments.Remove(segment);

        if (SelectedSegment == segment)
        {
            SelectedSegment = null;
        }

        ValidateBooking();
        this.RaisePropertyChanged(nameof(TotalDuration));
        this.RaisePropertyChanged(nameof(SegmentCount));

        Logger.Debug($"Segment supprimé : {segment.SegmentId}");
    }

    /// <summary>
    /// Déplace un segment vers le haut.
    /// </summary>
    public void MoveSegmentUp(SegmentViewModel? segment)
    {
        if (segment is null) return;

        var index = Segments.IndexOf(segment);
        if (index <= 0) return;

        Segments.Move(index, index - 1);
        SaveSegmentOrder();
    }

    /// <summary>
    /// Déplace un segment vers le bas.
    /// </summary>
    public void MoveSegmentDown(SegmentViewModel? segment)
    {
        if (segment is null) return;

        var index = Segments.IndexOf(segment);
        if (index < 0 || index >= Segments.Count - 1) return;

        Segments.Move(index, index + 1);
        SaveSegmentOrder();
    }

    /// <summary>
    /// Valide le booking complet.
    /// </summary>
    public void ValidateBooking()
    {
        if (_context is null)
        {
            return;
        }

        var plan = _builder.BuildBookingPlan(_context.Show.ShowId, Segments);
        var result = _validator.ValiderBooking(plan);

        ValidationIssues.Clear();
        foreach (var issue in result.Issues)
        {
            ValidationIssues.Add(new BookingIssueViewModel(issue));
        }

        var errors = ValidationIssues.Where(i => i.Severity == "Error").ToList();
        var warnings = ValidationIssues.Where(i => i.Severity == "Warning").ToList();

        ValidationErrors = errors.Any()
            ? $"{errors.Count} erreur(s)"
            : null;

        ValidationWarnings = warnings.Any()
            ? $"{warnings.Count} avertissement(s)"
            : null;

        Logger.Debug($"Validation : {errors.Count} erreurs, {warnings.Count} avertissements");
    }

    /// <summary>
    /// Simule le show et génère les résultats.
    /// </summary>
    public void SimulateShow()
    {
        if (_context is null)
        {
            Logger.Warning("Impossible de simuler : contexte non chargé");
            return;
        }

        Logger.Info("Simulation du show...");

        var plan = _builder.BuildBookingPlan(_context.Show.ShowId, Segments);
        var engine = new SimulationEngine();
        var result = engine.SimulerShow(plan, _context);

        Results.Clear();
        foreach (var segmentResult in result.SegmentResults)
        {
            Results.Add(new SegmentResultViewModel(segmentResult));
        }

        UpdateAnalysis(result);

        Logger.Info($"Simulation terminée : Note globale {result.NoteGlobale}");
    }

    #endregion

    #region Private Methods

    private void LoadSegmentTypes()
    {
        SegmentTypes.Clear();
        foreach (var type in _catalog.Types)
        {
            SegmentTypes.Add(new SegmentTypeOptionViewModel(type.Key, type.Value));
        }
    }

    private void LoadTemplates()
    {
        Templates.Clear();
        var templates = _templateService.LoadTemplates();
        foreach (var template in templates)
        {
            Templates.Add(new SegmentTemplateViewModel(template));
        }
    }

    private void LoadMatchTypes()
    {
        MatchTypes.Clear();
        MatchTypes.Add(new MatchTypeViewModel("Singles", "Simple"));
        MatchTypes.Add(new MatchTypeViewModel("Tag", "Tag Team"));
        MatchTypes.Add(new MatchTypeViewModel("Triple", "Triple Threat"));
        MatchTypes.Add(new MatchTypeViewModel("Fatal4", "Fatal 4-Way"));
        MatchTypes.Add(new MatchTypeViewModel("Battle", "Battle Royal"));
    }

    private void SaveSegmentOrder()
    {
        if (_context is null) return;

        for (int i = 0; i < Segments.Count; i++)
        {
            // Sauvegarder nouvel ordre dans DB
            _repository.MettreAJourOrdreSegment(Segments[i].SegmentId, i + 1);
        }
    }

    private void UpdateAnalysis(ShowSimulationResult result)
    {
        WhyNote.Clear();
        foreach (var reason in result.WhyNote)
        {
            WhyNote.Add(reason);
        }

        Tips.Clear();
        foreach (var tip in result.Tips)
        {
            Tips.Add(tip);
        }

        BookingGuidelines.Clear();
        foreach (var guideline in result.Guidelines)
        {
            BookingGuidelines.Add(guideline);
        }
    }

    #endregion
}
```

---

## 🔄 Intégration dans GameSessionViewModel

### Modifications GameSessionViewModel.cs

```csharp
public sealed class GameSessionViewModel : ViewModelBase
{
    // AVANT: Tout en interne
    // public ObservableCollection<SegmentViewModel> Segments { get; }
    // public ObservableCollection<BookingIssueViewModel> ValidationIssues { get; }
    // ...

    // APRÈS: Délégation vers ShowBookingViewModel
    public ShowBookingViewModel Booking { get; }

    public GameSessionViewModel(string? cheminDb = null, ServiceContainer? services = null)
    {
        // ...

        // Initialiser ShowBookingViewModel
        Booking = new ShowBookingViewModel(_repository, _segmentCatalog);

        // ...
    }

    private void ChargerShow()
    {
        // ...

        // Charger le booking
        Booking.LoadBooking(_context);

        // ...
    }

    public void SimulerShow()
    {
        // Déléguer vers Booking
        Booking.SimulateShow();
    }
}
```

---

## 🎨 Mise à Jour Bindings XAML

### Avant

```xml
<!-- Segments -->
<DataGrid ItemsSource="{Binding Segments}"
          SelectedItem="{Binding SegmentSelectionne}" />

<!-- Validation -->
<TextBlock Text="{Binding ValidationErreurs}" />

<!-- Boutons -->
<Button Command="{Binding SimulerShowCommand}" />
```

### Après

```xml
<!-- Segments -->
<DataGrid ItemsSource="{Binding Booking.Segments}"
          SelectedItem="{Binding Booking.SelectedSegment}" />

<!-- Validation -->
<TextBlock Text="{Binding Booking.ValidationErrors}" />

<!-- Boutons -->
<Button Command="{Binding Booking.SimulateShowCommand}" />
```

### Fichiers XAML à Mettre à Jour

1. `Views/Booking/BookingView.xaml`
2. `Views/Show/ShowView.xaml`
3. Tous les UserControls qui bindent vers Segments

**Outil de recherche** :
```bash
grep -r "Binding Segments" --include="*.xaml" src/RingGeneral.UI/Views/
grep -r "Binding ValidationErreurs" --include="*.xaml" src/RingGeneral.UI/Views/
grep -r "Binding SimulerShow" --include="*.xaml" src/RingGeneral.UI/Views/
```

---

## ✅ Checklist Phase 6.2

### Étape 1 - Création ShowBookingViewModel
- [ ] Créer `src/RingGeneral.UI/ViewModels/Booking/ShowBookingViewModel.cs`
- [ ] Copier toutes les collections (Segments, ValidationIssues, etc.)
- [ ] Copier toutes les propriétés (SelectedSegment, ValidationErrors, etc.)
- [ ] Copier tous les services privés (validator, builder, etc.)
- [ ] Implémenter constructeur avec dépendances

### Étape 2 - Méthodes Publiques
- [ ] Implémenter `LoadBooking(ShowContext)`
- [ ] Implémenter `AddSegment()`
- [ ] Implémenter `RemoveSegment()`
- [ ] Implémenter `MoveSegmentUp()` / `MoveSegmentDown()`
- [ ] Implémenter `ValidateBooking()`
- [ ] Implémenter `SimulateShow()`

### Étape 3 - Méthodes Privées
- [ ] Implémenter `LoadSegmentTypes()`
- [ ] Implémenter `LoadTemplates()`
- [ ] Implémenter `LoadMatchTypes()`
- [ ] Implémenter `SaveSegmentOrder()`
- [ ] Implémenter `UpdateAnalysis()`

### Étape 4 - Intégration GameSessionViewModel
- [ ] Ajouter propriété `public ShowBookingViewModel Booking { get; }`
- [ ] Initialiser dans constructeur
- [ ] Appeler `Booking.LoadBooking()` dans `ChargerShow()`
- [ ] Supprimer anciennes propriétés Segments, ValidationIssues, etc.
- [ ] Supprimer anciennes méthodes booking

### Étape 5 - XAML
- [ ] Chercher tous bindings `{Binding Segments}`
- [ ] Remplacer par `{Binding Booking.Segments}`
- [ ] Idem pour ValidationErrors, SelectedSegment, etc.
- [ ] Tester compilation XAML

### Étape 6 - Tests
- [ ] Compiler le projet
- [ ] Lancer l'application
- [ ] Tester ajout segment
- [ ] Tester suppression segment
- [ ] Tester validation
- [ ] Tester simulation

### Étape 7 - Commit
- [ ] `git add -A`
- [ ] `git commit -m "feat(refactor): Phase 6.2 - Extraction ShowBookingViewModel"`
- [ ] Vérifier que GameSessionViewModel a bien -600 lignes

---

## ⚠️ Pièges à Éviter

### 1. Dépendances Circulaires
**Problème** : ShowBookingViewModel pourrait référencer GameSessionViewModel

**Solution** :
- ShowBookingViewModel ne doit PAS connaître GameSessionViewModel
- Communication via événements ou callbacks si nécessaire
- Utiliser interfaces pour dépendances

### 2. État Partagé
**Problème** : Certaines données sont utilisées à la fois par booking et autres responsabilités

**Solution** :
- Passer ShowContext en paramètre à LoadBooking()
- Ne pas stocker de référence permanente à ShowContext si partagé
- Copier les données si nécessaire

### 3. Bindings XAML Cassés
**Problème** : Oublier de mettre à jour les bindings

**Solution** :
- Utiliser grep pour trouver TOUS les bindings
- Tester l'UI après chaque modification
- Commiter par petites étapes

### 4. Tests Cassés
**Problème** : Tests unitaires qui référencent anciennes propriétés

**Solution** :
- Mettre à jour tests AVANT de supprimer propriétés
- Ajouter tests pour ShowBookingViewModel
- Valider que tous les tests passent

---

## 📊 Estimation Temps

| Tâche | Durée |
|-------|-------|
| Créer ShowBookingViewModel | 1h |
| Implémenter méthodes | 1h30 |
| Intégration GameSessionViewModel | 30min |
| Mise à jour XAML | 45min |
| Tests et debugging | 45min |
| **TOTAL** | **~4h30** |

---

## 🎯 Résultat Attendu

**Avant** :
- GameSessionViewModel : 2,379 lignes

**Après** :
- GameSessionViewModel : ~1,779 lignes (-600)
- ShowBookingViewModel : ~600 lignes (nouveau)

**Bénéfices** :
- ✅ Responsabilités clairement séparées
- ✅ ShowBookingViewModel testable indépendamment
- ✅ Réutilisable dans autres contextes (booking backstage, etc.)
- ✅ Plus facile à maintenir

---

## 📝 Notes

- **Complexité** : ⭐⭐⭐ Difficile car beaucoup de dépendances
- **Risque** : Moyen (bindings XAML à mettre à jour)
- **Impact** : Haut (booking est une fonctionnalité centrale)
- **Réversibilité** : Facile (via git revert si problèmes)

---

**STATUS** : Guide complet prêt pour implémentation Phase 6.2
