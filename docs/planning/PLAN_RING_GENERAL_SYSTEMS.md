# 🎯 PLAN D'IMPLÉMENTATION - RING GENERAL SYSTEMS

**Date de création** : 8 janvier 2026
**Chef de Projet** : Claude DevOps
**Version** : 1.0
**Branche** : `claude/add-ring-general-summary-99o8j`
**Priorité** : 🟡 MOYENNE (Post-MVP)
**Référence Conceptuelle** : [PLAN_IMPLEMENTATION_TECHNIQUE.md - Vision & Design Conceptuel](./PLAN_IMPLEMENTATION_TECHNIQUE.md#-vision--design-conceptuel---version-verrouillée)

---

## 📋 SYNTHÈSE EXÉCUTIVE

### Objectif Global

Implémenter l'ensemble des **systèmes de simulation humaine** définis dans la Vision Conceptuelle Ring General, en transformant les 17 concepts validés en fonctionnalités jouables et testables.

### Périmètre

Ce plan couvre l'implémentation de :
- ✅ **Owner System** (stratégie économique)
- ✅ **Booker System** (AI booking + mémoire persistante)
- ✅ **Staff Systems** (hiérarchie, creative staff, trainers)
- ✅ **Personality & Mental Attributes** (10 attributs cachés + labels)
- ✅ **Relations & Nepotism** (4 types + intensité cachée)
- ✅ **Backstage Morale** (global + individuel)
- ✅ **Rumors System** (émergence progressive)
- ✅ **Player Communication** (4 types d'intervention)
- ✅ **Crisis Management** (pipeline en 5 étapes)
- ✅ **AI Companies** (simulation monde vivant)
- ✅ **Company Eras** (évolution créative/économique/médiatique)

### Pourquoi Après le MVP ?

Ces systèmes constituent la **profondeur stratégique** de Ring General, mais ne sont pas nécessaires pour la boucle de jeu de base :
- 🎯 **MVP** = Booker des shows + Simuler + Résultats + Passer semaine
- 🎯 **Post-MVP** = Simulation humaine complète avec IA évolutive

### Durée Estimée

**16-24 semaines** (4-6 mois) réparties en **6 phases**

---

## 📊 ARCHITECTURE GLOBALE

### Vue d'Ensemble des Systèmes

```
┌─────────────────────────────────────────────────────────────────────┐
│                           RING GENERAL                               │
│                      Human Simulation Layer                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐         │
│  │    OWNER     │───>│    BOOKER    │───>│     STAFF    │         │
│  │   SYSTEM     │    │   SYSTEM     │    │   SYSTEMS    │         │
│  └──────────────┘    └──────────────┘    └──────────────┘         │
│         │                    │                    │                 │
│         v                    v                    v                 │
│  ┌──────────────────────────────────────────────────────┐          │
│  │          PERSONALITY & MENTAL ATTRIBUTES             │          │
│  │              (10 hidden + labels)                    │          │
│  └──────────────────────────────────────────────────────┘          │
│         │                    │                    │                 │
│         v                    v                    v                 │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐         │
│  │  RELATIONS   │    │  BACKSTAGE   │    │   RUMORS     │         │
│  │  & NEPOTISM  │    │    MORALE    │    │   SYSTEM     │         │
│  └──────────────┘    └──────────────┘    └──────────────┘         │
│         │                    │                    │                 │
│         └────────────────────┴────────────────────┘                 │
│                              │                                      │
│                              v                                      │
│                    ┌──────────────────┐                            │
│                    │  CRISIS PIPELINE │                            │
│                    │   (5 stages)     │                            │
│                    └──────────────────┘                            │
│                              │                                      │
│                              v                                      │
│                    ┌──────────────────┐                            │
│                    │     PLAYER       │                            │
│                    │  COMMUNICATION   │                            │
│                    └──────────────────┘                            │
│                                                                      │
├─────────────────────────────────────────────────────────────────────┤
│                      AI WORLD SIMULATION                             │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐         │
│  │AI COMPANIES  │───>│ COMPANY ERAS │───>│  EMERGENT    │         │
│  │  (LOD-based) │    │  (evolution) │    │   HISTORY    │         │
│  └──────────────┘    └──────────────┘    └──────────────┘         │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🗓️ PLANNING GLOBAL (6 PHASES)

| Phase | Nom | Durée | Priorité | Dépendances |
|-------|-----|-------|----------|-------------|
| **Phase 1** | Personality & Mental System | 3-4 semaines | 🔴 HAUTE | Aucune (fondation) |
| **Phase 2** | Relations & Nepotism | 2-3 semaines | 🔴 HAUTE | Phase 1 |
| **Phase 3** | Backstage Morale & Rumors | 3-4 semaines | 🔴 HAUTE | Phase 1, Phase 2 |
| **Phase 4** | Owner & Booker Systems | 4-5 semaines | 🟡 MOYENNE | Phase 1, Phase 3 |
| **Phase 5** | Crisis & Communication | 2-3 semaines | 🟡 MOYENNE | Phase 3, Phase 4 |
| **Phase 6** | AI World & Company Eras | 2-5 semaines | 🟢 BASSE | Toutes |

**Durée Totale** : **16-24 semaines** (4-6 mois)

---

## 🏗️ PHASE 1 : PERSONALITY & MENTAL SYSTEM

**Durée** : 3-4 semaines
**Priorité** : 🔴 HAUTE (Fondation de tous les systèmes)
**Dépendances** : Aucune

### Objectifs

Implémenter le **système hybride** d'attributs mentaux et de personnalités pour workers et staff :
- 10 attributs mentaux cachés (0-20) jamais affichés
- Labels de personnalité visibles (FM-like)
- Système d'évolution des attributs et personnalités

### 1.1 Base de Données (Semaine 1)

#### Migration SQL

**Fichier** : `src/RingGeneral.Data/Migrations/005_mental_attributes_personalities.sql`

```sql
-- Table des attributs mentaux (cachés)
CREATE TABLE MentalAttributes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL, -- 'Worker', 'Staff', 'Trainee'

    -- 10 Attributs Mentaux (0-20)
    Professionalism INTEGER DEFAULT 10,
    Ambition INTEGER DEFAULT 10,
    Loyalty INTEGER DEFAULT 10,
    Ego INTEGER DEFAULT 10,
    Resilience INTEGER DEFAULT 10,
    Adaptability INTEGER DEFAULT 10,
    Creativity INTEGER DEFAULT 10,
    WorkEthic INTEGER DEFAULT 10,
    SocialSkills INTEGER DEFAULT 10,
    Temperament INTEGER DEFAULT 10,

    LastUpdated TEXT NOT NULL,

    FOREIGN KEY (EntityId) REFERENCES Workers(Id),
    UNIQUE(EntityId, EntityType)
);

-- Table des personnalités visibles
CREATE TABLE Personalities (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL,

    -- Label de personnalité (FM-like)
    PersonalityLabel TEXT NOT NULL, -- 'Professional', 'Ambitious', 'Loyal', 'Egotistic', etc.

    -- Traits secondaires (optionnel, max 2)
    SecondaryTrait1 TEXT,
    SecondaryTrait2 TEXT,

    -- Evolution tracking
    PreviousLabel TEXT,
    LabelChangedAt TEXT,

    LastUpdated TEXT NOT NULL,

    FOREIGN KEY (EntityId) REFERENCES Workers(Id),
    UNIQUE(EntityId, EntityType)
);

-- Table d'historique des changements de personnalité
CREATE TABLE PersonalityHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    OldLabel TEXT NOT NULL,
    NewLabel TEXT NOT NULL,
    ChangeReason TEXT, -- 'Success', 'Failure', 'Trauma', 'Growth'
    ChangedAt TEXT NOT NULL,

    FOREIGN KEY (EntityId) REFERENCES Workers(Id)
);

-- Index
CREATE INDEX idx_mental_entity ON MentalAttributes(EntityId, EntityType);
CREATE INDEX idx_personality_entity ON Personalities(EntityId, EntityType);
CREATE INDEX idx_personality_history ON PersonalityHistory(EntityId);
```

**Tâches** :
- [ ] Créer la migration SQL
- [ ] Tester la migration sur copie DB
- [ ] Script de rollback
- [ ] Validation intégrité référentielle

### 1.2 Models (Semaine 1)

**Fichiers à créer** :

1. **`src/RingGeneral.Core/Models/MentalAttributes.cs`**

```csharp
namespace RingGeneral.Core.Models;

/// <summary>
/// Attributs mentaux cachés (0-20) - JAMAIS affichés au joueur
/// </summary>
public class MentalAttributes
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // Worker, Staff, Trainee

    // 10 Attributs Mentaux (0-20)
    public int Professionalism { get; set; } = 10;
    public int Ambition { get; set; } = 10;
    public int Loyalty { get; set; } = 10;
    public int Ego { get; set; } = 10;
    public int Resilience { get; set; } = 10;
    public int Adaptability { get; set; } = 10;
    public int Creativity { get; set; } = 10;
    public int WorkEthic { get; set; } = 10;
    public int SocialSkills { get; set; } = 10;
    public int Temperament { get; set; } = 10; // 0=volatile, 20=calm

    public DateTime LastUpdated { get; set; }

    // Calculated property (internal use only, never displayed)
    public int AverageScore =>
        (Professionalism + Ambition + Loyalty + Ego + Resilience +
         Adaptability + Creativity + WorkEthic + SocialSkills + Temperament) / 10;
}
```

2. **`src/RingGeneral.Core/Models/Personality.cs`**

```csharp
namespace RingGeneral.Core.Models;

public class Personality
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;

    // Label visible (FM-like)
    public string PersonalityLabel { get; set; } = "Balanced";

    // Traits secondaires (max 2)
    public string? SecondaryTrait1 { get; set; }
    public string? SecondaryTrait2 { get; set; }

    // Evolution tracking
    public string? PreviousLabel { get; set; }
    public DateTime? LabelChangedAt { get; set; }

    public DateTime LastUpdated { get; set; }

    // Helper properties
    public bool HasSecondaryTraits => !string.IsNullOrEmpty(SecondaryTrait1);
    public bool RecentlyChanged => LabelChangedAt.HasValue &&
        (DateTime.Now - LabelChangedAt.Value).TotalDays < 30;
}
```

3. **`src/RingGeneral.Core/Models/PersonalityHistory.cs`**

```csharp
namespace RingGeneral.Core.Models;

