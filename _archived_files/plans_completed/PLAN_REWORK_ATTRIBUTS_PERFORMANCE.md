# 🎯 Plan de Rework : Attributs de Performance 2.0

**Date** : 7 janvier 2026
**Chef de Projet** : Claude DevOps
**Version** : 1.0
**Branche** : `claude/rework-performance-attributes-YBXRx`

---

## 📋 VUE D'ENSEMBLE

### Objectif

Refondre complètement le système d'attributs de performance pour passer d'un modèle simplifié à un système professionnel en 3 dimensions avec **30 attributs détaillés** (10 par catégorie).

### Périmètre du Rework

**Avant** : 13 attributs basiques (In-Ring, Entertainment, Story + sous-stats)
**Après** : 30 attributs professionnels répartis en 3 catégories :

1. **IN-RING** (Technique & Physique) - 10 attributs
2. **ENTERTAINMENT** (Présence & Micro) - 10 attributs
3. **STORY** (Écriture & Personnage) - 10 attributs

**+ Page Profil** principale avant les tabs avec fiche personnage complète (photo, identité, spécialisations, géographie)

---

## 🎯 CHANGEMENTS MAJEURS

### 1. Nouvelle Structure des Attributs

#### 🏗️ IN-RING (Technique & Physique)

Passage de l'exécution simple à la maîtrise des styles et condition physique :

| Attribut | Description | Échelle |
|----------|-------------|---------|
| **Striking** | Précision et impact des coups (poings, pieds, coudes) | 0-100 |
| **Grappling** | Maîtrise des prises au sol et soumissions | 0-100 |
| **High-Flying** | Agilité, acrobaties et prises aériennes | 0-100 |
| **Powerhouse** | Capacité à soulever adversaires lourds et force brute | 0-100 |
| **Timing** | Précision chirurgicale dans l'enchaînement | 0-100 |
| **Selling** | Capacité à rendre les coups de l'adversaire crédibles | 0-100 |
| **Psychology** | Construire un match pour raconter une histoire logique | 0-100 |
| **Stamina** | Endurance pour maintenir un rythme élevé 30+ min | 0-100 |
| **Safety** | Capacité à protéger son partenaire (limite blessures) | 0-100 |
| **Hardcore/Brawl** | Utilisation d'objets et combat de rue | 0-100 |

**Moyenne calculée** : `InRing_Avg = Moyenne des 10 attributs`

---

#### 🎤 ENTERTAINMENT (Présence & Micro)

Impact visuel et sonore sur l'audience :

| Attribut | Description | Échelle |
|----------|-------------|---------|
| **Charisma** | Magnétisme naturel, même sans parler | 0-100 |
| **Mic Work (Promo)** | Aisance verbale et capacité à délivrer un script | 0-100 |
| **Acting** | Crédibilité dans expressions faciales et segments backstage | 0-100 |
| **Crowd Connection** | Capacité à faire réagir la foule (Heat ou Cheers) | 0-100 |
| **Star Power** | Aura de "Main Eventer", look et prestance | 0-100 |
| **Improvisation** | Capacité à réagir aux imprévus ou chants du public | 0-100 |
| **Entrance** | Impact visuel et théâtralité de l'arrivée vers le ring | 0-100 |
| **Sex Appeal / Cool Factor** | Attrait esthétique ou facteur "tendance" | 0-100 |
| **Merchandise Appeal** | Potentiel de vente de produits dérivés (design, logos) | 0-100 |
| **Crossover Potential** | Capacité à attirer un public hors-catch (Cinéma, TV) | 0-100 |

**Moyenne calculée** : `Entertainment_Avg = Moyenne des 10 attributs`

---

#### 📖 STORY (Écriture & Personnage)

Profondeur du Gimmick et polyvalence narrative :

| Attribut | Description | Échelle |
|----------|-------------|---------|
| **Character Depth** | Complexité et nuances du personnage (pas juste gentil/méchant) | 0-100 |
| **Consistency** | Fidélité au personnage sur le long terme | 0-100 |
| **Heel Performance** | Efficacité dans le rôle de l'antagoniste | 0-100 |
| **Babyface Performance** | Efficacité dans le rôle du héros | 0-100 |
| **Storytelling (Long-term)** | Capacité à porter une rivalité sur plusieurs mois | 0-100 |
| **Emotional Range** | Capacité à générer tristesse, peur, joie | 0-100 |
| **Adaptability** | Facilité à changer de gimmick ou à évoluer | 0-100 |
| **Rivalry Chemistry** | Capacité naturelle à créer étincelle avec adversaire | 0-100 |
| **Creative Input** | Implication du catcheur dans ses propres storylines | 0-100 |
| **Moral Alignment** | Capacité à jouer les "Tweener" (zone grise morale) | 0-100 |

**Moyenne calculée** : `Story_Avg = Moyenne des 10 attributs`

