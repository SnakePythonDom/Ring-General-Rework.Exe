# 🎭 PLAN SYSTÈME PERSONNALITÉS - Ring General

**Version**: 1.0
**Date**: 2026-01-08
**Statut**: 📋 Planning Phase
**Priorité**: ⭐⭐⭐ High (Phase 8)

---

## 📋 Table des Matières

1. [Vue d'Ensemble](#vue-densemble)
2. [Objectifs](#objectifs)
3. [Architecture Technique](#architecture-technique)
4. [Schéma Base de Données](#schéma-base-de-données)
5. [Modèles C#](#modèles-c)
6. [Algorithmes de Détection](#algorithmes-de-détection)
7. [Système de Rapports Agent](#système-de-rapports-agent)
8. [Intégration UI](#intégration-ui)
9. [Migration et Import](#migration-et-import)
10. [Tests et Validation](#tests-et-validation)

---

## 🎯 Vue d'Ensemble

### Contexte

Le système actuel de Ring General possède **30 attributs de performance** (10 In-Ring, 10 Entertainment, 10 Story) sur une échelle 0-100. Ces attributs sont **visibles** et représentent les **compétences techniques**.

Ce nouveau système ajoute une **dimension psychologique cachée** avec:
- **10 Attributs Mentaux** (0-20) - Cachés, révélés par scouting
- **25+ Profils de Personnalité** - Étiquettes visibles assignées automatiquement
- **Rapports d'Agent** - Texte généré dynamiquement analysant le profil

### Inspiration

**Football Manager** - Système de personnalité avec attributs mentaux cachés et rapports d'éclaireurs détaillés.

### Différence Clé: Caché vs Visible

| Élément | Visibilité | Échelle | Utilisation |
|---------|-----------|---------|-------------|
| **Attributs Mentaux** | 🔒 Cachés (révélés par scouting) | 0-20 | IA, simulation comportement |
| **Profil Personnalité** | 👁️ Visible (label) | Enum (25+ valeurs) | UI, description, storytelling |
| **Rapport Agent** | 👁️ Visible (texte généré) | N/A | Analyse narrative pour le joueur |

---

## 🎯 Objectifs

### Objectif Principal

Créer un système de personnalité psychologique qui:
1. ✅ Ajoute de la **profondeur psychologique** aux workers
2. ✅ Influence le **comportement IA** (négociations, conflits, momentum)
3. ✅ Génère des **rapports narratifs** immersifs
4. ✅ S'intègre naturellement au système existant (30 attributs)
5. ✅ Reste **performant** (pas de surcharge DB)

### Objectifs Secondaires

- Permet au joueur de **découvrir progressivement** la personnalité via scouting
- Crée des **archétypes reconnaissables** (Le Pro, L'Égoïste, Le Vétéran Rusé)
- Génère **automatiquement** les profils à l'import
- Fournit une **UI inspirée FM** dans ProfileView

---

## 🏗️ Architecture Technique

### Stack Technique

- **Base de Données**: SQLite (nouvelle table `WorkerMentalAttributes`)
- **Models**: `WorkerMentalAttributes.cs`, `PersonalityProfile` enum, `AgentReport.cs`
- **Services**: `PersonalityDetectorService.cs`, `AgentReportGeneratorService.cs`
- **Repository**: Extension de `IWorkerAttributesRepository` ou nouveau `IMentalAttributesRepository`
- **ViewModel**: `PersonalityTabViewModel.cs` ou intégration dans `AttributesTabViewModel`
- **View**: Nouvelle section dans ProfileView (Tab "Personnalité" ou sidebar)

### Diagramme d'Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      ProfileView.axaml                       │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Tab "Personnalité" ou Sidebar                        │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │  🎭 Profil: "Professionnel Exemplaire"         │  │  │
│  │  │  📊 Rapport Agent (4 Piliers):                 │  │  │
│  │  │     Professionnalisme: ████████░░ 17/20        │  │  │
│  │  │     Pression: ███████░░░ 14/20                 │  │  │
│  │  │     Égoïsme: ██░░░░░░░░ 4/20                  │  │  │
│  │  │     Influence: ██████░░░░ 12/20                │  │  │
│  │  │                                                 │  │  │
│  │  │  📝 "Worker modèle, fiable sous pression..."   │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ▲
                           │ Binding
                           │
┌──────────────────────────┴──────────────────────────────────┐
│              ProfileViewModel.PersonalityTab                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Properties:                                           │ │
│  │  - PersonalityProfile Profile                         │ │
│  │  - AgentReport Report                                 │ │
│  │  - WorkerMentalAttributes MentalAttributes (if scout) │ │
│  │  - bool IsScoutingCompleted                           │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                           ▲
                           │ Uses
                           │
┌──────────────────────────┴──────────────────────────────────┐
│          Services: PersonalityDetectorService                │
│                    AgentReportGeneratorService               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  DetectProfile(MentalAttributes) → PersonalityProfile  │ │
│  │  GenerateReport(MentalAttributes) → AgentReport        │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                           ▲
                           │ Data from
                           │
┌──────────────────────────┴──────────────────────────────────┐
│         Repository: WorkerAttributesRepository               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  GetMentalAttributes(workerId) → MentalAttributes      │ │
│  │  UpdateMentalAttributes(...)                           │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                           ▲
                           │ SQL
                           │
┌──────────────────────────┴──────────────────────────────────┐
│              SQLite: WorkerMentalAttributes                  │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Id, WorkerId, Ambition, Loyauté, Professionnalisme,  │ │
│  │  Pression, Tempérament, Égoïsme, Détermination,       │ │
│  │  Adaptabilité, Influence, Sportivité                   │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 🗄️ Schéma Base de Données

### Nouvelle Table: WorkerMentalAttributes

```sql
CREATE TABLE IF NOT EXISTS WorkerMentalAttributes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL UNIQUE,

    -- 10 Attributs Mentaux (0-20)
    Ambition INTEGER NOT NULL DEFAULT 10 CHECK(Ambition BETWEEN 0 AND 20),
    Loyauté INTEGER NOT NULL DEFAULT 10 CHECK(Loyauté BETWEEN 0 AND 20),
    Professionnalisme INTEGER NOT NULL DEFAULT 10 CHECK(Professionnalisme BETWEEN 0 AND 20),
    Pression INTEGER NOT NULL DEFAULT 10 CHECK(Pression BETWEEN 0 AND 20),
    Tempérament INTEGER NOT NULL DEFAULT 10 CHECK(Tempérament BETWEEN 0 AND 20),
    Égoïsme INTEGER NOT NULL DEFAULT 10 CHECK(Égoïsme BETWEEN 0 AND 20),
    Détermination INTEGER NOT NULL DEFAULT 10 CHECK(Détermination BETWEEN 0 AND 20),
    Adaptabilité INTEGER NOT NULL DEFAULT 10 CHECK(Adaptabilité BETWEEN 0 AND 20),
    Influence INTEGER NOT NULL DEFAULT 10 CHECK(Influence BETWEEN 0 AND 20),
    Sportivité INTEGER NOT NULL DEFAULT 10 CHECK(Sportivité BETWEEN 0 AND 20),

    -- Metadata
    IsRevealed BOOLEAN NOT NULL DEFAULT 0, -- Scouting completed?
    ScoutingLevel INTEGER NOT NULL DEFAULT 0, -- 0=None, 1=Basic, 2=Full
    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Foreign Key
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_mental_worker ON WorkerMentalAttributes(WorkerId);
```

### Extension Table Workers

```sql
-- Ajout de colonnes à la table Workers existante
ALTER TABLE Workers ADD COLUMN PersonalityProfile TEXT DEFAULT NULL;
ALTER TABLE Workers ADD COLUMN PersonalityProfileDetectedAt TEXT DEFAULT NULL;
```

### Valeurs PersonalityProfile

Stocké comme **TEXT** (nom du profil), pas un INTEGER, pour flexibilité:
- `"Professionnel Exemplaire"`
- `"Citoyen Modèle"`
- `"Ambitieux"`
- `"Tempérament de Feu"`
- etc. (25+ valeurs)

---

## 📦 Modèles C#

### 1. WorkerMentalAttributes.cs

```csharp
using System;

namespace RingGeneral.Core.Models;

/// <summary>
/// Attributs mentaux et psychologiques d'un worker (0-20 échelle)
/// Ces attributs sont CACHÉS par défaut et révélés par scouting
/// </summary>
public sealed class WorkerMentalAttributes
{
    public int Id { get; set; }
    public int WorkerId { get; set; }

    // ========== 10 ATTRIBUTS MENTAUX (0-20) ==========

    /// <summary>
    /// Ambition - Désir de succès et de reconnaissance
    /// 0-5: Satisfait de sa position
    /// 6-12: Ambition modérée
    /// 13-16: Ambitieux
    /// 17-20: Ultra-compétiteur, veut être main event
    /// </summary>
    public int Ambition { get; set; } = 10;

    /// <summary>
    /// Loyauté - Fidélité envers la compagnie et les collègues
    /// 0-5: Mercenaire, changera pour plus d'argent
    /// 6-12: Loyauté conditionnelle
    /// 13-16: Loyal
    /// 17-20: Loyauté absolue, pilier du vestiaire
    /// </summary>
    public int Loyauté { get; set; } = 10;

    /// <summary>
    /// Professionnalisme - Éthique de travail et respect du métier
    /// 0-5: Paresseux, problématique
    /// 6-12: Professionnalisme basique
    /// 13-16: Très professionnel
    /// 17-20: Modèle absolu, travailleur acharné
    /// </summary>
    public int Professionnalisme { get; set; } = 10;

    /// <summary>
    /// Pression - Capacité à performer sous pression
    /// 0-5: Craque dans les grands moments
    /// 6-12: Instable sous pression
    /// 13-16: Fiable
    /// 17-20: Clutch player, brille dans les big matches
    /// </summary>
    public int Pression { get; set; } = 10;

    /// <summary>
    /// Tempérament - Contrôle émotionnel et calme
    /// 0-5: Explosif, bagarres backstage
    /// 6-12: Tempérament moyen
    /// 13-16: Calme et posé
    /// 17-20: Zen absolu, jamais de conflit
    /// </summary>
    public int Tempérament { get; set; } = 10;

    /// <summary>
    /// Égoïsme - Priorité à soi-même vs l'équipe
    /// 0-5: Altruiste, met toujours l'équipe avant
    /// 6-12: Équilibré
    /// 13-16: Égocentrique
    /// 17-20: Diva, refuse de perdre, politique backstage
    /// </summary>
    public int Égoïsme { get; set; } = 10;

    /// <summary>
    /// Détermination - Résilience face à l'adversité
    /// 0-5: Abandonne facilement
    /// 6-12: Détermination moyenne
    /// 13-16: Très déterminé
    /// 17-20: Machine de guerre, jamais découragé
    /// </summary>
    public int Détermination { get; set; } = 10;

    /// <summary>
    /// Adaptabilité - Capacité à changer de rôle/style
    /// 0-5: Rigide, un seul style
    /// 6-12: Adaptabilité limitée
    /// 13-16: Polyvalent
    /// 17-20: Caméléon, peut jouer n'importe quel rôle
    /// </summary>
    public int Adaptabilité { get; set; } = 10;

    /// <summary>
    /// Influence - Pouvoir dans le vestiaire et avec la direction
    /// 0-5: Aucune influence
    /// 6-12: Influence modérée
    /// 13-16: Leader respecté
    /// 17-20: Booker de l'ombre, creative control
    /// </summary>
    public int Influence { get; set; } = 10;

    /// <summary>
    /// Sportivité - Fair-play et respect des adversaires
    /// 0-5: Tricheur, saboteur
    /// 6-12: Sportivité basique
    /// 13-16: Fair-play
    /// 17-20: Respect absolu, élève les autres
    /// </summary>
    public int Sportivité { get; set; } = 10;

    // ========== METADATA ==========

    /// <summary>
    /// Les attributs ont-ils été révélés par scouting?
    /// </summary>
    public bool IsRevealed { get; set; } = false;

    /// <summary>
    /// Niveau de scouting: 0=None, 1=Basic (4 piliers), 2=Full (10 attributs)
    /// </summary>
    public int ScoutingLevel { get; set; } = 0;

    /// <summary>
    /// Dernière mise à jour (changement personnalité après événements)
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // ========== NAVIGATION PROPERTIES ==========

    public Worker? Worker { get; set; }

    // ========== COMPUTED PROPERTIES ==========

    /// <summary>
    /// Moyenne des 10 attributs mentaux (0-20)
    /// </summary>
    public double MentalAverage =>
        (Ambition + Loyauté + Professionnalisme + Pression + Tempérament +
         Égoïsme + Détermination + Adaptabilité + Influence + Sportivité) / 10.0;

    /// <summary>
    /// Score du pilier Professionnalisme (moyenne de 3 attributs)
    /// </summary>
    public double ProfessionnalismeScore =>
        (Professionnalisme + Sportivité + Loyauté) / 3.0;

    /// <summary>
    /// Score du pilier Pression (moyenne de 2 attributs)
    /// </summary>
    public double PressionScore =>
        (Pression + Détermination) / 2.0;

    /// <summary>
    /// Score du pilier Égoïsme (1 attribut)
    /// </summary>
    public double ÉgoïsmeScore => Égoïsme;

    /// <summary>
    /// Score du pilier Influence (moyenne de 2 attributs)
    /// </summary>
    public double InfluenceScore =>
        (Influence + Tempérament) / 2.0;
}
```

### 2. PersonalityProfile.cs (Enum)

```csharp
namespace RingGeneral.Core.Models;

/// <summary>
/// Profils de personnalité détectés automatiquement
/// Basés sur les 10 attributs mentaux
/// </summary>
public enum PersonalityProfile
{
    // ===== LES ÉLITES (High Pro, High Pressure) =====

    /// <summary>
    /// ⭐ Professionnel Exemplaire
    /// Professionnalisme 17+, Sportivité 15+, Tempérament 15+
    /// Le worker parfait
    /// </summary>
    ProfessionnelExemplaire,

    /// <summary>
    /// 🏆 Citoyen Modèle
    /// Loyauté 17+, Professionnalisme 15+, Égoïsme <6
    /// Pilier du vestiaire
    /// </summary>
    CitoyenModele,

    /// <summary>
    /// 💪 Déterminé
    /// Détermination 17+, Pression 15+
    /// Never gives up
    /// </summary>
    Déterminé,

    // ===== LES STARS À ÉGO (High Ambition + High Égoïsme) =====

    /// <summary>
    /// 🚀 Ambitieux
    /// Ambition 17+, Détermination 13+, Égoïsme 10+
    /// Veut être main event
    /// </summary>
    Ambitieux,

    /// <summary>
    /// 👑 Leader de Vestiaire
    /// Influence 17+, Professionnalisme 13+, Tempérament 13+
    /// Locker room general
    /// </summary>
    LeaderDeVestiaire,

    /// <summary>
    /// 💰 Mercenaire
    /// Loyauté <6, Ambition 13+, Égoïsme 13+
    /// Suivra l'argent
    /// </summary>
    Mercenaire,

    // ===== LES INSTABLES (Low Tempérament ou Pressure) =====

    /// <summary>
    /// 🔥 Tempérament de Feu
    /// Tempérament <6, Professionnalisme >10
    /// Explosif mais talentueux
    /// </summary>
    TempéramentDeFeu,

    /// <summary>
    /// 🎲 Franc-Tireur
    /// Adaptabilité 15+, Tempérament <8, Sportivité <8
    /// Imprévisible
    /// </summary>
    FrancTireur,

    /// <summary>
    /// 📉 Inconstant
    /// Pression <8, Détermination <8
    /// Performances erratiques
    /// </summary>
    Inconstant,

    // ===== LES TOXIQUES (High Égoïsme, Low Pro) =====

    /// <summary>
    /// 😈 Égoïste
    /// Égoïsme 17+, Sportivité <6
    /// Refuse de mettre over
    /// </summary>
    Égoïste,

    /// <summary>
    /// 👸 Diva
    /// Égoïsme 17+, Tempérament <6, Professionnalisme <10
    /// Problèmes backstage constants
    /// </summary>
    Diva,

    /// <summary>
    /// 💤 Paresseux
    /// Professionnalisme <6, Détermination <6
    /// Minimum d'effort
    /// </summary>
    Paresseux,

    // ===== LES STRATÈGES (High Experience correlated) =====

    /// <summary>
    /// 🦊 Vétéran Rusé
    /// Adaptabilité 15+, Influence 13+, Sportivité <10
    /// Politique backstage
    /// </summary>
    VétéranRusé,

    /// <summary>
    /// 📖 Maître du Storytelling
    /// Adaptabilité 17+, Professionnalisme 13+, Pression 13+
    /// Travaille l'histoire
    /// </summary>
    MaîtreDuStorytelling,

    /// <summary>
    /// 🎭 Politicien
    /// Influence 17+, Égoïsme 13+, Tempérament 13+
    /// Joue les coulisses
    /// </summary>
    Politicien,

    // ===== LES BÊTES DE COMPÉTITION =====

    /// <summary>
    /// 🥊 Accro au Ring
    /// Détermination 17+, Professionnalisme 15+, Ambition 13+
    /// Vit pour wrestler
    /// </summary>
    AccroAuRing,

    /// <summary>
    /// 🛡️ Pilier Fiable
    /// Loyauté 17+, Pression 15+, Professionnalisme 13+
    /// Toujours là quand on a besoin
    /// </summary>
    PilierFiable,

    /// <summary>
    /// ⚙️ Machine de Guerre
    /// Détermination 18+, Pression 17+, Tempérament 15+
    /// Indestructible
    /// </summary>
    MachineDeGuerre,

    // ===== LES CRÉATURES MÉDIATIQUES =====

    /// <summary>
    /// 📸 Obsédé par l'Image
    /// Ambition 15+, Égoïsme 15+, Professionnalisme <10
    /// Veut être celebrity
    /// </summary>
    ObsédéParLImage,

    /// <summary>
    /// ⚡ Charismatique Imprévisible
    /// Adaptabilité 15+, Tempérament <8, Ambition 13+
    /// Wild card
    /// </summary>
    CharismatiqueImprévisible,

    /// <summary>
    /// 🌟 Aimant à Public
    /// Sportivité 17+, Professionnalisme 15+, Tempérament 13+
    /// Connecte avec la foule
    /// </summary>
    AimantÀPublic,

    // ===== LES PROFILS DANGEREUX =====

    /// <summary>
    /// 🐍 Saboteur Passif
    /// Sportivité <5, Égoïsme 15+, Influence 10+
    /// Tire dans le dos
    /// </summary>
    SaboteurPassif,

    /// <summary>
    /// 💥 Instable Chronique
    /// Tempérament <5, Pression <5, Professionnalisme <8
    /// Risque constant
    /// </summary>
    InstableChronique,

    /// <summary>
    /// ⚠️ Poids Mort
    /// Professionnalisme <5, Détermination <5, Ambition <5
    /// Aucun intérêt
    /// </summary>
    PoidsMort,

    // ===== PROFIL PAR DÉFAUT =====

    /// <summary>
    /// 📊 Équilibré
    /// Tous attributs entre 8-13
    /// Profil standard
    /// </summary>
    Équilibré,

    /// <summary>
    /// ❓ Non Déterminé
    /// Profil non encore analysé
    /// </summary>
    NonDéterminé
}
```

### 3. AgentReport.cs

```csharp
using System;

namespace RingGeneral.Core.Models;

/// <summary>
/// Rapport d'agent généré dynamiquement
/// Analyse narrative des 4 piliers de personnalité
/// </summary>
public sealed class AgentReport
{
    public int Id { get; set; }
    public int WorkerId { get; set; }

    // ===== 4 PILIERS (scores 0-20) =====

    /// <summary>
    /// Pilier 1: Professionnalisme
    /// Moyenne de: Professionnalisme, Sportivité, Loyauté
    /// </summary>
    public double ProfessionnalismeScore { get; set; }

    /// <summary>
    /// Pilier 2: Gestion de la Pression
    /// Moyenne de: Pression, Détermination
    /// </summary>
    public double PressionScore { get; set; }

    /// <summary>
    /// Pilier 3: Égoïsme
    /// Valeur directe: Égoïsme
    /// </summary>
    public double ÉgoïsmeScore { get; set; }

    /// <summary>
    /// Pilier 4: Influence
    /// Moyenne de: Influence, Tempérament (inversé)
    /// </summary>
    public double InfluenceScore { get; set; }

    // ===== TEXTE GÉNÉRÉ =====

    /// <summary>
    /// Texte complet du rapport (2-4 paragraphes)
    /// Exemple: "Worker modèle avec un professionnalisme exemplaire..."
    /// </summary>
    public string ReportText { get; set; } = string.Empty;

    /// <summary>
    /// Résumé court (1 phrase)
    /// Exemple: "Professionnel fiable sous pression, peu égoïste"
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Date de génération du rapport
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // ===== NAVIGATION =====

    public Worker? Worker { get; set; }
}
```

---

## 🧠 Algorithmes de Détection

### Algorithme 1: Détection du Profil

**Logique de priorité** (ordre d'évaluation):

```csharp
public class PersonalityDetectorService
{
    public PersonalityProfile DetectProfile(WorkerMentalAttributes mental)
    {
        // ===== NIVEAU 1: PROFILS DANGEREUX (priorité max) =====

        // Poids Mort (tout faible)
        if (mental.Professionnalisme <= 5 && mental.Détermination <= 5 && mental.Ambition <= 5)
            return PersonalityProfile.PoidsMort;

        // Instable Chronique
        if (mental.Tempérament <= 5 && mental.Pression <= 5 && mental.Professionnalisme <= 8)
            return PersonalityProfile.InstableChronique;

        // Saboteur Passif
        if (mental.Sportivité <= 5 && mental.Égoïsme >= 15 && mental.Influence >= 10)
            return PersonalityProfile.SaboteurPassif;

        // ===== NIVEAU 2: PROFILS ÉLITES =====

        // Professionnel Exemplaire
        if (mental.Professionnalisme >= 17 && mental.Sportivité >= 15 && mental.Tempérament >= 15)
            return PersonalityProfile.ProfessionnelExemplaire;

        // Citoyen Modèle
        if (mental.Loyauté >= 17 && mental.Professionnalisme >= 15 && mental.Égoïsme <= 6)
            return PersonalityProfile.CitoyenModele;

        // Machine de Guerre
        if (mental.Détermination >= 18 && mental.Pression >= 17 && mental.Tempérament >= 15)
            return PersonalityProfile.MachineDeGuerre;

        // ===== NIVEAU 3: PROFILS TOXIQUES =====

        // Diva
        if (mental.Égoïsme >= 17 && mental.Tempérament <= 6 && mental.Professionnalisme <= 10)
            return PersonalityProfile.Diva;

        // Égoïste
        if (mental.Égoïsme >= 17 && mental.Sportivité <= 6)
            return PersonalityProfile.Égoïste;

        // Paresseux
        if (mental.Professionnalisme <= 6 && mental.Détermination <= 6)
            return PersonalityProfile.Paresseux;

        // ===== NIVEAU 4: PROFILS AMBITIEUX =====

        // Leader de Vestiaire
        if (mental.Influence >= 17 && mental.Professionnalisme >= 13 && mental.Tempérament >= 13)
            return PersonalityProfile.LeaderDeVestiaire;

        // Ambitieux
        if (mental.Ambition >= 17 && mental.Détermination >= 13 && mental.Égoïsme >= 10)
            return PersonalityProfile.Ambitieux;

        // Mercenaire
        if (mental.Loyauté <= 6 && mental.Ambition >= 13 && mental.Égoïsme >= 13)
            return PersonalityProfile.Mercenaire;

        // ===== NIVEAU 5: PROFILS INSTABLES =====

        // Tempérament de Feu
        if (mental.Tempérament <= 6 && mental.Professionnalisme >= 10)
            return PersonalityProfile.TempéramentDeFeu;

        // Franc-Tireur
        if (mental.Adaptabilité >= 15 && mental.Tempérament <= 8 && mental.Sportivité <= 8)
            return PersonalityProfile.FrancTireur;

        // Inconstant
        if (mental.Pression <= 8 && mental.Détermination <= 8)
            return PersonalityProfile.Inconstant;

        // ===== NIVEAU 6: PROFILS STRATÈGES =====

        // Politicien
        if (mental.Influence >= 17 && mental.Égoïsme >= 13 && mental.Tempérament >= 13)
            return PersonalityProfile.Politicien;

        // Vétéran Rusé
        if (mental.Adaptabilité >= 15 && mental.Influence >= 13 && mental.Sportivité <= 10)
            return PersonalityProfile.VétéranRusé;

        // Maître du Storytelling
        if (mental.Adaptabilité >= 17 && mental.Professionnalisme >= 13 && mental.Pression >= 13)
            return PersonalityProfile.MaîtreDuStorytelling;

        // ===== NIVEAU 7: PROFILS COMPÉTITION =====

        // Accro au Ring
        if (mental.Détermination >= 17 && mental.Professionnalisme >= 15 && mental.Ambition >= 13)
            return PersonalityProfile.AccroAuRing;

        // Pilier Fiable
        if (mental.Loyauté >= 17 && mental.Pression >= 15 && mental.Professionnalisme >= 13)
            return PersonalityProfile.PilierFiable;

        // Déterminé
        if (mental.Détermination >= 17 && mental.Pression >= 15)
            return PersonalityProfile.Déterminé;

        // ===== NIVEAU 8: PROFILS MÉDIATIQUES =====

        // Aimant à Public
        if (mental.Sportivité >= 17 && mental.Professionnalisme >= 15 && mental.Tempérament >= 13)
            return PersonalityProfile.AimantÀPublic;

        // Charismatique Imprévisible
        if (mental.Adaptabilité >= 15 && mental.Tempérament <= 8 && mental.Ambition >= 13)
            return PersonalityProfile.CharismatiqueImprévisible;

        // Obsédé par l'Image
        if (mental.Ambition >= 15 && mental.Égoïsme >= 15 && mental.Professionnalisme <= 10)
            return PersonalityProfile.ObsédéParLImage;

        // ===== PROFIL PAR DÉFAUT =====

        // Équilibré (tous attributs 8-13)
        if (mental.Ambition >= 8 && mental.Ambition <= 13 &&
            mental.Loyauté >= 8 && mental.Loyauté <= 13 &&
            mental.Professionnalisme >= 8 && mental.Professionnalisme <= 13 &&
            mental.Pression >= 8 && mental.Pression <= 13 &&
            mental.Tempérament >= 8 && mental.Tempérament <= 13 &&
            mental.Égoïsme >= 8 && mental.Égoïsme <= 13 &&
            mental.Détermination >= 8 && mental.Détermination <= 13 &&
            mental.Adaptabilité >= 8 && mental.Adaptabilité <= 13 &&
            mental.Influence >= 8 && mental.Influence <= 13 &&
            mental.Sportivité >= 8 && mental.Sportivité <= 13)
            return PersonalityProfile.Équilibré;

        // Si aucun profil ne match (rare)
        return PersonalityProfile.NonDéterminé;
    }
}
```

### Algorithme 2: Génération Rapport Agent

**Dictionnaire de templates** pour chaque pilier:

```csharp
public class AgentReportGeneratorService
{
    private readonly Dictionary<string, Dictionary<string, string>> _templates = new()
    {
        ["Professionnalisme"] = new()
        {
            ["VeryLow"] = "Worker problématique avec un professionnalisme défaillant. Manque de respect pour le métier et les collègues.",
            ["Low"] = "Professionnalisme en dessous de la moyenne. Peut causer des problèmes dans le vestiaire.",
            ["Average"] = "Professionnalisme correct, sans plus. Fait le minimum requis.",
            ["Good"] = "Worker professionnel et respectueux. Bonne éthique de travail.",
            ["VeryGood"] = "Très professionnel, fiable et respecté dans le vestiaire.",
            ["Exceptional"] = "Professionnalisme exemplaire. Modèle absolu pour les jeunes talents."
        },

        ["Pression"] = new()
        {
            ["VeryLow"] = "Craque systématiquement sous pression. Éviter les big matches.",
            ["Low"] = "Performances instables dans les moments importants.",
            ["Average"] = "Gestion moyenne de la pression. Fiable dans les mid-card matches.",
            ["Good"] = "Solide sous pression. Peut être utilisé en PPV.",
            ["VeryGood"] = "Très bon dans les grands moments. Elevate son niveau.",
            ["Exceptional"] = "Clutch player absolu. Brille dans les main events et PPV majeurs."
        },

        ["Égoïsme"] = new()
        {
            ["VeryLow"] = "Altruiste, met toujours l'équipe et la storyline avant son ego.",
            ["Low"] = "Peu égoïste, accepte facilement de mettre over.",
            ["Average"] = "Niveau d'ego normal pour un wrestler pro.",
            ["Good"] = "Tendance égocentrique. Peut résister à certaines finishes.",
            ["VeryGood"] = "Très égoïste. Négociations difficiles pour le faire perdre.",
            ["Exceptional"] = "Diva absolue. Refuse catégoriquement de jobber. Politique backstage intensive."
        },

        ["Influence"] = new()
        {
            ["VeryLow"] = "Aucune influence backstage. Suivra toutes les directives.",
            ["Low"] = "Faible influence. Pas de pouvoir politique.",
            ["Average"] = "Influence modérée dans le vestiaire.",
            ["Good"] = "Respecté et écouté backstage. Influence certaines décisions.",
            ["VeryGood"] = "Leader de vestiaire avec forte influence politique.",
            ["Exceptional"] = "Booker de l'ombre. Creative control de facto. Décisions majeures passent par lui."
        }
    };

    public AgentReport GenerateReport(WorkerMentalAttributes mental)
    {
        var report = new AgentReport
        {
            WorkerId = mental.WorkerId,
            ProfessionnalismeScore = mental.ProfessionnalismeScore,
            PressionScore = mental.PressionScore,
            ÉgoïsmeScore = mental.ÉgoïsmeScore,
            InfluenceScore = mental.InfluenceScore,
            GeneratedAt = DateTime.UtcNow
        };

        // Génération du texte complet
        var paragraphs = new List<string>
        {
            GetPillarText("Professionnalisme", report.ProfessionnalismeScore),
            GetPillarText("Pression", report.PressionScore),
            GetPillarText("Égoïsme", report.ÉgoïsmeScore),
            GetPillarText("Influence", report.InfluenceScore)
        };

        report.ReportText = string.Join("\n\n", paragraphs);
        report.Summary = GenerateSummary(mental);

        return report;
    }

    private string GetPillarText(string pillar, double score)
    {
        var level = score switch
        {
            <= 5 => "VeryLow",
            <= 9 => "Low",
            <= 13 => "Average",
            <= 16 => "Good",
            <= 19 => "VeryGood",
            _ => "Exceptional"
        };

        return _templates[pillar][level];
    }

    private string GenerateSummary(WorkerMentalAttributes mental)
    {
        var traits = new List<string>();

        if (mental.Professionnalisme >= 15) traits.Add("professionnel");
        if (mental.Pression >= 15) traits.Add("fiable sous pression");
        if (mental.Égoïsme <= 6) traits.Add("peu égoïste");
        if (mental.Égoïsme >= 15) traits.Add("égocentrique");
        if (mental.Influence >= 15) traits.Add("influent backstage");
        if (mental.Loyauté >= 15) traits.Add("loyal");
        if (mental.Tempérament <= 6) traits.Add("explosif");

        return traits.Count > 0
            ? $"Worker {string.Join(", ", traits)}."
            : "Profil équilibré sans traits dominants.";
    }
}
```

---

## 🎨 Intégration UI

### Option 1: Nouvel Onglet "Personnalité"

**ProfileView.axaml** - Ajout d'un 7ème tab:

```xaml
<!-- Tab 7: Personnalité -->
<TabItem Header="🎭 PERSONNALITÉ">
    <ScrollViewer>
        <StackPanel Margin="20" Spacing="25">

            <!-- Profil Card -->
            <Border Background="#1e293b" Padding="20" CornerRadius="8">
                <Grid ColumnDefinitions="Auto,*,Auto">
                    <TextBlock Grid.Column="0" Text="🎭" FontSize="48" Margin="0,0,20,0"/>
                    <StackPanel Grid.Column="1" Spacing="8">
                        <TextBlock Text="PROFIL DE PERSONNALITÉ" FontSize="14" Foreground="#94a3b8"/>
                        <TextBlock Text="{Binding PersonalityTab.ProfileDisplayName}"
                                   FontSize="28" FontWeight="Bold" Foreground="#3b82f6"/>
                        <TextBlock Text="{Binding PersonalityTab.ProfileDescription}"
                                   FontSize="14" Foreground="#cbd5e1" TextWrapping="Wrap"/>
                    </StackPanel>
                    <Button Grid.Column="2" Content="🔄 Recalculer"
                            Command="{Binding PersonalityTab.RecalculateProfileCommand}"
                            Padding="10,6" Background="#0ea5e9"/>
                </Grid>
            </Border>

            <!-- Rapport Agent -->
            <Border Background="#1e293b" Padding="20" CornerRadius="8">
                <StackPanel Spacing="15">
                    <TextBlock Text="📊 RAPPORT D'AGENT" FontSize="16" FontWeight="Bold" Foreground="#e0e0e0"/>

                    <!-- 4 Piliers -->
                    <Grid RowDefinitions="Auto,Auto,Auto,Auto" ColumnDefinitions="150,*,60">
                        <!-- Professionnalisme -->
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Professionnalisme"
                                   FontSize="13" Foreground="#cbd5e1" VerticalAlignment="Center"/>
                        <ProgressBar Grid.Row="0" Grid.Column="1"
                                     Value="{Binding PersonalityTab.AgentReport.ProfessionnalismeScore}"
                                     Maximum="20" Height="20" Margin="10,5"/>
                        <TextBlock Grid.Row="0" Grid.Column="2"
                                   Text="{Binding PersonalityTab.AgentReport.ProfessionnalismeScore, StringFormat='{}{0:F1}/20'}"
                                   FontSize="13" Foreground="#3b82f6" HorizontalAlignment="Right"/>

                        <!-- Pression -->
                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Gestion Pression"
                                   FontSize="13" Foreground="#cbd5e1" VerticalAlignment="Center"/>
                        <ProgressBar Grid.Row="1" Grid.Column="1"
                                     Value="{Binding PersonalityTab.AgentReport.PressionScore}"
                                     Maximum="20" Height="20" Margin="10,5"/>
                        <TextBlock Grid.Row="1" Grid.Column="2"
                                   Text="{Binding PersonalityTab.AgentReport.PressionScore, StringFormat='{}{0:F1}/20'}"
                                   FontSize="13" Foreground="#10b981" HorizontalAlignment="Right"/>

                        <!-- Égoïsme -->
                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Niveau d'Égo"
                                   FontSize="13" Foreground="#cbd5e1" VerticalAlignment="Center"/>
                        <ProgressBar Grid.Row="2" Grid.Column="1"
                                     Value="{Binding PersonalityTab.AgentReport.ÉgoïsmeScore}"
                                     Maximum="20" Height="20" Margin="10,5"/>
                        <TextBlock Grid.Row="2" Grid.Column="2"
                                   Text="{Binding PersonalityTab.AgentReport.ÉgoïsmeScore, StringFormat='{}{0:F1}/20'}"
                                   FontSize="13" Foreground="#ef4444" HorizontalAlignment="Right"/>

                        <!-- Influence -->
                        <TextBlock Grid.Row="3" Grid.Column="0" Text="Influence Backstage"
                                   FontSize="13" Foreground="#cbd5e1" VerticalAlignment="Center"/>
                        <ProgressBar Grid.Row="3" Grid.Column="1"
                                     Value="{Binding PersonalityTab.AgentReport.InfluenceScore}"
                                     Maximum="20" Height="20" Margin="10,5"/>
                        <TextBlock Grid.Row="3" Grid.Column="2"
                                   Text="{Binding PersonalityTab.AgentReport.InfluenceScore, StringFormat='{}{0:F1}/20'}"
                                   FontSize="13" Foreground="#a855f7" HorizontalAlignment="Right"/>
                    </Grid>

                    <!-- Texte du rapport -->
                    <Border Background="#0f172a" Padding="15" CornerRadius="4" Margin="0,10,0,0">
                        <TextBlock Text="{Binding PersonalityTab.AgentReport.ReportText}"
                                   FontSize="13" Foreground="#cbd5e1" TextWrapping="Wrap" LineHeight="22"/>
                    </Border>
                </StackPanel>
            </Border>

            <!-- Attributs Mentaux Détaillés (si scouting complet) -->
            <Border Background="#1e293b" Padding="20" CornerRadius="8"
                    IsVisible="{Binding PersonalityTab.IsScoutingCompleted}">
                <StackPanel Spacing="15">
                    <Grid ColumnDefinitions="*,Auto">
                        <TextBlock Grid.Column="0" Text="🔍 ATTRIBUTS MENTAUX DÉTAILLÉS"
                                   FontSize="16" FontWeight="Bold" Foreground="#e0e0e0"/>
                        <TextBlock Grid.Column="1" Text="(Révélé par scouting)"
                                   FontSize="12" Foreground="#94a3b8" VerticalAlignment="Center"/>
                    </Grid>

                    <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto,Auto,Auto">
                        <!-- Row 0 -->
                        <StackPanel Grid.Row="0" Grid.Column="0" Spacing="5" Margin="0,0,10,10">
                            <TextBlock Text="Ambition" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Ambition}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>
                        <StackPanel Grid.Row="0" Grid.Column="1" Spacing="5" Margin="10,0,0,10">
                            <TextBlock Text="Loyauté" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Loyauté}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>

                        <!-- Row 1 -->
                        <StackPanel Grid.Row="1" Grid.Column="0" Spacing="5" Margin="0,0,10,10">
                            <TextBlock Text="Professionnalisme" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Professionnalisme}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>
                        <StackPanel Grid.Row="1" Grid.Column="1" Spacing="5" Margin="10,0,0,10">
                            <TextBlock Text="Pression" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Pression}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>

                        <!-- Row 2 -->
                        <StackPanel Grid.Row="2" Grid.Column="0" Spacing="5" Margin="0,0,10,10">
                            <TextBlock Text="Tempérament" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Tempérament}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>
                        <StackPanel Grid.Row="2" Grid.Column="1" Spacing="5" Margin="10,0,0,10">
                            <TextBlock Text="Égoïsme" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Égoïsme}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>

                        <!-- Row 3 -->
                        <StackPanel Grid.Row="3" Grid.Column="0" Spacing="5" Margin="0,0,10,10">
                            <TextBlock Text="Détermination" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Détermination}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>
                        <StackPanel Grid.Row="3" Grid.Column="1" Spacing="5" Margin="10,0,0,10">
                            <TextBlock Text="Adaptabilité" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Adaptabilité}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>

                        <!-- Row 4 -->
                        <StackPanel Grid.Row="4" Grid.Column="0" Spacing="5" Margin="0,0,10,0">
                            <TextBlock Text="Influence" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Influence}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>
                        <StackPanel Grid.Row="4" Grid.Column="1" Spacing="5" Margin="10,0,0,0">
                            <TextBlock Text="Sportivité" FontSize="12" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding PersonalityTab.MentalAttributes.Sportivité}"
                                       FontSize="18" FontWeight="Bold" Foreground="#60a5fa"/>
                        </StackPanel>
                    </Grid>
                </StackPanel>
            </Border>

            <!-- Warning si pas encore scouté -->
            <Border Background="#1e293b" Padding="20" CornerRadius="8"
                    IsVisible="{Binding PersonalityTab.IsScoutingNotCompleted}">
                <StackPanel Spacing="10">
                    <TextBlock Text="⚠️ SCOUTING NON COMPLÉTÉ" FontSize="16" FontWeight="Bold" Foreground="#f59e0b"/>
                    <TextBlock Text="Les attributs mentaux détaillés ne sont pas encore révélés. Lancez une mission de scouting pour obtenir le rapport complet."
                               FontSize="13" Foreground="#cbd5e1" TextWrapping="Wrap"/>
                    <Button Content="🔍 Lancer Scouting" Command="{Binding PersonalityTab.LaunchScoutingCommand}"
                            Padding="12,8" Background="#0ea5e9" HorizontalAlignment="Left"/>
                </StackPanel>
            </Border>

        </StackPanel>
    </ScrollViewer>
</TabItem>
```

### PersonalityTabViewModel.cs

```csharp
using System;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

public sealed class PersonalityTabViewModel : ViewModelBase
{
    private readonly IWorkerAttributesRepository _attributesRepo;
    private readonly PersonalityDetectorService _detectorService;
    private readonly AgentReportGeneratorService _reportService;
    private readonly int _workerId;

    private PersonalityProfile _profile;
    private string _profileDisplayName = string.Empty;
    private string _profileDescription = string.Empty;
    private AgentReport? _agentReport;
    private WorkerMentalAttributes? _mentalAttributes;
    private bool _isScoutingCompleted;

    public PersonalityTabViewModel(
        int workerId,
        IWorkerAttributesRepository attributesRepo,
        PersonalityDetectorService detectorService,
        AgentReportGeneratorService reportService)
    {
        _workerId = workerId;
        _attributesRepo = attributesRepo;
        _detectorService = detectorService;
        _reportService = reportService;

        RecalculateProfileCommand = ReactiveCommand.Create(RecalculateProfile);
        LaunchScoutingCommand = ReactiveCommand.Create(LaunchScouting);

        LoadPersonalityData();
    }

    // ===== PROPERTIES =====

    public PersonalityProfile Profile
    {
        get => _profile;
        set => this.RaiseAndSetIfChanged(ref _profile, value);
    }

    public string ProfileDisplayName
    {
        get => _profileDisplayName;
        set => this.RaiseAndSetIfChanged(ref _profileDisplayName, value);
    }

    public string ProfileDescription
    {
        get => _profileDescription;
        set => this.RaiseAndSetIfChanged(ref _profileDescription, value);
    }

    public AgentReport? AgentReport
    {
        get => _agentReport;
        set => this.RaiseAndSetIfChanged(ref _agentReport, value);
    }

    public WorkerMentalAttributes? MentalAttributes
    {
        get => _mentalAttributes;
        set
        {
            this.RaiseAndSetIfChanged(ref _mentalAttributes, value);
            IsScoutingCompleted = value?.IsRevealed ?? false;
        }
    }

    public bool IsScoutingCompleted
    {
        get => _isScoutingCompleted;
        set
        {
            this.RaiseAndSetIfChanged(ref _isScoutingCompleted, value);
            this.RaisePropertyChanged(nameof(IsScoutingNotCompleted));
        }
    }

    public bool IsScoutingNotCompleted => !IsScoutingCompleted;

    // ===== COMMANDS =====

    public ReactiveCommand<Unit, Unit> RecalculateProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> LaunchScoutingCommand { get; }

    // ===== METHODS =====

    private void LoadPersonalityData()
    {
        MentalAttributes = _attributesRepo.GetMentalAttributes(_workerId);

        if (MentalAttributes != null)
        {
            Profile = _detectorService.DetectProfile(MentalAttributes);
            ProfileDisplayName = GetProfileDisplayName(Profile);
            ProfileDescription = GetProfileDescription(Profile);
            AgentReport = _reportService.GenerateReport(MentalAttributes);
        }
    }

    private void RecalculateProfile()
    {
        LoadPersonalityData();
    }

    private void LaunchScouting()
    {
        // TODO: Implémenter système de scouting
        // Pour l'instant, on révèle simplement
        if (MentalAttributes != null)
        {
            MentalAttributes.IsRevealed = true;
            MentalAttributes.ScoutingLevel = 2;
            _attributesRepo.UpdateMentalAttributes(MentalAttributes);
            IsScoutingCompleted = true;
        }
    }

    private string GetProfileDisplayName(PersonalityProfile profile)
    {
        return profile switch
        {
            PersonalityProfile.ProfessionnelExemplaire => "Professionnel Exemplaire ⭐",
            PersonalityProfile.CitoyenModele => "Citoyen Modèle 🏆",
            PersonalityProfile.Déterminé => "Déterminé 💪",
            PersonalityProfile.Ambitieux => "Ambitieux 🚀",
            PersonalityProfile.LeaderDeVestiaire => "Leader de Vestiaire 👑",
            PersonalityProfile.Mercenaire => "Mercenaire 💰",
            PersonalityProfile.TempéramentDeFeu => "Tempérament de Feu 🔥",
            PersonalityProfile.FrancTireur => "Franc-Tireur 🎲",
            PersonalityProfile.Inconstant => "Inconstant 📉",
            PersonalityProfile.Égoïste => "Égoïste 😈",
            PersonalityProfile.Diva => "Diva 👸",
            PersonalityProfile.Paresseux => "Paresseux 💤",
            PersonalityProfile.VétéranRusé => "Vétéran Rusé 🦊",
            PersonalityProfile.MaîtreDuStorytelling => "Maître du Storytelling 📖",
            PersonalityProfile.Politicien => "Politicien 🎭",
            PersonalityProfile.AccroAuRing => "Accro au Ring 🥊",
            PersonalityProfile.PilierFiable => "Pilier Fiable 🛡️",
            PersonalityProfile.MachineDeGuerre => "Machine de Guerre ⚙️",
            PersonalityProfile.ObsédéParLImage => "Obsédé par l'Image 📸",
            PersonalityProfile.CharismatiqueImprévisible => "Charismatique Imprévisible ⚡",
            PersonalityProfile.AimantÀPublic => "Aimant à Public 🌟",
            PersonalityProfile.SaboteurPassif => "Saboteur Passif 🐍",
            PersonalityProfile.InstableChronique => "Instable Chronique 💥",
            PersonalityProfile.PoidsMort => "Poids Mort ⚠️",
            PersonalityProfile.Équilibré => "Équilibré 📊",
            _ => "Non Déterminé ❓"
        };
    }

    private string GetProfileDescription(PersonalityProfile profile)
    {
        return profile switch
        {
            PersonalityProfile.ProfessionnelExemplaire =>
                "Le worker modèle. Professionnalisme exemplaire, fiable sous pression, respectueux et respecté.",
            PersonalityProfile.CitoyenModele =>
                "Pilier du vestiaire, loyal et peu égoïste. Met toujours l'entreprise avant son ego.",
            PersonalityProfile.Ambitieux =>
                "Déterminé à atteindre le sommet. Ambition forte et détermination sans faille.",
            PersonalityProfile.TempéramentDeFeu =>
                "Explosif mais talentueux. Risque de conflits backstage mais performances solides.",
            PersonalityProfile.Égoïste =>
                "Très égocentrique. Difficile à convaincre de perdre ou mettre over les autres.",
            PersonalityProfile.Diva =>
                "Problèmes constants backstage. Égo démesuré et mauvais tempérament.",
            PersonalityProfile.VétéranRusé =>
                "Politique backstage. Adaptable et influent, mais peu fair-play.",
            PersonalityProfile.MachineDeGuerre =>
                "Indestructible. Déterminé, fiable sous pression, tempérament d'acier.",
            PersonalityProfile.PoidsMort =>
                "Aucun intérêt. Pas professionnel, pas déterminé, pas ambitieux.",
            _ => "Profil en cours d'analyse."
        };
    }
}
```

---

## 🔄 Migration et Import

### Script de Migration

**Migration_Phase8_Personality.sql**:

```sql
-- ============================================================
-- MIGRATION PHASE 8: SYSTÈME PERSONNALITÉS
-- Date: 2026-01-08
-- Description: Ajout 10 attributs mentaux + profils personnalité
-- ============================================================

-- ===== 1. NOUVELLE TABLE ATTRIBUTS MENTAUX =====

CREATE TABLE IF NOT EXISTS WorkerMentalAttributes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL UNIQUE,

    -- 10 Attributs Mentaux (0-20)
    Ambition INTEGER NOT NULL DEFAULT 10 CHECK(Ambition BETWEEN 0 AND 20),
    Loyauté INTEGER NOT NULL DEFAULT 10 CHECK(Loyauté BETWEEN 0 AND 20),
    Professionnalisme INTEGER NOT NULL DEFAULT 10 CHECK(Professionnalisme BETWEEN 0 AND 20),
    Pression INTEGER NOT NULL DEFAULT 10 CHECK(Pression BETWEEN 0 AND 20),
    Tempérament INTEGER NOT NULL DEFAULT 10 CHECK(Tempérament BETWEEN 0 AND 20),
    Égoïsme INTEGER NOT NULL DEFAULT 10 CHECK(Égoïsme BETWEEN 0 AND 20),
    Détermination INTEGER NOT NULL DEFAULT 10 CHECK(Détermination BETWEEN 0 AND 20),
    Adaptabilité INTEGER NOT NULL DEFAULT 10 CHECK(Adaptabilité BETWEEN 0 AND 20),
    Influence INTEGER NOT NULL DEFAULT 10 CHECK(Influence BETWEEN 0 AND 20),
    Sportivité INTEGER NOT NULL DEFAULT 10 CHECK(Sportivité BETWEEN 0 AND 20),

    -- Metadata
    IsRevealed BOOLEAN NOT NULL DEFAULT 0,
    ScoutingLevel INTEGER NOT NULL DEFAULT 0,
    LastUpdated TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Foreign Key
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_mental_worker ON WorkerMentalAttributes(WorkerId);

-- ===== 2. EXTENSION TABLE WORKERS =====

ALTER TABLE Workers ADD COLUMN PersonalityProfile TEXT DEFAULT NULL;
ALTER TABLE Workers ADD COLUMN PersonalityProfileDetectedAt TEXT DEFAULT NULL;

-- ===== 3. GÉNÉRATION ATTRIBUTS MENTAUX POUR WORKERS EXISTANTS =====

-- Algorithme de génération basé sur attributs existants + expérience + popularité
INSERT INTO WorkerMentalAttributes (
    WorkerId,
    Ambition,
    Loyauté,
    Professionnalisme,
    Pression,
    Tempérament,
    Égoïsme,
    Détermination,
    Adaptabilité,
    Influence,
    Sportivité
)
SELECT
    w.Id AS WorkerId,

    -- Ambition: Corrélé avec popularité + PushLevel
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN w.Popularity > 80 THEN 5 WHEN w.Popularity > 60 THEN 3 ELSE 0 END
        + CASE WHEN w.PushLevel = 'MainEvent' THEN 5
               WHEN w.PushLevel = 'UpperMidcard' THEN 3
               ELSE 0 END
    )) AS Ambition,

    -- Loyauté: Corrélé avec expérience + âge
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN w.Experience >= 15 THEN 4 WHEN w.Experience >= 10 THEN 2 ELSE 0 END
        + CASE WHEN w.Age >= 35 THEN 3 ELSE 0 END
    )) AS Loyauté,

    -- Professionnalisme: Corrélé avec expérience + Safety
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN wir.Safety >= 80 THEN 5 WHEN wir.Safety >= 60 THEN 3 ELSE 0 END
        + CASE WHEN w.Experience >= 10 THEN 3 ELSE 0 END
    )) AS Professionnalisme,

    -- Pression: Corrélé avec Timing + Psychology
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN wir.Timing >= 80 THEN 5 WHEN wir.Timing >= 60 THEN 3 ELSE 0 END
        + CASE WHEN wir.Psychology >= 75 THEN 3 ELSE 0 END
    )) AS Pression,

    -- Tempérament: Aléatoire pur (non corrélé)
    MAX(0, MIN(20, 10 + (ABS(RANDOM()) % 21 - 10))) AS Tempérament,

    -- Égoïsme: Corrélé avec popularité + PushLevel (égo grandit avec succès)
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN w.Popularity > 85 THEN 5 WHEN w.Popularity > 70 THEN 3 ELSE 0 END
        + CASE WHEN w.PushLevel = 'MainEvent' THEN 4 ELSE 0 END
    )) AS Égoïsme,

    -- Détermination: Corrélé avec Stamina + expérience
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN wir.Stamina >= 80 THEN 4 WHEN wir.Stamina >= 60 THEN 2 ELSE 0 END
        + CASE WHEN w.Experience >= 8 THEN 3 ELSE 0 END
    )) AS Détermination,

    -- Adaptabilité: Corrélé avec expérience + moyennes équilibrées
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN w.Experience >= 12 THEN 4 WHEN w.Experience >= 7 THEN 2 ELSE 0 END
        + CASE
            WHEN ABS(wir.InRingAvg - wea.EntertainmentAvg) < 10 THEN 3
            ELSE 0
          END
    )) AS Adaptabilité,

    -- Influence: Corrélé avec expérience + popularité + PushLevel
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN w.Experience >= 15 THEN 5 WHEN w.Experience >= 10 THEN 3 ELSE 0 END
        + CASE WHEN w.Popularity > 80 THEN 4 ELSE 0 END
        + CASE WHEN w.PushLevel = 'MainEvent' THEN 3 ELSE 0 END
    )) AS Influence,

    -- Sportivité: Corrélé avec Safety + Psychology
    MAX(0, MIN(20,
        10 + (ABS(RANDOM()) % 11 - 5)
        + CASE WHEN wir.Safety >= 85 THEN 5 WHEN wir.Safety >= 70 THEN 3 ELSE 0 END
        + CASE WHEN wir.Psychology >= 75 THEN 2 ELSE 0 END
    )) AS Sportivité

FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId;

-- ===== 4. DÉTECTION AUTOMATIQUE DES PROFILS =====

-- Cette partie sera exécutée par le service C# PersonalityDetectorService
-- après l'import pour assigner PersonalityProfile

-- ===== 5. VALIDATION =====

-- Vérifier que tous les workers ont des attributs mentaux
SELECT
    COUNT(*) AS TotalWorkers,
    COUNT(wma.Id) AS WorkersWithMental,
    COUNT(*) - COUNT(wma.Id) AS Missing
FROM Workers w
LEFT JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId;

-- Distribution des attributs
SELECT
    'Ambition' AS Attribut,
    AVG(Ambition) AS Moyenne,
    MIN(Ambition) AS Min,
    MAX(Ambition) AS Max
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Loyauté', AVG(Loyauté), MIN(Loyauté), MAX(Loyauté) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Professionnalisme', AVG(Professionnalisme), MIN(Professionnalisme), MAX(Professionnalisme) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Pression', AVG(Pression), MIN(Pression), MAX(Pression) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Tempérament', AVG(Tempérament), MIN(Tempérament), MAX(Tempérament) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Égoïsme', AVG(Égoïsme), MIN(Égoïsme), MAX(Égoïsme) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Détermination', AVG(Détermination), MIN(Détermination), MAX(Détermination) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Adaptabilité', AVG(Adaptabilité), MIN(Adaptabilité), MAX(Adaptabilité) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Influence', AVG(Influence), MIN(Influence), MAX(Influence) FROM WorkerMentalAttributes
UNION ALL
SELECT 'Sportivité', AVG(Sportivité), MIN(Sportivité), MAX(Sportivité) FROM WorkerMentalAttributes;

-- Top 10 par Professionnalisme
SELECT w.Name, wma.Professionnalisme, wma.Sportivité, wma.Loyauté
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
ORDER BY wma.Professionnalisme DESC
LIMIT 10;

-- Top 10 Égoïstes
SELECT w.Name, wma.Égoïsme, wma.Ambition, wma.Sportivité
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
ORDER BY wma.Égoïsme DESC
LIMIT 10;

-- ===== FIN MIGRATION =====
```

---

## ✅ Tests et Validation

### Tests Unitaires

**PersonalityDetectorServiceTests.cs**:

```csharp
using Xunit;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;

namespace RingGeneral.Tests.Services;

public class PersonalityDetectorServiceTests
{
    private readonly PersonalityDetectorService _detector = new();

    [Fact]
    public void DetectProfile_ProfessionnelExemplaire_WhenAllHighPro()
    {
        var mental = new WorkerMentalAttributes
        {
            Professionnalisme = 18,
            Sportivité = 17,
            Tempérament = 16
        };

        var profile = _detector.DetectProfile(mental);

        Assert.Equal(PersonalityProfile.ProfessionnelExemplaire, profile);
    }

    [Fact]
    public void DetectProfile_Diva_WhenHighEgoLowTempérament()
    {
        var mental = new WorkerMentalAttributes
        {
            Égoïsme = 18,
            Tempérament = 4,
            Professionnalisme = 8
        };

        var profile = _detector.DetectProfile(mental);

        Assert.Equal(PersonalityProfile.Diva, profile);
    }

    [Fact]
    public void DetectProfile_PoidsMort_WhenAllLow()
    {
        var mental = new WorkerMentalAttributes
        {
            Professionnalisme = 3,
            Détermination = 4,
            Ambition = 2
        };

        var profile = _detector.DetectProfile(mental);

        Assert.Equal(PersonalityProfile.PoidsMort, profile);
    }

    [Fact]
    public void DetectProfile_Équilibré_WhenAllAverage()
    {
        var mental = new WorkerMentalAttributes
        {
            Ambition = 10,
            Loyauté = 11,
            Professionnalisme = 10,
            Pression = 12,
            Tempérament = 9,
            Égoïsme = 11,
            Détermination = 10,
            Adaptabilité = 12,
            Influence = 10,
            Sportivité = 11
        };

        var profile = _detector.DetectProfile(mental);

        Assert.Equal(PersonalityProfile.Équilibré, profile);
    }
}
```

### Tests d'Intégration

Validation post-migration:

```sql
-- Test 1: Tous les workers ont des attributs mentaux
SELECT COUNT(*) FROM Workers w
LEFT JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
WHERE wma.Id IS NULL;
-- Expected: 0

-- Test 2: Attributs dans la plage 0-20
SELECT COUNT(*) FROM WorkerMentalAttributes
WHERE Ambition < 0 OR Ambition > 20
   OR Loyauté < 0 OR Loyauté > 20
   OR Professionnalisme < 0 OR Professionnalisme > 20;
-- Expected: 0

-- Test 3: Distribution réaliste (moyenne ~10-12)
SELECT AVG(Ambition), AVG(Loyauté), AVG(Professionnalisme)
FROM WorkerMentalAttributes;
-- Expected: ~10-12 pour chaque

-- Test 4: Profils assignés
SELECT COUNT(*) FROM Workers WHERE PersonalityProfile IS NULL;
-- Expected: 0 (après exécution du service)
```

---

## 📊 Récapitulatif Phase 8

### Fichiers à Créer

1. **Database**:
   - `Migration_Phase8_Personality.sql`

2. **Models** (Core):
   - `WorkerMentalAttributes.cs`
   - `PersonalityProfile.cs` (enum)
   - `AgentReport.cs`

3. **Services** (Core):
   - `PersonalityDetectorService.cs`
   - `AgentReportGeneratorService.cs`

4. **Repositories** (Data):
   - Extension de `IWorkerAttributesRepository`
   - Implémentation dans `WorkerAttributesRepository.cs`

5. **ViewModels** (UI):
   - `PersonalityTabViewModel.cs`

6. **Views** (UI):
   - Modification de `ProfileView.axaml` (ajout tab)

7. **Tests**:
   - `PersonalityDetectorServiceTests.cs`
   - `AgentReportGeneratorServiceTests.cs`

### Ordre d'Implémentation

```
Phase 8.1: Database + Migration ✅
    ├── Migration_Phase8_Personality.sql
    └── Exécution + validation

Phase 8.2: Models ✅
    ├── WorkerMentalAttributes.cs
    ├── PersonalityProfile.cs
    └── AgentReport.cs

Phase 8.3: Services ✅
    ├── PersonalityDetectorService.cs
    └── AgentReportGeneratorService.cs

Phase 8.4: Repositories ✅
    └── Extension IWorkerAttributesRepository

Phase 8.5: ViewModels ✅
    └── PersonalityTabViewModel.cs

Phase 8.6: Views ✅
    └── ProfileView.axaml (tab Personnalité)

Phase 8.7: Integration ✅
    ├── DI registration (Program.cs)
    └── Service initialization

Phase 8.8: Tests + Validation ✅
    ├── Unit tests
    └── Integration tests
```

### Temps Estimé

- Phase 8.1-8.3: **2 heures** (Database + Models + Services)
- Phase 8.4-8.6: **2 heures** (Repos + ViewModels + UI)
- Phase 8.7-8.8: **1 heure** (Integration + Tests)
- **Total: 5 heures**

---

## 🎯 Next Steps

1. ✅ **Review ce plan** avec le chef de projet
2. ⏳ **Exécuter Phase 8.1**: Créer migration SQL
3. ⏳ **Exécuter Phase 8.2**: Créer les 3 models
4. ⏳ **Exécuter Phase 8.3**: Implémenter les services de détection
5. ⏳ **Exécuter Phase 8.4-8.6**: Repos + ViewModels + UI
6. ⏳ **Exécuter Phase 8.7**: Integration complète
7. ⏳ **Commit + Push** vers `claude/rework-performance-attributes-YBXRx`

---

**Prêt pour validation et implémentation !** 🚀