public class PersonalityHistory
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string OldLabel { get; set; } = string.Empty;
    public string NewLabel { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

4. **`src/RingGeneral.Core/Enums/PersonalityLabel.cs`**

```csharp
namespace RingGeneral.Core.Enums;

/// <summary>
/// Labels de personnalité visibles (FM-style)
/// Incomplets et interprétatifs par design
/// </summary>
public enum PersonalityLabel
{
    // Positive
    Professional,
    Loyal,
    Ambitious,
    Creative,
    Resilient,
    Adaptable,

    // Neutral
    Balanced,
    Reserved,

    // Negative
    Egotistic,
    Volatile,
    Unmotivated,
    Rebellious,

    // Complexe
    ProfessionalButEgotistic,
    AmbtiousButVolatile,
    CreativeButRebellious
}
```

**Tâches** :
- [ ] Créer les 4 Models
- [ ] Ajouter les enums PersonalityLabel
- [ ] Tests unitaires pour les propriétés calculées
- [ ] Validation des ranges (0-20)

### 1.3 Services (Semaine 2)

**Fichiers à créer** :

1. **`src/RingGeneral.Core/Services/PersonalityEngine.cs`**

```csharp
namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de calcul et évolution des personnalités
/// Transforme les attributs mentaux cachés en labels visibles
/// </summary>
public interface IPersonalityEngine
{
    /// <summary>
    /// Calcule le label de personnalité basé sur les attributs mentaux
    /// IMPORTANT: Le label est incomplet et interprétatif par design
    /// </summary>
    PersonalityLabel CalculatePersonalityLabel(MentalAttributes attributes);

    /// <summary>
    /// Met à jour les attributs mentaux suite à un événement
    /// </summary>
    void UpdateMentalAttributes(
        MentalAttributes attributes,
        string eventType,
        int intensity);

    /// <summary>
    /// Vérifie si un changement de personnalité est nécessaire
    /// </summary>
    bool ShouldPersonalityChange(
        MentalAttributes currentAttributes,
        Personality currentPersonality);

    /// <summary>
    /// Génère des traits secondaires basés sur les attributs
    /// </summary>
    List<string> GenerateSecondaryTraits(MentalAttributes attributes);
}
```

2. **`src/RingGeneral.Core/Services/Impl/PersonalityEngine.cs`**

```csharp
public class PersonalityEngine : IPersonalityEngine
{
    public PersonalityLabel CalculatePersonalityLabel(MentalAttributes attributes)
    {
        // Algorithme de mapping complexe (attributs cachés -> label visible)

        // Professionalism dominant
        if (attributes.Professionalism >= 15 && attributes.WorkEthic >= 15)
            return PersonalityLabel.Professional;

        // Ego dominant
        if (attributes.Ego >= 16 && attributes.SocialSkills < 10)
            return PersonalityLabel.Egotistic;

        // Ambition dominante
        if (attributes.Ambition >= 16)
            return PersonalityLabel.Ambitious;

        // Volatilité
        if (attributes.Temperament <= 5)
            return PersonalityLabel.Volatile;

        // Loyauté
        if (attributes.Loyalty >= 15 && attributes.Professionalism >= 12)
            return PersonalityLabel.Loyal;

        // Créativité
        if (attributes.Creativity >= 15 && attributes.Adaptability >= 12)
            return PersonalityLabel.Creative;

        // Complexe: Professional but Egotistic
        if (attributes.Professionalism >= 14 && attributes.Ego >= 14)
            return PersonalityLabel.ProfessionalButEgotistic;

        // Default: Balanced
        return PersonalityLabel.Balanced;
    }

    public void UpdateMentalAttributes(
        MentalAttributes attributes,
        string eventType,
        int intensity)
    {
        // Événements possibles:
        // - "MainEventPush" → +Ego, +Ambition
        // - "PushFailed" → -Resilience, -Ambition
        // - "ContractDispute" → -Loyalty, -Professionalism
        // - "InjuryReturn" → +Resilience
        // - "CreativeControl" → +Creativity

        switch (eventType)
        {
            case "MainEventPush":
                attributes.Ego = Math.Min(20, attributes.Ego + intensity);
                attributes.Ambition = Math.Min(20, attributes.Ambition + intensity);
                break;

            case "PushFailed":
                attributes.Resilience = Math.Max(0, attributes.Resilience - intensity);
                attributes.Ambition = Math.Max(0, attributes.Ambition - intensity);
                break;

            case "ContractDispute":
                attributes.Loyalty = Math.Max(0, attributes.Loyalty - intensity);
                attributes.Professionalism = Math.Max(0, attributes.Professionalism - intensity);
                break;

            // ... autres événements
        }

        attributes.LastUpdated = DateTime.Now;
    }

    public bool ShouldPersonalityChange(
        MentalAttributes currentAttributes,
        Personality currentPersonality)
    {
        var calculatedLabel = CalculatePersonalityLabel(currentAttributes);
        return calculatedLabel.ToString() != currentPersonality.PersonalityLabel;
    }

    public List<string> GenerateSecondaryTraits(MentalAttributes attributes)
    {
        var traits = new List<string>();

        // Max 2 traits secondaires
        if (attributes.Resilience >= 16) traits.Add("Resilient");
        if (attributes.Adaptability >= 16) traits.Add("Adaptable");
        if (attributes.SocialSkills >= 16) traits.Add("Charismatic");
        if (attributes.WorkEthic >= 16) traits.Add("Hardworking");

        return traits.Take(2).ToList();
    }
}
```

**Tâches** :
- [ ] Créer IPersonalityEngine interface
- [ ] Implémenter PersonalityEngine
- [ ] Tests unitaires pour chaque algorithme
- [ ] Valider les mappings attributs → labels
- [ ] Documenter la logique de calcul

### 1.4 Repository (Semaine 2)

**Fichier** : `src/RingGeneral.Data/Repositories/PersonalityRepository.cs`

```csharp
public interface IPersonalityRepository
{
    // Mental Attributes
    Task<MentalAttributes?> GetMentalAttributesAsync(string entityId, string entityType);
    Task SaveMentalAttributesAsync(MentalAttributes attributes);

    // Personality
    Task<Personality?> GetPersonalityAsync(string entityId, string entityType);
    Task SavePersonalityAsync(Personality personality);
    Task LogPersonalityChangeAsync(PersonalityHistory history);

    // Batch operations
    Task<List<MentalAttributes>> GetAllMentalAttributesByCompanyAsync(string companyId);
    Task<List<Personality>> GetAllPersonalitiesByCompanyAsync(string companyId);
}
```

**Tâches** :
- [ ] Créer interface IPersonalityRepository
- [ ] Implémenter avec ADO.NET
- [ ] Tests unitaires CRUD
- [ ] Tests batch operations
- [ ] Enregistrer dans DI (App.axaml.cs)

### 1.5 UI - PersonalityView (Semaine 3)

**Affichage dans ProfileView > Tab Attributs**

```xml
<!-- Après les attributs physiques, avant les attributs professionnels -->
<Border Classes="panel" Margin="0,16,0,0">
  <StackPanel Spacing="12">
    <TextBlock Classes="h3" Text="Personnalité"/>

    <!-- Label principal -->
    <StackPanel Orientation="Horizontal" Spacing="8">
      <TextBlock Classes="body" Text="Type: "/>
      <Border Classes="badge" Background="#3b82f6" Padding="8,4" CornerRadius="4">
        <TextBlock Classes="body" Foreground="White" FontWeight="SemiBold"
                   Text="{Binding PersonalityLabel}"/>
      </Border>
    </StackPanel>

    <!-- Traits secondaires -->
    <WrapPanel IsVisible="{Binding HasSecondaryTraits}">
      <TextBlock Classes="caption muted" Text="Traits: "/>
      <ItemsControl ItemsSource="{Binding SecondaryTraits}">
        <ItemsControl.ItemsPanel>
          <ItemsPanelTemplate><WrapPanel/></ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <Border Classes="badge-small" Background="#6b7280"
                    Margin="4,0" Padding="6,2" CornerRadius="3">
              <TextBlock Classes="caption" Foreground="White" Text="{Binding}"/>
            </Border>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>
    </WrapPanel>

    <!-- Indicateur de changement récent -->
    <StackPanel Orientation="Horizontal" Spacing="6"
                IsVisible="{Binding RecentlyChanged}">
      <TextBlock Text="⚠️" FontSize="14"/>
      <TextBlock Classes="caption warning">
        <Run Text="Changement récent: "/>
        <Run FontWeight="Medium" Text="{Binding PreviousLabel}"/>
        <Run Text=" → "/>
        <Run FontWeight="Medium" Text="{Binding PersonalityLabel}"/>
      </TextBlock>
    </StackPanel>

    <!-- Note explicative (UX Invisible) -->
    <TextBlock Classes="caption muted" TextWrapping="Wrap" Margin="0,8,0,0">
      Cette évaluation est basée sur les observations du staff et peut être incomplète ou subjective.
    </TextBlock>
  </StackPanel>
</Border>
```

**ViewModel** : Ajouter dans `AttributesTabViewModel.cs`

```csharp
// Personality properties
public string PersonalityLabel { get; set; } = "Balanced";
public ObservableCollection<string> SecondaryTraits { get; } = new();
public bool HasSecondaryTraits => SecondaryTraits.Any();
public bool RecentlyChanged { get; set; }
public string? PreviousLabel { get; set; }
```

**Tâches** :
- [ ] Modifier AttributesTabViewModel
- [ ] Ajouter section Personnalité dans AttributesTabView.axaml
- [ ] Tests UI (affichage correct des labels)
- [ ] Tooltips explicatifs

### 1.6 Integration & Tests (Semaine 3-4)

**Tests d'intégration** :
- [ ] Création worker → génération attributs mentaux aléatoires
- [ ] Calcul initial du label de personnalité
- [ ] Événement "MainEventPush" → évolution attributs → changement label
- [ ] Affichage dans ProfileView
- [ ] Persistance en DB

**Data Seeding** :
- [ ] Générer attributs mentaux pour 50+ workers existants
- [ ] Varier les profils (certains Egotistic, d'autres Professional, etc.)
- [ ] Validation cohérence (label correspond aux attributs)

**Livrables Phase 1** :
- ✅ 3 tables DB (MentalAttributes, Personalities, PersonalityHistory)
- ✅ 4 Models créés
- ✅ PersonalityEngine fonctionnel
- ✅ Repository complet
- ✅ UI intégrée dans ProfileView
- ✅ Tests passants (>80% coverage)
- ✅ Data seed pour 50+ workers

---

## 🤝 PHASE 2 : RELATIONS & NEPOTISM

**Durée** : 2-3 semaines
**Priorité** : 🔴 HAUTE
**Dépendances** : Phase 1 (Personality)

### Objectifs

Implémenter le **système de relations** entre workers/staff et les **biais de népotisme** qui en découlent :
- 4 types de relations (Familiale, Mentorat, Favoritisme, Rivalité)
- Intensité cachée (0-100)
- Impact sur décisions (push, sanctions, protection)

### 2.1 Base de Données (Semaine 4)

**Note** : Les tables `WorkerRelations` et `Factions` existent déjà (voir PLAN_MASTER_PROFILEVIEW_ATTRIBUTS.md).
Nous allons les **enrichir** avec les champs de népotisme.

#### Migration SQL

**Fichier** : `src/RingGeneral.Data/Migrations/006_nepotism_relations.sql`

```sql
-- Enrichir WorkerRelations avec népotisme
ALTER TABLE WorkerRelations ADD COLUMN IsHidden BOOLEAN DEFAULT 0; -- Relation backstage vs publique
ALTER TABLE WorkerRelations ADD COLUMN BiasStrength INTEGER DEFAULT 0; -- 0-100, intensité du biais
ALTER TABLE WorkerRelations ADD COLUMN OriginEvent TEXT; -- Event qui a créé la relation
ALTER TABLE WorkerRelations ADD COLUMN LastImpact TEXT; -- Dernier impact observable

-- Table des impacts de népotisme (log)
CREATE TABLE NepotismImpacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RelationId INTEGER NOT NULL,
    ImpactType TEXT NOT NULL, -- 'Push', 'Protection', 'Sanction', 'Opportunity'
    TargetEntityId TEXT NOT NULL,
    DecisionMakerId TEXT NOT NULL, -- Qui a pris la décision biaisée
    Severity INTEGER DEFAULT 1, -- 1-5
    IsVisible BOOLEAN DEFAULT 0, -- Si le joueur peut l'observer
    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (RelationId) REFERENCES WorkerRelations(Id)
);

-- Index
CREATE INDEX idx_nepotism_relation ON NepotismImpacts(RelationId);
CREATE INDEX idx_nepotism_target ON NepotismImpacts(TargetEntityId);
```

**Tâches** :
- [ ] Créer migration SQL
- [ ] Tester sur copie DB
- [ ] Script rollback
- [ ] Validation intégrité

### 2.2 Models (Semaine 4)

**Enrichir** : `src/RingGeneral.Core/Models/WorkerRelation.cs`

```csharp
public class WorkerRelation
{
    // ... propriétés existantes ...

    // Népotisme
    public bool IsHidden { get; set; } // Relation backstage vs publique (kayfabe)
    public int BiasStrength { get; set; } = 0; // 0-100
    public string? OriginEvent { get; set; } // "FamilyTie", "MentorshipStart", "OnScreenRivalry"
    public string? LastImpact { get; set; } // "Protected from firing", "Pushed despite low morale"

    // Calculated properties
    public bool HasStrongBias => BiasStrength >= 70;
    public bool HasModerateBias => BiasStrength >= 40 && BiasStrength < 70;
}
```

**Nouveau Model** : `src/RingGeneral.Core/Models/NepotismImpact.cs`

```csharp
public class NepotismImpact
{
    public int Id { get; set; }
    public int RelationId { get; set; }
    public string ImpactType { get; set; } = string.Empty; // Push, Protection, Sanction, Opportunity
    public string TargetEntityId { get; set; } = string.Empty;
    public string DecisionMakerId { get; set; } = string.Empty; // Owner, Booker, etc.
    public int Severity { get; set; } = 1; // 1-5
    public bool IsVisible { get; set; } // Le joueur peut-il l'observer ?
    public DateTime CreatedAt { get; set; }
}
```

**Tâches** :
- [ ] Enrichir WorkerRelation.cs
- [ ] Créer NepotismImpact.cs
- [ ] Tests unitaires

### 2.3 Services (Semaine 5)

**Nouveau Service** : `src/RingGeneral.Core/Services/NepotismEngine.cs`

```csharp
public interface INepotismEngine
{
    /// <summary>
    /// Vérifie si une décision est biaisée par une relation
    /// </summary>
    bool IsDecisionBiased(string decisionMakerId, string targetEntityId, string decisionType);

    /// <summary>
    /// Calcule le délai de sanction basé sur les relations
    /// Ex: Worker fils de l'Owner → sanction retardée
    /// </summary>
    int CalculateSanctionDelay(string targetEntityId, string offense);

    /// <summary>
    /// Détermine si un push est justifié ou biaisé
    /// </summary>
    bool IsPushJustified(string workerId, int currentPopularity, int currentSkill);

    /// <summary>
    /// Enregistre un impact de népotisme (pour tracking interne)
    /// </summary>
    void LogNepotismImpact(
        int relationId,
        string impactType,
        string targetEntityId,
        string decisionMakerId,
        int severity);
}
```

**Implémentation** : `src/RingGeneral.Core/Services/Impl/NepotismEngine.cs`

```csharp
public class NepotismEngine : INepotismEngine
{
    private readonly IRelationsRepository _relationsRepository;
    private readonly IPersonalityRepository _personalityRepository;

    public bool IsDecisionBiased(string decisionMakerId, string targetEntityId, string decisionType)
    {
        // Récupérer toutes les relations du decision maker
        var relations = _relationsRepository.GetRelationsByEntityId(decisionMakerId);

        // Chercher relation avec target
        var relation = relations.FirstOrDefault(r =>
            r.RelatedWorkerId == targetEntityId || r.WorkerId == targetEntityId);

        if (relation == null) return false;

        // Vérifier si la relation biaise la décision
        switch (decisionType)
        {
            case "Push":
                return relation.RelationType == "Familiale" ||
                       relation.RelationType == "Favoritisme";

            case "Sanction":
                return relation.BiasStrength >= 50; // Retard de sanction

            case "Promotion":
                return relation.RelationType == "Mentorat" && relation.BiasStrength >= 60;

            default:
                return false;
        }
    }

    public int CalculateSanctionDelay(string targetEntityId, string offense)
    {
        var relations = _relationsRepository.GetRelationsForEntity(targetEntityId);

        int maxDelay = 0;

        foreach (var relation in relations)
        {
            if (relation.RelationType == "Familiale" && relation.BiasStrength >= 80)
            {
                // Relation familiale forte → +4 semaines de délai
                maxDelay = Math.Max(maxDelay, 4);
            }
            else if (relation.RelationType == "Favoritisme" && relation.BiasStrength >= 60)
            {
                // Favoritisme → +2 semaines
                maxDelay = Math.Max(maxDelay, 2);
            }
        }

        return maxDelay;
    }

    public bool IsPushJustified(string workerId, int currentPopularity, int currentSkill)
    {
        // Un push est "justifié" si:
        // - Popularity >= 70 OU
        // - Skill >= 75 OU
        // - Momentum élevé

        // Si aucun de ces critères, c'est probablement du népotisme
        return currentPopularity >= 70 || currentSkill >= 75;
    }

    public void LogNepotismImpact(
        int relationId,
        string impactType,
        string targetEntityId,
        string decisionMakerId,
        int severity)
    {
        var impact = new NepotismImpact
        {
            RelationId = relationId,
            ImpactType = impactType,
            TargetEntityId = targetEntityId,
            DecisionMakerId = decisionMakerId,
            Severity = severity,
            IsVisible = severity >= 3, // Seulement si flagrant
            CreatedAt = DateTime.Now
        };

        _relationsRepository.SaveNepotismImpact(impact);
    }
}
```

**Tâches** :
- [ ] Créer INepotismEngine interface
- [ ] Implémenter NepotismEngine
- [ ] Tests unitaires pour chaque scénario
- [ ] Documenter les seuils

### 2.4 Integration avec Booker (Semaine 5)

**Modifier** : `src/RingGeneral.Core/Services/BookingBuilderService.cs`

```csharp
public class BookingBuilderService
{
    private readonly INepotismEngine _nepotismEngine;

    // Lors d'un push décidé par le Booker
    public void AssignPush(string workerId, string pushLevel)
    {
        var worker = _workerRepository.GetById(workerId);

        // Vérifier si le push est biaisé
        bool isBiased = _nepotismEngine.IsDecisionBiased(
            CurrentBookerId,
            workerId,
            "Push");

        if (isBiased)
        {
            // Enregistrer l'impact de népotisme
            _nepotismEngine.LogNepotismImpact(
                relationId: GetRelationId(CurrentBookerId, workerId),
                impactType: "Push",
                targetEntityId: workerId,
                decisionMakerId: CurrentBookerId,
                severity: CalculatePushBiasSeverity(worker)
            );
        }

        // Appliquer le push normalement
        worker.PushLevel = pushLevel;
        _workerRepository.Update(worker);
    }
}
```

**Tâches** :
- [ ] Intégrer NepotismEngine dans BookingBuilderService
- [ ] Intégrer dans système de sanctions (BackstageService)
- [ ] Tests d'intégration

### 2.5 UI - Indicators (Semaine 6)

**Affichage subtil dans UI (UX Invisible)** :

Dans **RosterView** ou **ProfileView**, afficher un indicateur *subtil* si népotisme détecté :

```xml
<!-- Dans la fiche worker, après le push level -->
<StackPanel Orientation="Horizontal" Spacing="4"
            IsVisible="{Binding HasSuspectedNepotism}">
  <TextBlock Text="⚠️" FontSize="12"/>
  <TextBlock Classes="caption warning">
    Push inhabituel pour ce niveau de popularité
  </TextBlock>
</StackPanel>
```

**Important** : Ne jamais afficher directement "Népotisme détecté", seulement des **signaux subtils** :
- "Push inhabituel"
- "Protection inhabituelle"
- "Décision controversée"

**Tâches** :
- [ ] Ajouter propriété `HasSuspectedNepotism` dans WorkerDetailViewModel
- [ ] Calculer basé sur NepotismImpacts visibles
- [ ] Afficher indicateur subtil
- [ ] Tests UI

### 2.6 Tests & Validation (Semaine 6)

**Scénarios de test** :
- [ ] Booker fils de l'Owner → push un worker médiocre → népotisme logged
- [ ] Worker avec relation "Mentorat" → promotion rapide → népotisme logged
- [ ] Worker sans relation → push mérité → PAS de népotisme
- [ ] Affichage indicateurs subtils dans UI
- [ ] Persistance en DB

**Livrables Phase 2** :
- ✅ Table NepotismImpacts créée
- ✅ WorkerRelation enrichi avec BiasStrength
- ✅ NepotismEngine fonctionnel
- ✅ Intégration avec Booking
- ✅ Indicateurs subtils en UI
- ✅ Tests passants

---

## 📊 PHASE 3 : BACKSTAGE MORALE & RUMORS

**Durée** : 3-4 semaines
**Priorité** : 🔴 HAUTE
**Dépendances** : Phase 1 (Personality), Phase 2 (Relations)

### Objectifs

Implémenter le **système de moral backstage** et le **système de rumeurs** :
- Moral global et individuel
- Rumeurs émergentes (interprétations collectives)
- Pipeline : Signaux faibles → Rumeurs → Crises

### 3.1 Base de Données (Semaine 7)

#### Migration SQL

**Fichier** : `src/RingGeneral.Data/Migrations/007_backstage_morale_rumors.sql`

```sql
-- Table du moral individuel
CREATE TABLE BackstageMorale (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL, -- Worker, Staff
    CompanyId TEXT NOT NULL,

    -- Moral (0-100)
    MoraleScore INTEGER DEFAULT 70,
    PreviousMoraleScore INTEGER DEFAULT 70,

    -- Facteurs contributeurs (cachés)
    PaySatisfaction INTEGER DEFAULT 50,
    PushSatisfaction INTEGER DEFAULT 50,
    RelationshipSatisfaction INTEGER DEFAULT 50,
    CreativeControlSatisfaction INTEGER DEFAULT 50,
    WorkloadSatisfaction INTEGER DEFAULT 50,

    -- Tracking
    LastUpdated TEXT NOT NULL,
    LastImpactEvent TEXT,

    FOREIGN KEY (EntityId) REFERENCES Workers(Id),
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    UNIQUE(EntityId, CompanyId)
);

-- Table du moral global de la compagnie
CREATE TABLE CompanyMorale (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL UNIQUE,

    -- Moral global (moyenne pondérée)
    GlobalMoraleScore INTEGER DEFAULT 70,

    -- Breakdown par type
    WorkersMoraleAvg INTEGER DEFAULT 70,
    StaffMoraleAvg INTEGER DEFAULT 70,

    -- Tendance
    Trend TEXT DEFAULT 'Stable', -- Improving, Stable, Declining

    LastUpdated TEXT NOT NULL,

    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

-- Table des rumeurs
CREATE TABLE Rumors (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,

    -- Contenu de la rumeur
    RumorType TEXT NOT NULL, -- 'Nepotism', 'UnfairPush', 'PayDisparity', 'Favoritism'
    RumorText TEXT NOT NULL, -- Texte généré dynamiquement

    -- Origine
    OriginEntityId TEXT, -- Worker qui a "lancé" la rumeur (optionnel)
    TriggerEvent TEXT, -- Événement déclencheur

    -- Statut
    Stage TEXT DEFAULT 'Emerging', -- Emerging, Growing, Widespread, Resolved, Ignored
    Severity INTEGER DEFAULT 1, -- 1-5

    -- Amplification
    AmplificationScore INTEGER DEFAULT 10, -- 0-100
    InfluencersInvolved TEXT, -- Liste des workers influents qui amplifient

    -- Dates
    CreatedAt TEXT NOT NULL,
    LastAmplifiedAt TEXT,
    ResolvedAt TEXT,

    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

-- Index
CREATE INDEX idx_morale_entity ON BackstageMorale(EntityId, CompanyId);
CREATE INDEX idx_morale_company ON CompanyMorale(CompanyId);
CREATE INDEX idx_rumors_company ON Rumors(CompanyId, Stage);
```

**Tâches** :
- [ ] Créer migration SQL
- [ ] Tester sur copie DB
- [ ] Script rollback
- [ ] Validation

### 3.2 Models (Semaine 7)

**Créer** : `src/RingGeneral.Core/Models/BackstageMorale.cs`

```csharp
public class BackstageMorale
{
    public int Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;

    // Moral (0-100)
    public int MoraleScore { get; set; } = 70;
    public int PreviousMoraleScore { get; set; } = 70;

    // Facteurs contributeurs (cachés)
    public int PaySatisfaction { get; set; } = 50;
    public int PushSatisfaction { get; set; } = 50;
    public int RelationshipSatisfaction { get; set; } = 50;
    public int CreativeControlSatisfaction { get; set; } = 50;
    public int WorkloadSatisfaction { get; set; } = 50;

    public DateTime LastUpdated { get; set; }
    public string? LastImpactEvent { get; set; }

    // Calculated properties
    public bool IsLow => MoraleScore < 40;
    public bool IsCritical => MoraleScore < 20;
    public bool IsImproving => MoraleScore > PreviousMoraleScore;
    public bool IsDeclining => MoraleScore < PreviousMoraleScore;

    public string MoraleLabel => MoraleScore switch
    {
        >= 80 => "Excellent",
        >= 60 => "Good",
        >= 40 => "Average",
        >= 20 => "Low",
        _ => "Critical"
    };
}
```

**Créer** : `src/RingGeneral.Core/Models/CompanyMorale.cs`

```csharp
public class CompanyMorale
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;

    public int GlobalMoraleScore { get; set; } = 70;
    public int WorkersMoraleAvg { get; set; } = 70;
    public int StaffMoraleAvg { get; set; } = 70;

    public string Trend { get; set; } = "Stable"; // Improving, Stable, Declining

    public DateTime LastUpdated { get; set; }

    // Calculated
    public bool IsHealthy => GlobalMoraleScore >= 60;
    public bool IsCrisis => GlobalMoraleScore < 30;
}
```

**Créer** : `src/RingGeneral.Core/Models/Rumor.cs`

```csharp
public class Rumor
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;

    public string RumorType { get; set; } = string.Empty;
    public string RumorText { get; set; } = string.Empty;

    public string? OriginEntityId { get; set; }
    public string? TriggerEvent { get; set; }

    public string Stage { get; set; } = "Emerging"; // Emerging, Growing, Widespread, Resolved, Ignored
    public int Severity { get; set; } = 1; // 1-5

    public int AmplificationScore { get; set; } = 10;
    public string? InfluencersInvolved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastAmplifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Calculated
    public bool IsActive => Stage != "Resolved" && Stage != "Ignored";
    public bool IsWideSpread => AmplificationScore >= 70;
}
```

**Tâches** :
- [ ] Créer 3 Models
- [ ] Tests unitaires
- [ ] Validation propriétés calculées

### 3.3 Services (Semaine 8-9)

**Service 1** : `src/RingGeneral.Core/Services/MoraleEngine.cs`

```csharp
public interface IMoraleEngine
{
    /// <summary>
    /// Met à jour le moral d'une entité suite à un événement
    /// </summary>
    void UpdateMorale(string entityId, string eventType, int impact);

    /// <summary>
    /// Calcule le moral global de la compagnie
    /// </summary>
    CompanyMorale CalculateCompanyMorale(string companyId);

    /// <summary>
    /// Détecte les signaux faibles (avant qu'ils ne deviennent rumeurs)
    /// </summary>
    List<string> DetectWeakSignals(string companyId);
}
```

**Service 2** : `src/RingGeneral.Core/Services/RumorEngine.cs`

```csharp
public interface IRumorEngine
{
    /// <summary>
    /// Vérifie si un événement doit déclencher une rumeur
    /// </summary>
    bool ShouldTriggerRumor(string companyId, string eventType, int eventSeverity);

    /// <summary>
    /// Génère une nouvelle rumeur
    /// </summary>
    Rumor GenerateRumor(string companyId, string rumorType, string triggerEvent);

    /// <summary>
    /// Amplifie une rumeur existante (si workers influents la reprennent)
    /// </summary>
    void AmplifyRumor(int rumorId, string influencerWorkerId);

    /// <summary>
    /// Fait progresser les rumeurs (Emerging → Growing → Widespread)
    /// </summary>
    void ProgressRumors(string companyId);

    /// <summary>
    /// Résout une rumeur (suite à action du joueur)
    /// </summary>
    void ResolveRumor(int rumorId, string resolutionMethod);
}
```

**Implémentation MoraleEngine** :

```csharp
public class MoraleEngine : IMoraleEngine
{
    public void UpdateMorale(string entityId, string eventType, int impact)
    {
        var morale = _moraleRepository.GetMoraleAsync(entityId);

        morale.PreviousMoraleScore = morale.MoraleScore;

        switch (eventType)
        {
            case "MainEventPush":
                morale.PushSatisfaction += impact;
                morale.MoraleScore += impact;
                break;

            case "PayCut":
                morale.PaySatisfaction -= impact;
                morale.MoraleScore -= impact;
                break;

            case "UnfairPush": // Népotisme observé
                morale.PushSatisfaction -= impact;
                morale.MoraleScore -= impact;
                break;

            case "CreativeControl":
                morale.CreativeControlSatisfaction += impact;
                morale.MoraleScore += impact;
                break;

            // ...
        }

        // Clamp 0-100
        morale.MoraleScore = Math.Clamp(morale.MoraleScore, 0, 100);
        morale.LastUpdated = DateTime.Now;
        morale.LastImpactEvent = eventType;

        _moraleRepository.SaveMoraleAsync(morale);
    }

    public CompanyMorale CalculateCompanyMorale(string companyId)
    {
        var allMorales = _moraleRepository.GetAllMoralesByCompanyAsync(companyId);

        var workerMorales = allMorales.Where(m => m.EntityType == "Worker").ToList();
        var staffMorales = allMorales.Where(m => m.EntityType == "Staff").ToList();

        var companyMorale = new CompanyMorale
        {
            CompanyId = companyId,
            WorkersMoraleAvg = (int)workerMorales.Average(m => m.MoraleScore),
            StaffMoraleAvg = (int)staffMorales.Average(m => m.MoraleScore),
            LastUpdated = DateTime.Now
        };

        // Moyenne pondérée (workers comptent plus)
        companyMorale.GlobalMoraleScore =
            (companyMorale.WorkersMoraleAvg * 70 + companyMorale.StaffMoraleAvg * 30) / 100;

        // Déterminer tendance
        var previousCompanyMorale = _moraleRepository.GetCompanyMoraleAsync(companyId);
        if (companyMorale.GlobalMoraleScore > previousCompanyMorale.GlobalMoraleScore + 5)
            companyMorale.Trend = "Improving";
        else if (companyMorale.GlobalMoraleScore < previousCompanyMorale.GlobalMoraleScore - 5)
            companyMorale.Trend = "Declining";
        else
            companyMorale.Trend = "Stable";

        return companyMorale;
    }

    public List<string> DetectWeakSignals(string companyId)
    {
        var signals = new List<string>();

        var morale = CalculateCompanyMorale(companyId);

        if (morale.GlobalMoraleScore < 50)
            signals.Add("Moral général en baisse");

        if (morale.Trend == "Declining")
            signals.Add("Tendance négative persistante");

        // Détecter clusters de mécontents
        var lowMorales = _moraleRepository.GetAllMoralesByCompanyAsync(companyId)
            .Where(m => m.MoraleScore < 40).ToList();

        if (lowMorales.Count >= 5)
            signals.Add($"{lowMorales.Count} talents insatisfaits");

        return signals;
    }
}
```

**Implémentation RumorEngine** :

```csharp
public class RumorEngine : IRumorEngine
{
    private readonly IRumorRepository _rumorRepository;
    private readonly IMoraleEngine _moraleEngine;
    private readonly INepotismEngine _nepotismEngine;

    public bool ShouldTriggerRumor(string companyId, string eventType, int eventSeverity)
    {
        // Conditions de déclenchement:
        // 1. Népotisme flagrant (severity >= 3)
        // 2. Moral bas (< 40) + événement négatif
        // 3. Pattern de décisions incohérentes

        if (eventType == "NepotismDetected" && eventSeverity >= 3)
            return true;

        var companyMorale = _moraleEngine.CalculateCompanyMorale(companyId);
        if (companyMorale.GlobalMoraleScore < 40)
            return true;

        return false;
    }

    public Rumor GenerateRumor(string companyId, string rumorType, string triggerEvent)
    {
        var rumor = new Rumor
        {
            CompanyId = companyId,
            RumorType = rumorType,
            RumorText = GenerateRumorText(rumorType, triggerEvent),
            TriggerEvent = triggerEvent,
            Stage = "Emerging",
            Severity = CalculateInitialSeverity(rumorType),
            AmplificationScore = 10,
            CreatedAt = DateTime.Now
        };

        _rumorRepository.SaveRumorAsync(rumor);
        return rumor;
    }

    private string GenerateRumorText(string rumorType, string triggerEvent)
    {
        return rumorType switch
        {
            "Nepotism" => "Certains talents murmurent que les décisions de booking ne sont pas toujours méritocratiques.",
            "UnfairPush" => "Des voix s'élèvent contre certains choix de push jugés injustifiés.",
            "PayDisparity" => "Des discussions sur les disparités salariales circulent dans les vestiaires.",
            "Favoritism" => "Plusieurs workers se plaignent d'un traitement de faveur envers certains.",
            _ => "Des tensions se font sentir en coulisses."
        };
    }

    public void AmplifyRumor(int rumorId, string influencerWorkerId)
    {
        var rumor = _rumorRepository.GetRumorByIdAsync(rumorId);

        // Ajouter l'influencer à la liste
        var influencers = rumor.InfluencersInvolved?.Split(',').ToList() ?? new List<string>();
        influencers.Add(influencerWorkerId);
        rumor.InfluencersInvolved = string.Join(",", influencers);

        // Augmenter amplification
        rumor.AmplificationScore += 15;
        rumor.LastAmplifiedAt = DateTime.Now;

        // Progression de stage
        if (rumor.AmplificationScore >= 70)
            rumor.Stage = "Widespread";
        else if (rumor.AmplificationScore >= 40)
            rumor.Stage = "Growing";

        _rumorRepository.UpdateRumorAsync(rumor);
    }

    public void ProgressRumors(string companyId)
    {
        var activeRumors = _rumorRepository.GetActiveRumorsByCompanyAsync(companyId);

        foreach (var rumor in activeRumors)
        {
            // Amplification naturelle (lente)
            rumor.AmplificationScore += 2;

            // Progression de stage
            if (rumor.AmplificationScore >= 70 && rumor.Stage != "Widespread")
            {
                rumor.Stage = "Widespread";
                // Déclencher événement "Crisis" si severity >= 4
                if (rumor.Severity >= 4)
                {
                    // Trigger Crisis Event
                }
            }
            else if (rumor.AmplificationScore >= 40 && rumor.Stage == "Emerging")
            {
                rumor.Stage = "Growing";
            }

            // Decay naturel si pas amplifié récemment
            if (rumor.LastAmplifiedAt.HasValue &&
                (DateTime.Now - rumor.LastAmplifiedAt.Value).TotalDays > 14)
            {
                rumor.AmplificationScore -= 5;

                if (rumor.AmplificationScore <= 0)
                {
                    rumor.Stage = "Ignored";
                    rumor.ResolvedAt = DateTime.Now;
                }
            }

            _rumorRepository.UpdateRumorAsync(rumor);
        }
    }

    public void ResolveRumor(int rumorId, string resolutionMethod)
    {
        var rumor = _rumorRepository.GetRumorByIdAsync(rumorId);

        rumor.Stage = "Resolved";
        rumor.ResolvedAt = DateTime.Now;

        // Impact sur moral selon méthode de résolution
        // "Addressed" → +10 morale
        // "Ignored" → -5 morale

        _rumorRepository.UpdateRumorAsync(rumor);
    }
}
```

**Tâches** :
- [ ] Créer MoraleEngine
- [ ] Créer RumorEngine
- [ ] Tests unitaires pour chaque scénario
- [ ] Tests d'intégration

### 3.4 Integration Weekly Cycle (Semaine 9)

**Modifier** : `src/RingGeneral.Core/Services/GameLoopService.cs`

```csharp
public class GameLoopService
{
    private readonly IMoraleEngine _moraleEngine;
    private readonly IRumorEngine _rumorEngine;

    public async Task PassWeekAsync()
    {
        // ... existing logic ...

        // Update company morale
        var companyMorale = _moraleEngine.CalculateCompanyMorale(CurrentCompanyId);

        // Detect weak signals
        var weakSignals = _moraleEngine.DetectWeakSignals(CurrentCompanyId);
        if (weakSignals.Any())
        {
            // Send notification to Inbox
            foreach (var signal in weakSignals)
            {
                _inboxService.AddMessageAsync(new InboxMessage
                {
                    Type = "WeakSignal",
                    Title = "Signal faible détecté",
                    Content = signal,
                    Priority = "Medium"
                });
            }
        }

        // Progress existing rumors
        _rumorEngine.ProgressRumors(CurrentCompanyId);

        // ... existing logic ...
    }
}
```

**Tâches** :
- [ ] Intégrer MoraleEngine dans GameLoopService
- [ ] Intégrer RumorEngine dans GameLoopService
- [ ] Tests end-to-end

### 3.5 UI - Dashboard Morale (Semaine 10)

**Ajouter dans DashboardView** :

```xml
<!-- Company Morale Card -->
<Border Classes="card" Grid.Column="0" Grid.Row="1">
  <StackPanel Spacing="12">
    <TextBlock Classes="h3" Text="Moral Général"/>

    <!-- Score global -->
    <StackPanel Orientation="Horizontal" Spacing="8">
      <TextBlock Classes="h1" FontWeight="Bold"
                 Text="{Binding CompanyMorale.GlobalMoraleScore}"/>
      <TextBlock Classes="body muted" VerticalAlignment="Bottom" Text="/100"/>
    </StackPanel>

    <!-- Tendance -->
    <StackPanel Orientation="Horizontal" Spacing="6">
      <TextBlock Text="{Binding CompanyMorale.TrendIcon}"/> <!-- ↑ ↓ → -->
      <TextBlock Classes="body" Text="{Binding CompanyMorale.Trend}"/>
    </StackPanel>

    <!-- Breakdown -->
    <StackPanel Spacing="4">
      <TextBlock Classes="caption muted">
        <Run Text="Workers: "/>
        <Run FontWeight="Medium" Text="{Binding CompanyMorale.WorkersMoraleAvg}"/>
      </TextBlock>
      <TextBlock Classes="caption muted">
        <Run Text="Staff: "/>
        <Run FontWeight="Medium" Text="{Binding CompanyMorale.StaffMoraleAvg}"/>
      </TextBlock>
    </StackPanel>

    <!-- Weak Signals -->
    <ItemsControl ItemsSource="{Binding WeakSignals}"
                  IsVisible="{Binding HasWeakSignals}">
      <ItemsControl.ItemTemplate>
        <DataTemplate>
          <StackPanel Orientation="Horizontal" Spacing="4" Margin="0,2">
            <TextBlock Text="⚠️" FontSize="12"/>
            <TextBlock Classes="caption warning" Text="{Binding}"/>
          </StackPanel>
        </DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
  </StackPanel>
</Border>
```

**UI - Rumors in Inbox** :

```xml
<!-- Dans InboxView -->
<Border Classes="card rumor-card"
        IsVisible="{Binding IsRumor}"
        Background="#fff3cd">
  <StackPanel Spacing="8">
    <StackPanel Orientation="Horizontal" Spacing="8">
      <TextBlock Text="🗣️" FontSize="20"/>
      <TextBlock Classes="body" FontWeight="SemiBold" Text="Rumeur"/>
      <Border Classes="badge" Background="#f59e0b">
        <TextBlock Classes="caption" Foreground="White" Text="{Binding RumorStage}"/>
      </Border>
    </StackPanel>

    <TextBlock Classes="body" TextWrapping="Wrap" Text="{Binding RumorText}"/>

    <!-- Actions -->
    <WrapPanel Spacing="8">
      <Button Classes="secondary" Content="Enquêter"
              Command="{Binding InvestigateCommand}"/>
      <Button Classes="secondary" Content="Ignorer"
              Command="{Binding IgnoreCommand}"/>
    </WrapPanel>
  </StackPanel>
</Border>
```

**Tâches** :
- [ ] Ajouter Company Morale card dans Dashboard
- [ ] Ajouter Rumors cards dans Inbox
- [ ] Tests UI

### 3.6 Tests & Validation (Semaine 10)

**Scénarios de test** :
- [ ] Push népotiste → moral baisse → weak signal → rumeur "Nepotism"
- [ ] Rumeur amplifiée par influencers → stage "Widespread"
- [ ] Joueur résout rumeur → moral remonte
- [ ] Rumeur ignorée → decay naturel → stage "Ignored"

**Livrables Phase 3** :
- ✅ 3 tables (BackstageMorale, CompanyMorale, Rumors)
- ✅ 3 Models créés
- ✅ MoraleEngine + RumorEngine fonctionnels
- ✅ Intégration weekly cycle
- ✅ UI Dashboard Morale + Inbox Rumors
- ✅ Tests passants

---

## 👔 PHASE 4 : OWNER & BOOKER SYSTEMS

**Durée** : 4-5 semaines
**Priorité** : 🟡 MOYENNE
**Dépendances** : Phase 1 (Personality), Phase 3 (Morale)

### Objectifs

Implémenter les **systèmes Owner et Booker** avec :
- Owner stratégique (économie, crises, eras)
- Booker avec mémoire persistante
- Préférences de produit du Booker
- Let the Booker Decide (AI booking)

### 4.1 Base de Données (Semaine 11)

#### Migration SQL

**Fichier** : `src/RingGeneral.Data/Migrations/008_owner_booker_systems.sql`

```sql
-- Table Owner (1 par compagnie)
CREATE TABLE Owners (
    Id TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL UNIQUE,

    -- Identité
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,

    -- Personnalité (utilise MentalAttributes + Personality)
    PersonalityId INTEGER,

    -- Strategic Priorities (0-100)
    FinancialPriority INTEGER DEFAULT 50,
    CreativePriority INTEGER DEFAULT 50,
    ExpansionPriority INTEGER DEFAULT 50,
    TalentDevelopmentPriority INTEGER DEFAULT 50,

    -- Decision Making Style
    DecisionStyle TEXT DEFAULT 'Balanced', -- Aggressive, Conservative, Balanced
    RiskTolerance INTEGER DEFAULT 50, -- 0-100

    -- Relations
    CurrentBookerId TEXT,

    CreatedAt TEXT NOT NULL,
    LastUpdated TEXT NOT NULL,

    FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (CurrentBookerId) REFERENCES Bookers(Id),
    FOREIGN KEY (PersonalityId) REFERENCES Personalities(Id)
);

-- Table Bookers (peut changer de compagnie)
CREATE TABLE Bookers (
    Id TEXT PRIMARY KEY,

    -- Identité
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,

    -- Current Employment
    CurrentCompanyId TEXT,
    HiredAt TEXT,

    -- Personnalité
    PersonalityId INTEGER,

    -- Product Preferences (0-100)
    LuchaPreference INTEGER DEFAULT 50,
    PuroresuPreference INTEGER DEFAULT 50,
    EntertainmentPreference INTEGER DEFAULT 50,
    HardcorePreference INTEGER DEFAULT 50,
    OldSchoolPreference INTEGER DEFAULT 50,

    -- Booking Style
    StarsFocus INTEGER DEFAULT 50, -- vs Young Talent Focus
    StabilityFocus INTEGER DEFAULT 50, -- vs Chaos Focus
    RiskTolerance INTEGER DEFAULT 50, -- vs Prudence

    CreatedAt TEXT NOT NULL,
    LastUpdated TEXT NOT NULL,

    FOREIGN KEY (CurrentCompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (PersonalityId) REFERENCES Personalities(Id)
);

-- Table Booker Memory (persistante entre compagnies)
CREATE TABLE BookerMemory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BookerId TEXT NOT NULL,

    -- Type de mémoire
    MemoryType TEXT NOT NULL, -- 'Bias', 'Trauma', 'Success', 'Protege', 'Grudge'

    -- Contenu
    TargetEntityId TEXT, -- Worker concerné (si applicable)
    Description TEXT NOT NULL,
    Intensity INTEGER DEFAULT 50, -- 0-100

    -- Origin
    OriginEvent TEXT,
    OriginCompanyId TEXT,
    CreatedAt TEXT NOT NULL,

    -- Decay
    DecayRate INTEGER DEFAULT 5, -- Perd 5 points d'intensité par an
    LastDecayAt TEXT,

    FOREIGN KEY (BookerId) REFERENCES Bookers(Id),
    FOREIGN KEY (OriginCompanyId) REFERENCES Companies(Id)
);

-- Table Booker Employment History
CREATE TABLE BookerEmploymentHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BookerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,

    StartDate TEXT NOT NULL,
    EndDate TEXT,
    TerminationReason TEXT, -- 'Fired', 'Quit', 'Promotion', 'Active'

    -- Performance during tenure
    AverageShowRating REAL,
    MajorSuccesses INTEGER DEFAULT 0,
    MajorFailures INTEGER DEFAULT 0,

    FOREIGN KEY (BookerId) REFERENCES Bookers(Id),
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

-- Index
CREATE INDEX idx_owner_company ON Owners(CompanyId);
CREATE INDEX idx_booker_company ON Bookers(CurrentCompanyId);
CREATE INDEX idx_booker_memory ON BookerMemory(BookerId);
```

**Tâches** :
- [ ] Créer migration SQL
- [ ] Tester sur copie DB
- [ ] Script rollback

### 4.2 Models (Semaine 11)

**Créer** : `src/RingGeneral.Core/Models/Owner.cs`

```csharp
public class Owner
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public int PersonalityId { get; set; }

    // Strategic Priorities (0-100)
    public int FinancialPriority { get; set; } = 50;
    public int CreativePriority { get; set; } = 50;
    public int ExpansionPriority { get; set; } = 50;
    public int TalentDevelopmentPriority { get; set; } = 50;

    public string DecisionStyle { get; set; } = "Balanced";
    public int RiskTolerance { get; set; } = 50;

    public string? CurrentBookerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation
    public Booker? CurrentBooker { get; set; }
    public Personality? Personality { get; set; }
}
```

**Créer** : `src/RingGeneral.Core/Models/Booker.cs`

```csharp
public class Booker
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public string? CurrentCompanyId { get; set; }
    public DateTime? HiredAt { get; set; }

    public int PersonalityId { get; set; }

    // Product Preferences (0-100)
    public int LuchaPreference { get; set; } = 50;
    public int PuroresuPreference { get; set; } = 50;
    public int EntertainmentPreference { get; set; } = 50;
    public int HardcorePreference { get; set; } = 50;
    public int OldSchoolPreference { get; set; } = 50;

    // Booking Style
    public int StarsFocus { get; set; } = 50;
    public int StabilityFocus { get; set; } = 50;
    public int RiskTolerance { get; set; } = 50;

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation
    public List<BookerMemory> Memories { get; set; } = new();
    public List<BookerEmploymentHistory> EmploymentHistory { get; set; } = new();
    public Personality? Personality { get; set; }

    // Calculated
    public bool IsEmployed => CurrentCompanyId != null;
    public string DominantStyle => GetDominantProductPreference();

    private string GetDominantProductPreference()
    {
        var max = Math.Max(LuchaPreference,
                  Math.Max(PuroresuPreference,
                  Math.Max(EntertainmentPreference,
                  Math.Max(HardcorePreference, OldSchoolPreference))));

        if (max == LuchaPreference) return "Lucha";
        if (max == PuroresuPreference) return "Puroresu";
        if (max == EntertainmentPreference) return "Entertainment";
        if (max == HardcorePreference) return "Hardcore";
        return "Old-School";
    }
}
```

**Créer** : `src/RingGeneral.Core/Models/BookerMemory.cs`

```csharp
public class BookerMemory
{
    public int Id { get; set; }
    public string BookerId { get; set; } = string.Empty;

    public string MemoryType { get; set; } = string.Empty; // Bias, Trauma, Success, Protege, Grudge

    public string? TargetEntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Intensity { get; set; } = 50; // 0-100

    public string? OriginEvent { get; set; }
    public string? OriginCompanyId { get; set; }
    public DateTime CreatedAt { get; set; }

    public int DecayRate { get; set; } = 5;
    public DateTime? LastDecayAt { get; set; }

    // Calculated
    public bool IsActive => Intensity > 20;
    public bool IsFaded => Intensity <= 20;
}
```

**Créer** : `src/RingGeneral.Core/Models/BookerEmploymentHistory.cs`

```csharp
public class BookerEmploymentHistory
{
    public int Id { get; set; }
    public string BookerId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TerminationReason { get; set; }

    public double AverageShowRating { get; set; }
    public int MajorSuccesses { get; set; }
    public int MajorFailures { get; set; }

    // Calculated
    public bool IsActive => !EndDate.HasValue;
    public int DurationInWeeks =>
        (int)((EndDate ?? DateTime.Now) - StartDate).TotalDays / 7;
}
```

**Tâches** :
- [ ] Créer 4 Models
- [ ] Tests unitaires
- [ ] Validation propriétés calculées

### 4.3 Services (Semaine 12-13)

**Service 1** : `src/RingGeneral.Core/Services/BookerAIEngine.cs`

```csharp
public interface IBookerAIEngine
{
    /// <summary>
    /// Auto-booke un show basé sur les préférences et mémoires du Booker
    /// </summary>
    Show AutoBookShow(string bookerId, string showId);

    /// <summary>
    /// Détermine si le Booker pousse un worker (basé sur mémoires + préférences)
    /// </summary>
    bool ShouldPushWorker(string bookerId, string workerId);

    /// <summary>
    /// Applique un trauma à la mémoire du Booker
    /// </summary>
    void AddTrauma(string bookerId, string traumaType, string targetEntityId, int intensity);

    /// <summary>
    /// Decay naturel des mémoires (chaque année)
    /// </summary>
    void DecayMemories(string bookerId);
}
```

**Implémentation** :

```csharp
public class BookerAIEngine : IBookerAIEngine
{
    private readonly IBookerRepository _bookerRepository;
    private readonly IPersonalityEngine _personalityEngine;

    public Show AutoBookShow(string bookerId, string showId)
    {
        var booker = _bookerRepository.GetByIdAsync(bookerId);
        var show = _showRepository.GetByIdAsync(showId);

        // Appliquer préférences de produit
        var segments = GenerateSegmentsBasedOnPreferences(booker, show);

        // Appliquer mémoires (protégés, grudges)
        ApplyMemoriesToBooking(booker, segments);

        // Finaliser le show
        show.Segments = segments;
        return show;
    }

    private List<Segment> GenerateSegmentsBasedOnPreferences(Booker booker, Show show)
    {
        var segments = new List<Segment>();

        // Si Entertainment dominant → plus de promos
        if (booker.EntertainmentPreference >= 70)
        {
            segments.Add(CreatePromoSegment());
            segments.Add(CreatePromoSegment());
        }

        // Si Puroresu dominant → longs singles
        if (booker.PuroresuPreference >= 70)
        {
            segments.Add(CreateSinglesMatch(duration: 20));
        }

        // Si Lucha dominant → trios/tags
        if (booker.LuchaPreference >= 70)
        {
            segments.Add(CreateTagTeamMatch());
            segments.Add(CreateTrioMatch());
        }

        // Si Hardcore dominant → stipulations
        if (booker.HardcorePreference >= 70)
        {
            segments.Add(CreateStipulationMatch("No DQ"));
        }

        return segments;
    }

    private void ApplyMemoriesToBooking(Booker booker, List<Segment> segments)
    {
        // Récupérer mémoires actives
        var activeMemories = booker.Memories.Where(m => m.IsActive).ToList();

        foreach (var memory in activeMemories)
        {
            if (memory.MemoryType == "Protege" && memory.TargetEntityId != null)
            {
                // Pousser le protégé
                var segment = segments.FirstOrDefault(s =>
                    s.Participants.Any(p => p.WorkerId == memory.TargetEntityId));

                if (segment != null)
                {
                    // Augmenter importance du segment
                    segment.ImportanceLevel = "High";
                }
            }

            if (memory.MemoryType == "Grudge" && memory.TargetEntityId != null)
            {
                // Enterrer le worker
                var segment = segments.FirstOrDefault(s =>
                    s.Participants.Any(p => p.WorkerId == memory.TargetEntityId));

                if (segment != null)
                {
                    // Diminuer importance
                    segment.ImportanceLevel = "Low";
                    // Faire perdre
                    var participant = segment.Participants.First(p =>
                        p.WorkerId == memory.TargetEntityId);
                    participant.Result = "Loss";
                }
            }
        }
    }

    public bool ShouldPushWorker(string bookerId, string workerId)
    {
        var booker = _bookerRepository.GetByIdAsync(bookerId);

        // Vérifier mémoires
        var protegeMemory = booker.Memories.FirstOrDefault(m =>
            m.MemoryType == "Protege" && m.TargetEntityId == workerId);

        if (protegeMemory != null && protegeMemory.Intensity >= 60)
            return true; // Push biaisé

        var grudgeMemory = booker.Memories.FirstOrDefault(m =>
            m.MemoryType == "Grudge" && m.TargetEntityId == workerId);

        if (grudgeMemory != null && grudgeMemory.Intensity >= 60)
            return false; // Refus de push biaisé

        // Sinon, décision basée sur mérite
        var worker = _workerRepository.GetByIdAsync(workerId);
        return worker.Popularity >= 70 || worker.InRing >= 75;
    }

    public void AddTrauma(string bookerId, string traumaType, string targetEntityId, int intensity)
    {
        var memory = new BookerMemory
        {
            BookerId = bookerId,
            MemoryType = "Trauma",
            TargetEntityId = targetEntityId,
            Description = $"Trauma: {traumaType}",
            Intensity = intensity,
            OriginEvent = traumaType,
            CreatedAt = DateTime.Now
        };

        _bookerRepository.AddMemoryAsync(memory);
    }

    public void DecayMemories(string bookerId)
    {
        var booker = _bookerRepository.GetByIdAsync(bookerId);

        foreach (var memory in booker.Memories)
        {
            memory.Intensity = Math.Max(0, memory.Intensity - memory.DecayRate);
            memory.LastDecayAt = DateTime.Now;
        }

        _bookerRepository.UpdateAsync(booker);
    }
}
```

**Service 2** : `src/RingGeneral.Core/Services/OwnerDecisionEngine.cs`

```csharp
public interface IOwnerDecisionEngine
{
    /// <summary>
    /// Détermine si l'Owner intervient dans une crise
    /// </summary>
    bool ShouldInterveneinCrisis(string ownerId, string crisisType, int severity);

    /// <summary>
    /// Décide de virer le Booker
    /// </summary>
    bool ShouldFireBooker(string ownerId, string bookerId);

    /// <summary>
    /// Valide ou bloque un changement d'era
    /// </summary>
    bool ApproveEraChange(string ownerId, string newEra);
}
```

**Tâches** :
- [ ] Créer BookerAIEngine
- [ ] Créer OwnerDecisionEngine
- [ ] Tests unitaires scénarios
- [ ] Tests d'intégration

### 4.4 UI - Owner/Booker Management (Semaine 14)

**Vue** : `src/RingGeneral.UI/Views/Management/OwnerBookerView.axaml`

```xml
<ScrollViewer>
  <StackPanel Spacing="20" Margin="16">
    <!-- Owner Card -->
    <Border Classes="card">
      <StackPanel Spacing="12">
        <TextBlock Classes="h2" Text="Owner"/>

        <TextBlock Classes="h3" Text="{Binding Owner.FullName}"/>

        <!-- Priorities -->
        <StackPanel Spacing="8">
          <TextBlock Classes="caption muted" Text="Priorités Stratégiques:"/>

          <StackPanel Spacing="4">
            <StackPanel Orientation="Horizontal" Spacing="8">
              <TextBlock Classes="body" Width="150" Text="Financier:"/>
              <ProgressBar Value="{Binding Owner.FinancialPriority}" Maximum="100" Width="200"/>
              <TextBlock Classes="body" Text="{Binding Owner.FinancialPriority}"/>
            </StackPanel>

            <StackPanel Orientation="Horizontal" Spacing="8">
              <TextBlock Classes="body" Width="150" Text="Créatif:"/>
              <ProgressBar Value="{Binding Owner.CreativePriority}" Maximum="100" Width="200"/>
              <TextBlock Classes="body" Text="{Binding Owner.CreativePriority}"/>
            </StackPanel>

            <!-- ... autres priorités ... -->
          </StackPanel>
        </StackPanel>

        <!-- Decision Style -->
        <TextBlock Classes="body">
          <Run Text="Style de décision: "/>
          <Run FontWeight="SemiBold" Text="{Binding Owner.DecisionStyle}"/>
        </TextBlock>
      </StackPanel>
    </Border>

    <!-- Booker Card -->
    <Border Classes="card">
      <StackPanel Spacing="12">
        <Grid ColumnDefinitions="*,Auto">
          <TextBlock Classes="h2" Text="Booker"/>
          <Button Grid.Column="1" Classes="secondary" Content="Virer"
                  Command="{Binding FireBookerCommand}"/>
        </Grid>

        <TextBlock Classes="h3" Text="{Binding Booker.FullName}"/>

        <!-- Product Preferences -->
        <StackPanel Spacing="8">
          <TextBlock Classes="caption muted" Text="Préférences de Produit:"/>

          <WrapPanel>
            <Border Classes="badge" Background="#3b82f6" Margin="4" Padding="8,4">
              <TextBlock Classes="body" Foreground="White">
                <Run Text="Lucha: "/>
                <Run FontWeight="SemiBold" Text="{Binding Booker.LuchaPreference}"/>
              </TextBlock>
            </Border>

            <Border Classes="badge" Background="#10b981" Margin="4" Padding="8,4">
              <TextBlock Classes="body" Foreground="White">
                <Run Text="Puroresu: "/>
                <Run FontWeight="SemiBold" Text="{Binding Booker.PuroresuPreference}"/>
              </TextBlock>
            </Border>

            <!-- ... autres préférences ... -->
          </WrapPanel>
        </StackPanel>

        <!-- Dominant Style -->
        <TextBlock Classes="body">
          <Run Text="Style dominant: "/>
          <Run FontWeight="SemiBold" Text="{Binding Booker.DominantStyle}"/>
        </TextBlock>

        <!-- Toggle: Let Booker Decide -->
        <Border Classes="panel" Background="#fef3c7" Padding="12">
          <StackPanel Spacing="8">
            <ToggleSwitch IsChecked="{Binding LetBookerDecide}">
              <TextBlock Classes="body" FontWeight="SemiBold" Text="Let the Booker Decide"/>
            </ToggleSwitch>
            <TextBlock Classes="caption muted" TextWrapping="Wrap">
              Active le booking automatique basé sur les préférences et mémoires du Booker.
            </TextBlock>
          </StackPanel>
        </Border>

        <!-- Memories -->
        <Expander Header="Mémoires et Biais">
          <ItemsControl ItemsSource="{Binding Booker.ActiveMemories}">
            <ItemsControl.ItemTemplate>
              <DataTemplate>
                <Border Classes="card-small" Margin="0,4">
                  <StackPanel Spacing="4">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                      <TextBlock Classes="caption" FontWeight="SemiBold"
                                 Text="{Binding MemoryType}"/>
                      <ProgressBar Value="{Binding Intensity}" Maximum="100" Width="100"/>
                    </StackPanel>
                    <TextBlock Classes="caption muted" TextWrapping="Wrap"
                               Text="{Binding Description}"/>
                  </StackPanel>
                </Border>
              </DataTemplate>
            </ItemsControl.ItemTemplate>
          </ItemsControl>
        </Expander>
      </StackPanel>
    </Border>
  </StackPanel>
</ScrollViewer>
```

**Tâches** :
- [ ] Créer OwnerBookerViewModel
- [ ] Créer OwnerBookerView.axaml
- [ ] Command FireBooker
- [ ] Toggle LetBookerDecide
- [ ] Tests UI

### 4.5 Integration & Tests (Semaine 15)

**Tests d'intégration** :
- [ ] Créer Owner + Booker
- [ ] Booker auto-booke un show (preferences appliquées)
- [ ] Booker pousse son protégé (mémoire)
- [ ] Owner vire Booker suite à mauvaises performances
- [ ] Booker change de compagnie → mémoires conservées

**Livrables Phase 4** :
- ✅ 4 tables (Owners, Bookers, BookerMemory, BookerEmploymentHistory)
- ✅ 4 Models créés
- ✅ BookerAIEngine + OwnerDecisionEngine fonctionnels
- ✅ UI Owner/Booker Management
- ✅ Toggle "Let the Booker Decide"
- ✅ Tests passants

---

## 🚨 PHASE 5 : CRISIS & COMMUNICATION

**Durée** : 2-3 semaines
**Priorité** : 🟡 MOYENNE
**Dépendances** : Phase 3 (Morale & Rumors), Phase 4 (Owner)

### Objectifs

Implémenter le **système de gestion de crises** et **communication joueur** :
- Pipeline 5 étapes (Signaux → Rumeurs → Crise → Décision → Conséquences)
- 4 types de communication joueur (Individuelle, Réunion, Publique, Médiation)

### 5.1 Crisis Pipeline (Semaine 16)

**Models** : `src/RingGeneral.Core/Models/Crisis.cs`

```csharp
public class Crisis
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;

    public string CrisisType { get; set; } = string.Empty; // PR, Financial, Sporting, Internal
    public string Description { get; set; } = string.Empty;

    public string Stage { get; set; } = "WeakSignals"; // WeakSignals, Rumors, Declared, InResolution, Resolved
    public int Severity { get; set; } = 1; // 1-5

    public DateTime DetectedAt { get; set; }
    public DateTime? DeclaredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public string? PlayerResponse { get; set; }

    // Calculated
    public bool IsActive => Stage != "Resolved";
    public int DaysActive => (DateTime.Now - DetectedAt).Days;
}
```

**Service** : `src/RingGeneral.Core/Services/CrisisEngine.cs`

```csharp
public interface ICrisisEngine
{
    /// <summary>
    /// Détecte une crise potentielle
    /// </summary>
    Crisis? DetectCrisis(string companyId);

    /// <summary>
    /// Fait progresser la crise à l'étape suivante
    /// </summary>
    void ProgressCrisis(int crisisId);

    /// <summary>
    /// Résout la crise (suite à action du joueur)
    /// </summary>
    void ResolveCrisis(int crisisId, string resolutionMethod);
}
```

**Tâches** :
- [ ] Créer Crisis model
- [ ] Créer CrisisEngine
- [ ] Tests pipeline

### 5.2 Player Communication (Semaine 17)

**Service** : `src/RingGeneral.Core/Services/CommunicationEngine.cs`

```csharp
public interface ICommunicationEngine
{
    /// <summary>
    /// Discussion individuelle avec un worker
    /// </summary>
    CommunicationResult OneOnOneDiscussion(string workerId, string topic);

    /// <summary>
    /// Réunion de vestiaire (tous les workers)
    /// </summary>
    CommunicationResult LockerRoomMeeting(string companyId, string message);

    /// <summary>
    /// Communication publique (média)
    /// </summary>
    CommunicationResult PublicStatement(string companyId, string statement);

    /// <summary>
    /// Médiation indirecte (via staff ou leaders)
    /// </summary>
    CommunicationResult IndirectMediation(string mediatorId, string targetWorkerId, string message);
}

public class CommunicationResult
{
    public bool Success { get; set; }
    public int MoraleImpact { get; set; }
    public List<string> Consequences { get; set; } = new();
}
```

**Tâches** :
- [ ] Créer CommunicationEngine
- [ ] Implémenter 4 types de communication
- [ ] Tests scénarios

### 5.3 UI - Crisis Management (Semaine 18)

**Vue dans Dashboard** :

```xml
<!-- Crisis Alert Card -->
<Border Classes="card crisis-card" Background="#fee2e2"
        IsVisible="{Binding HasActiveCrisis}">
  <StackPanel Spacing="12">
    <StackPanel Orientation="Horizontal" Spacing="8">
      <TextBlock Text="🚨" FontSize="24"/>
      <TextBlock Classes="h3" Text="Crise Active"/>
    </StackPanel>

    <TextBlock Classes="body" FontWeight="SemiBold" Text="{Binding Crisis.Description}"/>

    <StackPanel Spacing="4">
      <TextBlock Classes="caption muted">
        <Run Text="Type: "/>
        <Run FontWeight="Medium" Text="{Binding Crisis.CrisisType}"/>
      </TextBlock>
      <TextBlock Classes="caption muted">
        <Run Text="Sévérité: "/>
        <Run FontWeight="Medium" Text="{Binding Crisis.Severity}"/>
        <Run Text="/5"/>
      </TextBlock>
      <TextBlock Classes="caption muted">
        <Run Text="Jours actifs: "/>
        <Run FontWeight="Medium" Text="{Binding Crisis.DaysActive}"/>
      </TextBlock>
    </StackPanel>

    <!-- Actions -->
    <WrapPanel Spacing="8">
      <Button Classes="primary" Content="Réunion générale"
              Command="{Binding LockerRoomMeetingCommand}"/>
      <Button Classes="secondary" Content="Communication publique"
              Command="{Binding PublicStatementCommand}"/>
      <Button Classes="secondary" Content="Médiation"
              Command="{Binding MediationCommand}"/>
    </WrapPanel>
  </StackPanel>
</Border>
```

**Tâches** :
- [ ] Créer Crisis UI dans Dashboard
- [ ] Dialogs pour communication
- [ ] Tests UI

**Livrables Phase 5** :
- ✅ Crisis model + CrisisEngine
- ✅ CommunicationEngine (4 types)
- ✅ UI Crisis Management
- ✅ Tests passants

---

## 🌍 PHASE 6 : AI WORLD & COMPANY ERAS

**Durée** : 2-5 semaines
**Priorité** : 🟢 BASSE (Polish)
**Dépendances** : Toutes les phases précédentes

### Objectifs

Implémenter la **simulation du monde IA** :
- Compagnies IA avec Owners et Bookers évolutifs
- Company Eras (créatives, économiques, médiatiques)
- LOD (Level of Detail) pour performances
- Histoire émergente

### 6.1 Company Eras (Semaine 19-20)

**Model** : `src/RingGeneral.Core/Models/CompanyEra.cs`

```csharp
public class CompanyEra
{
    public int Id { get; set; }
    public string CompanyId { get; set; } = string.Empty;

    public string EraName { get; set; } = string.Empty; // "Attitude Era", "Golden Age", etc.

    // Era Characteristics
    public string CreativeDirection { get; set; } = "Balanced"; // Edgy, Family-Friendly, Sports-Based
    public string EconomicState { get; set; } = "Stable"; // Boom, Stable, Recession
    public string MediaPresence { get; set; } = "Regional"; // Local, Regional, National, Global

    // Show Structure
    public int TypicalShowDuration { get; set; } = 120; // minutes
    public int TypicalMatchCount { get; set; } = 7;

    // Dominant Match Types
    public List<string> DominantMatchTypes { get; set; } = new();

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Calculated
    public bool IsActive => !EndDate.HasValue;
    public int DurationInYears =>
        (int)((EndDate ?? DateTime.Now) - StartDate).TotalDays / 365;
}
```

**Service** : `src/RingGeneral.Core/Services/EraEvolutionEngine.cs`

```csharp
public interface IEraEvolutionEngine
{
    /// <summary>
    /// Détecte si une transition d'era est nécessaire
    /// </summary>
    bool ShouldTransitionEra(string companyId);

    /// <summary>
    /// Génère une nouvelle era
    /// </summary>
    CompanyEra GenerateNewEra(string companyId);

    /// <summary>
    /// Applique graduellement la transition (lente)
    /// </summary>
    void ApplyEraTransition(string companyId, CompanyEra newEra);
}
```

**Tâches** :
- [ ] Créer CompanyEra model
- [ ] Créer EraEvolutionEngine
- [ ] Tests transitions

### 6.2 AI Companies (Semaine 21-22)

**Service** : `src/RingGeneral.Core/Services/AIWorldSimulationEngine.cs`

```csharp
public interface IAIWorldSimulationEngine
{
    /// <summary>
    /// Simule toutes les compagnies IA (LOD-based)
    /// </summary>
    void SimulateAICompanies(int weekNumber);

    /// <summary>
    /// Génère des événements mondiaux (transfers, closures, new companies)
    /// </summary>
    List<WorldEvent> GenerateWorldEvents();
}
```

**LOD Implementation** :

```csharp
public enum SimulationLOD
{
    Full,       // Compagnie du joueur
    High,       // Compagnies majeures (top 3)
    Medium,     // Compagnies régionales
    Low         // Compagnies locales
}

public void SimulateCompanyBasedOnLOD(Company company, SimulationLOD lod)
{
    switch (lod)
    {
        case SimulationLOD.Full:
            // Simulation complète (tous les workers, shows, etc.)
            SimulateFullCompany(company);
            break;

        case SimulationLOD.High:
            // Simulation principale (shows majeurs uniquement)
            SimulateMainShows(company);
            break;

        case SimulationLOD.Medium:
            // Simulation résumée (1 show par mois)
            SimulateMonthlySummary(company);
            break;

        case SimulationLOD.Low:
            // Simulation minimale (changements majeurs uniquement)
            SimulateMajorChangesOnly(company);
            break;
    }
}
```

**Tâches** :
- [ ] Créer AIWorldSimulationEngine
- [ ] Implémenter LOD
- [ ] Tests performances

### 6.3 Emergent History (Semaine 23)

**Model** : `src/RingGeneral.Core/Models/WorldEvent.cs`

```csharp
public class WorldEvent
{
    public int Id { get; set; }

    public string EventType { get; set; } = string.Empty; // Transfer, NewCompany, Closure, Merger
    public string Description { get; set; } = string.Empty;

    public string? InvolvedCompanyId { get; set; }
    public string? InvolvedWorkerId { get; set; }

    public DateTime OccurredAt { get; set; }

    public int Significance { get; set; } = 1; // 1-5
}
```

**Tâches** :
- [ ] Créer WorldEvent model
- [ ] Générer événements mondiaux
- [ ] Afficher dans News Feed

**Livrables Phase 6** :
- ✅ CompanyEra system
- ✅ AI World Simulation (LOD)
- ✅ Emergent History
- ✅ Tests performances

---

## 📊 RÉCAPITULATIF FINAL

### Livrables Totaux

| Phase | Livrables Clés | Fichiers | Tests |
|-------|----------------|----------|-------|
| **Phase 1** | Personality & Mental System | 10+ | ✅ |
| **Phase 2** | Relations & Nepotism | 6+ | ✅ |
| **Phase 3** | Morale & Rumors | 12+ | ✅ |
| **Phase 4** | Owner & Booker Systems | 15+ | ✅ |
| **Phase 5** | Crisis & Communication | 8+ | ✅ |
| **Phase 6** | AI World & Eras | 10+ | ✅ |

**Total estimé** : **~60 fichiers** (Models, Services, Repositories, Views)

### Prochaines Étapes

Après validation de ce plan :
1. **Démarrage Phase 1** : Personality & Mental System (Semaine 1)
2. **Review hebdomadaires** avec Chef de Projet
3. **Tests continus** à chaque phase
4. **Documentation** au fur et à mesure

---

## 📞 CONTACT CHEF DE PROJET

**En cas de questions ou blocages** :
- Phase bloquante : Remonter immédiatement
- Besoin de clarification : Demander au Chef de Projet
- Découverte de risque : Alerter et proposer mitigation

---

**Version** : 1.0
**Auteur** : Chef de Projet DevOps (Claude)
**Date de création** : 8 janvier 2026
**Statut** : ⏸️ EN ATTENTE DE VALIDATION

---

**Prochaine Étape** : Validation par le client avant démarrage Phase 1 🚀
