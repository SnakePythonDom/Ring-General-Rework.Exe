# Youth Generation System & Child Company Integration

## Overview

This document details the technical implementation of the Youth Generation System and how it integrates with Child Companies in RingGeneral.

## 1. Youth Generation System

The Youth Generation System is responsible for spawning new workers (trainees) into the game world. This logic is encapsulated primarily in `RingGeneral.Core.Simulation.WorkerGenerationService`.

### 1.1 Trigger Mechanism

The system runs on a weekly basis via `WorkerGenerationService.GenerateWeekly(GameState state, int seed)`.

*   **Frequency:**
    *   **Annual (Default):** Occurs once a year on a specific "Pivot Week" (defined in options or defaults).
    *   **Monthly (Abundant Mode):** Can occur every month if the game option `YouthMode` is set to `Abundant`.
*   **Conditions:**
    *   The system checks if the current week matches the generation interval.
    *   If `YouthMode` is `Desactivee`, no generation occurs.

### 1.2 Generation Logic per Structure

The service iterates through all active `YouthStructures` in the database. For each structure, it performs the following checks and calculations:

1.  **Eligibility Checks:**
    *   **Active Status:** The structure must be active.
    *   **Cooldown:** Checks `DerniereGenerationSemaine` (Last Generation Week) against the cooldown period (Annual or Monthly).
    *   **Capacity:** Checks if the structure has reached its maximum trainee capacity.
    *   **Global/Regional Caps:** Checks if the global number of trainees or the specific region/company caps have been reached.

2.  **Quantity Calculation:**
    The number of workers to generate is calculated in `CalculerBaseQuantite(YouthStructureState youth)`:
    *   **Base:** Based on `YouthStructure.Type` (e.g., Dojo, Performance Center).
    *   **Bonuses:**
        *   **Infrastructure:** Bonus per level of `NiveauEquipements`.
        *   **Budget:** Bonus based on `BudgetAnnuel` tiers.
        *   **Coaching:** Bonus based on `QualiteCoaching`.
    *   **Random Fluctuation:** A random factor (0.85 to 1.15) is applied.
    *   **Clamping:** The final number is clamped to available slots (caps).

3.  **Worker Creation (`GenererWorker`):**
    *   **Attributes:** Base stats (In Ring, Entertainment, Story) are generated (typically 4-9 base).
    *   **Bonuses:**
        *   **Region:** Applies bonuses based on the region's style (e.g., Japan adds to Technique/Strong Style).
        *   **Philosophy:** Applies bonuses based on the structure's philosophy (e.g., "PURE" adds to In Ring, "ENTERTAINMENT" adds to Promo).
    *   **Identity:** Name, Age, and Nationality are assigned.
    *   **Type:** Created as `TRAINEE` and assigned to the `YouthStructure`.

### 1.3 Persistence

*   **`YouthRepository.EnregistrerGeneration`:** Persists the generated workers to the `Workers` table.
*   **`YouthTrainees` Table:** Links the new worker to the `YouthStructure`.
*   **Counters:** Updates global and regional counters for caps.
*   **State:** Updates `YouthGenerationState` with the current week to reset the cooldown.

---

## 2. Child Company Integration

Child Companies are integrated with the Youth System specifically when their objective is set to **Development**.

### 2.1 Data Model

*   **`ChildCompaniesExtended` Table:**
    *   `ChildCompanyId` (PK)
    *   `ParentCompanyId` (FK)
    *   `Objective`: Enum (Development, Expansion, Entertainment, Niche, Independence)
    *   `YouthStructureId`: Nullable Foreign Key to `YouthStructures`.

### 2.2 Automatic Structure Creation

The linkage is handled in `RingGeneral.Core.Services.ChildCompanyService`.

When `CreateChildCompanyAsync` is called:

1.  **Check Objective:** If `objective == ChildCompanyObjective.Development`.
2.  **Create Structure:** Calls `CreateYouthStructureForChildCompanyAsync`.
    *   Generates a new `YouthStructureId`.
    *   Creates a `YouthStructure` record via `YouthRepository.CreateYouthStructureAsync`.
    *   **Defaults:**
        *   **Name:** "Development Center - [Company ID]"
        *   **Type:** "DEVELOPMENT"
        *   **Budget:** 100,000
        *   **Capacity:** 20
        *   **Philosophy:** "HYBRIDE"
3.  **Link:** The returned `YouthStructureId` is saved in the `ChildCompaniesExtended` record.

### 2.3 Accessing the Structure

*   **From Code:** The `ChildCompanyExtended` object contains the `YouthStructureId`.
*   **From Database:** Join `ChildCompaniesExtended` with `YouthStructures` on `YouthStructureId`.

### 2.4 Summary of Flow

```
User Action -> Create Child Company (Objective: Development)
    |
    v
ChildCompanyService.CreateChildCompanyAsync
    |
    +--> YouthRepository.CreateYouthStructureAsync
    |       |
    |       +--> INSERT INTO YouthStructures (...)
    |
    +--> INSERT INTO ChildCompaniesExtended (..., YouthStructureId, ...)
```

This ensures that every "Development" child company automatically functions as a Youth Structure, capable of generating new workers for the parent company during the weekly generation loops.
