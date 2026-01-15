# 📋 PLAN D'EXÉCUTION - PHASES 4, 5 & 6

**Projet** : Ring General - Advanced Systems Implementation
**Responsable** : Chef de Projet
**Date de début** : 2026-01-08
**Durée estimée** : 9-13 semaines

---

## 🎯 OBJECTIFS GLOBAUX

Implémenter les **3 phases avancées** du système Ring General :

1. **Phase 4** : Owner & Booker Systems (IA booking, préférences, mémoires)
2. **Phase 5** : Crisis & Communication Management (pipeline de crises, communication joueur)
3. **Phase 6** : AI World & Company Eras (simulation monde, eras de compagnie, LOD)

Ces phases complètent la **fondation technique** posée par les Phases 1-3 (Personality, Nepotism, Morale & Rumors).

---

## 📊 STATUT ACTUEL

### ✅ Phases Complétées

| Phase | Système | Statut | Date Completion |
|-------|---------|--------|-----------------|
| **Phase 1** | Personality & Mental System | ✅ 100% | 2026-01-08 |
| **Phase 2** | Relations & Nepotism | ✅ 100% | 2026-01-08 |
| **Phase 3** | Morale & Rumors | ✅ 100% | 2026-01-08 |

**Livrables Phase 1-3** :
- 18 migrations SQL appliquées
- 25+ models créés
- 9 engines/services implémentés
- 6 repositories complets
- UI Dashboard intégrée (morale card)
- Weekly Loop intégré (morale/rumors)

### 🔜 Phases à Venir

| Phase | Système | Priorité | Durée | Dépendances |
|-------|---------|----------|-------|-------------|
| **Phase 4** | Owner & Booker Systems | 🟡 MOYENNE | 4-5 sem | Phase 1, 3 |
| **Phase 5** | Crisis & Communication | 🟡 MOYENNE | 2-3 sem | Phase 3, 4 |
| **Phase 6** | AI World & Company Eras | 🟢 BASSE | 2-5 sem | Toutes |

---

## 🏗️ PHASE 4 : OWNER & BOOKER SYSTEMS

**Durée** : 4-5 semaines
**Priorité** : 🟡 MOYENNE
**Objectif** : Implémenter les systèmes Owner stratégique et Booker IA avec mémoires persistantes

### Architecture Phase 4

```
┌─────────────────────────────────────────────────────────────────┐
│                    OWNER & BOOKER SYSTEMS                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐         ┌──────────────────┐                 │
│  │    OWNER     │────────>│  BOOKER AI       │                 │
│  │              │         │                  │                 │
│  │ - Strategic  │         │ - Auto-Booking   │                 │
│  │ - Priorities │         │ - Preferences    │                 │
│  │ - Decisions  │         │ - Memories       │                 │
│  └──────────────┘         └──────────────────┘                 │
│         │                          │                            │
│         v                          v                            │
│  ┌────────────────────────────────────────┐                    │
│  │      BOOKER MEMORY SYSTEM              │                    │
│  │  - Protégés (push bias)                │                    │
│  │  - Grudges (burial bias)               │                    │
│  │  - Traumas (avoid patterns)            │                    │
│  │  - Decay over time (5 pts/year)        │                    │
│  └────────────────────────────────────────┘                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 4.1 Database Layer (Semaine 1)

**Fichier** : `src/RingGeneral.Data/Migrations/008_owner_booker_systems.sql`

**Tables à créer** :
1. ✅ `Owners` - Propriétaires de compagnie avec priorités stratégiques
2. ✅ `Bookers` - Bookers avec préférences de produit
3. ✅ `BookerMemory` - Mémoires persistantes (biais, traumas)
4. ✅ `BookerEmploymentHistory` - Historique d'emploi

**Tâches Semaine 1** :
- [ ] Créer migration SQL `008_owner_booker_systems.sql`
- [ ] Définir contraintes et index
- [ ] Tester migration sur copie DB
- [ ] Créer script rollback
- [ ] Documenter schéma

**Critères de validation** :
- ✓ Migration applique sans erreurs
- ✓ Toutes les FK fonctionnent
- ✓ Index créés correctement
- ✓ Rollback fonctionne

### 4.2 Models Layer (Semaine 1-2)

**Fichiers à créer** :
1. `src/RingGeneral.Core/Models/Owner.cs`
2. `src/RingGeneral.Core/Models/Booker.cs`
3. `src/RingGeneral.Core/Models/BookerMemory.cs`
4. `src/RingGeneral.Core/Models/BookerEmploymentHistory.cs`

**Propriétés clés** :

**Owner** :
- Strategic Priorities (Financial, Creative, Expansion, TalentDevelopment)
- Decision Style (Aggressive, Conservative, Balanced)
- Risk Tolerance (0-100)

**Booker** :
- Product Preferences (Lucha, Puroresu, Entertainment, Hardcore, OldSchool)
- Booking Style (Stars vs Young Talent, Stability vs Chaos)
- Memories collection

**BookerMemory** :
- Types : Bias, Trauma, Success, Protege, Grudge
- Intensity (0-100) with decay (5 pts/year)
- Origin tracking (event, company)

**Tâches Semaine 1-2** :
- [ ] Créer 4 models avec propriétés complètes
- [ ] Ajouter propriétés calculées (IsActive, DominantStyle, etc.)
- [ ] Tests unitaires pour chaque model
- [ ] Validation business logic

**Critères de validation** :
- ✓ Tous les models compilent
- ✓ Propriétés calculées fonctionnent
- ✓ Tests unitaires passent (100%)

### 4.3 Repository Layer (Semaine 2)

**Fichiers à créer** :
1. `src/RingGeneral.Data/Repositories/IOwnerRepository.cs`
2. `src/RingGeneral.Data/Repositories/OwnerRepository.cs`
3. `src/RingGeneral.Data/Repositories/IBookerRepository.cs`
4. `src/RingGeneral.Data/Repositories/BookerRepository.cs`

**Méthodes clés** :

**OwnerRepository** :
- `GetByCompanyIdAsync()` - Récupère Owner par compagnie
- `UpdateStrategicPrioritiesAsync()` - Met à jour priorités
- `GetDecisionHistoryAsync()` - Historique décisions

**BookerRepository** :
- `GetByIdAsync()` - Récupère Booker avec mémoires
- `AddMemoryAsync()` - Ajoute mémoire
- `DecayMemoriesAsync()` - Applique decay annuel
- `GetEmploymentHistoryAsync()` - Historique emploi
- `ChangeCompanyAsync()` - Transfert de compagnie

**Tâches Semaine 2** :
- [ ] Créer interfaces repositories
- [ ] Implémenter repositories avec ADO.NET
- [ ] Mapper SQL → Models
- [ ] Tests d'intégration

**Critères de validation** :
- ✓ CRUD complet fonctionne
- ✓ Mémoires conservées entre compagnies
- ✓ Decay appliqué correctement

### 4.4 Service Layer (Semaine 3)

**Fichiers à créer** :
1. `src/RingGeneral.Core/Services/IBookerAIEngine.cs`
2. `src/RingGeneral.Core/Services/BookerAIEngine.cs`
3. `src/RingGeneral.Core/Services/IOwnerDecisionEngine.cs`
4. `src/RingGeneral.Core/Services/OwnerDecisionEngine.cs`

**BookerAIEngine - Fonctionnalités** :

```csharp
// Auto-booking basé sur préférences
Show AutoBookShow(string bookerId, string showId);