---

### 2. Nouvelle Page Profil (Main Page)

**Ajout AVANT les 6 tabs** d'une fiche personnage complète :

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ JOHN CENA [USA] [■■■■■] PROFIL                                                         │
├───────────────────┬─────────────────────────────────────────────────────────────────────┤
│ ┌─────────┐       │ Rôle : Main Eventer (Star)    Contrat : 3,500,000 € / an          │
│ │         │       │ Style : Brawler / Powerhouse   Moral : Excellent                   │
│ │  PHOTO  │       │ Poids : 114 kg  Taille : 185 cm                                    │
│ │         │       │ Droitier (Pied/Poing)  Exp. : 24 ans                               │
│ └─────────┘       │ ─────────────────────────────────────────────────────────────────  │
│ 46 ans            │ [ Condition: 78% ] [ Forme: 88% ] [ Fatigue: 35% ] [ Pop: 95 ]    │
└───────────────────┴─────────────────────────────────────────────────────────────────────┘

IN-RING (Moy: 82)          ENTERTAINMENT (Moy: 88)      STORY (Moy: 80)
┌───────────────────────┐  ┌───────────────────────┐    ┌───────────────────────┐
│ Striking       │ 75   │  │ Charisme       │ 92   │    │ Prof. Perso    │ 84   │
│ Grappling      │ 78   │  │ Mic Work       │ 95   │    │ Cohérence      │ 90   │
│ High-Flying    │ 45   │  │ Acting         │ 88   │    │ Perf. Heel     │ 80   │
│ Force Brute    │ 90   │  │ Connexion      │ 98   │    │ Perf. Face     │ 95   │
│ Timing         │ 85   │  │ Star Power     │ 95   │    │ Storytelling   │ 88   │
│ Selling        │ 82   │  │ Improvisation  │ 90   │    │ Émotion        │ 85   │
│ Psychologie    │ 88   │  │ Entrée         │ 92   │    │ Adaptabilité   │ 75   │
│ Stamina        │ 85   │  │ Sex Appeal     │ 85   │    │ Alchimie       │ 82   │
│ Sécurité       │ 94   │  │ Merchandising  │ 96   │    │ Vision Créative│ 78   │
│ Hardcore/Brawl │ 80   │  │ Aura           │ 94   │    │ Nuances        │ 72   │
└───────────────────────┘  └───────────────────────┘    └───────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ ▾ HISTORIQUE DES PERFORMANCES                                                           │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│ [RAW] vs Randy Orton ⭐⭐⭐⭐½ (92) | [SD!] vs AJ Styles ⭐⭐⭐⭐⭐ (97)                   │
│ [PPV] vs Kevin Owens ⭐⭐⭐⭐ (85)  | [RAW] vs Solo Sikoa ⭐⭐⭐½ (72)                    │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

**Nouveaux éléments** :
- Photo/Avatar (200x200px)
- Spécialisations (Brawler, Technical, High-Flyer, Power, etc.)
- Géographie complète (Naissance + Résidence)
- Barres de condition visuelles
- Historique récent des performances
- Display des 30 attributs en 3 colonnes

---

## 🗂️ ARCHITECTURE DES MODIFICATIONS

### Base de Données

#### Tables à Créer

**1. `WorkerInRingAttributes` - Nouvelle table**
```sql
CREATE TABLE WorkerInRingAttributes (
    WorkerId INTEGER PRIMARY KEY,
    Striking INTEGER DEFAULT 50,
    Grappling INTEGER DEFAULT 50,
    HighFlying INTEGER DEFAULT 50,
    Powerhouse INTEGER DEFAULT 50,
    Timing INTEGER DEFAULT 50,
    Selling INTEGER DEFAULT 50,
    Psychology INTEGER DEFAULT 50,
    Stamina INTEGER DEFAULT 50,
    Safety INTEGER DEFAULT 50,
    HardcoreBrawl INTEGER DEFAULT 50,
    InRingAvg INTEGER GENERATED ALWAYS AS (
        (Striking + Grappling + HighFlying + Powerhouse + Timing +
         Selling + Psychology + Stamina + Safety + HardcoreBrawl) / 10
    ) STORED,
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id)
);
```

**2. `WorkerEntertainmentAttributes` - Nouvelle table**
```sql
CREATE TABLE WorkerEntertainmentAttributes (
    WorkerId INTEGER PRIMARY KEY,
    Charisma INTEGER DEFAULT 50,
    MicWork INTEGER DEFAULT 50,
    Acting INTEGER DEFAULT 50,
    CrowdConnection INTEGER DEFAULT 50,
    StarPower INTEGER DEFAULT 50,
    Improvisation INTEGER DEFAULT 50,
    Entrance INTEGER DEFAULT 50,
    SexAppeal INTEGER DEFAULT 50,
    MerchandiseAppeal INTEGER DEFAULT 50,
    CrossoverPotential INTEGER DEFAULT 50,
    EntertainmentAvg INTEGER GENERATED ALWAYS AS (
        (Charisma + MicWork + Acting + CrowdConnection + StarPower +
         Improvisation + Entrance + SexAppeal + MerchandiseAppeal + CrossoverPotential) / 10
    ) STORED,
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id)
);
```

