# 🗺️ ROADMAP MISE À JOUR - RING GENERAL
**Date de mise à jour**: 2026-01-07
**Basé sur**: Analyse technique complète + roadmap.fr.json

---

## 📊 ÉTAT ACTUEL DU PROJET

### Progrès Global: ~15% (Phase 0-1)

**Phase actuelle**: **Phase 0 - Stabilisation Critique**

---

## 🎯 PHASES DE DÉVELOPPEMENT

### ✅ PHASE 0: STABILISATION CRITIQUE (EN COURS - 80%)

**Objectif**: Rendre le projet buildable et l'architecture navigable

| Tâche | Statut | Priorité | Notes |
|-------|--------|----------|-------|
| Corriger l'architecture UI (double MainWindow) | ✅ FAIT | 🔴 CRITIQUE | Ancien prototype archivé |
| Configurer le DI correctement | ✅ FAIT | 🔴 CRITIQUE | App.axaml.cs OK |
| Créer le système de navigation | ✅ FAIT | 🔴 CRITIQUE | ShellViewModel + NavigationService |
| Supprimer les fichiers obsolètes | ✅ FAIT | 🟡 MOYENNE | Archivés dans _archived_files/ |
| Documenter l'architecture | ✅ FAIT | 🟡 MOYENNE | RECAPITULATIF_TECHNIQUE.md |
| Créer les ViewModels manquants | ⏳ EN COURS | 🔴 HAUTE | 2/10 créés |
| Créer les Views correspondantes | ⏳ EN COURS | 🔴 HAUTE | 1/10 créées |
| Peupler la DB avec données de test | ❌ À FAIRE | 🔴 HAUTE | BAKI1.1.db disponible |

**Livrable**: Application qui démarre avec navigation fonctionnelle

---

### 🟡 PHASE 1: FONDATIONS UI/UX (0%)

**Objectif**: Interface complète et navigable

#### Tâche 1.1: Créer tous les ViewModels
**Durée estimée**: 3-5 jours

**ViewModels à créer**:
- [ ] `DashboardViewModel` - Vue d'ensemble (accueil)
- [ ] `RosterViewModel` - Liste des workers
- [ ] `WorkerDetailViewModel` - Fiche worker détaillée
- [ ] `TitlesViewModel` - Gestion des titres
- [ ] `InjuriesViewModel` - Suivi médical
- [ ] `ActiveStorylinesViewModel` - Storylines actives
- [ ] `StorylineDetailViewModel` - Détail storyline
- [ ] `YouthDashboardViewModel` - Vue d'ensemble youth
- [ ] `FinanceDashboardViewModel` - Finances
- [ ] `CalendarViewModel` - Calendrier des shows

#### Tâche 1.2: Créer toutes les Views
**Durée estimée**: 3-5 jours

**Pattern MVVM**:
```
Views/
├── Dashboard/
│   └── DashboardView.axaml
├── Roster/
│   ├── RosterView.axaml
│   └── WorkerDetailView.axaml
├── Storylines/
│   ├── StorylinesView.axaml
│   └── StorylineDetailView.axaml
├── Youth/
│   └── YouthDashboardView.axaml
├── Finance/
│   └── FinanceDashboardView.axaml
└── Calendar/
    └── CalendarView.axaml
```

#### Tâche 1.3: Implémenter les DataTemplates
**Durée estimée**: 1 jour

Dans `Shell/MainWindow.axaml`:
```xml
<Window.DataTemplates>
    <DataTemplate DataType="vmBooking:BookingViewModel">
        <booking:BookingView />
    </DataTemplate>
    <DataTemplate DataType="vmRoster:RosterViewModel">
        <roster:RosterView />
    </DataTemplate>
    <!-- ... autres templates ... -->
</Window.DataTemplates>
```

**Livrable**: Toutes les pages accessibles via navigation

---

### 🟡 PHASE 2: INTÉGRATION DONNÉES (0%)

**Objectif**: Afficher les vraies données depuis la DB

#### Tâche 2.1: Seed automatique de la DB
**Durée estimée**: 2-3 jours

**Actions**:
1. Créer `DbSeeder.cs` dans `RingGeneral.Data/Database/`
2. Implémenter `SeedFromBaki(string bakiDbPath)`
3. Appeler au premier lancement dans `DbInitializer`