// Décision de push (biaisée par mémoires)
bool ShouldPushWorker(string bookerId, string workerId);

// Gestion des mémoires
void AddProtege(string bookerId, string workerId, int intensity);
void AddGrudge(string bookerId, string workerId, int intensity);
void AddTrauma(string bookerId, string traumaType, int intensity);
void DecayMemories(string bookerId);
```

**Logic d'Auto-Booking** :
- Entertainment dominant (70+) → Plus de promos
- Puroresu dominant (70+) → Longs singles matches
- Lucha dominant (70+) → Trios/tag teams
- Hardcore dominant (70+) → Stipulation matches
- Protégé memory → Push segment importance
- Grudge memory → Bury segment importance

**OwnerDecisionEngine - Fonctionnalités** :

```csharp
// Intervention dans crises
bool ShouldInterveneinCrisis(string ownerId, string crisisType, int severity);

// Décision de licenciement
bool ShouldFireBooker(string ownerId, string bookerId);

// Validation changement d'era
bool ApproveEraChange(string ownerId, string newEra);
```

**Tâches Semaine 3** :
- [ ] Implémenter BookerAIEngine
- [ ] Implémenter OwnerDecisionEngine
- [ ] Tests unitaires scénarios
- [ ] Tests d'intégration avec repositories

**Critères de validation** :
- ✓ Auto-booking génère show cohérent
- ✓ Mémoires influencent décisions
- ✓ Owner intervient selon priorités

### 4.5 UI Layer (Semaine 4)

**Fichiers à créer** :
1. `src/RingGeneral.UI/ViewModels/Management/OwnerBookerViewModel.cs`
2. `src/RingGeneral.UI/Views/Management/OwnerBookerView.axaml`

**Composants UI** :

**Owner Card** :
- Affichage des priorités stratégiques (barres de progression)
- Decision Style badge
- Risk Tolerance gauge

**Booker Card** :
- Product Preferences (badges colorés)
- Dominant Style highlight
- **Toggle "Let the Booker Decide"** (auto-booking activation)
- Memories list (expandable)
- Fire Booker button

**Dialogs** :
- Edit Owner Priorities
- Edit Booker Preferences
- Add/Remove Booker Memory
- Hire New Booker

**Tâches Semaine 4** :
- [ ] Créer OwnerBookerViewModel
- [ ] Créer OwnerBookerView.axaml
- [ ] Implémenter commands (Fire, Edit, Toggle)
- [ ] Binding avec engines
- [ ] Tests UI

**Critères de validation** :
- ✓ UI affiche données correctement
- ✓ Toggle "Let Booker Decide" active auto-booking
- ✓ Modifications sauvegardées

### 4.6 Integration & Tests (Semaine 5)

**Scénarios de test complets** :

1. **Scénario Protégé** :
   - Booker a mémoire "Protege" pour Worker A (intensity 80)
   - Auto-booking → Worker A reçoit main event
   - Validation : segment importance = "High"

2. **Scénario Grudge** :
   - Booker a mémoire "Grudge" pour Worker B (intensity 70)
   - Auto-booking → Worker B enterré (jobber match)
   - Validation : segment importance = "Low", result = "Loss"

3. **Scénario Transfert Booker** :
   - Booker change Company A → Company B
   - Mémoires conservées
   - Auto-booking dans Company B → mémoires appliquées

4. **Scénario Owner Intervention** :
   - Crise Financial severity 4
   - Owner avec Financial Priority 80 → intervient
   - Validation : crisis resolved

**Tâches Semaine 5** :
- [ ] Implémenter 4 scénarios de test
- [ ] Tests end-to-end
- [ ] Performance tests (auto-booking)
- [ ] Documentation utilisateur

**Livrables Phase 4** :
- ✅ Migration SQL appliquée
- ✅ 4 models créés
- ✅ 2 repositories complets
- ✅ 2 engines (BookerAI, OwnerDecision)
- ✅ UI Owner/Booker management
- ✅ Feature "Let the Booker Decide" fonctionnelle
- ✅ Tests passants (100%)

---

## 🚨 PHASE 5 : CRISIS & COMMUNICATION MANAGEMENT

**Durée** : 2-3 semaines
**Priorité** : 🟡 MOYENNE
**Objectif** : Implémenter pipeline de crises (5 étapes) et système de communication joueur (4 types)

### Architecture Phase 5

```
┌─────────────────────────────────────────────────────────────────┐
│                  CRISIS MANAGEMENT PIPELINE                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Step 1          Step 2          Step 3          Step 4         │
│ ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌──────────┐     │
│ │  Weak   │───>│ Rumors  │───>│ Crisis  │───>│ Player   │     │
│ │ Signals │    │Spreading│    │Declared │    │ Response │     │
│ └─────────┘    └─────────┘    └─────────┘    └──────────┘     │
│                                                      │           │
│                                                      v           │
│                                              Step 5              │
│                                           ┌────────────────┐    │
│                                           │ Consequences   │    │
│                                           │ & Resolution   │    │
│                                           └────────────────┘    │
│                                                                  │
│  Communication Types:                                           │
│  1️⃣ One-on-One    (worker individuel)                          │
│  2️⃣ Locker Room   (réunion générale)                           │
│  3️⃣ Public        (statement médiatique)                       │
│  4️⃣ Mediation     (via staff/leaders)                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.1 Crisis System (Semaine 6)

