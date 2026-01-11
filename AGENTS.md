To adapt the 3-Layer Architecture specifically to the Ring General Rework, we treat the project as a transition from a simple data-entry tool to a complex, high-density simulation. This approach ensures that as you add features like "Chemistry," "Injuries," or "Brand Splits," the UI remains responsive and the game logic remains bulletproof.

The Ring General 3-Layer Framework
Layer 1: Directive (The Design Truth)
Location: docs/directives/

Role: This is the "Product Owner" layer. It contains the ASCII mockups, feature specifications, and rules for the wrestling world.

Example: The UI_MOCKUPS_ASCII.md file defines exactly how the 7-tab ProfileView should look and what data it must display (e.g., "Aura," "Mic Work," "Safety").

Layer 2: Orchestration (The UI logic)
Location: src/RingGeneral.UI/ViewModels/

Role: This is the "Glue." As the AI, you live here. You translate user intent (like clicking "Sauvegarder") into structured commands for the engine.

Example: When a player clicks [✏ Modifier], the ViewModel toggles the IsEditing state, changing the static text into input fields for the attributes.

Layer 3: Execution (The Simulation Engine)
Location: src/RingGeneral.Domain/Services/

Role: This is the "Referee." It handles the deterministic C# math that never changes, regardless of the UI.

Example: The calculation that determines if a match note is 85 or 92 happens here, using the worker's attributes and current momentum.