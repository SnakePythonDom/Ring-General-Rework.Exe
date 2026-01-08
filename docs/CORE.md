# RingGeneral.Core

Moteur de simulation de catch professionnel - 14557 lignes - .NET 8.0

## Architecture

```
RingGeneral.Core/
├── Models/          → Modèles de données (records)
├── Simulation/      → Moteurs de simulation
├── Services/        → Services métier
├── Contracts/       → Gestion des contrats
├── Medical/         → Système médical
├── Validation/      → Validateurs
├── Interfaces/      → Contrats d'abstraction
├── Import/          → Import de données Baki
└── Enums/          → Énumérations métier
```

## Composants Principaux

### Simulation

**ShowSimulationEngine** (`Simulation/ShowSimulationEngine.cs`)
- Simule un show complet (segments, matchs, promos)
- Calcule notes (0-100) basées sur InRing, Entertainment, Story
- Applique fatigue, blessures, momentum, popularité
- Gère chimie entre wrestlers, pacing, chaleur du public
- Retourne `ShowSimulationResult` avec rapport et delta d'état

**WorldSimScheduler** (`Simulation/WorldSimScheduler.cs`)
- Planifie niveau de détail (LOD) pour simulation monde
- LOD0 (Detail): compagnie joueur + top prestige
- LOD1 (Resume): simulation réduite
- LOD2 (Statistique): statistiques uniquement
- Ajuste selon budget ms/tick

**FinanceEngine** (`Simulation/FinanceEngine.cs`)
- Revenus: billetterie, merch, droits TV
- Calculs basés sur: capacité venue, taux remplissage, prix billet
- Dépenses: production, paie contrats (hebdo/mensuelle)
- Star power influence merchandising

**BackstageService** (`Simulation/BackstageService.cs`)
- Gère interactions backstage
- Moral des workers

**DisciplineService** (`Simulation/DisciplineService.cs`)
- Actions disciplinaires

**YouthProgressionService** (`Simulation/YouthProgressionService.cs`)
- Progression wrestlers jeunes

**WorkerGenerationService** (`Simulation/WorkerGenerationService.cs`)
- Génération procédurale de wrestlers
- Attributs: InRing, Entertainment, Story, Mental

**ScoutingService** (`Simulation/ScoutingService.cs`)
- Système de recrutement

### Services Métier

**BookingBuilderService** (`Services/BookingBuilderService.cs`)
- CRUD sur segments (matchs, promos, angles)
- Ajouter, modifier, supprimer, déplacer, dupliquer segments
- Gestion main event

**TitleService** (`Services/TitleService.cs`)
- Gestion ceintures/titres

**ContenderService** (`Services/ContenderService.cs`)
- Gestion contenders pour titres

**StorylineService** (`Services/StorylineService.cs`)
- Gestion storylines
- Phases: Setup, Build, Peak, Resolution
- Heat tracking par segment

**ShowSchedulerService** (`Services/ShowSchedulerService.cs`)
- Planification calendrier shows

### Contrats

**ContractNegotiationService** (`Contracts/ContractNegotiationService.cs`)
- Créer offre, contre-proposition
- Accepter/refuser offre
- États: proposée, contre, acceptée, refusée
- Clauses: durée, salaire, bonus show, buyout, non-compete
- Renouvellement automatique

**AIContractDecisionService** (`Contracts/AIContractDecisionService.cs`)
- Décisions IA pour négociations

### Médical

**InjuryService** (`Medical/InjuryService.cs`)
- Application blessures: LEGERE, MOYENNE, GRAVE
- Calcul risque basé sur fatigue
- Plan récupération (semaines repos recommandées)
- États: EN_COURS, TERMINÉ

**MedicalRecommendations** (`Medical/MedicalRecommendations.cs`)
- Recommandations médicales par sévérité

### Validation

**BookingValidator** (`Validation/BookingValidator.cs`)
- Validation segments avant simulation
- Vérification participants, durées, cohérence

### Import

**BakiAttributeConverter** (`Import/BakiAttributeConverter.cs`)
- Conversion attributs depuis format Baki

**BakiAttributeNormalizer** (`Import/BakiAttributeNormalizer.cs`)
- Normalisation 0-100