**Fichier Migration** : `src/RingGeneral.Data/Migrations/009_crisis_system.sql`

**Table** :
```sql
CREATE TABLE Crises (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    CrisisType TEXT NOT NULL, -- PR, Financial, Sporting, Internal
    Description TEXT NOT NULL,
    Stage TEXT DEFAULT 'WeakSignals', -- WeakSignals, Rumors, Declared, InResolution, Resolved
    Severity INTEGER DEFAULT 1 CHECK(Severity BETWEEN 1 AND 5),

    DetectedAt TEXT NOT NULL,
    DeclaredAt TEXT,
    ResolvedAt TEXT,

    PlayerResponse TEXT,
    ResolutionMethod TEXT,

    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

CREATE INDEX idx_crisis_company ON Crises(CompanyId);
CREATE INDEX idx_crisis_active ON Crises(Stage);
```

**Model** : `src/RingGeneral.Core/Models/Crisis.cs`

```csharp
public class Crisis
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;

    public string CrisisType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Stage { get; set; } = "WeakSignals";
    public int Severity { get; set; } = 1;

    public DateTime DetectedAt { get; set; }
    public DateTime? DeclaredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public string? PlayerResponse { get; set; }
    public string? ResolutionMethod { get; set; }

    // Calculated
    public bool IsActive => Stage != "Resolved";
    public int DaysActive => (DateTime.Now - DetectedAt).Days;
    public string SeverityLabel => Severity switch
    {
        1 => "Mineure",
        2 => "Modérée",
        3 => "Sérieuse",
        4 => "Majeure",
        5 => "Critique",
        _ => "Inconnue"
    };
}
```

**Service** : `src/RingGeneral.Core/Services/CrisisEngine.cs`

```csharp
public interface ICrisisEngine
{
    Crisis? DetectCrisis(string companyId);
    void ProgressCrisis(int crisisId);
    void ResolveCrisis(int crisisId, string resolutionMethod);
    List<Crisis> GetActiveCrises(string companyId);
}

public class CrisisEngine : ICrisisEngine
{
    private readonly ICrisisRepository _repository;
    private readonly IMoraleEngine _moraleEngine;
    private readonly IRumorEngine _rumorEngine;

    public Crisis? DetectCrisis(string companyId)
    {
        // Détecter depuis MoraleEngine weak signals
        var signals = _moraleEngine.DetectWeakSignals(companyId);

        if (signals.Count >= 3)
        {
            // Escalade en crise
            return new Crisis
            {
                CompanyId = companyId,
                CrisisType = "Internal",
                Description = "Moral backstage critique",
                Severity = 3,
                Stage = "WeakSignals",
                DetectedAt = DateTime.Now
            };
        }

        // Détecter depuis RumorEngine widespread rumors
        var rumors = _rumorEngine.GetWidespreadRumors(companyId);

        if (rumors.Count >= 2)
        {
            return new Crisis
            {
                CompanyId = companyId,
                CrisisType = "PR",
                Description = "Rumeurs répandues dans les médias",
                Severity = 4,
                Stage = "Rumors",
                DetectedAt = DateTime.Now
            };
        }

        return null;
    }

    public void ProgressCrisis(int crisisId)
    {
        var crisis = _repository.GetByIdAsync(crisisId).Result;

        // Pipeline progression
        crisis.Stage = crisis.Stage switch
        {
            "WeakSignals" => "Rumors",
            "Rumors" => "Declared",
            "Declared" => "InResolution",
            "InResolution" => "Resolved",
            _ => crisis.Stage
        };

        if (crisis.Stage == "Declared" && !crisis.DeclaredAt.HasValue)
        {
            crisis.DeclaredAt = DateTime.Now;
        }

        _repository.UpdateAsync(crisis).Wait();
    }

    public void ResolveCrisis(int crisisId, string resolutionMethod)
    {
        var crisis = _repository.GetByIdAsync(crisisId).Result;

        crisis.Stage = "Resolved";
        crisis.ResolutionMethod = resolutionMethod;
        crisis.ResolvedAt = DateTime.Now;

        _repository.UpdateAsync(crisis).Wait();
    }
}
```

