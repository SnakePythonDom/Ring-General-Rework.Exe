# System Overview & Architecture Status (Jan 2026)

This document provides a technical overview of the current state of the Ring General codebase, verified against the actual source code.

## 1. Core Architecture

The solution follows a standard 4-layer architecture:

- **RingGeneral.UI (Avalonia)**: MVVM pattern with ReactiveUI.
- **RingGeneral.Core**: Business logic, services, and domain models.
- **RingGeneral.Data**: SQLite repositories and database management.
- **RingGeneral.Specs**: JSON configuration files.

### Key Infrastructure
- **Dependency Injection**: Fully implemented in `App.axaml.cs`.
- **Database**: SQLite with versioned migrations (`data/migrations/`).
- **Repositories**: 30+ specialized repositories (Owner, Booker, Show, Worker, etc.).

## 2. Functional Subsystems (Verified)

### 2.1 Governance & Company Identity
*Implemented in: `OwnerRepository`, `BookerRepository`, `CatchStyleRepository`*

- **Entities**: `Owner` (Strategic) and `Booker` (Creative) entities are fully modeled and persisted.
- **Creation**: The `CreateCompanyViewModel` correctly generates a default Owner and Booker when a new game is started, ensuring no data orphanage.
- **Styles**: `CatchStyle` system is active, influencing match ratings and booking preferences.

### 2.2 Show Simulation Loop
*Implemented in: `ShowDayOrchestrator`, `ShowSimulationEngine`, `ImpactApplier`*

The "Show Day" flux is fully operational:
1.  **Detection**: `ShowDayOrchestrator.DetecterShowAVenir` finds today's show.
2.  **Simulation**: `ShowSimulationEngine` calculates segment ratings based on worker stats, match types, and chemistry.
3.  **Impacts**: `ImpactApplier` processes the results to update:
    -   **Finances**: Ticket sales, Merch, TV rights.
    -   **Popularity/Momentum**: Workers gain/lose standing.
    -   **Titles**: Championships change hands automatically.
    -   **Fatigue/Injury**: Physical toll is applied.
4.  **Post-Processing**:
    -   **Flux 2 (Per-Appearance)**: Fees are deducted immediately after the show.
    -   **Morale**: Workers *not* booked suffer a morale penalty (-3).

### 2.3 Auto-Booking (AI)
*Implemented in: `BookerAIEngine`*

The AI Booker is highly sophisticated and capable of generating full show cards:
-   **Archetypes**: Supports different booking styles (Power Booker, Puroresu, Attitude Era, Modern Indie).
-   **Memory System**: `BookerMemory` tracks past matches and events to influence future decisions (e.g., repeating successful pairings, pushing workers who succeeded).
-   **Constraints**: Respects fatigue, injuries, and owner mandates.
-   **Structure**: Can build Main Events, Mid-card matches, and Promos based on the "Era" influence.

### 2.4 Worker Generation & Youth
*Implemented in: `WorkerGenerationService`, `YouthRepository`, `ChildCompanyService`*

-   **Weekly Gen**: Triggered weekly to spawn Trainees or Free Agents based on caps (Global/Regional/Company).
-   **Child Companies**: Creating a "Development" child company automatically provisions a linked `YouthStructure`.
-   **Persistence**: Youth structures and their trainees are persisted in the database.

### 2.5 Trends & World Living
*Implemented in: `TrendEngine`, `EraTransitionService`*

-   **Trends**: Procedural generation of trends (Style, Format, Audience) that affect match ratings.
-   **Progress**: Trends evolve weekly (`ProgressTrendsAsync`) and eventually expire.

## 3. Gap Analysis & Missing Features

Despite robust backend logic, several UI and integration pieces are pending.

### 3.1 Company Hub UI (Phase 3 - Partial)
*Status: Skeleton Only*

The `CompanyHubViewModel` exists but is incomplete compared to the design spec:
-   **Missing Child ViewModels**: The architecture calls for 5 sub-tabs (`Profile`, `Staff`, `Roster`, `Teams`, `History`), but their corresponding ViewModels and Views are **missing** from `src/RingGeneral.UI/ViewModels/CompanyHub/`.
-   **Rivals View**: Logic to load rival companies is marked as `TODO`.
-   **Staff Hiring**: Basic logic exists (`HireStaffAsync`), but the UI selection flow is not implemented.

### 3.2 Creative Staff Integration
*Status: Pending*

-   `BookerAIEngine` contains `TODO` references to `ICreativeStaffRepository`. Currently, it uses hardcoded defaults for "Creative Staff Preferences".

### 3.3 Owner/Booker Customization
*Status: Basic*

-   While `CreateCompanyViewModel` generates these entities, it currently uses hardcoded defaults (e.g., "Balanced" vision, 50/50 priorities). The UI does not yet allow the player to customize their Owner personality or Booker stats during creation.

## 4. Next Steps Recommendation

1.  **Implement Company Hub Tabs**: Create the missing ViewModels and Views to make the Company Hub fully functional.
2.  **Rivals Integration**: Implement the data loading for rival companies in the Hub.
3.  **Governance UI**: Add UI controls in `CreateCompanyView` to let players tweak their Owner/Booker stats before game start.
