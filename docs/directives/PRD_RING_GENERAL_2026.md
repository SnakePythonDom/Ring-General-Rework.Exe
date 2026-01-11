# 📋 PRODUCT REQUIREMENTS DOCUMENT (PRD)
# Ring General — Wrestling Promotion Manager

**Version** : 2.0 (PRD Consolidé)
**Date** : 8 janvier 2026
**Chef de Projet** : Claude (Lead Software Architect)
**Client/Stakeholder** : SnakePythonDom
**Branche de Développement** : `claude/create-project-prd-16XV5`

---

## TABLE DES MATIÈRES

1. [Executive Summary](#executive-summary)
2. [Vision Produit](#vision-produit)
3. [Objectifs et Critères de Succès](#objectifs-et-critères-de-succès)
4. [Spécifications Fonctionnelles](#spécifications-fonctionnelles)
5. [Spécifications Techniques](#spécifications-techniques)
6. [Roadmap et Phasage](#roadmap-et-phasage)
7. [Risques et Mitigation](#risques-et-mitigation)
8. [Métriques et KPIs](#métriques-et-kpis)
9. [Glossaire](#glossaire)

---

## EXECUTIVE SUMMARY

### Élévateur Pitch
**Ring General** est un simulateur de gestion de compagnie de catch professionnel combinant la profondeur strategique de **Football Manager** avec la complexité créative de **Total Extreme Wrestling**. Les joueurs gèrent une promotion de catch, recrutent des talents, créent des storylines, et construisent une compagnie prospère sur plusieurs années.

### État Actuel du Projet
- **Progression** : 50-55% complété (Phase 1.9)
- **Architecture** : 100% complète, ✅ EXCELLENTE (score 8.5/10)
- **Infrastructure** : ✅ COMPLET
- **Core Systems** : 80% implémenté (Attributs, Personnalité, Auto-Booking IA, Show Day)
- **UI/UX** : 65% implémenté (13+ views, 48 ViewModels)
- **Base de Données** : ✅ OPÉRATIONNELLE

### Valeur Commerciale
- **Cible** : Fans de gestion (FM, TEW, OSM)
- **Plateforme** : Windows/Linux/macOS (Avalonia)
- **Modèle** : Jeu standalone (future: DLC, mods)
- **Potentiel** : Franchise à long terme avec écosystème de contenu

---

## VISION PRODUIT

### Déclaration de Vision
Ring General permet aux joueurs de devenir propriétaire et directeur créatif d'une compagnie de catch professionnel, en gérant tous les aspects du business (finances, talents, créativité) pour créer une promotion légendaire sur plusieurs décennies.

### Différenciation du Marché

| Aspect | Ring General | Football Manager | Total Extreme Wrestling |
|--------|-------------|------------------|------------------------|
| **Profondeur attributs** | 40 attributs (4 dimensions) | 32 attributs | ~20 attributs |
| **Système personnalité** | 25+ profils auto-détectés | 20+ profils | Basique |
| **Auto-booking IA** | ✅ Oui, avancé | ❌ Non | ❌ Non |
| **Système backstage** | ✅ Complet (moral, rumeurs, crises) | ✅ Partiel | ✅ Basique |
| **Simulation match** | ✅ Moteur sophistiqué | N/A | ✅ Basique |
| **Accessibilité** | ✅ Française, débutants | ⚠️ Complexe | ⚠️ Interface datée |

### Propositions de Valeur Joueur

#### Pour le Casual Gamer
- Interface moderne inspirée de FM 2026
- Gameplay accessible (mode auto-booking)
- Progression satisfaisante (de Local à Global)
- Durée de vie : 50+ heures minimum

#### Pour l'Hardcore Gamer
- 40 attributs détaillés à optimiser
- 25+ profils personnalité à maîtriser
- Simulation fine des matches
- Système backstage complexe (morale, rumeurs, crises)
- Storylines multi-années avec mécanique heat
- Durée de vie : 200+ heures+

---

## OBJECTIFS ET CRITÈRES DE SUCCÈS

### Objectifs Primaires (MVP)

#### 1. **Jouabilité Complète**
- ✅ Création de partie fonctionnelle
- ✅ Boucle de jeu hebdomadaire opérationnelle
- ⚠️ Tous les écrans UI implémentés et fonctionnels
- ✅ Système de sauvegarde automatique

**Critère d'Acceptation**
```
- Joueur peut créer une compagnie et jouer 1+ saison complète
- Aucun crash après 10+ heures de gameplay
- Toutes les actions UI sont réactives (< 200ms)
```

#### 2. **Profondeur Stratégique**
- ✅ 40 attributs de performance implémentés
- ✅ 25+ profils de personnalité automatiquement détectés
- ⚠️ Système de storylines avancé (phases : BUILD/PEAK/BLOWOFF)
- ✅ IA propriétaire & booker sophistiquée

**Critère d'Acceptation**
```
- Chaque décision booking affecte >= 5 systèmes différents
- Le gameplay à 20h est notablement différent de celui à 100h
- Deux playstyles différents produisent des résultats distincts
```

#### 3. **Immersion Créative**
- ✅ 5 styles de catch uniques avec impacts gameplay
- ⚠️ Système de contrats avancé
- ⚠️ Système de sponsorships et partenariats
- ✅ Simulation de shows avec impacts réalistes

**Critère d'Acceptation**
```
- Joueur peut créer une promotion "unique" (style + direction)
- Storylines progressent naturellement (sans forçage)
- Match ratings reflètent les choix booking
```

### Objectifs Secondaires

#### 4. **Accessibilité et Localisation** 🌍
- ✅ Interface 100% en français
- ✅ Guide de démarrage rapide
- ⚠️ Système d'aide in-game (tooltips, codex)
- ⚠️ Tutoriel interactif (1ère saison guidée)

#### 5. **Extensibilité et Modding**
- ✅ Architecture modulaire (spec JSON, repositories séparés)
- ✅ Import de données externes (BAKI)
- ⚠️ Système de mods (data + UI)
- ⚠️ Export de parties / partage de compagnies

#### 6. **Performance et Stabilité**
- ✅ Lancement en < 3 secondes
- ⚠️ Navigation UI < 200ms
- ⚠️ Simulation d'un show en < 500ms
- ✅ Sauvegarde transparente

### KPIs de Succès

#### Pour Beta/Soft Launch
| KPI | Cible | Success Criteria |
|-----|-------|------------------|
| Playtime average | 50+ heures | Retention > 70% après 10h |
| Crash rate | < 0.5% | Aucun crash identifié après 100h |
| Feature completion | 100% | Tous les systèmes jouables |
| User satisfaction | 4.5/5⭐ | Score Metacritic > 80 |

---

## SPÉCIFICATIONS FONCTIONNELLES

### 1. GESTION DE COMPAGNIE (Company Hub)

#### 1.1 Identité de la Compagnie
**Description** : Le joueur crée et personnalise son identité de promotion.

**Attributs** :
- Nom de la compagnie
- Année de fondation (1950-2100)
- Région/Pays d'origine
- Style de catch (8 styles uniques)
- Taille de la compagnie (Local → Global)
- Era actuelle
- Budget initial & revenus

**Système de Catch Styles** 🎪
```
8 Styles avec attributs distincts:
1. Pure Wrestling (1.3x Match Rating bonus)
   - High workrate, low entertainment
   - Fans attendent: Technique, storytelling

2. Sports Entertainment (1.2x Promo Rating bonus)
   - Balance technique/spectacle
   - Fans attendent: Spectacle, promos, storylines

3. Hardcore Wrestling (Violence multiplier)
   - Extreme spots, blessures réalistes
   - Fans attendent: Extreme violence, shock value

4. Lucha Libre (High-flying multiplier)
   - Acrobatique, tradition mexicaine
   - Fans attendent: Flying, spots, masks

5. Strong Style (Striking bonus)
   - Puroresu japonais, fighting spirit
   - Fans attendent: Tough guys, no selling

6. Hybrid Wrestling (Balanced, 1.0x multiplier)
   - Mix équilibré (DÉFAUT)
   - Fans attendent: Un peu de tout

7. Family-Friendly (Comedy bonus)
   - Tous publics, good guys vs bad guys
   - Fans attendent: Heroes, no blood

8. Indie Wrestling (Innovation bonus)
   - Passion, ressources limitées
   - Fans attendent: Innovation, effort
```

#### 1.2 Gouvernance (Owner & Booker)
**Description** : Deux rôles distincts contrôlent la stratégie et la créativité.

**OWNER (Vision Stratégique)** 📊
```
Rôle: Prend les décisions stratégiques à long terme
- VisionType: Balanced/Growth/Prestige/Profit
- RiskTolerance: 0-100 (conservative → aggressive)
- PreferredProductType: Technical/Entertainment/Hardcore/Balanced
- ShowFrequencyPreference: Weekly/Bi-Weekly/Monthly
- TalentDevelopmentFocus: 0-100
- FinancialPriority: 0-100
- FanSatisfactionPriority: 0-100

Décisions Owner:
- Budget allocation par département
- Long-term talent strategy
- Partenariats majeurs
- Nouvelles acquisitions
```

**BOOKER (Direction Créative)** 🎬
```
Rôle: Crée les cartes et storylines au jour le jour
- CreativityScore: 0-100 (créatif ↔ prévisible)
- LogicScore: 0-100 (logique ↔ chaotique)
- BiasResistance: 0-100 (objectif ↔ biaisé)
- PreferredStyle: 5 styles de product
- Biases: Like Underdog / Veteran / Fast Rise / Slow Burn
- IsAutoBookingEnabled: bool (utilise l'IA)
- EmploymentStatus: Active/Suspended/Fired

Décisions Booker:
- Composition des cartes
- Progression des storylines
- Résultats des matches
- Alliances & debuts
```

**Système de Mémoire du Booker** 🧠
```
Le Booker se souvient des événements passés et ajuste ses décisions:
- EventType: Match Won/Lost, Interview, Betrayal, Injury, etc.
- ImpactScore: Force de l'impact (-100 à +100)
- RecallStrength: Force du souvenir (0-100)
- WorkerId: Worker affecté
- EventDate: Quand ça s'est passé

Exemple:
- Worker X a gagné un match énorme : Booker le pousse plus
- Worker Y a blessé l'héros : Booker le book en feud
- Worker Z a été trahi : Booker se souvient pour revenge angles
```

#### 1.3 Système de Boucle de Jeu Hebdomadaire

**Workflow Principal** 📆
```
SEMAINE TYPE (7 jours)

┌─ LUNDI: INBOX & PLANNING
│  • Emails, incidents, demandes
│  • Rapports de scouting
│  • Offres de partenariat
│
├─ MARDI-MERCREDI: PRÉPARATION SHOW
│  • Booking de la carte
│  • Promo writing (future)
│  • Consignes aux workers
│
├─ JEUDI: SHOW DAY 📺
│  • Exécution de la carte
│  • Simulation de chaque segment
│  • Impacts en temps réel
│
├─ VENDREDI: RÉSULTATS & IMPACTS
│  • Ratings & revenue
│  • Heatsheets & reactions
│  • Blessures & suspensions
│  • Morale post-show
│
├─ SAMEDI-DIMANCHE: GESTION
│  • Gestion médicale
│  • Meetings staff
│  • Gestion des finances
│  • Événements backstage
│
└─ LUNDI PROCHAIN: ↻ RÉPÉTER
```

**Événements Hebdomadaires Aléatoires** 🎲
```
Chaque semaine, 0-3 événements aléatoires:

HIGH PROBABILITY (30%):
- Rumeur backstage
- Problème de morale
- Demande de push
- Offre d'un rival

MEDIUM PROBABILITY (15%):
- Blessure surprise
- Walk-out d'un worker
- Dispute backstage
- Incident médiatique

LOW PROBABILITY (5%):
- Mort d'un personnage (storyline)
- Strike du roster
- Conflit staff majeur
- Acquisition hostile
```

### 2. GESTION DES TALENTS (Roster)

#### 2.1 Système d'Attributs (40 attributs)

**A. Attributs IN-RING (10 attributs, 0-100)**
```
STRIKING: Kick & Punch quality
GRAPPLING: Takedown & submission technique
HIGH-FLYING: Aerial maneuvers safety & height
POWERHOUSE: Strength-based moves
TIMING: Spot execution precision
SELLING: Believable bumps & reactions
PSYCHOLOGY: Match storytelling
STAMINA: Endurance throughout card
SAFETY: Injury prevention ability
HARDCORE BRAWL: Street fight competence

IN-RING AVERAGE = (Sum / 10)
```

**B. Attributs ENTERTAINMENT (10 attributs, 0-100)**
```
CHARISMA: Natural star quality
MIC WORK: Promo delivery
ACTING: Character portrayal depth
CROWD CONNECTION: Fan relatability
STAR POWER: Drawing ability
IMPROVISATION: On-the-fly adaptation
ENTRANCE: Presentation quality
SEX APPEAL: Physical attraction (controversial but realistic)
MERCHANDISE APPEAL: Merch potential
CROSSOVER POTENTIAL: Mainstream appeal

ENTERTAINMENT AVERAGE = (Sum / 10)
```

**C. Attributs STORY (10 attributs, 0-100)**
```
CHARACTER DEPTH: Complexity of character
CONSISTENCY: Character maintenance
HEEL PERFORMANCE: Villain effectiveness
BABYFACE PERFORMANCE: Hero effectiveness
STORYTELLING LONG-TERM: Multi-year angle capability
EMOTIONAL RANGE: Ability to convey emotions
ADAPTABILITY: Genre switching ability
RIVALRY CHEMISTRY: Feud selling ability
CREATIVE INPUT: Own ideas value
MORAL ALIGNMENT: Character morality consistency

STORY AVERAGE = (Sum / 10)
```

**D. Attributs MENTAUX (10 attributs, 0-20) 🔒 CACHÉS**
```
Révélés uniquement par scouting avancé (ScoutingLevel)

AMBITION: Veux-t-il le monde?
DETERMINATION: Persévérance
LOYALTY: Fidélité à la compagnie
PROFESSIONALISM: Sérieux au travail
SPORTSMANSHIP: Fair play
PRESSURE: Gestion du stress
TEMPERAMENT: Contrôle des émotions
SELFISHNESS: Priorité personnelle
ADAPTABILITY: Flexibilité aux changements
INFLUENCE: Pouvoir d'influence sur les autres
```

#### 2.2 Système de Personnalité (25+ profils)

**5 Catégories Principales** 🎭

1. **ÉLITES** (The Professionals)
   - "Exemplary Professional" ⭐⭐⭐⭐⭐
   - "Model Citizen" ⭐⭐⭐⭐⭐
   - "Determined" ⭐⭐⭐⭐

2. **STARS À ÉGO** (The Ambitious)
   - "Ambitious" ⭐⭐⭐⭐
   - "Locker Room Leader" ⭐⭐⭐⭐
   - "Mercenary" ⭐⭐⭐

3. **INSTABLES** (The Volatiles)
   - "Fiery Temperament" ⚡⚡⚡
   - "Loose Cannon" ⚡⚡⚡
   - "Inconsistent" ⚡⚡

4. **TOXIQUES** (The Problems)
   - "Selfish" 💀💀💀
   - "Diva" 💀💀
   - "Lazy" 💀

5. **STRATÈGES** (The Smart Ones)
   - "Cunning Veteran" 🧠🧠🧠
   - "Storytelling Master" 🧠🧠🧠
   - "Politician" 🧠🧠

**Détection Automatique** 🔍
```
PersonalityDetectorService analyse les attributs mentaux du worker
et propose automatiquement un profil + rapport d'agent texte:

Exemple rapide:
- Ambition=18, Determination=19, Loyalty=5 → "Ambitious"
- Ambition=8, Determination=8, Loyalty=18 → "Model Citizen"
- Temperament=3, Selfishness=18 → "Diva"
```

#### 2.3 Morale & Backstage

**Morale Individuelle** 😊😐😞
```
Affecte: Perf en ring, loyauté, risk de turnover

FACTEURS:
+ Gagner des matches (+5)
+ Main event push (+10)
+ Contract raise (+8)
+ Story win (+15)
- Losing streak (-15)
- Être jamais booké (-10)
- Confrontation backstage (-8)
- Blessure (-20)

Morale Seuils:
> +50: Excellent (prêt pour main event)
0 à +50: Normal (content)
-50 à 0: Bas (considère de partir)
< -50: Critical (très probablement part)
```

**Rumeurs Backstage** 💬
```
RumorRepository génère automatiquement des rumeurs:

TYPES:
- Push rumors ("Worker X devrait être en title picture")
- Relationship rumors ("Worker A et B ensemble?")
- Injury rumors ("Worker Y never coming back")
- Contract rumors ("Worker Z going to rival")
- Performance rumors ("Worker W doesn't care anymore")

IMPACT:
- Morale (+/- 5 points si rumeur positive/negative)
- Heat storyline (accélère si rumeur supporte la feud)
- Loyalty (peut déclencher demande de push)
```

**Crises Backstage** 🚨
```
CrisisRepository gère les incidents majeurs:

TYPES:
- Personal (injury, death in family)
- Professional (contract dispute, public feud)
- Organizational (staff conflict, financial crisis)
- Reputational (scandal, lawsuit)

RÉSOLUTION:
- Communication choices (Ignore, Acknowledge, Resolve)
- Financial impact
- Morale impact
- Loyalty impact
```

### 3. SYSTÈME DE BOOKING

#### 3.1 Construction de Carte

**Structure de Show**
```
SHOW = Segments ordonnés (matches, promos, skits)

Chaque SEGMENT:
- Type: Match / Promo / Skit / Opening / Closing
- Workers: Participants
- Duration: 3-60 minutes
- Booking Style: 5 styles de match
- Storytelling Role: Progression de feud

CONTRAINTES:
- Durée totale: 120-240 min (2-4h)
- Main event: 20-40 min
- Mid-card: 10-25 min
- Undercard: 3-10 min
```

#### 3.2 Auto-Booking IA 🤖

**Mode AUTO-BOOKING (NEW!)** ✨

```
Le Booker peut activer "IsAutoBookingEnabled"
et l'IA génère automatiquement des cartes cohérentes.

GÉNÉRATION AUTOMATIQUE:
1. Récupère les storylines actives (BUILD/PEAK/BLOWOFF)
2. Identifie les workers disponibles (pas blessés)
3. Respecte les préférences du Booker
   - PreferredStyle (5 options)
   - Likes (Underdog / Veteran / Fast Rise / Slow Burn)
4. Respecte les contraintes de l'Owner
   - Budget wrestling (0-100%)
   - Workers bannis
   - Fatigue limits
5. Génère une carte équilibrée:
   - Heat distribution
   - Workrate variety
   - Story progression
6. Valide et propose au joueur

ALGORITHME SIMPLIFIÉ:
```
FOR each active_storyline:
  booker_preference_match = evaluate_match_type()
  worker_selection = select_best_workers()
  card_position = assign_timing(storyline.phase)

BALANCE pour toute la carte:
  - 30% Pure wrestling segments
  - 40% Entertainment segments
  - 20% Hardcore segments
  - 10% Storytelling segments
```

### 4. SYSTÈME DE SIMULATION DE SHOW

#### 4.1 Moteur de Simulation (ShowSimulationEngine)

**Calcul de Note de Match** 📊

```
MATCH_RATING =
  (IN_RING_SCORE * 0.3) +
  (ENTERTAINMENT_SCORE * 0.3) +
  (STORYTELLING_SCORE * 0.3) +
  (CROWD_HEAT_BONUS * 0.1)

IN_RING_SCORE:
  Base = (Worker1.InRingAvg + Worker2.InRingAvg) / 2
  ChemistryBonus = +0.25 si workers ont disputé feuds ensemble
  StyleMatchBonus = +0.15 si style favorite des workers

ENTERTAINMENT_SCORE:
  Base = (Worker1.EntertainmentAvg + Worker2.EntertainmentAvg) / 2
  CrowdConnection = +0.10 si un des workers très charismatique
  CharacterBonus = +0.15 si match a good storyline

STORYTELLING_SCORE:
  Base = (Worker1.StoryAvg + Worker2.StoryAvg) / 2
  FeudProgress = +0.20 si match in storyline progression
  HeatAccumulation = +0.10 si crowd heat > threshold

CROWD_HEAT_BONUS:
  Accumule during match, max 5.0
  Décrémente si timing est mauvais, si workers moins connus
```

**Simulation de Fatigue** 💪

```
FATIGUE ACCUMULATION:
- Chaque match ajoute 10-30 points de fatigue
- Chaque entrée ajoute 2-5 points
- Wrestlers main events = +20% fatigue

EFFET DE FATIGUE:
- Fatigue 0-25: No impact
- Fatigue 25-50: -5% perf tous les attributs
- Fatigue 50-75: -15% perf, injury risk +10%
- Fatigue 75-100: -30% perf, injury risk +30%

RÉCUPÉRATION:
- Reprise: -15 points par jour
- Show off: -25 points par jour
- Vacation: -50 points par week
```

**Gestion des Blessures** 🏥

```
BLESSURE RATE = Base + (Hardcoreness * 0.02) + (Fatigue * 0.001) - (Safety * 0.001)

TYPES:
- Minor (1-2 weeks): Loss of 10% perf
- Moderate (2-6 weeks): Loss of 30% perf
- Major (6-16 weeks): Loss of 60% perf
- Severe (16+ weeks): Worker peut se retirer

DÉCISION MÉDICALE:
Doctor recommande repos, joueur peut ignorer (risque re-injury)
```

#### 4.2 Impacts Post-Show

**Morale Post-Show** 😊

```
WORKERS UTILISÉS (+):
+ Match won: +10
+ Match lost (mais bon): +5
+ Main event spot: +15
+ Title change win: +30

WORKERS INUTILISÉS (-):
- Pas de segment: -10
- Semaine d'affilée: -15 (stacking penalty)
- Star quality (> 85): -20 (main event star qui chill)

CROWD REACTION:
+ Great show (avg rating > 7.0): +5 tout le roster
- Terrible show (avg rating < 3.0): -10 tout le roster
```

**Progression des Storylines** 📖

```
HEAT ACCUMULATION:
- Bout dans feud augmente heat de 5-20 points
- Blessure du rival augmente heat dramatiquement (+30)
- Interférence augmente heat (+15)
- Non-finish diminue heat (-5)

PHASES:
- BUILD (0-40): Accumulation
  → Wrestlers se cherchent, teasent
- PEAK (40-70): Climax
  → Match principal en préparation
  → Tension maximale
- BLOWOFF (70-100): Finale
  → Match final prévu
  → Heat resolution

PROGRESSION AUTO:
- Heat augmente naturellement avec les matches
- Reach PEAK trigger automatiquement (70+)
- Reach BLOWOFF trigger automatiquement (100+)
```

**Impact Financier** 💰

```
TICKET SALES:
Base = AudienceMean * TicketPrice
Multiplier = 1.0 + (AvgMatchRating / 10)
Total = Base * Multiplier

TV REVENUE:
Base = TV deal value
Multiplier = 1.0 + (AvgShowRating / 10)
Total = Base * Multiplier

MERCHANDISE:
Base = (Sum of worker merchandise appeal) * 100
Multiplier = 1.0 + (AvgShowRating / 10)
Total = Base * Multiplier

BROADCAST REVENUE:
PPV shows: TicketSales * 3
Regular shows: TV deal base + TV multiplier
```

---

## SPÉCIFICATIONS TECHNIQUES

### Stack Technologique

| Couche | Technologie | Version | Notes |
|--------|-------------|---------|-------|
| **Framework** | .NET | 8.0 LTS | Multiplateforme |
| **UI Framework** | Avalonia | 11.0.6 | Cross-platform |
| **Pattern** | MVVM | + ReactiveUI | Binding avancé |
| **Base de données** | SQLite | 8.0.0 | Portable, pas de serveur |
| **Langage** | C# 12 | + Nullable refs | Type-safe |
| **DI** | Microsoft.Extensions | Latest | Léger, standard |

### Architecture Logicielle

**Layered Architecture** 🏗️

```
┌────────────────────────────┐
│   UI (Avalonia MVVM)       │  RingGeneral.UI
│   Views, ViewModels        │  (95 fichiers)
├────────────────────────────┤
│   Business Logic           │  RingGeneral.Core
│   Services, Domain Models  │  (124 fichiers)
├────────────────────────────┤
│   Data Access (23+ Repos)  │  RingGeneral.Data
│   DB Context, SQL          │  (45 fichiers)
├────────────────────────────┤
│   Configuration            │  RingGeneral.Specs
│   JSON Specs (78 files)    │
└────────────────────────────┘
```

**Principes SOLID**

| Principe | Implémentation |
|----------|----------------|
| **S**ingle Responsibility | Un repository par domaine (WorkerRepo, TitleRepo, etc) |
| **O**pen/Closed | Extensibilité via specs JSON, pas modification code |
| **L**iskov Substitution | IRepository pattern pour tous les accès données |
| **I**nterface Segregation | Interfaces fines (IWorkerRepository, etc) |
| **D**ependency Inversion | DI container, injection de dépendances |

### Structure des Données

**23+ Repositories Spécialisés**

```
/src/RingGeneral.Data/Repositories/

Core Domain (5):
├── WorkerRepository (workers, stats, profils)
├── CompanyRepository (compagnie, finances)
├── TitleRepository (titres, règnes)
├── ContractRepository (contrats d'emploi)
└── ShowRepository (shows, segments, ratings)

Gameplay Systems (8):
├── BookingRepository (cartes de shows)
├── StorylineRepository (feuds, angles)
├── SimulationRepository (résultats de matches)
├── YouthRepository (développement des jeunes)
├── ScoutingRepository (rapports de scout)
├── MedicalRepository (blessures, médical)
├── FinanceRepository (revenus, budgets)
└── CalendarRepository (planning des shows)

Backstage Systems (8):
├── MoraleRepository (moral des workers)
├── RumorRepository (rumeurs backstage)
├── RelationsRepository (relations inter-workers)
├── NepotismRepository (détection biais)
├── BackstageRepository (événements coulisses)
├── CrisisRepository (gestion de crises)
├── BookerRepository (IA du booker)
└── OwnerRepository (IA du propriétaire)

Support (2):
├── NotesRepository (annotations joueur)
└── SettingsRepository (préférences)
```

**Schéma Base de Données (~30 tables)**

```
CORE:
├── Companies (id, name, region, treasury, prestige, era, style)
├── Workers (id, name, company_id, status, hire_date, salary)
├── Shows (id, company_id, date, rating, revenue)
├── Segments (id, show_id, type, duration, workers)
└── Titles (id, company_id, name, champion_id, lineage)

ATTRIBUTES:
├── WorkerInRingAttributes (striking, grappling, etc)
├── WorkerEntertainmentAttributes (charisma, micwork, etc)
├── WorkerStoryAttributes (character depth, chemistry, etc)
└── WorkerMentalAttributes (ambition, loyalty, etc) 🔒

GAMEPLAY:
├── Contracts (id, worker_id, start_date, end_date, salary)
├── Storylines (id, name, phase, heat, involved_workers)
├── Segments (detail de chaque segment)
└── Titles (règnes, champions)

BACKSTAGE:
├── Morale (worker_id, morale_score, last_updated)
├── Rumors (id, type, created_date, affected_worker)
├── Relations (worker1_id, worker2_id, relationship_type)
├── Injuries (worker_id, type, recovery_date)
└── Crises (id, type, impact, resolution_status)
```

### Patterns d'Architecture

#### Pattern MVVM (Model-View-ViewModel)

```csharp
// Model (Domain Model)
public record Worker(
    string Id,
    string Name,
    int InRingAvg,
    int EntertainmentAvg,
    int StoryAvg,
    PersonalityProfile Personality,
    int Morale
);

// ViewModel (Reactive Logic)
public class RosterViewModel : ViewModelBase {
    private ObservableCollection<WorkerDisplayItem> _workers;
    public ObservableCollection<WorkerDisplayItem> Workers => _workers;

    public RosterViewModel(IWorkerRepository repo) {
        // Reactive command binding
    }
}

// View (Avalonia XAML)
<DataGrid ItemsSource="{Binding Workers}"
          SelectedItem="{Binding SelectedWorker}">
    <DataGridTextColumn Header="Nom" Binding="{Binding Name}"/>
    <DataGridTextColumn Header="In-Ring" Binding="{Binding InRingAvg}"/>
</DataGrid>
```

#### Pattern Repository

```csharp
public interface IWorkerRepository {
    Task<Worker?> GetWorkerByIdAsync(string id);
    Task<IReadOnlyList<Worker>> GetWorkersByCompanyAsync(string companyId);
    Task<IReadOnlyList<Worker>> SearchWorkersAsync(string pattern);
    Task SaveWorkerAsync(Worker worker);
    Task DeleteWorkerAsync(string id);
}

public class WorkerRepository : IWorkerRepository {
    private readonly SqliteConnection _connection;

    public async Task<Worker?> GetWorkerByIdAsync(string id) {
        // SQL query with parameterization
    }
}
```

#### Pattern Dependency Injection

```csharp
// App.axaml.cs
var services = new ServiceCollection();

// Register Core Services
services.AddSingleton<INavigationService, NavigationService>();
services.AddSingleton<IEventAggregator, EventAggregator>();

// Register Repositories (23+)
var factory = new RepositoryFactory(dbPath);
services.AddSingleton(factory.CreateRepositories());

// Register ViewModels
services.AddTransient<DashboardViewModel>();
services.AddTransient<RosterViewModel>();
// ... 46+ ViewModels

var provider = services.BuildServiceProvider();
```

---

## ROADMAP ET PHASAGE

### Vue Globale (Octobre 2025 - Avril 2026)

| Phase | Nom | Description | Durée | Cible | Status |
|-------|-----|-------------|-------|-------|--------|
| **0** | Infrastructure | Architecture, Base de données, DI | Complétée | Sept 2025 | ✅ |
| **1** | Fondations UI/UX | Navigation, ViewModels, Views de base | 4 semaines | Nov 2025 | ✅ |
| **1.5** | Attributs & Personnalité | 40 attributs, 25+ profils | 2 semaines | Déc 2025 | ✅ |
| **1.9** | Show Day & Auto-Booking | Flux show complet, IA booker | 2 semaines | Jan 2026 | ✅ |
| **2** | Features Avancées | Company Hub, Finances, Contrats | 3 semaines | Jan-Fév 2026 | ⚠️ |
| **3** | Gameplay Complet | Boucle hebdomadaire, Storylines, Youth | 4 semaines | Fév-Mar 2026 | ⚠️ |
| **4** | Performance | Optimisation, cache, lazy load | 2 semaines | Mar 2026 | ❌ |
| **5** | QA & Polish | Tests, bugfixes, localisation | 2 semaines | Avr 2026 | ❌ |
| **Release** | 1.0 | Lancement officiel | - | Avr 2026 | 🎯 |

### Phase 2 : Features Avancées (Janvier 2026) 🔜

**Objectif** : Rendre le jeu pleinement jouable avec tous les systèmes principaux.

#### 2.1 Company Hub (1 semaine)
```
DELIVERABLES:
- CompanyHubView.axaml (parent TabControl)
- CompanyProfileTabView.axaml (identité + direction + stars)
- CompanyStaffTabView.axaml (staff listings)
- CompanyRosterTabView.axaml (data grid workers)
- CompanyTeamsTabView.axaml (tag teams, factions)
- CompanyHistoryTabView.axaml (titres, eras)
- 6 ViewModels correspondants
- Navigation vers écrans détail (Owner, Booker, Worker)

CRITÈRE ACCEPTATION:
✓ Tous les onglets chargent sans erreur
✓ Données affichées correctement
✓ Navigation vers détails fonctionnelle
✓ Performance: tab switch < 300ms
```

#### 2.2 Finances Avancées (1 semaine)
```
DELIVERABLES:
- FinanceDetailView.axaml (budget breakdown par département)
- Charts (revenus, dépenses par semaine/mois/year)
- Forecast 3-6 mois
- Gestion des contrats longue durée
- Prêts & dettes

CRITÈRE ACCEPTATION:
✓ Tous les revenus/dépenses comptabilisés
✓ Charts lisibles et accurates
✓ Forecast réaliste
✓ Équilibre budgétaire après 4 semaines
```

#### 2.3 Contrats Avancés (1 semaine)
```
DELIVERABLES:
- ContractView.axaml (liste des contrats actifs)
- ContractDetailView.axaml (détails d'un contrat)
- NegotiationView.axaml (dialogue d'offre)
- ContractTemplates (templates pré-faits)
- Automatic renewal system

CRITÈRE ACCEPTATION:
✓ CRUD contrats fonctionnel
✓ Négociation possible
✓ Salary calculation correct
✓ Expiration warnings
```

### Phase 3 : Gameplay Complet (Février-Mars 2026)

**Objectif** : Boucle de jeu complète avec toutes les interactions.

#### 3.1 Boucle Hebdomadaire (1.5 semaine)
```
DELIVERABLES:
- WeeklyLoopOrchestrator (orchestration centrale)
- "Advance Week" button sur Dashboard
- Événements aléatoires hebdomadaires
- Progression automatique des storylines
- Vieillissement des workers
- Salaire deduction automatique
- Morale adjustments post-show

CRITÈRE ACCEPTATION:
✓ Joueur peut jouer 1+ saison complète
✓ Tous les systèmes affectés par "advance week"
✓ Sauvegarde correcte entre semaines
✓ Aucune data corruption
```

#### 3.2 Système de Storylines Avancé (1.5 semaine)
```
DELIVERABLES:
- CreateStorylineView.axaml (création d'angles)
- StorylineDetailView.axaml (suivi détaillé)
- PhaseManager (gestion BUILD/PEAK/BLOWOFF)
- Heat-based predictions
- Automatic story progression
- Story branching (si un worker blessé, angle change)

CRITÈRE ACCEPTATION:
✓ Créer storyline depuis 0
✓ Progression naturelle
✓ Heat accumulation correcte
✓ BLOWOFF auto-trigger
✓ Worker injuries impactent story
```

#### 3.3 Youth Development & Scouting (1 semaine)
```
DELIVERABLES:
- YouthDetailView.axaml (détails jeune wrestler)
- TrainingPlanView.axaml (plans d'entraînement)
- ScoutingReportView.axaml (rapports détaillés)
- AttributeImprovement simulation
- Scouting level progression (0/1/2)
- Youth Systems

CRITÈRE ACCEPTATION:
✓ Créer plan d'entraînement pour jeune
✓ Attributs progressent selon plan
✓ Scouting révèle attributs mentaux
✓ Jeune wrestler peut débuter en show
```
 ✓ Système de Jeunes Procédural pour Ring General

1.0 Introduction et Vision Stratégique

Ce document d'exigences produit (PRD) a pour objectif de définir les spécifications fonctionnelles et stratégiques pour l'implémentation d'un système de génération procédurale de nouveaux catcheurs, désigné sous le nom de "Youth System", au sein du jeu de gestion de catch Ring General. L'objectif stratégique fondamental de cette fonctionnalité est de garantir la viabilité, la profondeur et l'engagement du jeu sur le très long terme. Cette orientation s'aligne directement sur notre vision centrale : créer un jeu de gestion complexe et profond, inspiré par des références du genre telles que Football Manager pour sa granularité et Total Extreme Wrestling pour sa complexité narrative.

Ce PRD est destiné aux équipes de développement et d'assurance qualité (QA). Il servira de source de vérité unique tout au long du cycle de vie de cette fonctionnalité, de la conception initiale à l'implémentation technique, jusqu'à la phase de validation et d'équilibrage.

Pour comprendre pleinement l'importance de cette fonctionnalité, il est essentiel de justifier pourquoi une approche procédurale est non seulement une option, mais une nécessité stratégique pour l'avenir de Ring General.

2.0 Contexte et Justification

Cette section vise à démontrer pourquoi un système de génération procédurale de talents est non seulement préférable à une base de données statique, mais constitue un pilier essentiel pour réaliser les ambitions de Ring General en matière de rejouabilité et de narration émergente. Il s'agit de passer d'un jeu avec une fin de contenu prévisible à un véritable générateur d'histoires dynamiques et infinies.

2.1 Alignement avec la Vision Produit

La génération procédurale est en parfaite adéquation avec la vision fondamentale du produit. Ring General a été explicitement conçu pour capturer la profondeur de Football Manager et la complexité de Total Extreme Wrestling. Le cœur de l'expérience de ces titres de référence ne réside pas dans leurs données initiales, mais dans le renouvellement constant et imprévisible de leurs univers respectifs. Les joueurs y reviennent pendant des centaines d'heures car chaque nouvelle saison apporte son lot de jeunes prodiges, de déceptions et de surprises. Une base de données statique, aussi vaste soit-elle, est par nature finie. Seule la génération procédurale peut garantir un flux inépuisable et unique de nouveaux talents, assurant que l'univers du jeu reste vivant, pertinent et surprenant, même après des décennies de simulation en jeu.

2.2 Impact sur l'Engagement à Long Terme

L'intégration d'un "Youth System" procédural est le principal levier pour garantir un engagement sur le long terme. Des jeux comme RimWorld et Dwarf Fortress ont prouvé que l'imprévisibilité est la clé d'une rejouabilité quasi infinie. Le joueur n'est plus un simple consommateur de contenu pré-écrit, mais un acteur réagissant à un monde en constante évolution.

* Renouvellement Infini : Chaque nouvelle partie, et même chaque saison au sein d'une même partie, présentera un paysage de talents entièrement unique. Cela empêche les joueurs d'établir des stratégies optimales et répétitives basées sur la connaissance d'une base de données fixe, rendant chaque carrière distincte.
* Défi Constant : Le système force le joueur à s'adapter en permanence. L'émergence de nouveaux archétypes de catcheurs oblige à revoir ses stratégies de recrutement, de formation et de booking. Ce "reactive gameplay" teste continuellement les compétences tactiques et la vision à long terme du joueur, bien au-delà de la simple gestion d'un effectif connu.
* Prévention de la Stagnation : Dans un système statique, le vivier de talents finit inévitablement par s'épuiser ou devenir prévisible. Les joueurs finissent par connaître les meilleurs espoirs et le défi s'amenuise. Un système procédural garantit qu'il n'y a jamais de moment où le joueur "a tout vu", maintenant ainsi une tension et une curiosité permanentes.

2.3 Catalyseur de Récits Émergents

La génération procédurale est le médium par excellence pour la création de récits émergents, un pilier de l'expérience que nous visons. Le système ne se contentera pas de générer des statistiques ; il créera des "accroches narratives" (narrative hooks), des profils complexes qui interagiront de manière imprévisible avec les systèmes de jeu existants. Ces personnages ne sont pas pré-scénarisés, mais leurs attributs et leur personnalité génèrent naturellement des histoires.

Voici quelques exemples de récits pouvant émerger de ce système :

* Un prodige technique doté d'un charisme minimal et d'une personnalité "difficile", créant des tensions en coulisses malgré son succès sur le ring.
* Le fils ou la fille d'une légende du catch qui vient de prendre sa retraite, arrivant avec une pression médiatique immense et des attributs mentaux faibles.
* Deux jeunes talents générés la même année, avec des styles radicalement opposés (l'un voltigeur, l'autre cogneur) mais un potentiel de croissance similaire, créant les bases d'une rivalité "naturelle" qui pourrait définir une décennie de jeu.

En conclusion, l'approche procédurale est le seul moyen de transformer Ring General d'un simple jeu de gestion en un véritable "générateur d'histoires". C'est cet investissement dans la narration systémique qui assurera sa longévité et le distinguera sur le marché.

3.0 Objectifs et Exigences Fonctionnelles

Cette section constitue le cœur de ce PRD. Elle détaille précisément ce que nous allons construire en définissant les fonctionnalités, les contraintes et les interactions du système de génération de jeunes catcheurs.

3.1 Objectifs Clés

Les objectifs de haut niveau de cette fonctionnalité sont les suivants :

1. Garantir un flux continu et unique de nouveaux talents dans le monde du jeu pour assurer sa pérennité.
2. Augmenter la rejouabilité et l'imprévisibilité des parties à long terme en évitant la stagnation du vivier de talents.
3. Créer des opportunités de narration émergente grâce à la génération de profils de catcheurs uniques et complexes.
4. S'intégrer de manière transparente avec les systèmes de jeu existants (backstage, booking, IA) pour créer une expérience cohérente.

3.2 Exigences Fonctionnelles du Moteur de Génération

Le moteur de génération doit être capable de créer des profils de catcheurs complets et cohérents. Les composants suivants sont requis :

Composant Généré	Description des Exigences
Attributs de Performance	Le système doit générer des valeurs pour les 40 attributs existants (In-Ring, Entertainment, Story, Mental), respectant une logique de distribution configurable via les fichiers JSON pour éviter la surpopulation de prodiges. Il doit aussi définir une valeur de "potentiel" qui dictera leur courbe de progression future.
Profil de Personnalité	Le système doit assigner l'un des 25+ profils de personnalité existants ou en générer un nouveau basé sur des traits fondamentaux. Ce profil doit avoir un impact direct et mesurable sur les systèmes de moral, de crises et de rumeurs.
Caractéristiques Physiques	Génération de la taille, du poids, et d'un style visuel de base (pouvant être représenté par des avatars ou des descriptions textuelles). Ces caractéristiques doivent être corrélées et cohérentes (ex: un catcheur de 2m10 ne pèsera pas 70kg).
Accroche Narrative (Hook)	Le système doit pouvoir générer une "accroche" textuelle simple qui fournit un contexte narratif de base (ex: "Ancienne star du football amateur", "Issu d'une famille de lutteurs", "Réputation sulfureuse sur le circuit indépendant").

3.3 Intégration avec les Systèmes Existants

Les catcheurs générés par le "Youth System" ne doivent pas être des entités isolées. Ils doivent interagir pleinement avec les fonctionnalités de base de Ring General pour que l'écosystème du jeu soit crédible.

* Systèmes Backstage : Les nouveaux catcheurs doivent être intégrés aux systèmes de Moral, Rumeurs, Népotisme et Crises. Leur personnalité générée doit directement influencer leur comportement, leur susceptibilité aux problèmes de moral et leur manière d'interagir avec les autres membres de l'effectif.
* IA Booker/Propriétaire : L'Auto-Booking IA doit être capable de reconnaître, d'évaluer et d'utiliser le potentiel des jeunes talents. L'IA devra respecter les préférences définies pour le Booker (ex: "Fast Rise", "Slow Burn") en intégrant progressivement ces nouveaux talents dans ses cartes.
* Flux "Show Day" : Les performances des jeunes talents durant le "Show Day" (matchs, segments) doivent influencer leur progression de carrière, leur moral et leur "heat" de manière dynamique et cohérente, tout comme pour les catcheurs existants.

La définition de ces exigences fonctionnelles doit maintenant être validée par une analyse de faisabilité technique et une évaluation des risques associés.

4.0 Analyse de Faisabilité et Risques

Cette section évalue la faisabilité technique de l'implémentation du système de génération procédurale au sein de l'architecture existante de Ring General. Elle a également pour but d'identifier et de proposer des stratégies pour atténuer les risques potentiels liés à cette nouvelle fonctionnalité majeure.

4.1 Impact sur l'Architecture Actuelle

L'architecture actuelle de Ring General a été conçue pour la modularité et l'évolutivité, ce qui facilite grandement l'intégration d'un tel système. Les points forts suivants rendent cette implémentation non seulement faisable, mais également alignée avec nos bonnes pratiques de développement :

* L'architecture modulaire, avec ses 23+ repositories spécialisés, nous permet de développer le moteur de génération de manière isolée, minimisant ainsi les risques d'effets de bord sur le reste du code base.
* L'utilisation de la Clean Architecture (avec les couches RingGeneral.Core pour la logique métier et RingGeneral.Data pour l'accès aux données) fournit un cadre de travail clair et robuste pour intégrer la nouvelle logique de génération et ses interactions avec la base de données SQLite.
* La configuration data-driven via des fichiers JSON (situés dans specs/) est la pierre angulaire de ce projet. Elle est idéale pour définir les règles, les archétypes et les pondérations de la génération procédurale. Cela nous permettra d'itérer et d'équilibrer le système sans avoir à recompiler le code, ce qui est un avantage considérable en phase de test.

4.2 Risques Potentiels et Stratégies d'Atténuation

Bien que la faisabilité soit élevée, plusieurs risques doivent être anticipés et gérés.

Risque	Impact Potentiel	Stratégie d'Atténuation
Déséquilibre du Jeu	Génération de catcheurs systématiquement trop puissants ("overpowered") ou, à l'inverse, complètement inutiles, ce qui rendrait le jeu trop facile ou frustrant.	Implémenter des règles de distribution et des pondérations strictes dans les fichiers de configuration JSON pour un ajustement fin et rapide. Planifier une phase de test et d'équilibrage dédiée post-implémentation.
Manque de Cohérence	Les catcheurs générés semblent purement aléatoires, sans "âme" ni logique interne (ex: un technicien de génie avec des attributs mentaux très faibles), ce qui briserait l'immersion du joueur.	Utiliser des archétypes de base (ex: "Brawler", "High-Flyer", "Technician") comme point de départ pour guider la génération. S'assurer que les attributs, la personnalité et l'"accroche narrative" sont corrélés logiquement.
Impact sur la Performance	Le processus de génération de centaines de nouveaux catcheurs chaque année pourrait ralentir les temps de chargement entre les saisons ou la simulation hebdomadaire.	Optimiser l'algorithme de génération. Effectuer la génération en arrière-plan durant les temps morts ou les écrans de chargement. Limiter le nombre de catcheurs générés par an via un paramètre configurable.

En dépit de ces risques identifiés, la structure robuste et flexible du projet rend cette fonctionnalité tout à fait réalisable dans des conditions maîtrisées. La prochaine étape consiste à définir une feuille de route pour sa mise en œuvre.

5.0 Proposition de Mini-Roadmap

Cette section propose une feuille de route macro pour le développement et l'intégration du "Youth System". Elle est conçue pour s'intégrer de manière cohérente dans la roadmap globale du projet, telle que définie dans docs/ROADMAP_MISE_A_JOUR.md.

1. Phase 1 : Conception et Spécification (Estimation : 1 semaine)
  * Objectif : Finaliser les algorithmes de génération, les règles de distribution des attributs et la structure des données.
  * Livrables : Création des fichiers de spécification JSON pour les archétypes de catcheurs, les distributions d'attributs et les pondérations. Rédaction d'un document de design technique détaillé décrivant la logique du moteur.
2. Phase 2 : Développement du Moteur de Génération (Estimation : 3 semaines)
  * Objectif : Coder le moteur de génération de base en C# 12 au sein du projet RingGeneral.Core.
  * Livrables : Une librairie autonome capable de générer des objets "catcheur" complets (avec attributs, personnalité, accroche narrative, etc.) en se basant sur les fichiers de spécification JSON.
3. Phase 3 : Intégration et Tests Unitaires (Estimation : 2 semaines)
  * Objectif : Intégrer le moteur dans la boucle de jeu principale pour déclencher la génération à des intervalles définis (ex: chaque 1er janvier en jeu). Assurer la compatibilité avec le système de base de données SQLite et les systèmes backstage.
  * Livrables : Le jeu génère et sauvegarde de nouveaux catcheurs chaque année dans la base de données. Couverture de tests unitaires complète validant l'intégration et la non-régression des systèmes existants.
4. Phase 4 : Équilibrage et Itération (Estimation : 2 semaines)
  * Objectif : Tester intensivement la qualité et la cohérence des talents générés sur le long terme. Ajuster les règles de génération pour assurer une expérience de jeu équilibrée et intéressante.
  * Livrables : Versions ajustées et validées des fichiers de configuration JSON. Un rapport de playtest détaillé sur la qualité des talents générés sur une période de 10 ans de jeu simulé.

Cette fonctionnalité s'inscrit dans la Phase 3 ("Fonctionnalités Métier complètes") de la roadmap globale du projet. Une date de release cible pourra être discutée et fixée après la validation de la Phase 2 de cette mini-roadmap.

6.0 Conclusion

Ce document établit la nécessité stratégique et les exigences fonctionnelles pour le "Youth System". La recommandation principale est claire : l'adoption d'un système de génération procédurale est une étape cruciale pour concrétiser la vision à long terme de Ring General.

Les bénéfices sont fondamentaux : une rejouabilité quasi infinie, un défi stratégique constant pour le joueur, et surtout, la capacité à générer des récits émergents uniques qui deviendront la véritable signature du jeu. En transformant Ring General en un générateur d'histoires, nous nous assurons non seulement de sa longévité, mais aussi de sa capacité à créer une communauté de joueurs investis et passionnés. Ce PRD fournit une base solide et actionnable pour que l'équipe de développement puisse désormais entamer la phase de conception technique détaillée.

### Phase 4 : Performance & Optimisation (Mars 2026)

**Objectif** : Jeu fluide même après 100+ heures.

#### 4.1 Optimisations
```
TARGETS:
- App launch: < 3 sec
- UI navigation: < 200ms
- Show simulation: < 500ms
- DB queries: < 100ms (99th percentile)

TECHNIQUES:
- Connection pooling
- Lazy loading Views
- Caching stratégique
- Index DB pour queries fréquentes
- Memory profiling
```

#### 4.2 Stabilité
```
TESTS:
- 200+ heures de gameplay test
- Stress tests (10+ seasons without save)
- Memory leaks detection
- Crash reporting
```

### Phase 5 : QA & Polish (Avril 2026)

**Objectif** : Jeu prêt pour lancement.

#### 5.1 QA Complete
```
- 100% test coverage des critères acceptation
- User testing (10+ beta testers)
- Localisation complète (FR/EN)
- Tutoriel interactif
```

#### 5.2 Polish
```
- Animations UI
- Sound effects (optional)
- Tooltips complets
- Codex in-game (help system)
- Achievement system (optional)
```

---

## RISQUES ET MITIGATION

### Risques Techniques

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|-------------|-----------|
| **Performance DB** | Jeu lag après 100h | 🟡 MOYENNE | Caching, indexes, connection pooling |
| **Memory Leaks** | Crash après 20h | 🟡 MOYENNE | Memory profiling hebdomadaire |
| **Data Corruption** | Sauvegardes inutilisables | 🔴 HAUTE | Backup system, transactions DB |
| **Compatibilité Avalonia** | App crash Windows/Mac | 🟢 BASSE | Tests cross-platform prioritaire |
| **Refactoring bug** | Regression gameplay | 🔴 HAUTE | Test coverage (target: 80%+) |

### Risques de Contenu

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|-------------|-----------|
| **Balance gameplay** | Certains playstyles OP | 🟡 MOYENNE | Extensive balancing tests |
| **Content stalling** | Joueur "stuck" à 20h | 🟡 MOYENNE | Dynamic event generation |
| **Exploits** | Joueur game economie | 🟡 MOYENNE | Economy rebalancing post-beta |

### Risques Organisationnels

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|-------------|-----------|
| **Scope creep** | Retard de lancement | 🟡 MOYENNE | Sprint planning serré, priorités claires |
| **Dépendances externes** | Blocage sur BAKI | 🟢 BASSE | Fallback seed data in-place |
| **Documentation** | Onboarding difficile | 🟡 MOYENNE | Doc inline, guides automatiques |

---

## MÉTRIQUES ET KPIs

### Métriques de Développement

**Code Quality** 📊
```
TARGET (Release):
- Code Coverage: > 80%
- Architecture Score: 8.5+/10
- Technical Debt: < 5%
- Cyclomatic Complexity: avg < 8

CURRENT (Jan 2026):
- Code Coverage: ~60%
- Architecture Score: 8.5/10 ✅
- Technical Debt: ~10%
- Cyclomatic Complexity: avg ~7 ✅
```

**Performance Metrics** ⚡
```
TARGETS:
- App Launch: < 3 seconds
- UI Responsiveness: < 200ms (99th percentile)
- Show Simulation: < 500ms
- DB Queries: < 100ms

MEASUREMENT:
- Profiling tool (JetBrains DotMemory)
- Load testing (simulated 100h+ gameplay)
- Frame rate monitoring
```

**Gameplay Metrics** 🎮
```
ENGAGEMENT:
- Average playtime: > 50 hours (beta testers)
- Retention after 10h: > 70%
- Completion rate: > 40% (finish 1 season)

BALANCE:
- Worker usage diversity: > 70% roster used
- Winning % variance: 30-70% range
- Playstyle diversity: 3+ distinct viable strategies
```

### KPIs Post-Launch

| KPI | Target | Measurement |
|-----|--------|-------------|
| **DAU (Daily Active Users)** | 500+ | Login tracking |
| **Avg Session Length** | 2+ hours | Session analytics |
| **Retention (30 days)** | 50%+ | Returning users |
| **User Satisfaction** | 4.5+/5 ⭐ | Review aggregation |
| **Crash Rate** | < 0.1% | Error tracking |
| **Performance (P99)** | < 200ms | APM monitoring |

---

## GLOSSAIRE

### Termes Métier

**WORKER** : Catcheur/Lutteur/Wrestler
- Talent employé par une compagnie
- A des attributs (40), personnalité, morale
- Peut être blessé, suspendu, viré

**COMPANY** : Promotion de catch
- Entité gérée par le joueur
- A un Owner (stratégie) et Booker (créativité)
- A un style de catch, une région, un budget

**SHOW** : Événement de catch
- Composé de segments (matches, promos)
- A un rating, revenue, audience
- Simule automatiquement quand présenté

**SEGMENT** : Match, promo, ou skit dans un show
- Implique 1+ workers
- A une durée, type, booking style
- Génère un rating individuellement

**STORYLINE** : Feud ou angle entre workers
- Phases: BUILD → PEAK → BLOWOFF
- Accumule du heat chaque semaine
- Climax dans un match

**BOOKING** : Art de créer une carte de show
- Arrange les segments dans l'ordre
- Choisit les workers et les styles
- Valide structure et timing

**MORALE** : État émotionnel d'un worker
- Affecte sa performance (-30% si très basse)
- Affectée par les victoires, losses, push
- Peut déclencher departure si trop basse

**HEAT** : Émotion du crowd envers une feud
- Accumule à chaque segment
- Détermine quand passer à PEAK/BLOWOFF
- Plus haut = plus de revenus

**PERSONALITY** : Profil psychologique d'un worker
- 25+ types (Exemplary Professional, Diva, etc)
- Détecté automatiquement des attributs
- Affecte réactions à booking, morale, loyauté

**ATTRIBUTE** : Stat de compétence d'un worker
- 40 attributs au total (In-Ring, Entertainment, Story, Mental)
- 0-100 scale (sauf Mental: 0-20)
- Calculé sur base d'expérience et training

**OWNER** : Propriétaire strategique de la compagnie
- Prend décisions long-term
- Gère budget, personnel, partenariats
- Peut être contrôlé par joueur ou IA

**BOOKER** : Directeur créatif de la compagnie
- Crée les cartes et storylines
- Peut utiliser auto-booking IA
- Affecté par préférences et memories

**ERA** : Époque/période de la compagnie
- De 5 à 20 ans
- Marque la progression historique
- Peut avoir un thème (Foundation, Golden Age, etc)

---

## APPENDIX A: EXEMPLE DE GAMEPLAY

### Session Première Saison (Scénario)

```
SEMAINE 1-2: CRÉATION & PRÉPARATION
┌─ Joueur crée compagnie "Rising Sun Wrestling"
│  - Style: Lucha Libre
│  - Région: Mexico
│  - Owner Vision: Growth-focused
│  - Booker Preference: High-flying
│
├─ Recrute 5 workers locaux
├─ Crée 2 storylines embryonnaires
└─ Prépare premier show

SEMAINE 3: PREMIER SHOW
┌─ Book 6 matches (2h de duration)
├─ Execute show:
│  ├─ Match 1 (Undercard): 4.2 rating
│  ├─ Match 2 (Undercard): 5.1 rating
│  ├─ Match 3 (Mid-card): 6.8 rating (storyline progression!)
│  ├─ Match 4 (Mid-card): 5.9 rating
│  ├─ Main Event #1: 7.4 rating
│  └─ Main Event #2: 8.1 rating (crowd LOVED it)
│
└─ Overall show rating: 6.3/10
   Attendance: 400 (~good for Local)
   Revenue: $8,500 (tickets + merch)

SEMAINE 4: AFTERMATH & AJUSTEMENTS
┌─ Morale post-show:
│  ├─ Winners: +10 morale
│  ├─ Main eventers: +5 morale
│  ├─ Unused workers: -10 morale (building resentment)
│
├─ Storyline progression:
│  ├─ Storyline #1: 20 → 35 heat (in BUILD phase)
│  ├─ Storyline #2: 10 → 28 heat (gathering momentum)
│
├─ Events:
│  ├─ Rumor: "Local worker X says he's going pro"
│  └─ Injury: Worker Y gets minor injury (2 weeks out)
│
└─ Decision: Book week 4 show, focus on high-heat storylines

WEEKS 5-12: MID-SEASON DEVELOPMENT
├─ Shows progressively better (ratings improve)
├─ Storyline #1 reaches PEAK (heat: 65)
│  → Main event booking vs Storyline #2 (different feud)
│  → Creates interesting card dynamic
│
├─ Worker X development:
│  ├─ Started as undercard
│  ├─ Good matches → personality detected "Ambitious"
│  ├─ Morale high from push → asks for raise
│  ├─ Booker biases toward X (remembers good matches)
│  └─ X becomes mid-card staple
│
├─ Owner AI:
│  ├─ Notices good financial progress (+$15k)
│  ├─ Approves training budget increase
│  ├─ Recruits 2 more experienced workers
│
└─ Financial trajectory:
   Week 1: -$2k (startup costs)
   Week 4: +$6.5k (stable profitability)
   Week 12: +$8.2k/week average

WEEK 13: SEASON FINALE
├─ Storylines #1 & #2 both at BLOWOFF (heat: 100+)
├─ Book massive double main event
├─ Show rating: 8.8/10 (best of season!)
│
├─ Worker X vs Worker Y (Storyline #1)
│  → X wins decisively → Massive pop
│  → X morale to 85 (top reputation)
│
├─ Worker A vs Worker B (Storyline #2)
│  → Surprise finish (ref bump, outside interference)
│  → Story not over (continues to Season 2)
│
└─ Season Results:
   ✓ 13 shows completed
   ✓ 4 workers developed
   ✓ Avg show rating: 6.2
   ✓ Financial profit: +$95,000
   ✓ Promotion sized up: Local → Regional
   ✓ Next season: Can afford bigger shows, more workers
```

---

## APPENDIX B: MATRICE DE FEATURES PAR PHASE

| Feature | Phase | Notes |
|---------|-------|-------|
| Core Navigation | 1 | ✅ DONE |
| Dashboard | 1 | ✅ DONE |
| Worker Management | 1 | ✅ DONE |
| 40 Attributes | 1.5 | ✅ DONE |
| 25+ Personality Types | 1.5 | ✅ DONE |
| Booking System | 1.5 | ⚠️ 60% (missing UI) |
| Show Simulation | 1.9 | ✅ DONE |
| Auto-Booking IA | 1.9 | ✅ DONE |
| Company Hub | 2 | 🔜 In Design |
| Advanced Finances | 2 | 🔜 In Design |
| Advanced Contracts | 2 | 🔜 In Design |
| Weekly Loop | 3 | 🔜 In Design |
| Storylines Advanced | 3 | 🔜 In Design |
| Youth Development | 3 | ⚠️ 30% (partial) |
| Scouting System | 3 | ⚠️ 40% (partial) |
| Medical/Injuries | 4 | ⚠️ 30% (partial) |
| Broadcasting Deals | 4 | 🔜 In Design |
| Modding Support | 4 | 🔜 In Design |
| Achievement System | 5 | 🔜 Optional |
| Localization EN | 5 | 🔜 Optional |

---

## APPENDIX C: DÉFINITION DONE (Definition of Done)

### Pour une Feature

**Acceptation Criteria:**
- [ ] Code implémenté selon design
- [ ] Tests unitaires (min 80% coverage)
- [ ] Tests d'intégration réussis
- [ ] Aucun crash identifié (10h+ testing)
- [ ] Performance acceptable (< 200ms UI, < 500ms simulation)
- [ ] Code review approuvé
- [ ] Documentation mise à jour
- [ ] Commit avec message clair

### Pour une Phase

**Acceptation Criteria:**
- [ ] Tous les features marqués DONE
- [ ] Aucun bug critique ouvert
- [ ] Playtesting 20+ heures sans issue
- [ ] Performance testing réussi
- [ ] Documentation complète
- [ ] README/CHANGELOG mis à jour
- [ ] Tag git créé (v1.0, v1.5, etc)
- [ ] Branche mergée vers main

---

## APPENDIX D: QUESTIONS FRÉQUENTES (FAQ)

### Pourquoi Avalonia vs WPF/WinForms?
**R:** Cross-platform support (Windows/Mac/Linux), moderne, reactive UI naturel avec MVVM.

### Peut-on moder le jeu?
**R:** OUI - Specs JSON sont data-driven, futures releases auront modding SDK.

### Support multijoueur?
**R:** Non pour 1.0, envisagé pour 2.0+ (online leagues).

### Sauvegarde cloud?
**R:** Non pour 1.0, envisagé pour 2.0+ (Steam Cloud ou custom).

### Performance sur vieilles machines?
**R:** Minimum requirement: .NET 8 runtime (~100MB). Optimisation cible: 60 FPS sur GPU intégré.

---

## REMERCIEMENTS & RESSOURCES

**Inspirations**
- Football Manager 2026 (interface, depth)
- Total Extreme Wrestling (gameplay)
- Wrestling simulation communities

**Documentation Officielle**
- [Avalonia UI Docs](https://docs.avaloniaui.net/)
- [.NET 8 Docs](https://learn.microsoft.com/dotnet/)
- [SQLite Docs](https://www.sqlite.org/docs.html)

**Contacts & Support**
- **Développement** : Claude (AI Architect)
- **Repository** : github.com/SnakePythonDom/Ring-General-Rework.Exe
- **Issues** : GitHub Issues tracker

---

**Document PRD Version 2.0**
**Date:** 8 janvier 2026
**Statut:** 🎯 ACTIF (Phase 1.9 complète, Phase 2 en démmarrage)
**Prochaine Révision:** 15 janvier 2026

*Ce document est la source de vérité unique pour tous les aspects produit de Ring General.*