**Tâches Semaine 6** :
- [ ] Créer migration SQL
- [ ] Créer Crisis model
- [ ] Créer CrisisRepository
- [ ] Implémenter CrisisEngine
- [ ] Tests pipeline 5 étapes

### 5.2 Communication System (Semaine 7)

**Service** : `src/RingGeneral.Core/Services/CommunicationEngine.cs`

```csharp
public interface ICommunicationEngine
{
    CommunicationResult OneOnOneDiscussion(string workerId, string topic);
    CommunicationResult LockerRoomMeeting(string companyId, string message);
    CommunicationResult PublicStatement(string companyId, string statement);
    CommunicationResult IndirectMediation(string mediatorId, string targetWorkerId, string message);
}

public class CommunicationResult
{
    public bool Success { get; set; }
    public int MoraleImpact { get; set; } // +/- points
    public List<string> Consequences { get; set; } = new();
    public string ResultMessage { get; set; } = string.Empty;
}

public class CommunicationEngine : ICommunicationEngine
{
    private readonly IMoraleEngine _moraleEngine;
    private readonly IPersonalityEngine _personalityEngine;

    public CommunicationResult OneOnOneDiscussion(string workerId, string topic)
    {
        // Récupérer personnalité du worker
        var personality = _personalityEngine.GetPersonalityAsync(workerId, "Worker").Result;

        var result = new CommunicationResult();

        // Impact basé sur personnalité
        if (personality?.Label == "Professional")
        {
            result.Success = true;
            result.MoraleImpact = +10;
            result.ResultMessage = "Le worker apprécie la communication directe.";
        }
        else if (personality?.Label == "Egotistic")
        {
            result.Success = false;
            result.MoraleImpact = -5;
            result.ResultMessage = "Le worker se sent micro-managé.";
        }
        else
        {
            result.Success = true;
            result.MoraleImpact = +5;
            result.ResultMessage = "Discussion constructive.";
        }

        // Appliquer impact moral
        _moraleEngine.UpdateMorale(workerId, "Communication", result.MoraleImpact);

        return result;
    }

    public CommunicationResult LockerRoomMeeting(string companyId, string message)
    {
        var result = new CommunicationResult
        {
            Success = true,
            MoraleImpact = +15,
            ResultMessage = "Réunion générale a boosté le moral."
        };

        // Appliquer à tous les workers
        var workers = _workerRepository.GetAllByCompanyAsync(companyId).Result;

        foreach (var worker in workers)
        {
            _moraleEngine.UpdateMorale(worker.Id, "LockerRoomMeeting", +15);
        }

        return result;
    }

    public CommunicationResult PublicStatement(string companyId, string statement)
    {
        var result = new CommunicationResult
        {
            Success = true,
            MoraleImpact = +10,
            ResultMessage = "Statement public a rassuré le roster."
        };

        // Impact modéré sur tous
        var workers = _workerRepository.GetAllByCompanyAsync(companyId).Result;

        foreach (var worker in workers)
        {
            _moraleEngine.UpdateMorale(worker.Id, "PublicStatement", +10);
        }

        // Peut générer rumeur positive
        _rumorEngine.GenerateRumor(companyId, "PositiveStatement", statement);

        return result;
    }

    public CommunicationResult IndirectMediation(string mediatorId, string targetWorkerId, string message)
    {
        // Médiation via staff ou leader
        var result = new CommunicationResult();

        // Vérifier relation entre médiateur et target
        var relation = _relationRepository.GetRelationAsync(mediatorId, targetWorkerId).Result;

        if (relation != null && relation.RelationStrength >= 70)
        {
            result.Success = true;
            result.MoraleImpact = +20;
            result.ResultMessage = "Médiation par un proche a été très efficace.";
        }
        else
        {
            result.Success = false;
            result.MoraleImpact = +5;
            result.ResultMessage = "Médiation peu efficace (relation faible).";
        }

        _moraleEngine.UpdateMorale(targetWorkerId, "Mediation", result.MoraleImpact);

        return result;
    }
}
```

**Tâches Semaine 7** :
- [ ] Implémenter CommunicationEngine
- [ ] 4 types de communication
- [ ] Tests scénarios communication
- [ ] Validation impacts moral

