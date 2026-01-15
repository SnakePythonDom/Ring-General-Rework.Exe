# 🎯 RING GENERAL - DIAGRAMMES D'ARCHITECTURE ET FLUX

**Date** : 2026-01-15  
**Version** : Architecture complète du projet

---

## 📐 1. ARCHITECTURE GLOBALE DU SYSTÈME

```mermaid
graph TB
    subgraph "UI Layer (Avalonia + ReactiveUI)"
        UI[ViewModels & Views]
        Shell[Shell/Navigation]
        Dashboard[Dashboard]
        Booking[Booking Views]
        Roster[Roster Management]
        CompanyHub[Company Hub]
    end

    subgraph "Application Layer"
        App[App.axaml.cs<br/>DI Container]
    end

    subgraph "Core Layer (Business Logic)"
        Services[Services<br/>⚙️ 30+ Services]
        Engines[Engines<br/>🤖 AI/Simulation]
        Models[Domain Models<br/>📦 Owner, Booker, Worker, Show]
        Interfaces[Interfaces<br/>🔌 Contracts]
    end

    subgraph "Data Layer (SQLite)"
        Repos[Repositories<br/>🗄️ 36+ Repos]
        DB[(SQLite Database<br/>📊 Migrations)]
    end

    subgraph "Specs Layer"
        JSON[JSON Configs<br/>📋 Match Types, Segments]
    end

    UI --> App
    App --> Services
    App --> Repos
    Services --> Engines
    Services --> Models
    Services --> Interfaces
    Repos --> DB
    Engines --> Repos
    Services --> JSON

    style UI fill:#e1f5ff
    style Services fill:#fff4e1
    style Engines fill:#ffe1f5
    style Repos fill:#e1ffe1
    style DB fill:#f0f0f0
```

---

## ⏱️ 2. FLUX: SYSTÈME QUOTIDIEN (DAILY TIME SYSTEM)

```mermaid
sequenceDiagram
    participant User
    participant UI as DashboardViewModel
    participant Time as TimeOrchestratorService
    participant Daily as DailyServices
    participant Finance as DailyFinanceService
    participant Show as ShowDayOrchestrator
    participant Contract as DailyContractService
    participant DB as Database

    User->>UI: Clic "Jour Suivant"
    UI->>Time: AdvanceDayAsync(companyId)
    
    activate Time
    Time->>DB: GetCurrentDate(companyId)
    DB-->>Time: 2026-01-15
    Time->>DB: IncrementDay(companyId)
    DB-->>Time: NewDate = 2026-01-16
    
    rect rgb(255, 240, 225)
        Note over Time,Daily: Phase 1: Services Quotidiens
        Time->>Daily: ExecuteDailyServicesAsync(companyId, date)
        activate Daily
        
        Daily->>Finance: ProcessDailyFinances(companyId, date)
        Finance->>DB: Apply Arena Rental, Utilities
        
        Daily->>Contract: ProcessContractPayments(companyId, date)
        Contract->>DB: Hybrid Contract Payments
        
        Daily-->>Time: Services Completed
        deactivate Daily
    end
    
    rect rgb(225, 255, 240)
        Note over Time,Show: Phase 2: Show Day Detection
        Time->>Show: DetecterShowAVenir(companyId, date)
        activate Show
        Show->>DB: GetShowsForDate(date)
        DB-->>Show: Show "Monday Night Raw"
        
        alt Show exists
            Show->>Show: SimulateShow()
            Show->>DB: Update Results, Finances
            Show-->>Time: ShowReport
        else No show
            Show-->>Time: null
        end
        deactivate Show
    end
    
    Time-->>UI: DayAdvanceResult
    deactivate Time
    UI-->>User: Update Dashboard
```

---

## 🎬 3. FLUX: SHOW DAY (SIMULATION COMPLÈTE)