**BakiQuantileMap** (`Import/BakiQuantileMap.cs`)
- Mapping quantiles

**BakiAttributeMappingMath** (`Import/BakiAttributeMappingMath.cs`)
- Transformations mathématiques

## Modèles de Données

### Core

**ShowContext**
- Show, Compagnie, Workers, Titres, Storylines, Segments
- Chimies entre workers
- Deal TV optionnel

**ShowDefinition**
- ID, Nom, Semaine, Région, Durée, Compagnie, Lieu, Diffusion

**CompanyState**
- Compagnie: ID, Nom, Région, Prestige, Trésorerie
- Audience moyenne, Reach
- Era courante, Catch style, Taille
- Owner, Booker
- Contrôle joueur

**WorkerSnapshot**
- Attributs: InRing, Entertainment, Story
- Popularité, Momentum, Morale
- Fatigue, Blessure
- Rôle TV

**SegmentDefinition**
- Type: match, promo, angle_backstage, interview
- Participants, Durée, Intensité
- Storyline, Titre
- Vainqueur/Perdant (matchs)
- Main event flag

**GameStateDelta**
- Deltas: Fatigue, Momentum, Popularité
- Blessures nouvelles
- Heat storylines, Prestige titres
- Transactions financières

### Gouvernance (Migration 004-005)

**OwnerSnapshot**
- Vision, tolérance risque
- Préférences: type produit, fréquence shows
- Priorités: talent, finances, satisfaction fans

**BookerSnapshot**
- Scores: Créativité, Logique, Résistance biais
- Style préféré
- Préférences: underdog, vétéran, fast rise, slow burn
- Auto-booking activé
- Statut emploi

**BookerMemoryEntry**
- Mémoire événements pour IA booker
- Impact score, force rappel

**CompanyEra**
- Ères compagnie: Foundation, Golden, etc.
- Début/fin, stats moyennes

### Catch Style (Migration 006)

**CatchStyle**
- Caractéristiques: Wrestling Purity, Entertainment Focus
- Influences: Hardcore, Lucha, Strong Style
- Attentes fans: qualité match, storylines, promos
- Multiplicateurs rating

### Storylines

**StorylineInfo**
- Phase: Setup, Build, Peak, Resolution
- Heat (0-100)
- Statut: active, inactive, completed
- Participants avec rôles

### Contrats

**ContractOffer**
- Type contrat, période (StartWeek-EndWeek)
- Salaire hebdo, bonus show
- Buyout, non-compete weeks
- Exclusivité, renouvellement auto
- Statut: proposée, contre, acceptée, refusée

**ActiveContract**
- Contrat actif signé
- Statut: actif, expiré, résilié

**ContractNegotiationState**
- État négociation en cours
- Dernière offre, dernière MAJ

### Médical

**InjuryRecord**
- Type blessure, sévérité
- Semaines début/fin
- Actif/inactif

**RecoveryPlan**
- Plan récupération
- Semaines repos recommandées
- Niveau risque: FAIBLE, MODÉRÉ, ÉLEVÉ

### Finance

**FinanceTransaction**
- Type: billetterie, merch, tv, production, paie
- Montant, libellé

**ShowHistoryEntry**
- Historique show: note, audience, résumé

### Rapports

**ShowReport**
- Note globale, audience
- Détails audience (AudienceDetails)
- Revenus: billetterie, merch, TV
- Impact popularité compagnie
- Rapports segments

**SegmentReport**
- Note segment (0-100)
- Scores InRing, Entertainment, Story
- Chaleur public avant/après
- Pénalités pacing, bonus chimie
- Événements (botch, blessures, incidents)
- Impact: fatigue, momentum, popularité, heat storyline

**SegmentImpact**
- Deltas fatigue, momentum, popularité
- Heat storylines, prestige titres
- Blessures segment

## Modèles de Calcul

### AudienceModel (`Simulation/AudienceModel.cs`)
- **Inputs**: Reach, Note show, Star power, Saturation
- Calcul audience estimée avec détails

### HeatModel (`Simulation/HeatModel.cs`)
- Delta heat storyline par segment
- Pénalité segments inactifs
- Réduction surexposition (segments multiples même show)

### DealRevenueModel (`Simulation/DealRevenueModel.cs`)
- Calcul revenus contrats TV