**3. `WorkerStoryAttributes` - Nouvelle table**
```sql
CREATE TABLE WorkerStoryAttributes (
    WorkerId INTEGER PRIMARY KEY,
    CharacterDepth INTEGER DEFAULT 50,
    Consistency INTEGER DEFAULT 50,
    HeelPerformance INTEGER DEFAULT 50,
    BabyfacePerformance INTEGER DEFAULT 50,
    StorytellingLongTerm INTEGER DEFAULT 50,
    EmotionalRange INTEGER DEFAULT 50,
    Adaptability INTEGER DEFAULT 50,
    RivalryChemistry INTEGER DEFAULT 50,
    CreativeInput INTEGER DEFAULT 50,
    MoralAlignment INTEGER DEFAULT 50,
    StoryAvg INTEGER GENERATED ALWAYS AS (
        (CharacterDepth + Consistency + HeelPerformance + BabyfacePerformance +
         StorytellingLongTerm + EmotionalRange + Adaptability + RivalryChemistry +
         CreativeInput + MoralAlignment) / 10
    ) STORED,
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id)
);
```

**4. `WorkerSpecializations` - Nouvelle table**
```sql
CREATE TABLE WorkerSpecializations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId INTEGER NOT NULL,
    Specialization TEXT NOT NULL, -- 'Brawler', 'Technical', 'HighFlyer', etc.
    Level INTEGER DEFAULT 1, -- 1-3 (Primary, Secondary, Tertiary)
    FOREIGN KEY (WorkerId) REFERENCES Workers(Id)
);
```

**5. Mise à jour de `Workers` table**
```sql
ALTER TABLE Workers ADD COLUMN BirthCity TEXT;
ALTER TABLE Workers ADD COLUMN BirthCountry TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceCity TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceState TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceCountry TEXT;
ALTER TABLE Workers ADD COLUMN PhotoPath TEXT;
ALTER TABLE Workers ADD COLUMN Handedness TEXT DEFAULT 'Right'; -- Right, Left, Ambidextrous
ALTER TABLE Workers ADD COLUMN FightingStance TEXT DEFAULT 'Orthodox'; -- Orthodox, Southpaw, Switch
```

#### Migration Script

**Fichier** : `/src/RingGeneral.Data/Migrations/Migration_v2_AttributesRework.sql`

---

### Models

#### Fichiers à Créer

**1. `/src/RingGeneral.Core/Models/Attributes/WorkerInRingAttributes.cs`**
```csharp
namespace RingGeneral.Core.Models.Attributes
{
    public class WorkerInRingAttributes
    {
        public int WorkerId { get; set; }

        // Core Attributes (0-100)
        public int Striking { get; set; } = 50;
        public int Grappling { get; set; } = 50;
        public int HighFlying { get; set; } = 50;
        public int Powerhouse { get; set; } = 50;
        public int Timing { get; set; } = 50;
        public int Selling { get; set; } = 50;
        public int Psychology { get; set; } = 50;
        public int Stamina { get; set; } = 50;
        public int Safety { get; set; } = 50;
        public int HardcoreBrawl { get; set; } = 50;

        // Calculated Average
        public int InRingAvg => (Striking + Grappling + HighFlying + Powerhouse +
                                 Timing + Selling + Psychology + Stamina +
                                 Safety + HardcoreBrawl) / 10;
    }
}
```

**2. `/src/RingGeneral.Core/Models/Attributes/WorkerEntertainmentAttributes.cs`**
**3. `/src/RingGeneral.Core/Models/Attributes/WorkerStoryAttributes.cs`**

**4. `/src/RingGeneral.Core/Models/WorkerSpecialization.cs`**
```csharp
namespace RingGeneral.Core.Models
{
    public enum SpecializationType
    {
        Brawler,
        Technical,
        HighFlyer,
        Power,
        Hardcore,
        Submission,
        Showman,
        AllRounder
    }

    public class WorkerSpecialization
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public SpecializationType Specialization { get; set; }
        public int Level { get; set; } // 1 = Primary, 2 = Secondary, 3 = Tertiary
    }
}
```

