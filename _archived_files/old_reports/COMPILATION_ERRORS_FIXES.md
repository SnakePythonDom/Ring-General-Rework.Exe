# Corrections des erreurs de compilation - Phase 6.3

## Résumé
Ce document liste toutes les erreurs de compilation détectées après Phase 6.3 et les corrections nécessaires.

## Erreurs critiques nécessitant des corrections

### 1. Properties renommées dans les models (incompatibilité)

**SegmentViewModel** - Propriétés manquantes utilisées dans ShowBookingViewModel:
- `Type` → devrait être `TypeSegment`
- `IsMainEvent` → devrait être `EstMainEvent`
- `TitleId` → devrait être `TitreId`
- `Importance` → devrait être `Intensite`
- `WinnerId` → devrait être `VainqueurId`
- `FinishType` → devrait être `PerdantId` (?)
- `Settings` → existe mais mal référencé

**ParticipantViewModel** - Propriétés manquantes :
- `InRing`, `Popularite`, `RoleTv` → Ces propriétés n'existent pas dans ParticipantViewModel (seulement WorkerId et Nom)
- Solution : Ces données doivent venir de Worker, pas de ParticipantViewModel

**Worker model** - Propriété manquante:
- `Blessure` → Vérifier si renommé en `Injury` ou autre

### 2. Constructeurs avec paramètres manquants

**BookingIssueViewModel** - Signature actuelle vs utilisée:
- Actuel : `BookingIssueViewModel(Code, Message, Severity, SegmentId, ActionLabel)`
- Utilisé : `new BookingIssueViewModel(issue)` où issue est `ValidationIssue`
- Solution : Ajouter constructeur prenant `ValidationIssue` OU mapper correctement

**SegmentResultViewModel** - Paramètre manquant:
- Actuel : `SegmentResultViewModel(report, workerNames, libelle?)`
- Utilisé : `new SegmentResultViewModel(segmentResult)`
- Solution : Fournir dictionnaire workerNames vide ou créer surcharge

**SegmentTemplateViewModel** - Paramètre manquant:
- Besoin d'ajouter paramètre `nom`

**MatchTypeViewModel** - Paramètre manquant:
- Besoin d'ajouter paramètre `description` (7 occurrences)

**ParticipantViewModel** - Constructeur:
- Actuel : `ParticipantViewModel(workerId, nom)`
- Utilisé dans ShowWorkersViewModel sans `nom`
- Solution : Fournir le nom depuis Worker

**SegmentTypeCatalog** - Constructeur sans paramètres:
- Actuel : nécessite 4 dictionnaires
- Utilisé : `new SegmentTypeCatalog()`
- Solution : Créer dictionnaires vides ou constructeur par défaut

### 3. Types manquants

**SimulationEngine** - Type introuvable:
- Utilisé dans : ShowBookingViewModel.cs:353
- Solution : Vérifier namespace ou créer le type manquant

**ShowSimulationResult** - Propriétés manquantes:
- `WhyNote`, `Tips`, `Guidelines` manquent
- Solution : Ajouter ces propriétés au record

### 4. Méthodes manquantes

**GameRepository.MettreAJourOrdreSegment()** :
- Utilisée dans ShowBookingViewModel mais n'existe pas
- Solution : Ajouter cette méthode au repository

**InboxViewModel.RaisePropertyChanged()** :
- InboxViewModel n'hérite pas de ViewModelBase
- Solution : Faire hériter de ViewModelBase

### 5. Comparaisons de types invalides

**ValidationSeverity comparisons** (2x):
- `issue.Severity == "Error"` → devrait être `issue.Severite == ValidationSeverity.Erreur`
- `issue.Severity == "Warning"` → devrait être `issue.Severite == ValidationSeverity.Avertissement`

**StorylinePhase enum values manquants**:
- `StorylinePhase.Developpement` n'existe pas
- `StorylinePhase.Resolution` n'existe pas
- Valeurs existantes probablement différentes (Setup, Rising, Climax, etc.)

**GlobalSearchViewModel comparison**:
- `string == StorylineParticipant` - comparaison invalide
- Ligne 145:68

### 6. Type mismatch

**ShowBookingViewModel.cs:269** :
- BuildBookingPlan() passe `List<ParticipantViewModel>`
- Attendu : `IReadOnlyList<string>` (les IDs)
- Solution : `.Select(p => p.WorkerId).ToList()`

**ShowWorkersViewModel.cs:111**:
- Passe `WorkerSnapshot` où `Worker` attendu
- Solution : Convertir ou adapter

## Fichiers concernés

- `ShowBookingViewModel.cs` - 15+ erreurs
- `ShowWorkersViewModel.cs` - 8+ erreurs
- `InboxViewModel.cs` - 5+ erreurs
- `GlobalSearchViewModel.cs` - 3 erreurs
- `SegmentViewModel.cs` - Usage incompatible

## Priorité de correction

1. **URGENT** : Héritages ViewModelBase manquants (InboxViewModel, InboxItemViewModel)
2. **URGENT** : Corrections de noms de propriétés (SegmentViewModel usages)
3. **HIGH** : Constructeurs manquants (BookingIssueViewModel, SegmentResultViewModel)
4. **HIGH** : ValidationSeverity comparisons
5. **MEDIUM** : Types manquants (SimulationEngine)
6. **MEDIUM** : Méthodes repository manquantes
7. **LOW** : Enum values manquants

## Notes

- Beaucoup d'erreurs viennent d'une incohérence entre noms français/anglais
- Les ViewModels Phase 6.1-6.2 utilisent des noms anglais, Phase 6.3 utilise du français
- Il faudrait standardiser la nomenclature
