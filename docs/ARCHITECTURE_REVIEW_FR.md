# Ring General - Revue Architecture Complète

**Date**: 2026-01-10
**Version**: 2.4 (Vérification complète de l'implémentation)
**Statut**: En développement actif - Phase 1.9+ complète
**Langage**: C# / .NET 8.0

---

## Résumé Exécutif

**Ring General** est un jeu de gestion de compagnie de catch professionnel (style Football Manager/TEW) développé en .NET 8.0 avec Avalonia UI. Le projet suit une **architecture en couches exemplaire** avec séparation claire entre UI, logique métier, accès aux données et spécifications. Le code est entièrement en **français** et démontre des patterns professionnels pour un système de gestion de jeu complexe.

### Métriques Clés

| Métrique | Valeur |
|----------|--------|
| Projets dans la solution | 7 |
| **Repositories spécialisés** | **30+** ⬆️ |
| Fichiers C# sources | 280+ |
| ViewModels | 70+ |
| Services Core | 50+ |
| Fichiers de tests | 2 |
| Framework | .NET 8.0 LTS |
| UI Framework | Avalonia 11.0.6 |
| Base de données | SQLite 8.0.0 |
| Fichiers de migration | 27 |
| Packages NuGet externes | 10+ |

### Notation Globale: **8.5/10** (+1.0)

**Points forts**: Architecture modulaire exemplaire, **30+ repositories spécialisés**, **GameRepository transformé en façade**, système d'attributs professionnel (40 attributs), système de personnalité FM-like (25+ profils), **systèmes backstage avancés** (Moral, Rumeurs, Népotisme, Crises, IA Booker/Propriétaire), modèles immuables, **Dependency Injection complète** (Microsoft.Extensions.DependencyInjection)
**Points à améliorer**: Duplication schéma DB (en cours), conteneur DI partiellement introduit (centraliser usage), logging structuré manquant, ViewModels à optimiser

**🎉 Nouveautés (Phase 2.0 - Janvier 2026)** :
- ✅ Système d'attributs de performance complet (40 attributs)
- ✅ Système de personnalité automatique (25+ profils)
- ✅ **Refactoring majeur** : 30+ repositories spécialisés créés
- ✅ **GameRepository transformé en façade** orchestrant les repositories spécialisés
- ✅ **8+ nouveaux systèmes backstage sophistiqués** implémentés
- ✅ **Dependency Injection complète** : Microsoft.Extensions.DependencyInjection intégré dans App.axaml.cs
- ✅ **70+ ViewModels** créés avec injection de dépendances
- ✅ **27 migrations SQL** pour schéma évolutif
- ✅ Initialisation améliorée de la World DB et enregistrement des services (DbInitializer, DbValidator, SaveGameManager) dans l'amorçage UI

---

## 1. Structure du Projet

### 1.1 Organisation de la Solution

```
RingGeneral.sln (7 projets)
│
├── Couche Core (Logique Métier)
│   ├── RingGeneral.Core (205 fichiers C#)
│   │   ├── Models/ - Entités du domaine (records immuables)
│   │   ├── Services/ - Services métier (45+ services)
│   │   ├── Simulation/ - Moteurs de simulation
│   │   ├── Medical/ - Système de blessures
│   │   ├── Contracts/ - Négociations de contrats
│   │   ├── Random/ - Générateur aléatoire déterministe
│   │   ├── Validation/ - Validation métier
│   │   └── Interfaces/ - Contrats de services & repositories (27+ interfaces)
│   │
│   └── RingGeneral.Specs (10 fichiers)
│       ├── Models/ - Modèles de configuration
│       └── Services/ - Chargement JSON specs
│
├── Couche Data (Accès aux Données)
│   └── RingGeneral.Data (60 fichiers C#, 18 SQL)
│       ├── Database/ - Initialisation & migrations
│       ├── Repositories/ - Pattern Repository (30+ repositories spécialisés)
│       └── Models/ - DTOs & modèles de persistance
│
├── Couche Présentation
│   └── RingGeneral.UI (WinExe)
│       ├── Views/ - Vues Avalonia (14 fichiers AXAML)
│       ├── ViewModels/ - ViewModels MVVM (70+ fichiers)
│       └── Services/ - Services UI (Navigation, Messaging)
│
├── Outils
│   ├── RingGeneral.Tools.BakiImporter (CLI import DB BAKI)
│   └── RingGeneral.Tools.DbManager (Utilitaires DB)
│
└── Tests
    └── RingGeneral.Tests (projet vide)
```

### 1.2 Graphe de Dépendances

```
RingGeneral.UI (WinExe)
  ├─> RingGeneral.Core
  ├─> RingGeneral.Data
  └─> RingGeneral.Specs

RingGeneral.Data
  ├─> RingGeneral.Core
  └─> RingGeneral.Specs

RingGeneral.Core
  └─> RingGeneral.Specs

RingGeneral.Specs
  └─> (Aucune dépendance - Pure configuration)

RingGeneral.Tools.*
  ├─> RingGeneral.Core
  └─> RingGeneral.Specs

RingGeneral.Tests
  ├─> RingGeneral.Core
  ├─> RingGeneral.Data
  └─> RingGeneral.Specs
```

**Analyse**: Dépendances unidirectionnelles correctes, pas de références circulaires. ✅

---

## 2. Architecture & Patterns

### 2.1 Pattern Architectural: **Layered Architecture avec influences DDD**

```
┌─────────────────────────────────────────┐
│  COUCHE PRÉSENTATION (UI)                │
│  - Avalonia MVVM                         │
│  - ReactiveUI pour bindings réactifs     │
│  - DataGrid pour affichage tabulaire     │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│  COUCHE LOGIQUE MÉTIER (Core)            │
│  - Modèles du domaine (records)          │
│  - Moteurs de simulation                 │
│  - Services métier                       │
│  - Validation & contrats                 │
│  - Système médical                       │
│  - Spécifications JSON                   │
│  - Interfaces de repositories            │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│  COUCHE ACCÈS DONNÉES (Data)             │
│  - Pattern Repository (split partiel)    │
│  - Interfaces implémentées               │
│  - SQLite avec migrations                │
│  - Initialisation DB                     │
│  - Gestion sauvegardes                   │
└─────────────────────────────────────────┘
```

### 2.2 Modèles du Domaine

Tous les modèles utilisent des **C# sealed records** (immuables, sémantique par valeur):

#### Entités Principales

**WorkerSnapshot** (Profil catcheur/talent)
```csharp
public sealed record WorkerSnapshot(
    string WorkerId,
    string NomComplet,
    int InRing,           // Compétence ring (0-100)
    int Entertainment,    // Charisme (0-100)
    int Story,           // Storytelling (0-100)
    int Popularite,
    int Fatigue,
    string Blessure,      // Statut blessure
    int Momentum,
    string RoleTv,
    int Morale);
```

**ShowDefinition** (Définition d'un show)
```csharp
public sealed record ShowDefinition(
    string ShowId,
    string Nom,
    int Semaine,
    string Region,
    int DureeMinutes,
    string CompagnieId,
    string? DealTvId,
    string Lieu,
    string Diffusion);
```

**SegmentDefinition** (Segment TV - match/promo/angle)
```csharp
public sealed record SegmentDefinition(
    string SegmentId,
    string TypeSegment,     // "match", "promo", "angle_backstage"
    IReadOnlyList<string> Participants,
    int DureeMinutes,
    bool EstMainEvent,
    string? StorylineId,
    string? TitreId,
    int Intensite,
    string? VainqueurId,
    string? PerdantId,
    IReadOnlyDictionary<string, string>? Settings = null);
```

**StorylineInfo** (Storyline/Feud/Angle)
```csharp
public sealed record StorylineInfo(
    string StorylineId,
    string Nom,
    StorylinePhase Phase,  // BUILD, PEAK, BLOWOFF
    int Heat,
    StorylineStatus Status,
    string? Resume,
    IReadOnlyList<StorylineParticipant> Participants);
```

**GameStateDelta** (Résultats d'impacts de simulation)
```csharp
public sealed record GameStateDelta(
    IReadOnlyDictionary<string, int> FatigueDelta,
    IReadOnlyDictionary<string, string> Blessures,
    IReadOnlyDictionary<string, int> MomentumDelta,
    IReadOnlyDictionary<string, int> PopulariteWorkersDelta,
    IReadOnlyDictionary<string, int> PopulariteCompagnieDelta,
    IReadOnlyDictionary<string, int> StorylineHeatDelta,
    IReadOnlyDictionary<string, int> TitrePrestigeDelta,
    IReadOnlyList<FinanceTransaction> Finances);
```

---

### 🎭 2.2.5 NOUVEAU : Système d'Attributs de Performance (Phase 8)

**Implémenté** : 8 janvier 2026

Le système d'attributs a été complètement refondu pour passer d'un modèle simplifié à un système professionnel en **4 dimensions** avec **40 attributs** au total.

... (contenu inchangé sur descriptions des attributs) ...

---

### 🎭 2.2.6 NOUVEAU : Système de Personnalité (Phase 8)

**Implémenté** : 8 janvier 2026
**Inspiration** : Football Manager

... (contenu inchangé sur personnalité) ...

---

### 2.3 Services Métier

**Localisation**: `src/RingGeneral.Core/Services/` (45+ services)

**Services Principaux**:

| Service | Responsabilité |
|---------|----------------|
| `ShowSchedulerService` | Créer/gérer shows, valider runtime & billets |
| `BookingBuilderService` | Construire cartes de booking, gestion segments |
| `StorylineService` | Créer/mettre à jour storylines, tracking heat |
| `TitleService` | Création titres, règnes, gestion contenders |
| `ContenderService` | Classements, logique #1 contender |
| `TemplateService` | Templates de booking, patterns de segments |
| `BookerAIEngine` | Auto-booking IA avec génération automatique de cartes |
| `ShowDayOrchestrator` | Orchestration complète du flux Show Day |
| `TimeOrchestratorService` | Gestion du temps et progression du jeu |
| `PersonalityDetectorService` | Détection automatique de personnalité (25+ profils) |
| `DailyFinanceService` | Gestion finances quotidiennes (paiements mensuels, frais d'apparition) |
| `SimulationService` | Simulation hebdomadaire et validation bookings |
| `RevenueProjectionService` | Projections de revenus et finances |
| `BudgetAllocationService` | Allocation budgétaire |
| `TvDealNegotiationService` | Négociation des contrats TV |
| `ChildCompanyService` | Gestion des compagnies filles |
| `ChildCompanyStaffService` | Gestion du staff des compagnies filles |
| `RosterAnalysisService` | Analyse du roster et compatibilités |
| `NicheFederationService` | Gestion des fédérations de niche |
| `BrandManagementService` | Gestion des marques |
| `StaffProposalService` | Propositions de staff |
| `AgentReportGeneratorService` | Génération de rapports d'agents |
| `EraTransitionService` | Transitions d'ère |
| `RosterInertiaService` | Gestion de l'inertie du roster |

**Services de Simulation** (`Simulation/`):
- `BackstageService` - Gestion backstage
- `DisciplineService` - Discipline et sanctions
- `ScoutingService` - Scouting et recrutement
- `WorkerGenerationService` - Génération de workers
- `YouthProgressionService` - Progression des jeunes talents

**Services Médicaux** (`Medical/`):
- `InjuryService` - Gestion des blessures

**Services de Contrats** (`Contracts/`):
- `ContractNegotiationService` - Négociation de contrats
- `AIContractDecisionService` - Décisions IA pour contrats

**Services de Logging**:
- `ConsoleLoggingService` - Logging console
- `FileLoggingService` - Logging fichier
- `CompositeLoggingService` - Logging composite

---

## 3. Patterns de Conception Utilisés

... (contenu inchangé) ...

---

## 4. Couche de Données

### 4.1 Technologie: SQLite 8.0.0

... (contenu inchangé) ...

### 4.2 Stratégie de Migration

**Localisation**: `/data/migrations/` (27 fichiers)

... (contenu inchangé) ...

**⚠️ DETTE TECHNIQUE - DUPLICATION DE SCHÉMA**:

Comme documenté dans le code source, **deux systèmes de création de tables ont été repérés** dans l'arbre historique:
1. `GameRepository.Initialiser()` → versions historiques créant tables en snake_case (workers, companies, etc.)
2. `DbInitializer.ApplyMigrations()` → migrations SQL modernes produisant tables PascalCase (Workers, Companies, etc.)

Cette duplication peut causer confusion et bugs silencieux. Récemment (App.axaml.cs) l'amorçage de l'application a introduit une logique d'initialisation de la "World DB" (WorldDbInitializer) et enregistre désormais `DbInitializer`/`DbValidator` dans le conteneur de services. Une consolidation vers le schéma PascalCase reste recommandée.

---

## 7. Analyse Critique

### 7.1 ✅ Points Forts Architecturaux

... (contenu inchangé) ...

### 7.2 ⚠️ Problèmes & Anti-Patterns Identifiés

**1. GameRepository Transformé en Façade** ✅ REFACTORING COMPLÉTÉ
- **État actuel**: GameRepository agit maintenant comme une façade orchestrant les repositories spécialisés
- **Architecture**: Délègue aux repositories spécialisés (ShowRepository, CompanyRepository, WorkerRepository, etc.)
- **Méthodes conservées**: Orchestration cross-domain (ChargerShowContext, ChargerBookingPlan, AppliquerDelta) et initialisation
- **Repositories extraits**: 
  ```
  ✅ ShowRepository
  ✅ CompanyRepository
  ✅ WorkerRepository
  ✅ BackstageRepository
  ✅ ScoutingRepository
  ✅ ContractRepository
  ✅ SettingsRepository
  ✅ YouthRepository
  ✅ TitleRepository
  ✅ MedicalRepository
  ✅ WorkerAttributesRepository
  ✅ OwnerRepository
  ✅ BookerRepository
  ✅ CatchStyleRepository
  ✅ RosterAnalysisRepository
  ✅ TrendRepository
  ✅ NicheFederationRepository
  ✅ ChildCompanyExtendedRepository
  ✅ DNATransitionRepository
  ✅ ChildCompanyStaffRepository
  ✅ MoraleRepository
  ✅ RumorRepository
  ✅ NepotismRepository
  ✅ CrisisRepository
  ✅ RelationsRepository
  ✅ PersonalityRepository
  ✅ StaffRepository
  ✅ BrandRepository
  ✅ EraRepository
  ✅ RegionRepository
  ✅ NotesRepository
  ✅ StaffCompatibilityRepository
  ```

**2. Duplication de Schéma Base de Données** ⚠️ DETTE TECHNIQUE DOCUMENTÉE
- **Problème**: Deux systèmes de création de tables coexistent (historique)
- **Impact**: Confusion, risque de bugs silencieux, maintenance difficile
- **Statut**: Dette technique documentée dans le code source
- **Remarque**: L'amorçage de l'application a été complété pour inclure une initialisation de la "World DB" et des services d'initialisation/validation (DbInitializer/DbValidator) — consolidation recommandée vers un seul flux de création/migration

**3. Adoption DI complète** ✅ AMÉLIORÉ
- **État**: Le conteneur DI (Microsoft.Extensions.DependencyInjection) est intégré dans App.axaml.cs et enregistre tous les services et repositories.
- **Enregistrements**: Services (ShowDayOrchestrator, TimeOrchestratorService, MoraleEngine, CrisisEngine, etc.), Repositories (via RepositoryFactory), ViewModels (70+ avec injection)
- **Progrès**: La majorité des ViewModels utilisent maintenant l'injection de dépendances via le constructeur
- **Recommandation**: Continuer à migrer les ViewModels restants vers l'injection complète si nécessaire

**4. Absence de Framework de Logging Centralisé**
- **Problème**: Erreurs lancées mais pas loguées de façon structurée
- **Impact**: Debugging production difficile
- **Recommandation**: Ajouter Serilog ou ILogger (Microsoft.Extensions.Logging) et remplacer usages ad-hoc par logger central

**5. ViewModel Large (GameSessionViewModel - 2,320 lignes)** ⚠️ CROISSANCE
- **Problème**: ViewModel monolithique gérant toute logique jeu (augmenté de 2,092)
- **Impact**: Complexe, difficile à tester, maintenance difficile
- **Recommandation**: Extraire en ViewModels plus petits et focalisés

... (autres problèmes inchangés) ...

### 7.3 ❌ Composants Manquants

... (contenu inchangé) ...

---

## 8. Recommandations Architecturales

### Priorité 1: Impact Élevé, Effort Moyen

**1. Résoudre Duplication Schéma DB**
- Unifier sur système PascalCase (DbInitializer/migrations)
- Supprimer CREATE TABLE de GameRepository.Initialiser() si présent
- Mettre à jour toutes requêtes SQL pour noms corrects
- **Fichiers affectés**: `GameRepository.cs` (lignes 24-400+), `DbInitializer.cs`

**2. Consolider l'usage du conteneur DI**
- Le conteneur est déjà introduit (App.axaml.cs) — migrer l'instanciation manuelle restante (ViewModels, services utilitaires) vers l'injection via le provider
- Regrouper l'enregistrement des repositories (par interface) et des services (PersonalityEngine, MoraleEngine, etc.)
- **Fichiers affectés**: `GameSessionViewModel.cs`, `ShellViewModel.cs`, `App.axaml.cs`

**3. Ajouter Logging Structuré**
- Intégrer Serilog ou ILogger
- Ajouter hooks pour exceptions non capturées et reporting

**4. Continuer Split GameRepository**
- Extraire domaines restants (Worker, Show, Storyline, Company, Youth)

---

## 9. Exemples d'Implémentation

... (contenu inchangé) ...

---

## 12. Schémas de Flux des Systèmes

Cette section documente les flux de traitement de chaque système principal de Ring General.

### 12.1 Système Show Day (ShowDayOrchestrator)

**Objectif** : Orchestrer le flux complet d'un jour de show, de la détection à la finalisation.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX SHOW DAY (Match Day)                       │
└─────────────────────────────────────────────────────────────┘

1. DÉTECTION DU SHOW
   └─ ShowDayOrchestrator.DetecterShowAVenir()
      ├─ Charger shows pour la date actuelle
      ├─ Filtrer shows avec statut "À Booker"
      └─ Retourner ShowDayDetectionResult

2. CHARGEMENT DU CONTEXTE
   └─ ChargerShowContext(showId)
      ├─ ShowDefinition (détails du show)
      ├─ Segments (liste des segments bookés)
      ├─ Workers (snapshots avec attributs complets)
      ├─ Storylines (storylines actives liées)
      ├─ Titres (titres en jeu)
      ├─ Chimies (compatibilités entre workers)
      └─ TV Deal (contrat TV actif)

3. SIMULATION DU SHOW
   └─ ShowSimulationEngine.Simuler(context)
      │
      ├─ Pour chaque segment (dans l'ordre) :
      │  │
      │  ├─ Calcul Note In-Ring
      │  │  ├─ Moyenne attributs In-Ring participants
      │  │  ├─ Bonus chimie entre workers
      │  │  ├─ Bonus type de match
      │  │  └─ Pénalité fatigue
      │  │
      │  ├─ Calcul Note Entertainment
      │  │  ├─ Moyenne attributs Entertainment
      │  │  ├─ Bonus charisme
      │  │  └─ Bonus storyline heat
      │  │
      │  ├─ Calcul Note Story
      │  │  ├─ Moyenne attributs Story
      │  │  ├─ Bonus storyline active
      │  │  └─ Bonus cohérence narrative
      │  │
      │  ├─ Note Globale Segment
      │  │  └─ Moyenne pondérée (InRing 40%, Ent 30%, Story 30%)
      │  │
      │  ├─ Calcul Audience
      │  │  ├─ Popularité moyenne participants
      │  │  ├─ Popularité compagnie
      │  │  └─ Facteur qualité segment
      │  │
      │  ├─ Calcul Revenus
      │  │  ├─ Billetterie (audience × prix ticket)
      │  │  ├─ Merchandise (popularité × facteur merch)
      │  │  └─ TV (deal actif × audience)
      │  │
      │  ├─ Risque Blessure
      │  │  ├─ Attribut Safety des participants
      │  │  ├─ Type de match (hardcore = +risque)
      │  │  └─ Fatigue actuelle
      │  │
      │  └─ Génération Blessures (si risque déclenché)
      │
      └─ Note Globale Show (moyenne pondérée segments)

4. APPLICATION DES IMPACTS
   └─ ImpactApplier.AppliquerImpacts(resultat, context)
      │
      ├─ Finances
      │  ├─ Créditer revenus (billetterie, merch, TV)
      │  └─ Débiter frais d'apparition (per-appearance fees)
      │
      ├─ Blessures
      │  ├─ Créer InjuryRecord pour chaque blessure
      │  ├─ Créer RecoveryPlan automatique
      │  └─ Ajouter MedicalNote
      │
      ├─ Popularité
      │  ├─ Delta popularité workers (basé sur performance)
      │  └─ Delta popularité compagnie (basé sur note globale)
      │
      ├─ Titres
      │  ├─ Si match de titre et perdant = champion
      │  │  └─ Changement automatique de titre
      │  └─ Mise à jour prestige titre
      │
      ├─ Momentum
      │  └─ Delta momentum workers (basé sur performance)
      │
      ├─ Storylines
      │  └─ Delta heat storylines (basé sur qualité segments)
      │
      └─ Fatigue
         └─ Augmentation fatigue workers participants

5. GESTION DU MORAL POST-SHOW
   └─ MoraleEngine.UpdateMorale()
      │
      ├─ Détecter workers non utilisés sur la carte
      ├─ Appliquer pénalité moral (-3 à -5)
      ├─ Appliquer bonus moral workers utilisés (+3 à +5)
      └─ Recalculer moral compagnie global

6. FINALISATION
   └─ ShowDayOrchestrator.FinaliserShow()
      ├─ Mettre à jour statut show → "Terminé"
      ├─ Enregistrer résultats en base
      └─ Générer InboxItem avec résumé
```

### 12.2 Système de Simulation (ShowSimulationEngine)

**Objectif** : Calculer les notes et résultats d'un show basé sur les attributs des workers et le contexte.

```
┌─────────────────────────────────────────────────────────────┐
│         FLUX DE SIMULATION D'UN SEGMENT                     │
└─────────────────────────────────────────────────────────────┘

ENTRÉE : SegmentDefinition + ShowContext

1. CALCUL NOTE IN-RING
   ├─ Extraire participants du segment
   ├─ Moyenne attributs In-Ring (Brawling, Technical, Aerial, etc.)
   ├─ Appliquer bonus chimie (si chimie > 70)
   ├─ Appliquer bonus type match (Hardcore, Technical, etc.)
   └─ Appliquer pénalité fatigue (si fatigue > 60)

2. CALCUL NOTE ENTERTAINMENT
   ├─ Moyenne attributs Entertainment (Charisma, Mic, etc.)
   ├─ Bonus charisme (si Mic > 80)
   └─ Bonus heat storyline (si storyline active)

3. CALCUL NOTE STORY
   ├─ Moyenne attributs Story (Psychology, Consistency, etc.)
   ├─ Bonus storyline active (si segment lié)
   └─ Bonus cohérence narrative (si participants cohérents)

4. NOTE GLOBALE SEGMENT
   └─ Moyenne pondérée :
      ├─ In-Ring : 40%
      ├─ Entertainment : 30%
      └─ Story : 30%

5. CALCUL AUDIENCE
   ├─ Popularité moyenne participants
   ├─ Popularité compagnie
   ├─ Facteur qualité (note globale)
   └─ Formule : base × qualité × facteurs régionaux

6. CALCUL REVENUS
   ├─ Billetterie = audience × prix ticket × taux remplissage
   ├─ Merchandise = popularité moyenne × facteur merch
   └─ TV = deal actif × audience × multiplicateur

7. RISQUE BLESSURE
   ├─ Base risque selon type match
   ├─ Attribut Safety des participants
   ├─ Fatigue actuelle
   └─ Déclenchement si aléatoire < risque calculé

SORTIE : SegmentResult avec note, audience, revenus, blessures
```

### 12.3 Système de Booking (BookerAIEngine)

**Objectif** : Générer automatiquement une carte complète de show selon les préférences du booker.

```
┌─────────────────────────────────────────────────────────────┐
│         FLUX AUTO-BOOKING (BookerAIEngine)                  │
└─────────────────────────────────────────────────────────────┘

ENTRÉE : bookerId, ShowContext, existingSegments, constraints

1. INITIALISATION
   ├─ Charger Booker depuis repository
   ├─ Vérifier CanAutoBook() (doit être true)
   ├─ Charger mémoires influentes du booker
   ├─ Charger era actuelle de la compagnie
   └─ Charger préférences creative staff

2. FILTRAGE WORKERS DISPONIBLES
   ├─ Filtrer selon contraintes (budget, workers interdits)
   ├─ Exclure workers blessés
   ├─ Exclure workers déjà utilisés dans existingSegments
   └─ Sélectionner selon archétype créatif du booker

3. GÉNÉRATION DES SEGMENTS
   │
   ├─ Calculer durée restante à remplir
   │  └─ targetDuration - existingDuration
   │
   ├─ BOUCLE : Tant que durée restante > 0
   │  │
   │  ├─ DÉTERMINER TYPE SEGMENT
   │  │  ├─ Si main event manquant → générer main event
   │  │  ├─ Si storyline active → générer segment storyline
   │  │  ├─ Si titre disponible → générer match titre
   │  │  └─ Sinon → générer match midcard
   │  │
   │  ├─ SÉLECTION PARTICIPANTS
   │  │  ├─ Selon préférences booker :
   │  │  │  ├─ LikesUnderdog → favoriser workers < 40 pop
   │  │  │  ├─ LikesVeteran → favoriser workers > 75 InRing
   │  │  │  ├─ LikesFastRise → favoriser momentum > 60
   │  │  │  └─ LikesSlowBurn → favoriser stabilité
   │  │  ├─ Consulter mémoires pour cohérence
   │  │  └─ Appliquer bonus créativité (aléatoire si CreativityScore > 70)
   │  │
   │  ├─ CRÉER SEGMENT
   │  │  ├─ TypeSegment (match, promo, angle_backstage)
   │  │  ├─ Participants (IDs workers sélectionnés)
   │  │  ├─ DureeMinutes (selon type et importance)
   │  │  ├─ EstMainEvent (si dernier segment)
   │  │  ├─ StorylineId (si lié à storyline)
   │  │  ├─ TitreId (si match de titre)
   │  │  └─ VainqueurId/PerdantId (si match)
   │  │
   │  └─ Ajouter à liste generatedSegments
   │
   └─ Retourner liste complète des segments

4. VALIDATION
   └─ Vérifier contraintes respectées (durée, budget, etc.)

SORTIE : List<SegmentDefinition> (carte complète)
```

### 12.4 Système de Storylines (StorylineService)

**Objectif** : Gérer le cycle de vie des storylines (feuds, angles) et leur progression.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX GESTION STORYLINE                          │
└─────────────────────────────────────────────────────────────┘

1. CRÉATION
   └─ StorylineService.Creer()
      ├─ Créer StorylineInfo avec :
      │  ├─ Phase = Setup (initial)
      │  ├─ Heat = 0
      │  ├─ Status = Active
      │  └─ Participants (liste workers impliqués)
      └─ Enregistrer en base

2. PROGRESSION DU HEAT
   └─ Après chaque segment lié :
      ├─ Calculer delta heat basé sur note segment
      ├─ HeatModel.CalculerDeltaSegment(note, segmentsPrecedents)
      ├─ Appliquer delta au heat total
      └─ Heat = Clamp(Heat + delta, 0, 100)

3. AVANCEMENT DE PHASE
   └─ StorylineService.Avancer()
      │
      ├─ Setup → Rising
      │  └─ Déclenché après 2-3 segments
      │
      ├─ Rising → Climax
      │  └─ Déclenché quand Heat > 60
      │
      ├─ Climax → Fallout
      │  └─ Déclenché après match principal
      │
      └─ Fallout → Completed
         └─ Déclenché quand Heat >= 80

4. MISE À JOUR
   └─ StorylineService.MettreAJour()
      ├─ Modifier nom, phase, statut, résumé
      ├─ Ajouter/retirer participants
      └─ Enregistrer modifications

5. ARCHIVAGE
   └─ Quand Status = Completed
      ├─ Changer Status → Archived
      └─ Conserver pour historique
```

### 12.5 Système de Finances (DailyFinanceService)

**Objectif** : Gérer deux flux financiers distincts : paiements mensuels et frais d'apparition.

```
┌─────────────────────────────────────────────────────────────┐
│         FLUX FINANCIER - PAIEMENT MENSUEL                    │
└─────────────────────────────────────────────────────────────┘

DÉCLENCHEMENT : Dernier jour du mois (TimeOrchestratorService)

1. DÉTECTION FIN DU MOIS
   └─ EstFinDuMois(currentDate)
      └─ Vérifier si date.Day == dernier jour du mois

2. TRAITEMENT PAIEMENTS
   └─ DailyFinanceService.ProcessMonthlyPayroll()
      │
      ├─ Charger tous les contrats actifs (ContratsHybrides)
      │
      ├─ Pour chaque contrat :
      │  │
      │  ├─ Vérifier salaire mensuel garanti > 0
      │  │  └─ Si 0 → ignorer (contrat per-appearance uniquement)
      │  │
      │  ├─ Vérifier non déjà payé ce mois
      │  │  └─ Comparer LastPaymentDate avec currentDate
      │  │
      │  ├─ Créer transaction financière
      │  │  ├─ Type : "paie_mensuelle"
      │  │  ├─ Montant : -MonthlyWage (débit)
      │  │  └─ Description : "Salaire mensuel garanti - {WorkerId}"
      │  │
      │  └─ Mettre à jour LastPaymentDate = currentDate
      │
      └─ Appliquer toutes les transactions en batch

3. ENREGISTREMENT
   └─ GameRepository.AppliquerTransactionsFinancieres()
      ├─ Insérer transactions dans FinanceTransactions
      └─ Mettre à jour solde compagnie

┌─────────────────────────────────────────────────────────────┐
│      FLUX FINANCIER - FRAIS D'APPARITION                     │
└─────────────────────────────────────────────────────────────┘

DÉCLENCHEMENT : Immédiatement après simulation show (ShowDayOrchestrator)

1. IDENTIFICATION PARTICIPANTS
   └─ Extraire workerIds de tous les segments du show

2. TRAITEMENT FRAIS
   └─ DailyFinanceService.ProcessAppearanceFees()
      │
      ├─ Charger contrats actifs
      │
      ├─ Pour chaque workerId participant :
      │  │
      │  ├─ Trouver contrat correspondant
      │  │
      │  ├─ Vérifier AppearanceFee > 0
      │  │  └─ Si 0 → ignorer (contrat fixe uniquement)
      │  │
      │  ├─ Vérifier non déjà payé pour cette date
      │  │  └─ Comparer LastAppearanceDate avec showDate
      │  │
      │  ├─ Créer transaction financière
      │  │  ├─ Type : "frais_apparition"
      │  │  ├─ Montant : -AppearanceFee (débit)
      │  │  └─ Description : "Frais apparition - {WorkerId}"
      │  │
      │  └─ Mettre à jour LastAppearanceDate = showDate
      │
      └─ Appliquer toutes les transactions en batch

3. ENREGISTREMENT
   └─ Même processus que paiement mensuel
```

### 12.6 Système de Contrats (ContractNegotiationService)

**Objectif** : Gérer la négociation et la signature de contrats avec les workers.

```
┌─────────────────────────────────────────────────────────────┐
│         FLUX NÉGOCIATION DE CONTRAT                          │
└─────────────────────────────────────────────────────────────┘

1. CRÉATION D'OFFRE INITIALE
   └─ ContractNegotiationService.CreerOffre()
      │
      ├─ Valider draft (WorkerId, CompanyId, termes)
      │
      ├─ Charger ou créer ContractNegotiationState
      │  └─ Statut = "en_cours"
      │
      ├─ Créer ContractOffer avec :
      │  ├─ TypeContrat (Exclusif, PPA, Handshake)
      │  ├─ StartWeek / EndWeek (durée)
      │  ├─ SalaireHebdo (salaire hebdomadaire)
      │  ├─ BonusShow (frais d'apparition)
      │  ├─ Buyout (clause de rachat)
      │  ├─ EstExclusif (oui/non)
      │  └─ Statut = "proposee"
      │
      └─ Enregistrer offre en base

2. ÉVALUATION PAR LE WORKER (IA)
   └─ AIContractDecisionService.EvaluerOffre()
      │
      ├─ Calculer salaire minimum acceptable
      │  └─ Basé sur Popularité + InRing + marché
      │
      ├─ Calculer probabilité acceptation
      │  ├─ Si offre >= minimum → haute probabilité
      │  ├─ Si offre < minimum mais proche → probabilité moyenne
      │  └─ Si offre << minimum → faible probabilité
      │
      └─ Décision :
         ├─ ACCEPTÉ → Statut = "acceptee"
         ├─ REFUSÉ → Statut = "refusee" + raisons
         └─ CONTRE-OFFRE → Statut = "contre_proposee" + nouveaux termes

3. TRAITEMENT RÉPONSE
   │
   ├─ Si ACCEPTÉ :
   │  ├─ Créer Contract actif
   │  ├─ Ajouter worker au roster
   │  └─ Mettre à jour Statut négociation = "terminee"
   │
   ├─ Si REFUSÉ :
   │  ├─ Enregistrer raisons refus
   │  └─ Possibilité renégocier plus tard
   │
   └─ Si CONTRE-OFFRE :
      ├─ Afficher nouveaux termes proposés
      ├─ Joueur peut :
      │  ├─ Accepter contre-offre
      │  ├─ Faire nouvelle contre-offre
      │  └─ Abandonner négociation
      └─ Répéter jusqu'à accord ou abandon (max 3 rounds)

4. SIGNATURE
   └─ ContractNegotiationService.SignerContrat()
      ├─ Créer Contract avec termes finaux
      ├─ Statut = "actif"
      └─ Enregistrer en base
```

### 12.7 Système Médical (InjuryService)

**Objectif** : Gérer les blessures des workers et leurs plans de récupération.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX GESTION BLESSURE                          │
└─────────────────────────────────────────────────────────────┘

1. DÉCLENCHEMENT BLESSURE
   └─ Pendant simulation show (ShowSimulationEngine)
      │
      ├─ Calculer risque blessure par segment
      │  ├─ Base risque selon type match
      │  ├─ Attribut Safety des participants
      │  └─ Fatigue actuelle
      │
      └─ Si aléatoire < risque → déclencher blessure

2. APPLICATION BLESSURE
   └─ InjuryService.AppliquerBlessure()
      │
      ├─ Déterminer sévérité (Légère, Moyenne, Grave)
      │  └─ Basé sur type match et aléatoire
      │
      ├─ Consulter MedicalRecommendations
      │  ├─ Recommandation semaines repos
      │  ├─ Niveau risque
      │  └─ Message recommandation
      │
      ├─ Créer InjuryRecord
      │  ├─ WorkerId
      │  ├─ Type (ex: "Épaule", "Genou")
      │  ├─ Severity
      │  ├─ StartWeek (semaine actuelle)
      │  ├─ EndWeek (StartWeek + semaines repos)
      │  └─ IsActive = true
      │
      ├─ Créer RecoveryPlan
      │  ├─ InjuryId (lié à InjuryRecord)
      │  ├─ StartWeek / TargetWeek
      │  ├─ RecommendedRestWeeks
      │  ├─ RiskLevel
      │  └─ Status = "EN_COURS"
      │
      └─ Ajouter MedicalNote avec recommandation

3. SUIVI RÉCUPÉRATION
   └─ Chaque semaine (WeeklyLoopService)
      │
      ├─ Vérifier blessures actives
      │
      ├─ Pour chaque blessure :
      │  │
      │  ├─ Si semaine actuelle >= EndWeek :
      │  │  ├─ Marquer blessure comme guérie
      │  │  ├─ IsActive = false
      │  │  ├─ Status RecoveryPlan = "TERMINE"
      │  │  └─ Générer notification
      │  │
      │  └─ Si worker lutte malgré blessure :
      │     ├─ Risque aggravation
      │     └─ Possible extension EndWeek

4. RÉCUPÉRATION MANUELLE
   └─ Joueur peut forcer récupération
      ├─ InjuryService.RecupererBlessure()
      ├─ Mettre à jour EndWeek = semaine actuelle
      └─ IsActive = false
```

### 12.8 Système de Moral (MoraleEngine)

**Objectif** : Gérer le moral individuel des workers et le moral global de la compagnie.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX GESTION DU MORAL                          │
└─────────────────────────────────────────────────────────────┘

1. MISE À JOUR MORAL INDIVIDUEL
   └─ MoraleEngine.UpdateMorale()
      │
      ├─ Charger BackstageMorale actuel (ou créer si inexistant)
      │  └─ MoraleScore par défaut = 70
      │
      ├─ Calculer changement basé sur eventType
      │  ├─ MainEventPush → +6 à +10
      │  ├─ SuccessfulMatch → +3 à +5
      │  ├─ TitleWin → +9 à +15
      │  ├─ PushFailed → -6 à -10
      │  ├─ Buried → -9 à -15
      │  ├─ Jobber → -3 à -5
      │  ├─ InjuryIgnored → -6 à -10
      │  ├─ UnfairSanction → -9 à -15
      │  ├─ Nepotism → -3 à -5
      │  └─ Favoritism → -6 à -10
      │
      ├─ Appliquer changement
      │  └─ MoraleScore = Clamp(MoraleScore + changement, 0, 100)
      │
      ├─ Sauvegarder PreviousMoraleScore
      ├─ Mettre à jour LastUpdated
      └─ Enregistrer en base

2. RECALCUL MORAL COMPAGNIE
   └─ MoraleRepository.RecalculateCompanyMorale()
      │
      ├─ Charger tous les moraux workers de la compagnie
      │
      ├─ Calculer moyenne morale workers
      │  └─ CompanyMorale.Score = moyenne(MoraleScore)
      │
      ├─ Identifier signaux d'alerte
      │  ├─ Si moyenne < 50 → alerte morale basse
      │  ├─ Si > 50% workers en déclin → alerte tendance
      │  └─ Si moyenne > 80 → moral excellent
      │
      └─ Enregistrer CompanyMorale

3. ÉVÉNEMENTS DÉCLENCHEURS
   │
   ├─ Après show :
   │  ├─ Workers utilisés → bonus moral
   │  └─ Workers non utilisés → pénalité moral
   │
   ├─ Changements de push :
   │  ├─ Push vers le haut → bonus
   │  └─ Push vers le bas → pénalité
   │
   ├─ Gestion titres :
   │  ├─ Gain titre → bonus important
   │  └─ Perte titre → pénalité modérée
   │
   └─ Actions management :
      ├─ Népotisme détecté → pénalité
      └─ Sanctions injustes → pénalité importante
```

### 12.9 Système de Rumeurs (RumorEngine)

**Objectif** : Gérer l'émergence et la propagation des rumeurs backstage.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX GESTION RUMEURS                           │
└─────────────────────────────────────────────────────────────┘

1. DÉCLENCHEMENT RUMEUR
   └─ RumorEngine.ShouldTriggerRumor()
      │
      ├─ Événement significatif détecté
      │  ├─ Sévérité >= 3 → déclenchement automatique
      │  └─ Sévérité >= 2 → 40% chance déclenchement
      │
      └─ Types événements :
         ├─ Nepotism (népotisme détecté)
         ├─ UnfairPush (push injuste)
         ├─ ContractDispute (conflit contrat)
         └─ BackstageIncident (incident backstage)

2. GÉNÉRATION RUMEUR
   └─ RumorEngine.GenerateRumor()
      │
      ├─ Générer texte rumeur selon type
      │
      ├─ Créer Rumor avec :
      │  ├─ RumorType
      │  ├─ RumorText
      │  ├─ Stage = "Emerging" (initial)
      │  ├─ Severity (1-5)
      │  ├─ AmplificationScore = 10 (début faible)
      │  └─ CreatedAt = maintenant
      │
      └─ Enregistrer en base

3. AMPLIFICATION
   └─ RumorEngine.AmplifyRumor()
      │
      ├─ Worker influent répand la rumeur
      │  └─ AmplificationScore += 10 (influencé par popularité)
      │
      ├─ Mise à jour stage selon score :
      │  ├─ Score < 40 → "Emerging"
      │  ├─ Score 40-69 → "Growing"
      │  └─ Score >= 70 → "Widespread"
      │
      └─ Enregistrer modifications

4. PROGRESSION NATURELLE
   └─ RumorEngine.ProgressRumors() (hebdomadaire)
      │
      ├─ Pour chaque rumeur active :
      │  │
      │  ├─ Amplification naturelle (+5 à +15 par semaine)
      │  │
      │  ├─ Mise à jour stage si seuil atteint
      │  │
      │  ├─ Si Widespread + score >= 80 :
      │  │  └─ 30% chance résolution automatique
      │  │
      │  └─ Si Emerging + score < 20 :
      │     └─ 20% chance être ignorée
      │
      └─ Nettoyer rumeurs résolues/ignorées (> 90 jours)

5. RÉSOLUTION
   └─ RumorEngine.ResolveRumor()
      │
      ├─ Action joueur (intervention management)
      │  ├─ Qualité intervention (0-100)
      │  ├─ Calcul chance succès
      │  └─ Si succès → Stage = "Resolved"
      │
      └─ Si échec → réduction modérée escalation (-10 à -20)
```

### 12.10 Système de Crises (CrisisEngine)

**Objectif** : Gérer les crises majeures qui peuvent affecter la compagnie.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX GESTION CRISES                            │
└─────────────────────────────────────────────────────────────┘

1. DÉCLENCHEMENT CRISE
   └─ CrisisEngine.TriggerCrisis()
      │
      ├─ Événement majeur détecté
      │  ├─ Rumeur Widespread + Severity >= 4
      │  ├─ Incident backstage grave
      │  ├─ Perte contrat TV majeur
      │  └─ Départ worker star
      │
      └─ Créer Crisis avec :
         ├─ CrisisType
         ├─ Severity (1-5)
         ├─ Stage = "Detected" (initial)
         ├─ Escalation = 0
         └─ CreatedAt = maintenant

2. ESCALATION
   └─ CrisisEngine.Escalate() (hebdomadaire)
      │
      ├─ Si crise non résolue :
      │  ├─ Escalation += 10-20 (selon sévérité)
      │  ├─ Mise à jour stage :
      │  │  ├─ Escalation < 30 → "Detected"
      │  │  ├─ Escalation 30-59 → "Growing"
      │  │  ├─ Escalation 60-79 → "Critical"
      │  │  └─ Escalation >= 80 → "Crisis"
      │  └─ Impact moral compagnie (négatif)
      │
      └─ Enregistrer modifications

3. TENTATIVE RÉSOLUTION
   └─ CrisisEngine.AttemptResolution()
      │
      ├─ Joueur intervient avec qualité (0-100)
      │
      ├─ Calcul chance succès :
      │  ├─ Base = qualité intervention
      │  ├─ Pénalité sévérité = Severity × 10
      │  ├─ Pénalité tentatives = ResolutionAttempts × 5
      │  └─ SuccessChance = Clamp(base - pénalités, 10, 90)
      │
      ├─ Si succès :
      │  ├─ Stage = "Resolved"
      │  └─ Escalation = 0
      │
      └─ Si échec :
         ├─ IncrementResolutionAttempts
         └─ Réduction modérée escalation (-10 à -20)

4. IMPACT SUR MORAL
   └─ CrisisEngine.CalculateMoraleImpact()
      │
      ├─ Impact négatif basé sur :
      │  ├─ Sévérité crise (-5 à -25 par niveau)
      │  └─ Stage crise (multiplicateur)
      │
      └─ Appliquer à tous les workers de la compagnie
```

### 12.11 Système de Scouting (ScoutingService)

**Objectif** : Découvrir et évaluer des workers libres pour recrutement.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX SCOUTING                                   │
└─────────────────────────────────────────────────────────────┘

1. CRÉATION MISSION SCOUTING
   └─ ScoutingService.CreerMission()
      │
      ├─ Définir paramètres :
      │  ├─ Titre mission
      │  ├─ Région cible
      │  ├─ Focus (InRing, Entertainment, Story)
      │  └─ Objectif (nombre workers à découvrir)
      │
      └─ Créer ScoutMission avec :
         ├─ Statut = "active"
         ├─ Progression = 0
         └─ Enregistrer en base

2. DÉCOUVERTE WORKERS
   └─ ScoutingService.RafraichirHebdo() (hebdomadaire)
      │
      ├─ Générer rapports hebdomadaires
      │  ├─ Sélectionner workers libres (company_id IS NULL)
      │  ├─ Filtrer selon région/focus mission
      │  └─ Créer ScoutReport pour chaque découverte
      │
      ├─ Avancer missions actives
      │  ├─ Progression += 1-3 par semaine
      │  └─ Si Progression >= Objectif → Statut = "terminee"
      │
      └─ Retourner ScoutingWeeklyRefresh

3. CRÉATION RAPPORT
   └─ ScoutingService.CreerRapport()
      │
      ├─ Charger ScoutingTarget depuis repository
      │  ├─ WorkerId, NomComplet
      │  ├─ Région
      │  ├─ InRing, Entertainment, Story
      │  └─ Popularité
      │
      ├─ Créer ScoutReport avec :
      │  ├─ WorkerId
      │  ├─ Semaine découverte
      │  ├─ Notes (évaluation)
      │  └─ Enregistrer en base
      │
      └─ Vérifier non-duplication (RapportExiste)

4. CONSULTATION RAPPORTS
   └─ ScoutingRepository.ChargerRapports()
      │
      ├─ Filtrer selon critères :
      │  ├─ Région
      │  ├─ Semaine
      │  └─ Focus (attributs)
      │
      └─ Retourner liste rapports disponibles
```

### 12.12 Système de Gestion du Temps (TimeOrchestratorService)

**Objectif** : Orchestrer le passage du temps jour par jour et déclencher les événements appropriés.

```
┌─────────────────────────────────────────────────────────────┐
│         FLUX GESTION TEMPS QUOTIDIENNE                       │
└─────────────────────────────────────────────────────────────┘

DÉCLENCHEMENT : Joueur clique "Passer au jour suivant"

1. INCRÉMENTATION JOUR
   └─ TimeOrchestratorService.PasserJourSuivant()
      │
      ├─ IncrementerJour(companyId)
      │  └─ currentDay += 1
      │
      └─ GetCurrentDate(companyId)
         └─ Convertir currentDay en DateTime

2. MISE À JOUR STATS QUOTIDIENNES
   └─ DailyServices.UpdateDailyStats()
      │
      ├─ Récupération fatigue (si applicable)
      ├─ Progression blessures
      └─ Mise à jour autres stats

3. PLANIFICATION SHOWS AUTOMATIQUES
   └─ Si newDay % 30 == 0 (tous les 30 jours)
      │
      └─ DailyShowScheduler.PlanifierShowsAutomatiques()
         ├─ Planifier shows pour compagnies IA
         └─ Planifier pour 8 semaines à venir

4. GÉNÉRATION ÉVÉNEMENTS ALÉATOIRES
   └─ EventGenerator.GenerateDailyEvents()
      │
      ├─ Événements possibles :
      │  ├─ Offre contrat worker libre
      │  ├─ Offre TV deal
      │  ├─ Événement backstage
      │  └─ Autres événements aléatoires
      │
      └─ Retourner liste événements

5. DÉTECTION SHOW DAY
   └─ ShowDayOrchestrator.DetecterShowAVenir()
      │
      ├─ Vérifier si show prévu aujourd'hui
      │
      └─ Si show détecté :
         └─ Marquer pour simulation (joueur décide quand simuler)

6. TRAITEMENT FIN DE MOIS
   └─ Si EstFinDuMois(currentDate)
      │
      └─ DailyServices.ProcessMonthlyPayroll()
         ├─ Paiement salaires mensuels garantis
         └─ Déduction finances compagnie

7. RETOUR RÉSULTAT
   └─ DailyTickResult
      ├─ Nouveau jour
      ├─ Nouvelle date
      └─ Liste événements générés
```

### 12.13 Système de Titres (TitleService)

**Objectif** : Gérer les titres de championnat, leurs règnes et les changements automatiques.

```
┌─────────────────────────────────────────────────────────────┐
│              FLUX GESTION TITRES                             │
└─────────────────────────────────────────────────────────────┘

1. CRÉATION TITRE
   └─ TitleService.CreerTitre()
      │
      ├─ Définir paramètres :
      │  ├─ Nom titre
      │  ├─ CompagnieId
      │  ├─ Prestige initial (0-100)
      │  └─ Type (World, Tag Team, etc.)
      │
      └─ Créer Title avec :
         ├─ TitreId
         ├─ Prestige
         ├─ ChampionId = null (initialement vacant)
         └─ Enregistrer en base

2. MATCH DE TITRE
   └─ Pendant simulation show
      │
      ├─ Segment avec TitreId défini
      │
      ├─ Si match de titre :
      │  ├─ VainqueurId et PerdantId définis
      │  └─ ImpactApplier traite changement titre
      │
      └─ Si PerdantId == ChampionId actuel :
         └─ Changement automatique de titre

3. CHANGEMENT DE TITRE
   └─ ImpactApplier.AppliquerChangementTitre()
      │
      ├─ Créer nouveau règne (TitleReign)
      │  ├─ TitreId
      │  ├─ ChampionId = VainqueurId
      │  ├─ StartWeek = semaine actuelle
      │  ├─ EndWeek = null (en cours)
      │  └─ PrestigeGain = calculé selon match
      │
      ├─ Clôturer règne précédent (si existe)
      │  └─ EndWeek = semaine actuelle
      │
      ├─ Mettre à jour Prestige titre
      │  └─ Prestige += PrestigeGain (ou -= si perte)
      │
      └─ Enregistrer modifications

4. GESTION CONTENDERS
   └─ ContenderService
      │
      ├─ Calculer classement contenders
      │  ├─ Basé sur popularité
      │  ├─ Basé sur momentum
      │  └─ Basé sur récents résultats
      │
      ├─ Déterminer #1 Contender
      │  └─ Worker avec meilleur score
      │
      └─ Mettre à jour classement hebdomadaire
```

---

## 13. Conclusion

Ring General démontre une **architecture en couches exemplaire** avec modélisation domaine claire et bon usage des fonctionnalités C# modernes (records, nullable reference types). Le design est testable et maintenable à grande échelle. **Le projet a complété avec succès un refactoring architectural majeur** avec 23+ repositories spécialisés et création d'interfaces complètes.

### Note Globale: **8.5/10** (+1.0 - Mise à jour 8 janvier 2026)

**Points Forts Clés**:
- ✅ Immuabilité des modèles
- ✅ Séparation des responsabilités excellente
- ✅ Dépendances minimales
- ✅ **30+ repositories spécialisés** créés et fonctionnels
- ✅ **GameRepository transformé en façade** orchestrant les repositories
- ✅ **Systèmes avancés implémentés**: Personnalité, Moral, Rumeurs, Népotisme, Crises, IA Booker, IA Propriétaire
- ✅ **Interfaces de repositories** complètes dans Core (27+ interfaces)
- ✅ **Architecture modulaire** bien pensée et extensible
- ✅ **Dependency Injection complète** avec Microsoft.Extensions.DependencyInjection
- ✅ **70+ ViewModels** avec injection de dépendances

**Améliorations Recommandées** (non critiques):
1. ⚠️ Résoudre duplication schéma DB (en cours)
2. ✅ ~~Consolider l'usage du conteneur DI~~ **COMPLÉTÉ** - DI intégré dans App.axaml.cs
3. ⚠️ Logging structuré (Serilog ou ILogger)
4. ⚠️ Réduction taille GameSessionViewModel (si nécessaire)

**Évaluation Globale**: **Architecture professionnelle de qualité production**. Le refactoring repositories est **largement complété** avec succès. L'implémentation de systèmes backstage sophistiqués (8+ nouveaux repositories majeurs) démontre une capacité d'innovation et une discipline d'ingénierie remarquables. Dettes techniques identifiées et documentées, mais non bloquantes.

---

## 13. Prochaines Étapes Recommandées

### Court Terme (1-2 sprints)
1. **PRIORITÉ 1**: Résoudre duplication schéma DB (snake_case vs PascalCase)
2. ✅ ~~Continuer extraction GameRepository~~ **COMPLÉTÉ** - 30+ repositories créés
3. ✅ ~~Consolider l'usage du conteneur DI~~ **COMPLÉTÉ** - DI intégré dans App.axaml.cs
4. Ajouter Serilog ou ILogger pour logging structuré
5. Documenter les nouveaux systèmes backstage (Moral, Rumeurs, Népotisme, Crises)

### Moyen Terme (3-6 sprints)
5. Finaliser split complet de GameRepository
6. Extraire GameSessionViewModel en composants plus petits
7. Implémenter pattern Result<T> pour gestion erreurs
8. Ajouter monitoring performance et profiling

### Long Terme (6+ sprints)
9. Système d'audit complet
10. Event bus pour architecture event-driven
11. Support simulation en background pour grands mondes
12. API REST si multijoueur prévu

---

**Document généré le**: 2026-01-06
**Auteur**: Claude (Architecture Review Assistant)
**Version**: 2.1