**5. Mise à jour de `/src/RingGeneral.Core/Models/Worker.cs`**
```csharp
// Ajouter propriétés
public string? BirthCity { get; set; }
public string? BirthCountry { get; set; }
public string? ResidenceCity { get; set; }
public string? ResidenceState { get; set; }
public string? ResidenceCountry { get; set; }
public string? PhotoPath { get; set; }
public string Handedness { get; set; } = "Right";
public string FightingStance { get; set; } = "Orthodox";

// Navigation properties
public WorkerInRingAttributes? InRingAttributes { get; set; }
public WorkerEntertainmentAttributes? EntertainmentAttributes { get; set; }
public WorkerStoryAttributes? StoryAttributes { get; set; }
public List<WorkerSpecialization> Specializations { get; set; } = new();
```

---

### Repositories

#### Fichiers à Créer

**1. `/src/RingGeneral.Data/Repositories/Interfaces/IWorkerAttributesRepository.cs`**
```csharp
namespace RingGeneral.Data.Repositories.Interfaces
{
    public interface IWorkerAttributesRepository
    {
        // In-Ring
        WorkerInRingAttributes? GetInRingAttributes(int workerId);
        void SaveInRingAttributes(WorkerInRingAttributes attributes);
        void UpdateInRingAttribute(int workerId, string attributeName, int value);

        // Entertainment
        WorkerEntertainmentAttributes? GetEntertainmentAttributes(int workerId);
        void SaveEntertainmentAttributes(WorkerEntertainmentAttributes attributes);
        void UpdateEntertainmentAttribute(int workerId, string attributeName, int value);

        // Story
        WorkerStoryAttributes? GetStoryAttributes(int workerId);
        void SaveStoryAttributes(WorkerStoryAttributes attributes);
        void UpdateStoryAttribute(int workerId, string attributeName, int value);

        // Specializations
        List<WorkerSpecialization> GetSpecializations(int workerId);
        void AddSpecialization(WorkerSpecialization specialization);
        void RemoveSpecialization(int specializationId);
    }
}
```

**2. `/src/RingGeneral.Data/Repositories/WorkerAttributesRepository.cs`**
- Implémentation ADO.NET complète
- Requêtes SQL paramétrées
- Gestion des transactions

---

### ViewModels

#### Fichiers à Créer/Modifier

**1. `/src/RingGeneral.UI/ViewModels/Profile/ProfileMainViewModel.cs`** (NOUVEAU)
```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class ProfileMainViewModel : ViewModelBase
    {
        // Photo & Identity
        public string PhotoPath { get; }
        public string FullName { get; }
        public int Age { get; }
        public string Nationality { get; }

        // Contract & Role
        public string Role { get; } // Main Eventer, Upper Mid-Carder, etc.
        public decimal AnnualSalary { get; }
        public string ContractStatus { get; }

        // Physical
        public int Weight { get; }
        public int Height { get; }
        public string Handedness { get; }
        public int YearsExperience { get; }

        // Geography
        public string Birthplace { get; } // "West Newbury, USA"
        public string Residence { get; } // "Tampa, Floride, USA"

        // Quick Stats
        public int Condition { get; }
        public int Forme { get; }
        public int Fatigue { get; }
        public int Popularity { get; }

        // Specializations
        public ObservableCollection<string> Specializations { get; }

        // Commands
        public ReactiveCommand<Unit, Unit> ChangePhotoCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateAvatarCommand { get; }
    }
}
```

**2. `/src/RingGeneral.UI/ViewModels/Profile/AttributesTabViewModel.cs`** (REFONTE COMPLÈTE)
```csharp
namespace RingGeneral.UI.ViewModels.Profile
{
    public class AttributesTabViewModel : ViewModelBase
    {
        // IN-RING (10 attributs)
        public int InRingAvg { get; }
        public int Striking { get; set; }
        public int Grappling { get; set; }
        public int HighFlying { get; set; }
        public int Powerhouse { get; set; }
        public int Timing { get; set; }
        public int Selling { get; set; }
        public int Psychology { get; set; }
        public int Stamina { get; set; }
        public int Safety { get; set; }
        public int HardcoreBrawl { get; set; }

        // ENTERTAINMENT (10 attributs)
        public int EntertainmentAvg { get; }
        public int Charisma { get; set; }
        public int MicWork { get; set; }
        public int Acting { get; set; }
        public int CrowdConnection { get; set; }
        public int StarPower { get; set; }
        public int Improvisation { get; set; }
        public int Entrance { get; set; }
        public int SexAppeal { get; set; }
        public int MerchandiseAppeal { get; set; }
        public int CrossoverPotential { get; set; }

        // STORY (10 attributs)
        public int StoryAvg { get; }
        public int CharacterDepth { get; set; }
        public int Consistency { get; set; }
        public int HeelPerformance { get; set; }
        public int BabyfacePerformance { get; set; }
        public int StorytellingLongTerm { get; set; }
        public int EmotionalRange { get; set; }
        public int Adaptability { get; set; }
        public int RivalryChemistry { get; set; }
        public int CreativeInput { get; set; }
        public int MoralAlignment { get; set; }

        // Previous values for change indicators
        public Dictionary<string, int> PreviousValues { get; }

        public bool IsWorker { get; }
    }
}
```