```csharp
public static class DbSeeder
{
    public static void SeedIfEmpty(string connectionString)
    {
        if (EstBaseDonnéesVide(connectionString))
        {
            var bakiPath = Path.Combine(Directory.GetCurrentDirectory(), "BAKI1.1.db");
            if (File.Exists(bakiPath))
            {
                ImporterDepuisBaki(bakiPath, connectionString);
            }
            else
            {
                SeedDonnéesParDéfaut(connectionString);
            }
        }
    }
}
```

#### Tâche 2.2: Mapper les ViewModels aux Repositories
**Durée estimée**: 2-3 jours

**Pour chaque ViewModel**:
```csharp
public class RosterViewModel : ViewModelBase
{
    private readonly GameRepository _repository;

    public ObservableCollection<WorkerViewModel> Workers { get; }

    public void LoadWorkers()
    {
        var workers = _repository.ChargerTousLesWorkers();
        Workers.Clear();
        foreach (var w in workers)
        {
            Workers.Add(new WorkerViewModel(w));
        }
    }
}
```

#### Tâche 2.3: Tester le chargement des données
**Durée estimée**: 1 jour

**Checklist**:
- [ ] BookingView affiche les segments
- [ ] RosterView affiche les workers
- [ ] TitlesView affiche les titres
- [ ] StorylinesView affiche les storylines
- [ ] YouthView affiche les trainees
- [ ] FinanceView affiche les transactions
- [ ] CalendarView affiche les shows

**Livrable**: Toutes les pages affichent les vraies données

---

### 🟢 PHASE 3: FONCTIONNALITÉS MÉTIER (0%)

**Objectif**: Implémenter la boucle de jeu complète

#### Étape 6: Contrats v1
**Source**: roadmap.fr.json

**Fonctionnalités**:
- Négociation style FM : offre, contre-offre
- Clauses contractuelles
- Validation accepter/refuser

**ViewModels nécessaires**:
- `ContractNegotiationViewModel`
- `ContractDetailViewModel`

#### Étape 7: Inbox & News
**Fonctionnalités**:
- Génération hebdomadaire de messages
- Filtrage par type
- Liens vers fiches

**ViewModels nécessaires**:
- `InboxViewModel` (existe déjà partiellement)

#### Étape 8: Scouting v1
**Fonctionnalités**:
- Rapports par région
- Shortlist
- Missions de scouting

**Onglets**:
- Régions, Rapports, Shortlist, Missions

#### Étape 9: Youth v1
**Fonctionnalités**:
- Création de structures Youth
- Gestion des élèves
- Progression basique

#### Étape 10: Shows + Calendrier
**Fonctionnalités**:
- Créer show
- Assigner runtime, lieu, diffusion
- Calendrier visuel

#### Étape 11: Booking v1
**Statut**: Partiellement implémenté ✅

**Fonctionnalités manquantes**:
- ❌ Scripts pour promos/angles
- ❌ Système de templates avancé

#### Étape 12: Simulation show + ratings
**Statut**: Backend existe ✅
**À faire**: Améliorer l'affichage des résultats

#### Étape 13: Storylines + Heat + Momentum
**Statut**: Structure DB existe ✅
**À faire**: UI complète

#### Étape 14: Titres + historique + contenders
**Statut**: Structure DB existe ✅
**À faire**: UI complète

#### Étape 15: Finances
**Statut**: Structure DB existe ✅
**À faire**: UI complète

#### Étape 16: Diffusion (TV/Streaming)
**Statut**: Backend existe (DealRevenueModel) ✅
**À faire**: UI de gestion

#### Étape 17: Blessures/Fatigue + médical
**Statut**: InjuryService existe ✅
**À faire**: UI de suivi

#### Étape 18: Profondeur backstage
**Statut**: BackstageService existe ✅
**À faire**: UI de gestion

#### Étape 19: Bibliothèque segments + templates
**Statut**: SegmentTemplateViewModel existe ✅
**À faire**: UI enrichie

#### Étape 20: Modding + import/export
**Statut**: Outils existent ✅
**À faire**: UI dans l'app

---

### 🟢 PHASE 4: PERFORMANCE & OPTIMISATION (0%)

**Objectif**: Supporter 200k workers sans ralentissement

#### Tâche 4.1: Cache mémoire
**Actions**:
- Implémenter `GameCache.cs`
- Cache des workers par compagnie
- TTL configurable
- Invalidation intelligente

#### Tâche 4.2: Pagination
**Actions**:
- PagedResult<T> pour toutes les listes
- Limit/Offset dans les requêtes SQL
- Virtual scrolling dans les DataGrids