### 5.3 UI Integration (Semaine 8)

**Dashboard Crisis Card** :

```xml
<!-- Crisis Alert in Dashboard -->
<Border Classes="alert-card crisis-alert" Background="#fee2e2"
        IsVisible="{Binding HasActiveCrisis}">
  <StackPanel Spacing="12">
    <StackPanel Orientation="Horizontal" Spacing="8">
      <TextBlock Text="🚨" FontSize="32"/>
      <StackPanel Spacing="4">
        <TextBlock Classes="h3" Text="Crise Active"/>
        <TextBlock Classes="body" Text="{Binding Crisis.CrisisType}"/>
      </StackPanel>
    </StackPanel>

    <TextBlock Classes="body" FontWeight="SemiBold" TextWrapping="Wrap"
               Text="{Binding Crisis.Description}"/>

    <!-- Crisis Info -->
    <Grid ColumnDefinitions="*,*,*">
      <StackPanel Grid.Column="0" Spacing="2">
        <TextBlock Classes="caption muted" Text="Sévérité"/>
        <TextBlock Classes="body" FontWeight="SemiBold" Text="{Binding Crisis.SeverityLabel}"/>
      </StackPanel>

      <StackPanel Grid.Column="1" Spacing="2">
        <TextBlock Classes="caption muted" Text="Stage"/>
        <TextBlock Classes="body" FontWeight="SemiBold" Text="{Binding Crisis.Stage}"/>
      </StackPanel>

      <StackPanel Grid.Column="2" Spacing="2">
        <TextBlock Classes="caption muted" Text="Jours actifs"/>
        <TextBlock Classes="body" FontWeight="SemiBold" Text="{Binding Crisis.DaysActive}"/>
      </StackPanel>
    </Grid>

    <!-- Actions -->
    <StackPanel Spacing="8">
      <TextBlock Classes="caption muted" Text="Actions disponibles:"/>

      <Button Classes="primary" Content="📞 Réunion générale"
              Command="{Binding LockerRoomMeetingCommand}"/>

      <Button Classes="secondary" Content="📢 Communication publique"
              Command="{Binding PublicStatementCommand}"/>

      <Button Classes="secondary" Content="🤝 Médiation indirecte"
              Command="{Binding MediationCommand}"/>

      <Button Classes="secondary" Content="💬 Discussion one-on-one"
              Command="{Binding OneOnOneCommand}"/>
    </StackPanel>
  </StackPanel>
</Border>
```

**Communication Dialogs** :

```xml
<!-- LockerRoomMeetingDialog.axaml -->
<Window Title="Réunion de Vestiaire">
  <StackPanel Spacing="16" Margin="20">
    <TextBlock Classes="h3" Text="Message au roster"/>

    <TextBox AcceptsReturn="True" Height="150"
             Watermark="Entrez votre message à l'équipe..."
             Text="{Binding MeetingMessage}"/>

    <TextBlock Classes="caption muted" TextWrapping="Wrap">
      Impact prévu: +15 moral pour tous les workers
    </TextBlock>

    <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Right">
      <Button Classes="secondary" Content="Annuler" Command="{Binding CancelCommand}"/>
      <Button Classes="primary" Content="Tenir la réunion" Command="{Binding ConfirmCommand}"/>
    </StackPanel>
  </StackPanel>
</Window>
```

**Tâches Semaine 8** :
- [ ] Créer Crisis Alert Card dans Dashboard
- [ ] Créer 4 dialogs de communication
- [ ] Implémenter commands
- [ ] Tests UI interaction

**Livrables Phase 5** :
- ✅ Crisis system (pipeline 5 étapes)
- ✅ CommunicationEngine (4 types)
- ✅ Crisis UI dans Dashboard
- ✅ Communication dialogs
- ✅ Tests passants (100%)

---

## 🌍 PHASE 6 : AI WORLD & COMPANY ERAS

**Durée** : 2-5 semaines
**Priorité** : 🟢 BASSE (Polish)
**Objectif** : Simulation monde IA avec LOD, eras de compagnie, histoire émergente

### Architecture Phase 6

```
┌─────────────────────────────────────────────────────────────────┐
│                    AI WORLD SIMULATION                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────┐      │
│  │             LOD (Level of Detail)                     │      │
│  │                                                        │      │
│  │  FULL      → Player Company (100% simulation)         │      │
│  │  HIGH      → Top 3 Companies (major shows only)       │      │
│  │  MEDIUM    → Regional (monthly summary)               │      │
│  │  LOW       → Local (major changes only)               │      │
│  └──────────────────────────────────────────────────────┘      │
│                                                                  │
│  ┌─────────────┐    ┌─────────────┐    ┌──────────────┐       │
│  │ Company Era │───>│ World Events│───>│  History     │       │
│  │             │    │             │    │  Generator   │       │
│  │ - Creative  │    │ - Transfers │    │              │       │
│  │ - Economic  │    │ - Closures  │    │ Emergent     │       │
│  │ - Media     │    │ - Mergers   │    │ Narratives   │       │
│  └─────────────┘    └─────────────┘    └──────────────┘       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 6.1 Company Eras (Semaine 9-10)

**Migration** : `src/RingGeneral.Data/Migrations/010_company_eras.sql`

```sql
CREATE TABLE CompanyEras (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,

    EraName TEXT NOT NULL,

    -- Era Characteristics
    CreativeDirection TEXT DEFAULT 'Balanced', -- Edgy, FamilyFriendly, SportsBased
    EconomicState TEXT DEFAULT 'Stable', -- Boom, Stable, Recession
    MediaPresence TEXT DEFAULT 'Regional', -- Local, Regional, National, Global

    -- Show Structure
    TypicalShowDuration INTEGER DEFAULT 120,
    TypicalMatchCount INTEGER DEFAULT 7,

    -- Dates
    StartDate TEXT NOT NULL,
    EndDate TEXT,

    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

CREATE TABLE EraDominantMatchTypes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EraId INTEGER NOT NULL,
    MatchType TEXT NOT NULL,
    Frequency INTEGER DEFAULT 50, -- 0-100

    FOREIGN KEY (EraId) REFERENCES CompanyEras(Id)
);