**3. Mise à jour de `/src/RingGeneral.UI/ViewModels/Profile/ProfileViewModel.cs`**
- Ajout de `ProfileMainViewModel` comme propriété
- Intégration des 30 nouveaux attributs

---

### Views

#### Fichiers à Créer/Modifier

**1. `/src/RingGeneral.UI/Views/Profile/ProfileMainView.axaml`** (NOUVEAU)

Vue de la fiche personnage principale avec :
- Grid 2 colonnes (Photo | Infos)
- Affichage des 30 attributs en 3 colonnes
- Section historique des performances
- Barres de condition visuelles

**2. `/src/RingGeneral.UI/Views/Profile/AttributesTabView.axaml`** (REFONTE)

```xml
<ScrollViewer>
  <StackPanel Spacing="12" Margin="16">
    <!-- IN-RING -->
    <Expander Header="🏗️ IN-RING (Technique & Physique)" IsExpanded="True">
      <StackPanel Spacing="6" Margin="0,8,0,0">
        <TextBlock Classes="caption muted"
                   Text="{Binding InRingAvg, StringFormat='Moyenne: {0}/100'}"
                   FontWeight="SemiBold" Margin="0,0,0,8"/>

        <components:AttributeBar AttributeName="Striking"
                                 Value="{Binding Striking}"
                                 PreviousValue="{Binding PreviousValues[Striking]}"
                                 Description="Précision et impact des coups (poings, pieds, coudes)"/>
        <components:AttributeBar AttributeName="Grappling"
                                 Value="{Binding Grappling}"
                                 Description="Maîtrise des prises au sol et soumissions"/>
        <!-- ... 8 autres attributs ... -->
      </StackPanel>
    </Expander>

    <!-- ENTERTAINMENT -->
    <Expander Header="🎤 ENTERTAINMENT (Présence & Micro)" IsExpanded="True">
      <!-- ... 10 attributs ... -->
    </Expander>

    <!-- STORY -->
    <Expander Header="📖 STORY (Écriture & Personnage)" IsExpanded="True">
      <!-- ... 10 attributs ... -->
    </Expander>
  </StackPanel>
</ScrollViewer>
```

**3. Mise à jour de `/src/RingGeneral.UI/Views/Profile/ProfileView.axaml`**

Ajout de la section ProfileMainView AVANT le TabControl :

```xml
<Grid RowDefinitions="Auto,*">
  <!-- MAIN PROFILE (Fiche personnage) -->
  <ProfileMainView Grid.Row="0" DataContext="{Binding ProfileMain}"/>

  <!-- TABS -->
  <TabControl Grid.Row="1" SelectedIndex="{Binding SelectedTabIndex}">
    <TabItem Header="📊 ATTRIBUTS">
      <AttributesTabView DataContext="{Binding AttributesTab}"/>
    </TabItem>
    <!-- ... autres tabs ... -->
  </TabControl>
</Grid>
```

---

### Resources

#### Fichiers à Créer/Modifier

**1. `/src/RingGeneral.UI/Resources/AttributeDescriptions.fr.resx`** (MISE À JOUR)

Ajout de 30 nouvelles descriptions détaillées :

```xml
<!-- IN-RING -->
<data name="Striking" xml:space="preserve">
  <value>Précision et impact des coups portés (poings, pieds, coudes, genoux). Influence la crédibilité des phases de frappe dans les matchs.</value>
</data>
<data name="Grappling" xml:space="preserve">
  <value>Maîtrise des prises au sol, du mat wrestling et des soumissions. Détermine la qualité des séquences techniques.</value>
</data>
<!-- ... 28 autres descriptions ... -->
```

**Total** : 30 descriptions complètes en français

---

## 🗓️ PLAN D'IMPLÉMENTATION

### Phase 1 : Base de Données (2-3 jours)

**Agent Responsable** : **Systems Architect**

#### Tâche 1.1 : Migration Script
- [ ] Créer `Migration_v2_AttributesRework.sql`
- [ ] Créer 4 nouvelles tables
- [ ] Ajouter colonnes à `Workers`
- [ ] Script de migration des données existantes

#### Tâche 1.2 : Data Seeding
- [ ] Générer attributs pour les 50+ workers existants
- [ ] Assigner spécialisations réalistes
- [ ] Remplir géographie (ville, pays)
- [ ] Tester l'intégrité des données

**Livrables** :
- Migration SQL complète
- Script de seed pour 30 attributs × 50+ workers
- Tests de validation

**Durée** : 2-3 jours

---

### Phase 2 : Models & Repositories (3-4 jours)

**Agent Responsable** : **Systems Architect**