```mermaid
graph TB
    Start([Show Detected])
    
    Start --> Load[ShowDayOrchestrator.DetecterShowAVenir]
    Load --> Context[Charger ShowContext<br/>- Show, Company, Segments<br/>- Workers, Titles, Chemistry]
    
    Context --> Plan[Créer BookingPlan<br/>- SegmentSimulationContext<br/>- WorkerHealth]
    
    Plan --> Sim[ShowSimulationEngine.SimulerShow]
    
    subgraph "Simulation Segment par Segment"
        Sim --> S1[Segment 1: Calcul Rating]
        S1 --> S2[Segment 2: Calcul Rating]
        S2 --> S3[Segment N: Calcul Rating]
    end
    
    S3 --> Rating[Calculer AudienceRating<br/>- Base + TvDeal Bonus<br/>- Match Quality<br/>- Entropy]
    
    Rating --> Report[Créer ShowReport<br/>- SegmentResults<br/>- AudienceRating<br/>- Revenue]
    
    Report --> Impact[ImpactApplier.AppliquerImpacts]
    
    subgraph "Application des Impacts"
        Impact --> I1[💰 Finances<br/>Tickets + Merch + TV]
        Impact --> I2[⭐ Popularité<br/>Workers Momentum]
        Impact --> I3[🏆 Titles<br/>Championship Changes]
        Impact --> I4[💪 Fatigue/Injury<br/>Physical Toll]
    end
    
    I4 --> Morale[BackstageRepository.AppliquerMoraleImpacts<br/>Workers non-bookés: -3 morale]
    
    Morale --> Flux2[DailyContractService<br/>Per-Appearance Fees]
    
    Flux2 --> Save[Enregistrer ShowReport<br/>+ AudienceHistory]
    
    Save --> End([Show Terminé])
    
    style Sim fill:#ffe1f5
    style Impact fill:#fff4e1
    style Save fill:#e1ffe1
```

---

## 🤖 4. FLUX: AUTO-BOOKING (BOOKER AI)

```mermaid
graph LR
    subgraph "Input"
        User[Joueur active<br/>'Let Booker Decide']
        ShowCtx[ShowContext<br/>Workers disponibles]
        Constraints[AutoBookingConstraints<br/>Budget, Durée]
    end
    
    subgraph "BookerAIEngine"
        Load[Charger Booker<br/>+ Memories + Era]
        
        Load --> Filter[Filtrer Workers<br/>- Fatigue<br/>- Injuries<br/>- Personality Conflicts]
        
        Filter --> Archetype[Sélection par Archétype<br/>- PowerBooker: Stars<br/>- Puroresu: Technique<br/>- AttitudeEra: Entertainment<br/>- ModernIndie: Rotation]
        
        Archetype --> MainEvent[Créer Main Event<br/>Basé sur préférences]
        
        MainEvent --> Storylines[Segments Storylines<br/>Si actives]
        
        Storylines --> Fill[Remplir Show<br/>Matches + Promos]
        
        Fill --> Style[Appliquer Style Archétype<br/>- Durée segments<br/>- Intensité]
        
        Style --> Era[Appliquer Influence Era<br/>- Technical: +5min<br/>- Hardcore: +25 intensity]
    end
    
    subgraph "Output"
        Segments[Liste SegmentDefinition<br/>Prête pour simulation]
    end
    
    User --> Load
    ShowCtx --> Filter
    Constraints --> Filter
    Era --> Segments
    
    style Load fill:#e1f5ff
    style Archetype fill:#ffe1f5
    style Era fill:#fff4e1
```

---

## 👥 5. FLUX: RECRUTEMENT (MARCHÉ DES AGENTS LIBRES)

```mermaid
sequenceDiagram
    participant User
    participant UI as FreeAgentsViewModel
    participant Recruit as RecruitmentService
    participant Dialog as RecruitmentDialogViewModel
    participant Worker as WorkerRepository
    participant Contract as ContractRepository
    participant Youth as YouthRepository
    participant DB as Database

    User->>UI: Browse Free Agents
    UI->>Worker: GetFreeAgents()
    Worker->>DB: SELECT * FROM Workers WHERE CompanyId IS NULL
    DB-->>Worker: List<Worker>
    Worker-->>UI: Free Agents

    User->>UI: Select Agent + Clic "Recruit"
    UI->>Dialog: OpenRecruitmentDialog(agentId)
    
    Dialog-->>User: Afficher options:<br/>1️⃣ Main Roster<br/>2️⃣ Child Company<br/>3️⃣ Youth Structure

    alt Option 1: Main Roster
        User->>Dialog: SignToMainRoster(salary)
        Dialog->>Recruit: SignToMainRosterAsync(agentId, companyId, salary)
        
        activate Recruit
        Recruit->>Worker: GetWorker(agentId)
        Worker-->>Recruit: Worker
        
        alt Worker Age < 18
            Recruit-->>Dialog: ⚠️ Warning: Talent pas fini
        end
        
        Recruit->>Worker: UpdateWorker(companyId, Type=Wrestler)
        Recruit->>Contract: AjouterContratActif(workerId, salary)
        Recruit-->>Dialog: ✅ Success
        deactivate Recruit

    else Option 2: Child Company
        User->>Dialog: SignToChildCompany(childCompanyId, salary)
        Dialog->>Recruit: SignToChildCompanyAsync(agentId, parentId, childId, salary)
        Recruit->>Worker: UpdateWorker(childCompanyId, Type=ChildCompanyWrestler)
        Recruit-->>Dialog: ✅ Envoyé en développement

    else Option 3: Youth Structure
        User->>Dialog: SignToYouthStructure(youthId)
        Dialog->>Recruit: SignToYouthStructureAsync(agentId, companyId, youthId)
        
        activate Recruit
        alt Worker Age >= 25
            Recruit-->>Dialog: ❌ Trop âgé (Max 25 ans)
        else
            Recruit->>Worker: UpdateWorker(companyId, Type=Trainee)
            Recruit->>Youth: LinkTraineeToStructure(youthId, workerId)
            Recruit-->>Dialog: ✅ Maintenant élève
        end
        deactivate Recruit
    end

    Dialog-->>User: Close Dialog + Refresh List
```