CREATE INDEX idx_era_company ON CompanyEras(CompanyId);
CREATE INDEX idx_era_active ON CompanyEras(EndDate);
```

**Model** : `src/RingGeneral.Core/Models/CompanyEra.cs`

```csharp
public class CompanyEra
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;

    public string EraName { get; set; } = string.Empty;

    public string CreativeDirection { get; set; } = "Balanced";
    public string EconomicState { get; set; } = "Stable";
    public string MediaPresence { get; set; } = "Regional";

    public int TypicalShowDuration { get; set; } = 120;
    public int TypicalMatchCount { get; set; } = 7;

    public List<string> DominantMatchTypes { get; set; } = new();

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Calculated
    public bool IsActive => !EndDate.HasValue;
    public int DurationInYears => (int)((EndDate ?? DateTime.Now) - StartDate).TotalDays / 365;

    public string EraLabel => $"{EraName} ({CreativeDirection}, {EconomicState})";
}
```

**Service** : `src/RingGeneral.Core/Services/EraEvolutionEngine.cs`

```csharp
public interface IEraEvolutionEngine
{
    bool ShouldTransitionEra(string companyId);
    CompanyEra GenerateNewEra(string companyId);
    void ApplyEraTransition(string companyId, CompanyEra newEra);
}

public class EraEvolutionEngine : IEraEvolutionEngine
{
    public bool ShouldTransitionEra(string companyId)
    {
        var currentEra = _eraRepository.GetActiveEraAsync(companyId).Result;

        if (currentEra == null) return true;

        // Transition si:
        // - 5+ ans écoulés
        // - Changement économique majeur
        // - Changement de Booker

        if (currentEra.DurationInYears >= 5)
            return true;

        var company = _companyRepository.GetByIdAsync(companyId).Result;

        // Transition si crise économique
        if (currentEra.EconomicState == "Boom" && company.Treasury < 100000)
            return true;

        return false;
    }

    public CompanyEra GenerateNewEra(string companyId)
    {
        var company = _companyRepository.GetByIdAsync(companyId).Result;

        // Générer nom d'era
        var eraNames = new[] { "Attitude Era", "Golden Age", "Modern Era", "New Generation" };
        var eraName = eraNames[_random.Next(eraNames.Length)];

        // Déterminer direction basée sur Owner
        var owner = _ownerRepository.GetByCompanyIdAsync(companyId).Result;

        var creativeDirection = owner?.CreativePriority >= 70 ? "Edgy" : "FamilyFriendly";

        // Déterminer état économique basé sur trésorerie
        var economicState = company.Treasury switch
        {
            >= 1000000 => "Boom",
            >= 500000 => "Stable",
            _ => "Recession"
        };

        return new CompanyEra
        {
            CompanyId = companyId,
            EraName = eraName,
            CreativeDirection = creativeDirection,
            EconomicState = economicState,
            MediaPresence = "Regional",
            StartDate = DateTime.Now
        };
    }

    public void ApplyEraTransition(string companyId, CompanyEra newEra)
    {
        // Terminer l'era actuelle
        var currentEra = _eraRepository.GetActiveEraAsync(companyId).Result;

        if (currentEra != null)
        {
            currentEra.EndDate = DateTime.Now;
            _eraRepository.UpdateAsync(currentEra).Wait();
        }

        // Activer nouvelle era
        _eraRepository.SaveAsync(newEra).Wait();

        // Générer world event
        var worldEvent = new WorldEvent
        {
            EventType = "EraTransition",
            Description = $"{company.Name} entre dans une nouvelle ère: {newEra.EraName}",
            InvolvedCompanyId = companyId,
            OccurredAt = DateTime.Now,
            Significance = 4
        };

        _worldEventRepository.SaveAsync(worldEvent).Wait();
    }
}
```

**Tâches Semaine 9-10** :
- [ ] Créer migration SQL
- [ ] Créer CompanyEra model
- [ ] Créer EraRepository
- [ ] Implémenter EraEvolutionEngine
- [ ] Tests transitions eras

### 6.2 AI World Simulation (Semaine 11-12)

**Model** : `src/RingGeneral.Core/Models/WorldEvent.cs`

```csharp
public class WorldEvent
{
    public int Id { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? InvolvedCompanyId { get; set; }
    public string? InvolvedWorkerId { get; set; }

    public DateTime OccurredAt { get; set; }

    public int Significance { get; set; } = 1; // 1-5

    public string SignificanceLabel => Significance switch
    {
        1 => "Mineure",
        2 => "Modérée",
        3 => "Importante",
        4 => "Majeure",
        5 => "Historique",
        _ => "Inconnue"
    };
}
```

**Service** : `src/RingGeneral.Core/Services/AIWorldSimulationEngine.cs`

```csharp
public enum SimulationLOD
{
    Full,    // Player company
    High,    // Top 3
    Medium,  // Regional
    Low      // Local
}

public interface IAIWorldSimulationEngine
{
    void SimulateAICompanies(int weekNumber);
    List<WorldEvent> GenerateWorldEvents();
}

public class AIWorldSimulationEngine : IAIWorldSimulationEngine
{
    public void SimulateAICompanies(int weekNumber)
    {
        var allCompanies = _companyRepository.GetAllAsync().Result;

        foreach (var company in allCompanies)
        {
            if (company.IsPlayerControlled)
            {
                // Full simulation (déjà fait par player)
                continue;
            }

            var lod = DetermineLOD(company);

            SimulateCompanyBasedOnLOD(company, lod, weekNumber);
        }
    }

