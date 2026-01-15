RingGeneral: Architectural Evolution Master Plan
Objective: Decouple business logic from the UI (MVVM) moving toward Clean Architecture, without a total rewrite ("Strangler Fig Pattern"). Current State: Bloated ViewModels, heavy dependencies, hard to test. Target State: Lightweight ViewModels → Facades → Use Cases → Domain/Data.

1. The 3 Ironclad Rules (Non-Negotiable)
🛑 UI Immunity: Never touch existing XAML or bindings. The game must remain visually identical.

🐢 Atomic Migration: Migrate one command at a time. No "Big Bang" refactors or massive module rewrites.

✅ Zero Regression: The game must compile and be playable after every single commit. Old code coexists with new code until safely removed.

2. The Target Architecture
We are introducing an intermediate layer to sanitize the code:

Presentation Layer (Legacy): Existing ViewModels. They no longer contain logic; they only delegate.

Application Layer (New):

Facades: Unique entry points per domain (e.g., BookingActions).

Use Cases: 1 Class = 1 Business Action (e.g., ValidateBooking).

Result Pattern: Standardized return types (Success/Failure) to eliminate exception handling in UI.

Infrastructure Layer (Legacy/Evolution): Existing Repositories, progressively masked behind specific interfaces (ReadModels).

3. Agent Protocol (Roles)
Any contributor (Human or AI) must adopt one of these roles for a given task:

📐 The Architect
Mission: Define contracts (Interfaces), folder structure, and data models (DTOs).

Focus: Organization & Solidity.

🔨 The Worker (Refactorer)
Mission: Extract logic from the ViewModel ("Dirty Code") into a Use Case ("Clean Code").

Focus: Smart copy-paste & dependency cleanup.

🛡️ The Controller (QA)
Mission: Verify that migration hasn't altered behavior. Write unit tests for new Use Cases.

Focus: Stability & Security.

4. Migration Workflow (Standard Loop)
For every feature to be migrated, follow this loop:

Isolation: Identify the specific Command in the ViewModel.

Creation: Create the corresponding Use Case in the Application layer.

Derivation: Replace the code in the ViewModel with a call to the Facade.

Cleanup: Remove old injected services that are no longer needed in the ViewModel.

Verification: Manual test or Unit test.