---

## 🏢 6. FLUX: GESTION ENTREPRISE (OWNER DECISION ENGINE)

```mermaid
graph TB
    Start([Événement Décisionnel])
    
    Start --> Type{Type de Décision}
    
    Type -->|Budget| Budget[OwnerDecisionEngine<br/>ApprovesBudget]
    Budget --> BudgetLogic[Vérifier:<br/>- RiskTolerance<br/>- VisionType<br/>- % Trésorerie]
    BudgetLogic --> BudgetResult{Approved?}
    BudgetResult -->|Oui| Proceed[Autoriser Dépense]
    BudgetResult -->|Non| Block[❌ Bloquer Dépense]
    
    Type -->|Embauche| Hire[OwnerDecisionEngine<br/>ShouldHireTalent]
    Hire --> HireLogic[Évaluer:<br/>- Roster Size<br/>- Talent Pop/Skill<br/>- Owner VisionType]
    HireLogic --> HireResult{Embaucher?}
    HireResult -->|Oui| DoHire[✅ Offrir Contrat]
    HireResult -->|Non| Skip[Ignorer Talent]
    
    Type -->|Satisfaction| Sat[OwnerDecisionEngine<br/>CalculateOwnerSatisfaction]
    Sat --> SatLogic[Calculer basé sur:<br/>- Financial Performance<br/>- Creative Performance<br/>- Fan Growth<br/>Pondéré par priorités]
    SatLogic --> SatResult[Score 0-100]
    SatResult --> SatAction{Score < 40?}
    SatAction -->|Oui| Fire[ShouldReplaceBooker<br/>Évaluer licenciement]
    SatAction -->|Non| Keep[Conserver Booker]
    
    Type -->|Transition DNA| Trans[AnalyzeTransitionCost]
    Trans --> TransLogic[Calculer:<br/>- Distance DNA<br/>- Coût estimé<br/>- Durée semaines<br/>- Risques]
    TransLogic --> TransResult[TransitionCostAnalysis]
    TransResult --> TransAction{Viable?}
    TransAction -->|Oui| Approve[✅ Approuver Transition]
    TransAction -->|Non| Reject[❌ Rejeter Transition]
    
    style Budget fill:#e1f5ff
    style Hire fill:#ffe1f5
    style Sat fill:#fff4e1
    style Trans fill:#e1ffe1
```

---

## 🔄 7. DIAGRAMME D'INTERACTION DES COMPOSANTS

```mermaid
graph TB
    subgraph "Frontend (UI)"
        Dashboard[DashboardViewModel]
        Booking[BookingViewModel]
        Roster[RosterViewModel]
        Profile[ProfileViewModel]
        CompanyHub[CompanyHubViewModel]
    end
    
    subgraph "Orchestration Services"
        TimeOrch[TimeOrchestratorService<br/>⏱️ Daily Progression]
        ShowOrch[ShowDayOrchestrator<br/>🎬 Show Simulation]
    end
    
    subgraph "Core Services"
        BookerAI[BookerAIEngine<br/>🤖 Auto-Booking]
        OwnerDecision[OwnerDecisionEngine<br/>🏢 Strategic Decisions]
        Recruitment[RecruitmentService<br/>👥 Hiring]
        SimEngine[ShowSimulationEngine<br/>📊 Rating Calculation]
        ImpactApplier[ImpactApplier<br/>💥 Apply Results]
    end
    
    subgraph "Secondary Services"
        Morale[MoraleEngine]
        Personality[PersonalityEngine]
        Trends[TrendEngine]
        Youth[WorkerGenerationService]
    end
    
    subgraph "Repositories"
        GameRepo[GameRepository<br/>🎮 Façade]
        ShowRepo[ShowRepository]
        WorkerRepo[WorkerRepository]
        OwnerRepo[OwnerRepository]
        BookerRepo[BookerRepository]
    end
    
    subgraph "Database"
        SQLite[(SQLite DB<br/>📊 36+ Tables)]
    end
    
    Dashboard --> TimeOrch
    Booking --> BookerAI
    CompanyHub --> OwnerDecision
    Roster --> Recruitment
    
    TimeOrch --> ShowOrch
    ShowOrch --> SimEngine
    SimEngine --> ImpactApplier
    
    BookerAI --> BookerRepo
    BookerAI --> Personality
    
    OwnerDecision --> OwnerRepo
    OwnerDecision --> Morale
    
    Recruitment --> WorkerRepo
    Recruitment --> Youth
    
    ImpactApplier --> GameRepo
    
    GameRepo --> ShowRepo
    GameRepo --> WorkerRepo
    GameRepo --> OwnerRepo
    
    ShowRepo --> SQLite
    WorkerRepo --> SQLite
    OwnerRepo --> SQLite
    BookerRepo --> SQLite
    
    Trends --> SQLite
    Youth --> SQLite
    
    style Dashboard fill:#e1f5ff
    style TimeOrch fill:#fff4e1
    style BookerAI fill:#ffe1f5
    style GameRepo fill:#e1ffe1
    style SQLite fill:#f0f0f0
```