    private SimulationLOD DetermineLOD(Company company)
    {
        // Top 3 companies → HIGH
        var topCompanies = _companyRepository.GetTopCompaniesByTreasuryAsync(3).Result;

        if (topCompanies.Contains(company))
            return SimulationLOD.High;

        // Regional companies (50k-500k treasury) → MEDIUM
        if (company.Treasury >= 50000 && company.Treasury < 500000)
            return SimulationLOD.Medium;

        // Local companies → LOW
        return SimulationLOD.Low;
    }

    private void SimulateCompanyBasedOnLOD(Company company, SimulationLOD lod, int weekNumber)
    {
        switch (lod)
        {
            case SimulationLOD.High:
                // Simuler shows majeurs uniquement
                if (weekNumber % 4 == 0) // Monthly show
                {
                    SimulateShow(company);
                }
                break;

            case SimulationLOD.Medium:
                // Simuler résumé mensuel
                if (weekNumber % 4 == 0)
                {
                    SimulateMonthlySummary(company);
                }
                break;

            case SimulationLOD.Low:
                // Changements majeurs uniquement (transfers, closures)
                if (_random.Next(100) < 5) // 5% chance per week
                {
                    GenerateMajorChange(company);
                }
                break;
        }
    }

    private void SimulateShow(Company company)
    {
        // Generate simple show with results
        var show = new Show
        {
            CompanyId = company.Id,
            Name = $"{company.Name} Weekly Show",
            Date = DateTime.Now
        };

        // Auto-book with BookerAI
        var booker = _bookerRepository.GetByCompanyIdAsync(company.Id).Result;

        if (booker != null)
        {
            show = _bookerAIEngine.AutoBookShow(booker.Id, show.Id);
        }

        _showRepository.SaveAsync(show).Wait();
    }

    private void SimulateMonthlySummary(Company company)
    {
        // Adjust treasury
        var monthlyRevenue = _random.Next(50000, 150000);
        var monthlyCosts = _random.Next(30000, 100000);

        company.Treasury += (monthlyRevenue - monthlyCosts);

        _companyRepository.UpdateAsync(company).Wait();
    }

    private void GenerateMajorChange(Company company)
    {
        // Random major event
        var eventType = _random.Next(3);

        switch (eventType)
        {
            case 0:
                // Worker transfer
                TransferRandomWorker(company);
                break;

            case 1:
                // Company closure (if bankruptcy)
                if (company.Treasury < 0)
                {
                    CloseCompany(company);
                }
                break;

            case 2:
                // Booker firing
                FireBooker(company);
                break;
        }
    }

    public List<WorldEvent> GenerateWorldEvents()
    {
        var events = new List<WorldEvent>();

        // Scan for major changes
        var recentTransfers = _transferRepository.GetRecentAsync(days: 7).Result;

        foreach (var transfer in recentTransfers)
        {
            events.Add(new WorldEvent
            {
                EventType = "Transfer",
                Description = $"{transfer.WorkerName} signe avec {transfer.ToCompanyName}",
                InvolvedWorkerId = transfer.WorkerId,
                InvolvedCompanyId = transfer.ToCompanyId,
                OccurredAt = transfer.TransferDate,
                Significance = 3
            });
        }

        return events;
    }
}
```

**Tâches Semaine 11-12** :
- [ ] Créer WorldEvent model
- [ ] Créer AIWorldSimulationEngine
- [ ] Implémenter LOD (4 niveaux)
- [ ] Tests performances (1000+ companies)
- [ ] Optimisation LOD

### 6.3 UI - World News Feed (Semaine 13)

**Vue** : `src/RingGeneral.UI/Views/World/WorldNewsFeedView.axaml`

```xml
<ScrollViewer>
  <StackPanel Spacing="16" Margin="16">
    <TextBlock Classes="h2" Text="🌍 Actualités du Monde"/>

    <!-- Filters -->
    <WrapPanel Spacing="8">
      <Button Classes="filter" Content="Tous" IsChecked="True"/>
      <Button Classes="filter" Content="Transfers"/>
      <Button Classes="filter" Content="Closures"/>
      <Button Classes="filter" Content="Eras"/>
      <Button Classes="filter" Content="Majeurs uniquement"/>
    </WrapPanel>