## Interfaces Repository

**IContractRepository** - Stockage contrats/négociations
**IMedicalRepository** - Stockage blessures/plans récupération
**IScoutingRepository** - Stockage scouting
**ITitleRepository** - Stockage titres
**IContenderRepository** - Stockage contenders
**IContenderRankingRepository** - Rankings contenders
**IShowSchedulerStore** - Stockage planning shows
**IBrandRepository** - Stockage brands
**IBookerRepository** - Stockage bookers
**IOwnerRepository** - Stockage owners
**IEraRepository** - Stockage eras compagnies
**ICrisisRepository** - Stockage crises
**IMoraleRepository** - Stockage moral
**IRumorRepository** - Stockage rumeurs
**INepotismRepository** - Stockage népotisme
**IStaffRepository** - Stockage staff
**IStaffCompatibilityRepository** - Compatibilité staff

## Interfaces Moteurs

**ISimulationEngine** - Contrat simulation
**IRandomProvider** - Générateur aléatoire (seed support)
**IRatingModel** - Modèle rating
**IImpactApplier** - Application impacts
**IBookerAIEngine** - IA booker
**ICommunicationEngine** - Communications
**ICrisisEngine** - Gestion crises
**IOwnerDecisionEngine** - Décisions owner
**IValidator** - Validation générique
**IWorkerGenerationService** - Génération workers

## Enums

**BrandObjective** - Objectifs brand
**EraType** - Types d'ères
**PersonalityLabel** - Labels personnalité
**StaffRole** - Rôles staff
**ProductStyle** - Styles de produit
**StorylinePhase** - Phases storyline
**StorylineStatus** - Statuts storyline
**InjurySeverity** - Sévérité blessures
**PayrollFrequency** - Fréquence paie
**WorldSimLod** - Niveaux détail simulation

## Systèmes Clés

### Système de Segments
1. Match - combat wrestlers
2. Promo - discours/interview
3. Angle backstage - scène arrière-scène
4. Interview - interview segment

### Calcul Note Segment
```
baseScore = moyenne(InRing, Entertainment, Story)
crowdBonus = (crowdHeat - 50) / 10
moraleBonus = (morale - 50) / 10
note = baseScore + crowdBonus + pacingPenalty + chimieBonus + moraleBonus
```

Modifiers:
- Storyline: +6 story, +4 note
- Chimie: -8 à +8
- Pacing penalty: promos consécutives, segments lents répétés
- Morale participants

### Fatigue et Blessures
```
fatigue = durée * (intensité/50) * multiplicateur
multiplicateur = 1.0 (match), 0.6 (autre)

risqueBlessure = baseRisk + (intensité/150) + (fatigue/250)
baseRisk = 0.06 (match), 0.03 (autre)
sévérité = fatigue > 80 ? MOYENNE : LEGERE
```

### Momentum
```
vainqueur: +2 + (note-50)/20
perdant: -2 + (note-50)/25
```

### Popularité
```
deltaBase = (note-50)/10
vainqueur: +2 supplémentaire
perdant: -2 supplémentaire
```

### Heat Storyline
- Active dans segment: delta basé sur note + pénalité surexposition
- Inactive: delta négatif

### Star Power
```
starPower = moyenne 3 meilleurs popularités
```

### Saturation
```
saturation = audienceMoyenne * 0.6 + durée/4 + segments * 2
```

## Patterns

- **Records immuables** pour tous les modèles
- **Services stateless** avec dépendances injectées
- **Repository pattern** pour persistance
- **Delta pattern** pour changements d'état
- **Seed random** pour reproductibilité
- **Validation explicite** avant opérations

## Configuration

**WorldSimSettings**
- Budget ms/tick
- Nombre compagnies LOD0
- Fréquences LOD1/LOD2

**FinanceSettings**
- Venue: capacité base/min/max, coefficients
- Billetterie: prix base/min/max, taux remplissage
- Merch: dépense/fan, multiplicateur stars
- TV: revenus base + par audience
- Production: coûts base + par minute/spectateur
- Paie: semaines/mois

## Dépendances

- RingGeneral.Specs (référence projet)
- .NET 8.0
- Nullable enabled
- ImplicitUsings enabled