---

## 📊 8. FLUX DE DONNÉES: WORKER LIFECYCLE

```mermaid
stateDiagram-v2
    [*] --> Generated: WorkerGenerationService
    
    Generated --> FreeAgent: Spawn as Free Agent
    Generated --> Trainee: Spawn in Youth Structure
    
    FreeAgent --> MainRoster: RecruitmentService<br/>SignToMainRoster
    FreeAgent --> ChildCompany: SignToChildCompany
    FreeAgent --> YouthStructure: SignToYouthStructure
    
    Trainee --> Graduated: YouthRepository<br/>DiplomerTrainee
    Graduated --> MainRoster
    
    ChildCompany --> MainRoster: Promotion
    YouthStructure --> MainRoster: Graduation
    
    MainRoster --> Booked: Selected for Show
    Booked --> Performance: ShowSimulationEngine
    Performance --> MainRoster: Results Applied
    
    MainRoster --> Inactive: Contract Expired
    MainRoster --> Released: Terminated
    
    Inactive --> Retired: NegotiateReconversion<br/>(if veteran)
    Retired --> Staff: Become Trainer/Coach
    
    Released --> FreeAgent: Back to Market
    
    note right of Performance
        Impacts applied:
        - Fatigue +10-30
        - Popularity ±5-15
        - Momentum ±10
        - Morale (if not booked: -3)
    end note
```

---

## 🎯 9. RÉSUMÉ DES FLUX PRINCIPAUX

| Flux | Orchestrateur | Fréquence | Composants Clés |
|------|---------------|-----------|-----------------|
| **Daily Time System** | `TimeOrchestratorService` | Quotidien | DailyServices, ShowDayOrchestrator |
| **Show Day** | `ShowDayOrchestrator` | Par show | ShowSimulationEngine, ImpactApplier |
| **Auto-Booking** | `BookerAIEngine` | On-demand | BookerRepository, Memories, Era |
| **Recruitment** | `RecruitmentService` | Manuel | WorkerRepository, ContractRepository |
| **Owner Decisions** | `OwnerDecisionEngine` | Événementiel | OwnerRepository, Budget validation |
| **Worker Generation** | `WorkerGenerationService` | Hebdomadaire | YouthRepository, Caps tracking |
| **Morale/Rumors** | `MoraleEngine` | Weekly Loop | BackstageRepository |
| **Trends** | `TrendEngine` | Weekly | TrendRepository, Era influence |

---

## 📋 10. LÉGENDE DES SYMBOLES

| Symbole | Signification |
|---------|---------------|
| 🎮 | GameRepository (Façade) |
| ⏱️ | Time/Orchestration Services |
| 🎬 | Show/Simulation Services |
| 🤖 | AI/Engine Services |
| 👥 | Worker/Recruitment Services |
| 🏢 | Company/Owner Services |
| 💰 | Finance Services |
| 🗄️ | Repositories |
| 📊 | Database/Models |
| 🔌 | Interfaces |
| ⚙️ | Services |

---

## 🔍 NOTES D'ARCHITECTURE

### Points Forts

✅ **Séparation claire des responsabilités** (UI → Services → Repos → DB)  
✅ **Pattern Façade** avec GameRepository orchestrant 9 repositories spécialisés  
✅ **Injection de dépendances** complète dans App.axaml.cs  
✅ **Services hautement cohésifs** (BookerAI = 1404 lignes de logique pure)  
✅ **Flux orchestrés** (TimeOrchestrator → ShowOrchestrator → Simulation → Impact)

### Points d'Attention

⚠️ **UI Phase 4 manquante** : Owner/Booker management UI non créée  
⚠️ **Tests limités** : Scénarios d'intégration Phase 4 manquants  
⚠️ **Phase 5 en attente** : Crisis et Communication définis mais non implémentés