#### Tâche 2.1 : Models
- [ ] Créer `WorkerInRingAttributes.cs`
- [ ] Créer `WorkerEntertainmentAttributes.cs`
- [ ] Créer `WorkerStoryAttributes.cs`
- [ ] Créer `WorkerSpecialization.cs`
- [ ] Mettre à jour `Worker.cs` avec navigation properties

#### Tâche 2.2 : Repository
- [ ] Créer `IWorkerAttributesRepository.cs` (interface)
- [ ] Implémenter `WorkerAttributesRepository.cs` (ADO.NET)
- [ ] Méthodes CRUD pour chaque catégorie d'attributs
- [ ] Gestion des spécialisations
- [ ] Tests unitaires

#### Tâche 2.3 : Dependency Injection
- [ ] Enregistrer `IWorkerAttributesRepository` dans `App.axaml.cs`
- [ ] Tester la résolution DI

**Livrables** :
- 4 Models complets
- Repository fonctionnel avec tests
- DI configuré

**Durée** : 3-4 jours

---

### Phase 3 : ViewModels (4-5 jours)

**Agent Responsable** : **Systems Architect**

#### Tâche 3.1 : ProfileMainViewModel
- [ ] Créer classe avec toutes les propriétés
- [ ] Charger données depuis repository
- [ ] Implémenter commands (ChangePhoto, GenerateAvatar)
- [ ] Calculs dérivés (âge, années d'expérience)

#### Tâche 3.2 : AttributesTabViewModel (Refonte)
- [ ] Ajouter 30 propriétés d'attributs
- [ ] Calcul des moyennes (InRingAvg, EntertainmentAvg, StoryAvg)
- [ ] Système de tracking des changements (PreviousValues)
- [ ] Data binding bidirectionnel

#### Tâche 3.3 : Intégration dans ProfileViewModel
- [ ] Ajouter `ProfileMainViewModel` comme propriété
- [ ] Coordination entre ProfileMain et Tabs
- [ ] Navigation entre sections

**Livrables** :
- ProfileMainViewModel complet
- AttributesTabViewModel refactorisé (30 attributs)
- Intégration dans ProfileViewModel
- Tests de binding

**Durée** : 4-5 jours

---

### Phase 4 : Views & UI (5-7 jours)

**Agent Responsable** : **UI Specialist**

#### Tâche 4.1 : ProfileMainView
- [ ] Créer layout 2 colonnes (Photo | Infos)
- [ ] Section photo avec boutons (Changer, Générer Avatar)
- [ ] Affichage identité complète
- [ ] Barres de condition visuelles
- [ ] Section spécialisations avec badges
- [ ] Géographie (naissance + résidence)
- [ ] Section attributs en 3 colonnes

#### Tâche 4.2 : AttributesTabView (Refonte)
- [ ] 3 Expanders (IN-RING, ENTERTAINMENT, STORY)
- [ ] 10 AttributeBar par catégorie (30 total)
- [ ] Display des moyennes
- [ ] Binding vers AttributesTabViewModel
- [ ] Indicateurs de changement (↑↓)

#### Tâche 4.3 : Mise à jour de ProfileView
- [ ] Intégrer ProfileMainView en haut
- [ ] Ajuster layout global (Grid avec 2 rows)
- [ ] Tests de navigation

#### Tâche 4.4 : Styling
- [ ] Styles pour les badges de spécialisation
- [ ] Styles pour les barres de condition
- [ ] Couleurs par catégorie d'attributs
- [ ] Responsive design (largeurs min/max)

**Livrables** :
- ProfileMainView.axaml complet
- AttributesTabView.axaml refactorisé (30 AttributeBar)
- ProfileView.axaml mis à jour
- Styles harmonisés

**Durée** : 5-7 jours

---

### Phase 5 : Resources & Localisation (2 jours)

**Agent Responsable** : **Content Creator**

#### Tâche 5.1 : Descriptions d'Attributs
- [ ] Rédiger 30 descriptions détaillées en français
- [ ] Ajouter à `AttributeDescriptions.fr.resx`
- [ ] Validation linguistique
- [ ] Tooltips complets

#### Tâche 5.2 : Données de Seed
- [ ] Générer valeurs réalistes pour John Cena (exemple fourni)
- [ ] Générer valeurs pour 50+ workers
- [ ] Assigner spécialisations cohérentes
- [ ] Remplir géographie

**Livrables** :
- 30 descriptions en français
- Data seed pour 50+ workers
- Validation qualité

**Durée** : 2 jours

---

### Phase 6 : Integration & Tests (3-4 jours)

**Agent Responsable** : **Systems Architect + UI Specialist**

#### Tâche 6.1 : Tests Unitaires
- [ ] Tests repository (CRUD complet)
- [ ] Tests ViewModels (calculs de moyennes)
- [ ] Tests de binding

#### Tâche 6.2 : Tests d'Intégration
- [ ] Chargement complet d'un profil avec 30 attributs
- [ ] Modification d'attributs et persistance
- [ ] Navigation ProfileView → Tabs
- [ ] Performance (chargement < 500ms)

#### Tâche 6.3 : Tests UI
- [ ] Affichage correct de tous les attributs
- [ ] Tooltips fonctionnels
- [ ] Indicateurs de changement (↑↓)
- [ ] Responsive design

#### Tâche 6.4 : Migration des Données
- [ ] Migrer les attributs existants vers le nouveau système
- [ ] Validation de l'intégrité
- [ ] Backup avant migration

**Livrables** :
- Suite de tests complète
- Migration réussie
- Validation qualité

**Durée** : 3-4 jours

---

### Phase 7 : Nettoyage & Documentation (2 jours)

**Agent Responsable** : **File Cleaner**

#### Tâche 7.1 : Nettoyage du Code
- [ ] Vérifier tous les namespaces
- [ ] Supprimer les fichiers obsolètes
- [ ] Nettoyer les using inutilisés
- [ ] Organiser les dossiers

#### Tâche 7.2 : Documentation
- [ ] Documenter le nouveau système d'attributs
- [ ] Guide de migration pour les développeurs
- [ ] Update du CURRENT_STATE.md
- [ ] Update du PLAN_SPRINT_REVISE.md

**Livrables** :
- Code propre et organisé
- Documentation complète
- Guides mis à jour

**Durée** : 2 jours

---

## 📊 PLANNING GLOBAL

| Phase | Responsable | Durée | Dépendances |
|-------|-------------|-------|-------------|
| **Phase 1** : Base de Données | Systems Architect | 2-3 jours | - |
| **Phase 2** : Models & Repos | Systems Architect | 3-4 jours | Phase 1 |
| **Phase 3** : ViewModels | Systems Architect | 4-5 jours | Phase 2 |
| **Phase 4** : Views & UI | UI Specialist | 5-7 jours | Phase 3 |
| **Phase 5** : Resources | Content Creator | 2 jours | Parallèle à Phase 4 |
| **Phase 6** : Integration | Systems Arch + UI | 3-4 jours | Phases 4 & 5 |
| **Phase 7** : Nettoyage | File Cleaner | 2 jours | Phase 6 |

**Durée Totale Estimée** : **21-30 jours** (3-4.5 semaines)

**Avec Parallélisation** : Phase 5 en parallèle de Phase 4 → **~3.5 semaines**

---

## 🎯 CRITÈRES DE VALIDATION

### Critères Techniques

- [ ] 3 nouvelles tables créées et peuplées
- [ ] 4 nouveaux Models créés
- [ ] WorkerAttributesRepository complet et testé
- [ ] 30 attributs affichés correctement dans l'UI
- [ ] Moyennes calculées automatiquement
- [ ] Indicateurs de changement (↑↓) fonctionnels
- [ ] Page profil principale affichée avant les tabs
- [ ] Géographie complète (naissance + résidence)
- [ ] Spécialisations affichées avec badges
- [ ] Performance : chargement profil < 500ms

### Critères Fonctionnels

- [ ] Utilisateur peut voir les 30 attributs d'un worker
- [ ] Chaque attribut a une description tooltip
- [ ] Les moyennes sont affichées par catégorie
- [ ] La fiche personnage montre photo, identité, géo
- [ ] Les spécialisations sont visibles et éditables
- [ ] L'historique des performances s'affiche
- [ ] Migration des données existantes réussie

### Critères Qualité

- [ ] Tous les tests passent (unitaires + intégration)
- [ ] Aucune régression sur les fonctionnalités existantes
- [ ] Code respecte les conventions C# et MVVM
- [ ] Namespaces corrects
- [ ] Documentation complète

---

## 📝 FICHIERS IMPACTÉS

### Nouveaux Fichiers (23 fichiers)

**Base de Données** :
1. `/src/RingGeneral.Data/Migrations/Migration_v2_AttributesRework.sql`

**Models** (4) :
2. `/src/RingGeneral.Core/Models/Attributes/WorkerInRingAttributes.cs`
3. `/src/RingGeneral.Core/Models/Attributes/WorkerEntertainmentAttributes.cs`
4. `/src/RingGeneral.Core/Models/Attributes/WorkerStoryAttributes.cs`
5. `/src/RingGeneral.Core/Models/WorkerSpecialization.cs`

**Repositories** (2) :
6. `/src/RingGeneral.Data/Repositories/Interfaces/IWorkerAttributesRepository.cs`
7. `/src/RingGeneral.Data/Repositories/WorkerAttributesRepository.cs`

**ViewModels** (1) :
8. `/src/RingGeneral.UI/ViewModels/Profile/ProfileMainViewModel.cs`

**Views** (1) :
9. `/src/RingGeneral.UI/Views/Profile/ProfileMainView.axaml`
10. `/src/RingGeneral.UI/Views/Profile/ProfileMainView.axaml.cs`

**Tests** (6) :
11. `/tests/RingGeneral.Tests/Repositories/WorkerAttributesRepositoryTests.cs`
12. `/tests/RingGeneral.Tests/ViewModels/ProfileMainViewModelTests.cs`
13. `/tests/RingGeneral.Tests/ViewModels/AttributesTabViewModelTests.cs`
14. `/tests/RingGeneral.Tests/Integration/AttributesMigrationTests.cs`

**Documentation** (3) :
15. `/docs/ATTRIBUTES_SYSTEM_V2.md`
16. `/docs/MIGRATION_GUIDE_ATTRIBUTES.md`

**Data Seed** (2) :
17. `/src/RingGeneral.Data/Seed/WorkersAttributesSeed.sql`
18. `/src/RingGeneral.Data/Seed/WorkersSpecializationsSeed.sql`

### Fichiers Modifiés (8 fichiers)

1. `/src/RingGeneral.Core/Models/Worker.cs` - Ajout navigation properties
2. `/src/RingGeneral.UI/ViewModels/Profile/ProfileViewModel.cs` - Intégration ProfileMain
3. `/src/RingGeneral.UI/ViewModels/Profile/AttributesTabViewModel.cs` - Refonte complète
4. `/src/RingGeneral.UI/Views/Profile/ProfileView.axaml` - Ajout ProfileMainView
5. `/src/RingGeneral.UI/Views/Profile/AttributesTabView.axaml` - Refonte complète
6. `/src/RingGeneral.UI/Resources/AttributeDescriptions.fr.resx` - Ajout 30 descriptions
7. `/src/RingGeneral.UI/App.axaml.cs` - Enregistrement DI
8. `CURRENT_STATE.md` - Mise à jour état du projet

**Total** : 23 nouveaux fichiers + 8 fichiers modifiés = **31 fichiers**

---

## ⚠️ RISQUES ET MITIGATION

### Risque 1 : Migration des Données Existantes

**Impact** : Perte ou corruption des attributs existants

**Mitigation** :
- Backup complet de la DB avant migration
- Script de rollback
- Tests de migration sur copie de la DB
- Validation de l'intégrité post-migration

### Risque 2 : Performance

**Impact** : Ralentissement du chargement des profils (30 attributs au lieu de 13)

**Mitigation** :
- Utiliser des colonnes calculées (GENERATED) pour les moyennes
- Indexer les clés étrangères
- Charger les attributs en lazy loading si nécessaire
- Tests de performance < 500ms

### Risque 3 : Complexité UI

**Impact** : UI surchargée, difficile à lire avec 30 attributs

**Mitigation** :
- Utiliser Expanders (collapsibles)
- Grouper par catégorie claire
- Couleurs distinctes par section
- Tooltips pour éviter surcharge visuelle

### Risque 4 : Régression

**Impact** : Casser des fonctionnalités existantes

**Mitigation** :
- Tests de non-régression complets
- Branching (feature branch)
- Code review avant merge
- Compilation réussie obligatoire

---

## 🚀 PROCHAINES ÉTAPES APRÈS REWORK

Une fois ce rework complété, débloquer :

1. **Simulation avancée** : Utiliser les 30 attributs pour des matchs plus réalistes
2. **Scouting amélioré** : Recherche par spécialisation
3. **Booking intelligent** : Suggestions basées sur Chemistry et Specializations
4. **Progression détaillée** : Évolution des 30 attributs séparément
5. **Analytics** : Graphiques d'évolution par catégorie

---

## ✅ CHECKLIST DE DÉMARRAGE

Avant de commencer :

- [ ] Créer la branche `claude/rework-performance-attributes-YBXRx`
- [ ] Backup complet de la base de données
- [ ] Valider que Sprint 1 (Composants UI) est terminé
- [ ] S'assurer que `AttributeBar` component est fonctionnel
- [ ] Lire ce plan avec l'équipe
- [ ] Assigner les phases aux sous-agents
- [ ] Configurer l'environnement de test

---

## 📞 COORDINATION DES SOUS-AGENTS

### Workflow

```
Phase 1-2 : Systems Architect
    ↓
Phase 3 : Systems Architect
    ↓
Phase 4 : UI Specialist ←→ Phase 5 : Content Creator (Parallèle)
    ↓
Phase 6 : Systems Architect + UI Specialist
    ↓
Phase 7 : File Cleaner
```

### Communication

- Daily sync entre Systems Architect et UI Specialist (Phases 4-6)
- Review par Chef de Projet à la fin de chaque phase
- Tests de compilation après chaque merge
- Documentation continue

---

**Version** : 1.0
**Auteur** : Chef de Projet DevOps (Claude)
**Date de création** : 7 janvier 2026
**Statut** : ✅ PRÊT POUR APPROBATION

---

**Prochaine Action** : Attendre validation de ce plan avant de démarrer Phase 1.