    <!-- World Events -->
    <ItemsControl ItemsSource="{Binding WorldEvents}">
      <ItemsControl.ItemTemplate>
        <DataTemplate>
          <Border Classes="card event-card" Margin="0,0,0,12">
            <StackPanel Spacing="8">
              <!-- Event Header -->
              <Grid ColumnDefinitions="Auto,*,Auto">
                <TextBlock Grid.Column="0" Classes="badge"
                           Text="{Binding EventType}"/>

                <TextBlock Grid.Column="2" Classes="caption muted"
                           Text="{Binding OccurredAt, StringFormat='yyyy-MM-dd'}"/>
              </Grid>

              <!-- Event Description -->
              <TextBlock Classes="body" TextWrapping="Wrap"
                         Text="{Binding Description}"/>

              <!-- Significance -->
              <StackPanel Orientation="Horizontal" Spacing="8">
                <TextBlock Classes="caption muted" Text="Importance:"/>
                <ItemsRepeater ItemsSource="{Binding SignificanceStars}">
                  <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                      <TextBlock Text="⭐" FontSize="12"/>
                    </DataTemplate>
                  </ItemsRepeater.ItemTemplate>
                </ItemsRepeater>
              </StackPanel>
            </StackPanel>
          </Border>
        </DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
  </StackPanel>
</ScrollViewer>
```

**Tâches Semaine 13** :
- [ ] Créer WorldNewsFeedViewModel
- [ ] Créer WorldNewsFeedView
- [ ] Filtres événements
- [ ] Tests UI

**Livrables Phase 6** :
- ✅ CompanyEra system
- ✅ AI World Simulation (LOD)
- ✅ WorldEvent tracking
- ✅ World News Feed UI
- ✅ Tests performances passants

---

## 📅 TIMELINE GLOBALE

```
Semaine  1-2   : Phase 4 - DB & Models
Semaine  3     : Phase 4 - Services
Semaine  4-5   : Phase 4 - UI & Tests
Semaine  6     : Phase 5 - Crisis System
Semaine  7     : Phase 5 - Communication
Semaine  8     : Phase 5 - UI Integration
Semaine  9-10  : Phase 6 - Company Eras
Semaine  11-12 : Phase 6 - AI Simulation
Semaine  13    : Phase 6 - UI & Polish
```

**Total** : 13 semaines (3,25 mois)

---

## 🎯 JALONS (MILESTONES)

| Date Cible | Jalon | Livrables |
|------------|-------|-----------|
| **Sem 2** | Phase 4 Models Complete | Owner, Booker, Memory models |
| **Sem 5** | Phase 4 Complete | BookerAI, OwnerDecision, UI |
| **Sem 8** | Phase 5 Complete | Crisis, Communication systems |
| **Sem 13** | Phase 6 Complete | AI World, Eras, News Feed |

---

## ✅ CRITÈRES DE SUCCÈS

### Phase 4
- [ ] Auto-booking génère shows cohérents basés sur préférences Booker
- [ ] Mémoires influencent correctement les décisions (protégés, grudges)
- [ ] Mémoires conservées lors de transferts Booker
- [ ] Toggle "Let Booker Decide" active/désactive auto-booking
- [ ] Owner intervient dans crises selon priorités

### Phase 5
- [ ] Pipeline 5 étapes fonctionne (Signals → Rumors → Declared → Resolution → Resolved)
- [ ] 4 types de communication impactent moral correctement
- [ ] Crisis UI affiche alertes et actions
- [ ] Dialogs de communication fonctionnels

### Phase 6
- [ ] LOD réduit charge simulation (1000+ companies)
- [ ] Company Eras transitionnent automatiquement
- [ ] World Events générés et affichés dans News Feed
- [ ] Histoire émergente crédible

---

## 🚀 PROCHAINES ÉTAPES

### Actions Immédiates (Cette Semaine)

1. **Lancer Phase 4 - Semaine 1** :
   - Créer migration `008_owner_booker_systems.sql`
   - Créer 4 models (Owner, Booker, BookerMemory, BookerEmploymentHistory)
   - Tests unitaires models

2. **Planification Détaillée** :
   - Réviser estimations de durée
   - Identifier risques techniques
   - Préparer user stories

3. **Communication** :
   - Informer équipe du planning
   - Définir points de synchronisation hebdomadaires

### Long Terme (Après Phase 6)

**Phases Futures Potentielles** :
- Phase 7 : Advanced UI (drag-and-drop booking, visual timeline)
- Phase 8 : Multiplayer (online leagues, shared universe)
- Phase 9 : Modding Support (custom eras, workers, companies)
- Phase 10 : Mobile/Web (cross-platform support)

**Maintenance Continue** :
- Performance optimization
- Bug fixes
- Community feedback integration

---

## 📝 NOTES

- **Flexibilité** : Les durées sont estimatives et peuvent être ajustées
- **Priorisation** : Phase 4 et 5 sont prioritaires, Phase 6 peut être différée
- **Tests** : Tests automatisés obligatoires pour chaque phase
- **Documentation** : Documenter au fil de l'eau (pas en fin de phase)

---

**Dernière mise à jour** : 2026-01-08
**Prochaine révision** : Fin Semaine 1 (Phase 4)