#### Tâche 4.3: Index SQL
**Actions**:
- Créer `003_performance_indexes.sql`
- Index sur Workers(CompanyId, Popularity)
- Index sur Contracts(WorkerId, Status)
- Index sur Storylines(CompanyId, IsActive)

#### Tâche 4.4: LOD (Level of Detail)
**Actions**:
- WorkerReference (minimal)
- WorkerSummary (attributs principaux)
- WorkerSnapshot (tous détails)

#### Tâche 4.5: Tests de charge
**Actions**:
- Générer DB avec 200k workers
- Mesurer temps de chargement
- Seuils: <2s pour ChargerShowContext, <5s pour PasserSemaine

---

### 🟢 PHASE 5: QA & POLISH (0%)

**Objectif**: Application stable et prête pour release

#### Tâche 5.1: Corriger les tests
**Actions**:
- Synchroniser MedicalFlowTests
- Synchroniser SimulationEngineTests
- Ajouter tests de navigation
- Viser 70%+ couverture

#### Tâche 5.2: Améliorer le packaging
**Actions**:
- ZIP contenant exe + specs + migrations
- Release automatique sur tag GitHub
- README inclus

#### Tâche 5.3: Documentation utilisateur
**Actions**:
- QUICKSTART_FR.md
- CONTROLS_FR.md (raccourcis clavier)
- FAQ_FR.md
- Tutoriel vidéo ?

---

## 📅 PLANNING ESTIMÉ

| Phase | Durée | Dates cibles |
|-------|-------|--------------|
| Phase 0 (reste) | 3-5 jours | Semaine 2-2026 |
| Phase 1 (UI/UX) | 7-10 jours | Semaine 3-4/2026 |
| Phase 2 (Données) | 5-7 jours | Semaine 5/2026 |
| Phase 3 (Métier) | 30-45 jours | Semaines 6-13/2026 |
| Phase 4 (Perf) | 7-10 jours | Semaines 14-15/2026 |
| Phase 5 (QA) | 7-10 jours | Semaines 16-17/2026 |

**Release cible**: Avril 2026

---

## 🎯 PROCHAINES ACTIONS IMMÉDIATES

### Cette semaine (Semaine 2/2026)

1. ✅ **Nettoyer le projet** (FAIT)
   - Archiver fichiers obsolètes
   - Documenter l'architecture

2. ⏳ **Réparer la navigation** (EN COURS)
   - Créer RosterViewModel
   - Créer RosterView
   - Tester la navigation Booking → Roster

3. ⏳ **Peupler la DB** (À FAIRE)
   - Implémenter DbSeeder
   - Importer depuis BAKI1.1.db
   - Vérifier que les données s'affichent

4. ⏳ **Créer 3 ViewModels prioritaires** (À FAIRE)
   - DashboardViewModel
   - RosterViewModel
   - TitlesViewModel

5. ⏳ **Créer 3 Views prioritaires** (À FAIRE)
   - DashboardView
   - RosterView
   - TitlesView

---

## 📊 MÉTRIQUES DE PROGRESSION

### Complétude par couche

| Couche | Complétude | Commentaire |
|--------|------------|-------------|
| **Base de données** | 90% | Schéma complet, manque seed |
| **Repositories** | 80% | Refactoring en cours |
| **Core Services** | 70% | Booking, Simulation, Injury OK |
| **ViewModels** | 20% | 2/10 créés |
| **Views** | 10% | 1/10 créées |
| **Navigation** | 80% | Système OK, Views manquantes |

### Tests

| Type de test | Couverture |
|--------------|------------|
| Tests unitaires Core | ~60% |
| Tests unitaires Data | ~40% |
| Tests d'intégration | ~20% |
| Tests UI | 0% |

---

## 🔗 RÉFÉRENCES

- [RECAPITULATIF_TECHNIQUE.md](./RECAPITULATIF_TECHNIQUE.md) - État actuel détaillé
- [README.md](./README.md) - Documentation principale
- [QUICK_START_GUIDE.md](./QUICK_START_GUIDE.md) - Guide de démarrage
- [docs/PLAN_ACTION_FR.md](./docs/PLAN_ACTION_FR.md) - Plan d'action détaillé (ancien)
- [specs/roadmap.fr.json](./specs/roadmap.fr.json) - Roadmap JSON

---

**Dernière mise à jour**: 2026-01-07 par Claude Code
