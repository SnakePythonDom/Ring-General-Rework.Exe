# 🎯 Feature Plan : Refonte des Attributs de Performance

**Date** : 7 janvier 2026
**Type** : Rework majeur (Architecture + UI)
**Profil** : Feature Builder
**Priorité** : 🔴 HAUTE (Impact sur toute la simulation)

---

## 📋 Vue d'ensemble

### Objectif
Remplacer le système d'attributs actuel (3 attributs simples) par un système granulaire de **30 attributs** répartis en 3 catégories, permettant une simulation plus réaliste et nuancée des performances.

### Justification
- ✅ **Profondeur** : Les attributs actuels (InRing, Entertainment, Story) sont trop simplistes
- ✅ **Simulation** : Impossible de différencier un "Brawler" d'un "High-Flyer"
- ✅ **Réalisme** : Les fans de wrestling attendent cette granularité
- ✅ **Modding** : Permet aux moddeurs d'affiner leurs rosters

### Impact Estimé
- 🟡 **Database** : Migration majeure (30 nouvelles colonnes)
- 🟡 **Core Models** : Refonte des records Worker
- 🟡 **Repositories** : Adaptation des requêtes SQL
- 🔴 **Simulation** : Réécriture complète des formules de calcul
- 🔴 **UI** : Nouvelle page ProfileView avec affichage des 30 attributs
- 🟢 **Tests** : Nouveaux tests unitaires requis

---

## 🏗️ PHASE 1 : ANALYSE DE LA STRUCTURE

### 1.1 État Actuel Identifié

#### Table Workers (SQLite)
```sql
CREATE TABLE Workers (
    WorkerId TEXT PRIMARY KEY,
    FullName TEXT,
    CompanyId TEXT,
    InRing INTEGER,        -- ❌ À remplacer par 10 attributs
    Entertainment INTEGER, -- ❌ À remplacer par 10 attributs
    Story INTEGER,         -- ❌ À remplacer par 10 attributs
    Popularity INTEGER,
    Fatigue INTEGER,
    Morale INTEGER,
    TvRole TEXT
);
```

#### Modèles Core Impactés
- `WorkerSnapshot` (DomainModels.cs) - ligne 23-34
- Tous les repositories utilisant ces attributs
- `ShowSimulationEngine` - formules de calcul
- `WorkerService` - extraction des données

#### UI/ViewModels Impactés
- `RosterViewModel` - affichage des workers
- `WorkerDetailViewModel` - détail du profil (existe mais basique)
- `AttributesTabViewModel` - **À créer** (nouveau)

### 1.2 Patterns Identifiés

✅ **Architecture Clean** : Core → Data → UI
✅ **Records immutables** : Tous les modèles sont des `sealed record`
✅ **Repository Pattern** : Héritage de `RepositoryBase`
✅ **ReactiveUI** : Bindings MVVM avec `RaiseAndSetIfChanged`
✅ **Transactions SQL** : Utilisation de `WithTransaction`
✅ **Nommage français** : Propriétés et méthodes en français

---

## 📐 PHASE 2 : PLAN D'IMPLÉMENTATION

### Étape 2.1 : Migration de la Base de Données (Jour 1)

#### Fichier à modifier : `/src/RingGeneral.Data/Database/DbMigrations.cs`

**Action** : Créer une migration pour ajouter 30 nouvelles colonnes

```csharp
public static class PerformanceAttributesMigration
{
    public static void MigrateToGranularAttributes(SqliteConnection connexion)
    {
        using var transaction = connexion.BeginTransaction();

        try
        {
            // 1. Ajouter les 30 nouvelles colonnes (avec valeurs par défaut = 50)
            AjouterAttributsInRing(connexion);
            AjouterAttributsEntertainment(connexion);
            AjouterAttributsStory(connexion);

            // 2. Migrer les données existantes (répartir les scores)
            MigrerDonneesExistantes(connexion);

            // 3. Optionnel : Supprimer les anciennes colonnes (ou les garder pour rollback)
            // SupprimerAnciennesColonnes(connexion);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void AjouterAttributsInRing(SqliteConnection connexion)
    {
        var attributs = new[]
        {
            "Striking", "Grappling", "HighFlying", "Powerhouse", "Timing",
            "Selling", "Psychology", "Stamina", "Safety", "HardcoreBrawl"
        };

        foreach (var attr in attributs)
        {
            using var cmd = connexion.CreateCommand();
            cmd.CommandText = $"ALTER TABLE Workers ADD COLUMN {attr} INTEGER DEFAULT 50;";
            cmd.ExecuteNonQuery();
        }
    }

    private static void AjouterAttributsEntertainment(SqliteConnection connexion)
    {
        var attributs = new[]
        {
            "Charisma", "MicWork", "Acting", "CrowdConnection", "StarPower",
            "Improvisation", "Entrance", "SexAppeal", "MerchandiseAppeal", "CrossoverPotential"
        };

        foreach (var attr in attributs)
        {
            using var cmd = connexion.CreateCommand();
            cmd.CommandText = $"ALTER TABLE Workers ADD COLUMN {attr} INTEGER DEFAULT 50;";
            cmd.ExecuteNonQuery();
        }
    }

    private static void AjouterAttributsStory(SqliteConnection connexion)
    {
        var attributs = new[]
        {
            "CharacterDepth", "Consistency", "HeelPerformance", "BabyfacePerformance",
            "StorytellingLongTerm", "EmotionalRange", "Adaptability",
            "RivalryChemistry", "CreativeInput", "MoralAlignment"
        };

        foreach (var attr in attributs)
        {
            using var cmd = connexion.CreateCommand();
            cmd.CommandText = $"ALTER TABLE Workers ADD COLUMN {attr} INTEGER DEFAULT 50;";
            cmd.ExecuteNonQuery();
        }
    }

    private static void MigrerDonneesExistantes(SqliteConnection connexion)
    {
        // Stratégie : Répartir le score global sur les 10 attributs de chaque catégorie
        // Exemple : InRing=85 → Tous les attributs IN-RING autour de 85 (avec variance +/- 10)

        using var cmd = connexion.CreateCommand();
        cmd.CommandText = @"
            UPDATE Workers
            SET
                -- IN-RING : Copier le score InRing avec légère variance
                Striking = InRing + (RANDOM() % 20 - 10),
                Grappling = InRing + (RANDOM() % 20 - 10),
                HighFlying = InRing + (RANDOM() % 30 - 15), -- Plus de variance (spécialisation)
                Powerhouse = InRing + (RANDOM() % 30 - 15),
                Timing = InRing + (RANDOM() % 10 - 5),
                Selling = InRing + (RANDOM() % 10 - 5),
                Psychology = InRing + (RANDOM() % 10 - 5),
                Stamina = InRing + (RANDOM() % 15 - 7),
                Safety = InRing + (RANDOM() % 10 - 5),
                HardcoreBrawl = InRing + (RANDOM() % 30 - 15),

                -- ENTERTAINMENT : Copier le score Entertainment
                Charisma = Entertainment + (RANDOM() % 15 - 7),
                MicWork = Entertainment + (RANDOM() % 15 - 7),
                Acting = Entertainment + (RANDOM() % 15 - 7),
                CrowdConnection = Entertainment + (RANDOM() % 10 - 5),
                StarPower = Entertainment + (RANDOM() % 20 - 10),
                Improvisation = Entertainment + (RANDOM() % 15 - 7),
                Entrance = Entertainment + (RANDOM() % 10 - 5),
                SexAppeal = Entertainment + (RANDOM() % 20 - 10),
                MerchandiseAppeal = Entertainment + (RANDOM() % 20 - 10),
                CrossoverPotential = Entertainment + (RANDOM() % 25 - 12),

                -- STORY : Copier le score Story
                CharacterDepth = Story + (RANDOM() % 15 - 7),
                Consistency = Story + (RANDOM() % 10 - 5),
                HeelPerformance = Story + (RANDOM() % 20 - 10),
                BabyfacePerformance = Story + (RANDOM() % 20 - 10),
                StorytellingLongTerm = Story + (RANDOM() % 10 - 5),
                EmotionalRange = Story + (RANDOM() % 15 - 7),
                Adaptability = Story + (RANDOM() % 15 - 7),
                RivalryChemistry = Story + (RANDOM() % 10 - 5),
                CreativeInput = Story + (RANDOM() % 20 - 10),
                MoralAlignment = 50 + (RANDOM() % 40 - 20); -- Neutre par défaut
        ";
        cmd.ExecuteNonQuery();

        // Contraindre les valeurs entre 1 et 100
        using var clampCmd = connexion.CreateCommand();
        clampCmd.CommandText = @"
            UPDATE Workers
            SET
                Striking = MIN(100, MAX(1, Striking)),
                Grappling = MIN(100, MAX(1, Grappling)),
                HighFlying = MIN(100, MAX(1, HighFlying)),
                Powerhouse = MIN(100, MAX(1, Powerhouse)),
                Timing = MIN(100, MAX(1, Timing)),
                Selling = MIN(100, MAX(1, Selling)),
                Psychology = MIN(100, MAX(1, Psychology)),
                Stamina = MIN(100, MAX(1, Stamina)),
                Safety = MIN(100, MAX(1, Safety)),
                HardcoreBrawl = MIN(100, MAX(1, HardcoreBrawl)),

                Charisma = MIN(100, MAX(1, Charisma)),
                MicWork = MIN(100, MAX(1, MicWork)),
                Acting = MIN(100, MAX(1, Acting)),
                CrowdConnection = MIN(100, MAX(1, CrowdConnection)),
                StarPower = MIN(100, MAX(1, StarPower)),
                Improvisation = MIN(100, MAX(1, Improvisation)),
                Entrance = MIN(100, MAX(1, Entrance)),
                SexAppeal = MIN(100, MAX(1, SexAppeal)),
                MerchandiseAppeal = MIN(100, MAX(1, MerchandiseAppeal)),
                CrossoverPotential = MIN(100, MAX(1, CrossoverPotential)),

                CharacterDepth = MIN(100, MAX(1, CharacterDepth)),
                Consistency = MIN(100, MAX(1, Consistency)),
                HeelPerformance = MIN(100, MAX(1, HeelPerformance)),
                BabyfacePerformance = MIN(100, MAX(1, BabyfacePerformance)),
                StorytellingLongTerm = MIN(100, MAX(1, StorytellingLongTerm)),
                EmotionalRange = MIN(100, MAX(1, EmotionalRange)),
                Adaptability = MIN(100, MAX(1, Adaptability)),
                RivalryChemistry = MIN(100, MAX(1, RivalryChemistry)),
                CreativeInput = MIN(100, MAX(1, CreativeInput)),
                MoralAlignment = MIN(100, MAX(1, MoralAlignment));
        ";
        clampCmd.ExecuteNonQuery();
    }
}
```

**Test de Migration** :
```csharp
// tests/RingGeneral.Tests/Migrations/PerformanceAttributesMigrationTests.cs
public class PerformanceAttributesMigrationTests
{
    [Fact]
    public void Migration_QuandExecutee_DoitAjouter30NouvellesColonnes()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_migration_{Guid.NewGuid():N}.db");
        var factory = new SqliteConnectionFactory($"Data Source={dbPath}");

        // Créer une DB avec l'ancien schéma
        CreerAncienSchema(factory);

        // Act
        using (var conn = factory.CreateConnection())
        {
            PerformanceAttributesMigration.MigrateToGranularAttributes(conn);
        }

        // Assert
        using (var conn = factory.CreateConnection())
        {
            var colonnes = ListerColonnes(conn, "Workers");
            Assert.Contains("Striking", colonnes);
            Assert.Contains("Charisma", colonnes);
            Assert.Contains("CharacterDepth", colonnes);
            // ... vérifier les 30 colonnes
        }
    }
}
```

---

### Étape 2.2 : Mise à Jour des Modèles Core (Jour 1-2)

#### Fichier : `/src/RingGeneral.Core/Models/PerformanceAttributes.cs` (NOUVEAU)

Créer un modèle dédié pour les attributs de performance :

```csharp
namespace RingGeneral.Core.Models;

/// <summary>
/// Attributs de performance IN-RING (Technique & Physique)
/// </summary>
public sealed record InRingAttributes(
    int Striking,           // Précision et impact des coups (poings, pieds, coudes)
    int Grappling,          // Maîtrise des prises au sol et des soumissions
    int HighFlying,         // Agilité, acrobaties et prises aériennes
    int Powerhouse,         // Capacité à soulever des adversaires lourds et force brute
    int Timing,             // Précision chirurgicale dans l'enchaînement des mouvements
    int Selling,            // Capacité à rendre les coups de l'adversaire crédibles
    int Psychology,         // Savoir construire un match pour raconter une histoire logique
    int Stamina,            // Endurance pour maintenir un rythme élevé sur 30+ minutes
    int Safety,             // Capacité à protéger son partenaire de ring (limite les blessures)
    int HardcoreBrawl)      // Utilisation d'objets et combat de rue
{
    /// <summary>
    /// Calcule la moyenne des attributs IN-RING (0-100)
    /// </summary>
    public int Moyenne => (Striking + Grappling + HighFlying + Powerhouse + Timing +
                           Selling + Psychology + Stamina + Safety + HardcoreBrawl) / 10;
}

/// <summary>
/// Attributs de performance ENTERTAINMENT (Présence & Micro)
/// </summary>
public sealed record EntertainmentAttributes(
    int Charisma,           // Magnétisme naturel, même sans parler
    int MicWork,            // Aisance verbale et capacité à délivrer un script
    int Acting,             // Crédibilité dans les expressions faciales et les segments backstage
    int CrowdConnection,    // Capacité à faire réagir la foule (Heat ou Cheers)
    int StarPower,          // L'aura de "Main Eventer", le look et la prestance
    int Improvisation,      // Capacité à réagir aux imprévus ou aux chants du public
    int Entrance,           // Impact visuel et théâtralité de l'arrivée vers le ring
    int SexAppeal,          // Attrait esthétique ou facteur "cool"
    int MerchandiseAppeal,  // Potentiel de vente de produits dérivés (design, logos)
    int CrossoverPotential) // Capacité à attirer un public hors-catch (Cinéma, TV)
{
    /// <summary>
    /// Calcule la moyenne des attributs ENTERTAINMENT (0-100)
    /// </summary>
    public int Moyenne => (Charisma + MicWork + Acting + CrowdConnection + StarPower +
                           Improvisation + Entrance + SexAppeal + MerchandiseAppeal + CrossoverPotential) / 10;
}

/// <summary>
/// Attributs de performance STORY (Écriture & Personnage)
/// </summary>
public sealed record StoryAttributes(
    int CharacterDepth,         // Complexité et nuances du personnage (pas juste "gentil" ou "méchant")
    int Consistency,            // Fidélité au personnage sur le long terme
    int HeelPerformance,        // Efficacité dans le rôle de l'antagoniste
    int BabyfacePerformance,    // Efficacité dans le rôle du héros
    int StorytellingLongTerm,   // Capacité à porter une rivalité sur plusieurs mois
    int EmotionalRange,         // Capacité à générer de la tristesse, de la peur, de la joie
    int Adaptability,           // Facilité à changer de gimmick ou à évoluer
    int RivalryChemistry,       // Capacité naturelle à créer une étincelle avec n'importe quel adversaire
    int CreativeInput,          // L'implication du catcheur dans ses propres idées de storylines
    int MoralAlignment)         // Capacité à jouer les "Tweener" (zone grise morale)
{
    /// <summary>
    /// Calcule la moyenne des attributs STORY (0-100)
    /// </summary>
    public int Moyenne => (CharacterDepth + Consistency + HeelPerformance + BabyfacePerformance +
                           StorytellingLongTerm + EmotionalRange + Adaptability +
                           RivalryChemistry + CreativeInput + MoralAlignment) / 10;
}

/// <summary>
/// Agrégat complet des attributs de performance d'un worker
/// </summary>
public sealed record PerformanceAttributes(
    InRingAttributes InRing,
    EntertainmentAttributes Entertainment,
    StoryAttributes Story)
{
    /// <summary>
    /// Score IN-RING global (moyenne des 10 attributs)
    /// </summary>
    public int InRingGlobal => InRing.Moyenne;

    /// <summary>
    /// Score ENTERTAINMENT global (moyenne des 10 attributs)
    /// </summary>
    public int EntertainmentGlobal => Entertainment.Moyenne;

    /// <summary>
    /// Score STORY global (moyenne des 10 attributs)
    /// </summary>
    public int StoryGlobal => Story.Moyenne;

    /// <summary>
    /// Note globale du worker (moyenne des 3 catégories)
    /// </summary>
    public int NoteGlobale => (InRingGlobal + EntertainmentGlobal + StoryGlobal) / 3;
}
```

#### Fichier : `/src/RingGeneral.Core/Models/DomainModels.cs` (MODIFIER)

Mettre à jour `WorkerSnapshot` :

```csharp
public sealed record WorkerSnapshot(
    string WorkerId,
    string NomComplet,

    // ✅ NOUVEAU : Attributs granulaires
    PerformanceAttributes Attributs,

    // ⚠️ DEPRECATED : Garder pour compatibilité (calculés à partir des attributs)
    [Obsolete("Utiliser Attributs.InRingGlobal")]
    int InRing,
    [Obsolete("Utiliser Attributs.EntertainmentGlobal")]
    int Entertainment,
    [Obsolete("Utiliser Attributs.StoryGlobal")]
    int Story,

    // Autres propriétés inchangées
    int Popularite,
    int Fatigue,
    string Blessure,
    int Momentum,
    string RoleTv,
    int Morale);
```

---

### Étape 2.3 : Mise à Jour des Repositories (Jour 2)

#### Fichier : `/src/RingGeneral.Data/Repositories/WorkerRepository.cs` (MODIFIER)

Ajouter des méthodes pour charger les attributs granulaires :

```csharp
public PerformanceAttributes ChargerAttributsPerformance(string workerId)
{
    using var connexion = OpenConnection();
    using var commande = connexion.CreateCommand();

    commande.CommandText = @"
        SELECT
            -- IN-RING
            Striking, Grappling, HighFlying, Powerhouse, Timing,
            Selling, Psychology, Stamina, Safety, HardcoreBrawl,
            -- ENTERTAINMENT
            Charisma, MicWork, Acting, CrowdConnection, StarPower,
            Improvisation, Entrance, SexAppeal, MerchandiseAppeal, CrossoverPotential,
            -- STORY
            CharacterDepth, Consistency, HeelPerformance, BabyfacePerformance,
            StorytellingLongTerm, EmotionalRange, Adaptability,
            RivalryChemistry, CreativeInput, MoralAlignment
        FROM Workers
        WHERE WorkerId = $workerId;
    ";

    AjouterParametre(commande, "$workerId", workerId);

    using var reader = commande.ExecuteReader();

    if (!reader.Read())
        throw new InvalidOperationException($"Worker {workerId} introuvable");

    var inRing = new InRingAttributes(
        Striking: reader.GetInt32(0),
        Grappling: reader.GetInt32(1),
        HighFlying: reader.GetInt32(2),
        Powerhouse: reader.GetInt32(3),
        Timing: reader.GetInt32(4),
        Selling: reader.GetInt32(5),
        Psychology: reader.GetInt32(6),
        Stamina: reader.GetInt32(7),
        Safety: reader.GetInt32(8),
        HardcoreBrawl: reader.GetInt32(9)
    );

    var entertainment = new EntertainmentAttributes(
        Charisma: reader.GetInt32(10),
        MicWork: reader.GetInt32(11),
        Acting: reader.GetInt32(12),
        CrowdConnection: reader.GetInt32(13),
        StarPower: reader.GetInt32(14),
        Improvisation: reader.GetInt32(15),
        Entrance: reader.GetInt32(16),
        SexAppeal: reader.GetInt32(17),
        MerchandiseAppeal: reader.GetInt32(18),
        CrossoverPotential: reader.GetInt32(19)
    );

    var story = new StoryAttributes(
        CharacterDepth: reader.GetInt32(20),
        Consistency: reader.GetInt32(21),
        HeelPerformance: reader.GetInt32(22),
        BabyfacePerformance: reader.GetInt32(23),
        StorytellingLongTerm: reader.GetInt32(24),
        EmotionalRange: reader.GetInt32(25),
        Adaptability: reader.GetInt32(26),
        RivalryChemistry: reader.GetInt32(27),
        CreativeInput: reader.GetInt32(28),
        MoralAlignment: reader.GetInt32(29)
    );

    return new PerformanceAttributes(inRing, entertainment, story);
}
```

---

### Étape 2.4 : Nouvelle Page ProfileView (Jours 3-4)

#### Fichier : `/src/RingGeneral.UI/ViewModels/Profile/ProfileAttributesViewModel.cs` (NOUVEAU)

```csharp
namespace RingGeneral.UI.ViewModels.Profile;

public sealed class ProfileAttributesViewModel : ViewModelBase
{
    private readonly WorkerRepository _workerRepository;
    private readonly string _workerId;

    // HEADER : Fiche personnage
    public string PhotoPath { get; }
    public string NomComplet { get; }
    public int Age { get; }
    public string Role { get; } // Main Eventer, Upper Mid-Carder, etc.
    public string Contrat { get; } // "3,500,000 € / an"
    public string Style { get; } // "Brawler / Powerhouse"
    public string Moral { get; } // "Excellent"

    // Barres de condition
    public int ConditionPhysique { get; }
    public int Forme { get; }
    public int Fatigue { get; }
    public int Popularite { get; }

    // IN-RING (10 attributs)
    public int Striking { get; private set; }
    public int Grappling { get; private set; }
    public int HighFlying { get; private set; }
    public int Powerhouse { get; private set; }
    public int Timing { get; private set; }
    public int Selling { get; private set; }
    public int Psychology { get; private set; }
    public int Stamina { get; private set; }
    public int Safety { get; private set; }
    public int HardcoreBrawl { get; private set; }
    public int InRingMoyenne { get; private set; }

    // ENTERTAINMENT (10 attributs)
    public int Charisma { get; private set; }
    public int MicWork { get; private set; }
    public int Acting { get; private set; }
    public int CrowdConnection { get; private set; }
    public int StarPower { get; private set; }
    public int Improvisation { get; private set; }
    public int Entrance { get; private set; }
    public int SexAppeal { get; private set; }
    public int MerchandiseAppeal { get; private set; }
    public int CrossoverPotential { get; private set; }
    public int EntertainmentMoyenne { get; private set; }

    // STORY (10 attributs)
    public int CharacterDepth { get; private set; }
    public int Consistency { get; private set; }
    public int HeelPerformance { get; private set; }
    public int BabyfacePerformance { get; private set; }
    public int StorytellingLongTerm { get; private set; }
    public int EmotionalRange { get; private set; }
    public int Adaptability { get; private set; }
    public int RivalryChemistry { get; private set; }
    public int CreativeInput { get; private set; }
    public int MoralAlignment { get; private set; }
    public int StoryMoyenne { get; private set; }

    public ProfileAttributesViewModel(WorkerRepository workerRepository, string workerId)
    {
        _workerRepository = workerRepository;
        _workerId = workerId;

        ChargerAttributs();
    }

    private void ChargerAttributs()
    {
        var attributs = _workerRepository.ChargerAttributsPerformance(_workerId);

        // IN-RING
        Striking = attributs.InRing.Striking;
        Grappling = attributs.InRing.Grappling;
        HighFlying = attributs.InRing.HighFlying;
        Powerhouse = attributs.InRing.Powerhouse;
        Timing = attributs.InRing.Timing;
        Selling = attributs.InRing.Selling;
        Psychology = attributs.InRing.Psychology;
        Stamina = attributs.InRing.Stamina;
        Safety = attributs.InRing.Safety;
        HardcoreBrawl = attributs.InRing.HardcoreBrawl;
        InRingMoyenne = attributs.InRingGlobal;

        // ENTERTAINMENT
        Charisma = attributs.Entertainment.Charisma;
        MicWork = attributs.Entertainment.MicWork;
        Acting = attributs.Entertainment.Acting;
        CrowdConnection = attributs.Entertainment.CrowdConnection;
        StarPower = attributs.Entertainment.StarPower;
        Improvisation = attributs.Entertainment.Improvisation;
        Entrance = attributs.Entertainment.Entrance;
        SexAppeal = attributs.Entertainment.SexAppeal;
        MerchandiseAppeal = attributs.Entertainment.MerchandiseAppeal;
        CrossoverPotential = attributs.Entertainment.CrossoverPotential;
        EntertainmentMoyenne = attributs.EntertainmentGlobal;

        // STORY
        CharacterDepth = attributs.Story.CharacterDepth;
        Consistency = attributs.Story.Consistency;
        HeelPerformance = attributs.Story.HeelPerformance;
        BabyfacePerformance = attributs.Story.BabyfacePerformance;
        StorytellingLongTerm = attributs.Story.StorytellingLongTerm;
        EmotionalRange = attributs.Story.EmotionalRange;
        Adaptability = attributs.Story.Adaptability;
        RivalryChemistry = attributs.Story.RivalryChemistry;
        CreativeInput = attributs.Story.CreativeInput;
        MoralAlignment = attributs.Story.MoralAlignment;
        StoryMoyenne = attributs.StoryGlobal;
    }
}
```

#### Fichier : `/src/RingGeneral.UI/Views/Profile/ProfileAttributesView.axaml` (NOUVEAU)

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:components="using:RingGeneral.UI.Components"
             x:Class="RingGeneral.UI.Views.Profile.ProfileAttributesView">

    <ScrollViewer>
        <StackPanel Spacing="16" Margin="16">

            <!-- HEADER : Fiche Personnage (comme dans votre mockup) -->
            <Border Classes="panel" Padding="16">
                <Grid RowDefinitions="Auto,Auto,Auto,Auto">

                    <!-- Ligne 1 : Photo + Infos de base -->
                    <Grid Grid.Row="0" ColumnDefinitions="Auto,*">
                        <!-- Photo -->
                        <Border Grid.Column="0" Width="100" Height="100"
                                CornerRadius="8" ClipToBounds="True"
                                Margin="0,0,16,0">
                            <Image Source="{Binding PhotoPath}" Stretch="UniformToFill"/>
                        </Border>

                        <!-- Infos -->
                        <StackPanel Grid.Column="1" VerticalAlignment="Center">
                            <TextBlock Classes="h2" Text="{Binding NomComplet}"/>
                            <Grid ColumnDefinitions="Auto,*,Auto,*" Margin="0,8,0,0">
                                <TextBlock Grid.Column="0" Classes="caption" Text="Rôle :"/>
                                <TextBlock Grid.Column="1" Classes="body" Text="{Binding Role}" Margin="8,0"/>
                                <TextBlock Grid.Column="2" Classes="caption" Text="Contrat :"/>
                                <TextBlock Grid.Column="3" Classes="body" Text="{Binding Contrat}" Margin="8,0"/>
                            </Grid>
                            <Grid ColumnDefinitions="Auto,*,Auto,*,Auto,*" Margin="0,4,0,0">
                                <TextBlock Grid.Column="0" Classes="caption" Text="Style :"/>
                                <TextBlock Grid.Column="1" Classes="body" Text="{Binding Style}" Margin="8,0"/>
                                <TextBlock Grid.Column="2" Classes="caption" Text="Moral :"/>
                                <TextBlock Grid.Column="3" Classes="body" Text="{Binding Moral}" Margin="8,0"/>
                                <TextBlock Grid.Column="4" Classes="caption" Text="Âge :"/>
                                <TextBlock Grid.Column="5" Classes="body" Text="{Binding Age}" Margin="8,0"/>
                            </Grid>
                        </StackPanel>
                    </Grid>

                    <!-- Ligne 2 : Barres de condition -->
                    <Grid Grid.Row="1" ColumnDefinitions="*,*,*,*" Margin="0,16,0,0">
                        <StackPanel Grid.Column="0" Margin="0,0,8,0">
                            <TextBlock Classes="caption" Text="Condition"/>
                            <components:AttributeBar Value="{Binding ConditionPhysique}" MaxValue="100" ShowLabel="False"/>
                        </StackPanel>
                        <StackPanel Grid.Column="1" Margin="8,0">
                            <TextBlock Classes="caption" Text="Forme"/>
                            <components:AttributeBar Value="{Binding Forme}" MaxValue="100" ShowLabel="False"/>
                        </StackPanel>
                        <StackPanel Grid.Column="2" Margin="8,0">
                            <TextBlock Classes="caption" Text="Fatigue"/>
                            <components:AttributeBar Value="{Binding Fatigue}" MaxValue="100" ShowLabel="False"/>
                        </StackPanel>
                        <StackPanel Grid.Column="3" Margin="8,0,0,0">
                            <TextBlock Classes="caption" Text="Pop"/>
                            <components:AttributeBar Value="{Binding Popularite}" MaxValue="100" ShowLabel="False"/>
                        </StackPanel>
                    </Grid>
                </Grid>
            </Border>

            <!-- 3 COLONNES D'ATTRIBUTS -->
            <Grid ColumnDefinitions="*,*,*" ColumnSpacing="16">

                <!-- Colonne 1 : IN-RING -->
                <Border Grid.Column="0" Classes="panel" Padding="12">
                    <StackPanel Spacing="8">
                        <TextBlock Classes="h3" Text="{Binding InRingMoyenne, StringFormat='IN-RING (Moy: {0})'}"
                                   HorizontalAlignment="Center" Margin="0,0,0,8"/>
                        <components:AttributeBar AttributeName="Striking" Value="{Binding Striking}"/>
                        <components:AttributeBar AttributeName="Grappling" Value="{Binding Grappling}"/>
                        <components:AttributeBar AttributeName="High-Flying" Value="{Binding HighFlying}"/>
                        <components:AttributeBar AttributeName="Force Brute" Value="{Binding Powerhouse}"/>
                        <components:AttributeBar AttributeName="Timing" Value="{Binding Timing}"/>
                        <components:AttributeBar AttributeName="Selling" Value="{Binding Selling}"/>
                        <components:AttributeBar AttributeName="Psychologie" Value="{Binding Psychology}"/>
                        <components:AttributeBar AttributeName="Stamina" Value="{Binding Stamina}"/>
                        <components:AttributeBar AttributeName="Sécurité" Value="{Binding Safety}"/>
                        <components:AttributeBar AttributeName="Hardcore/Brawl" Value="{Binding HardcoreBrawl}"/>
                    </StackPanel>
                </Border>

                <!-- Colonne 2 : ENTERTAINMENT -->
                <Border Grid.Column="1" Classes="panel" Padding="12">
                    <StackPanel Spacing="8">
                        <TextBlock Classes="h3" Text="{Binding EntertainmentMoyenne, StringFormat='ENTERTAINMENT (Moy: {0})'}"
                                   HorizontalAlignment="Center" Margin="0,0,0,8"/>
                        <components:AttributeBar AttributeName="Charisme" Value="{Binding Charisma}"/>
                        <components:AttributeBar AttributeName="Mic Work" Value="{Binding MicWork}"/>
                        <components:AttributeBar AttributeName="Acting" Value="{Binding Acting}"/>
                        <components:AttributeBar AttributeName="Connexion Crowd" Value="{Binding CrowdConnection}"/>
                        <components:AttributeBar AttributeName="Star Power" Value="{Binding StarPower}"/>
                        <components:AttributeBar AttributeName="Improvisation" Value="{Binding Improvisation}"/>
                        <components:AttributeBar AttributeName="Entrée" Value="{Binding Entrance}"/>
                        <components:AttributeBar AttributeName="Sex Appeal" Value="{Binding SexAppeal}"/>
                        <components:AttributeBar AttributeName="Merchandising" Value="{Binding MerchandiseAppeal}"/>
                        <components:AttributeBar AttributeName="Aura" Value="{Binding CrossoverPotential}"/>
                    </StackPanel>
                </Border>

                <!-- Colonne 3 : STORY -->
                <Border Grid.Column="2" Classes="panel" Padding="12">
                    <StackPanel Spacing="8">
                        <TextBlock Classes="h3" Text="{Binding StoryMoyenne, StringFormat='STORY (Moy: {0})'}"
                                   HorizontalAlignment="Center" Margin="0,0,0,8"/>
                        <components:AttributeBar AttributeName="Prof. Perso" Value="{Binding CharacterDepth}"/>
                        <components:AttributeBar AttributeName="Cohérence" Value="{Binding Consistency}"/>
                        <components:AttributeBar AttributeName="Perf. Heel" Value="{Binding HeelPerformance}"/>
                        <components:AttributeBar AttributeName="Perf. Face" Value="{Binding BabyfacePerformance}"/>
                        <components:AttributeBar AttributeName="Storytelling" Value="{Binding StorytellingLongTerm}"/>
                        <components:AttributeBar AttributeName="Émotion" Value="{Binding EmotionalRange}"/>
                        <components:AttributeBar AttributeName="Adaptabilité" Value="{Binding Adaptability}"/>
                        <components:AttributeBar AttributeName="Alchimie" Value="{Binding RivalryChemistry}"/>
                        <components:AttributeBar AttributeName="Vision Créative" Value="{Binding CreativeInput}"/>
                        <components:AttributeBar AttributeName="Nuances (Moral)" Value="{Binding MoralAlignment}"/>
                    </StackPanel>
                </Border>
            </Grid>

            <!-- HISTORIQUE DES PERFORMANCES (Optionnel) -->
            <Expander Header="▾ HISTORIQUE DES PERFORMANCES" IsExpanded="False">
                <StackPanel Spacing="4" Margin="0,8,0,0">
                    <TextBlock Classes="caption" Text="[RAW] vs Randy Orton ⭐⭐⭐⭐½ (92)"/>
                    <TextBlock Classes="caption" Text="[SD!] vs AJ Styles ⭐⭐⭐⭐⭐ (97)"/>
                    <TextBlock Classes="caption" Text="[PPV] vs Kevin Owens ⭐⭐⭐⭐ (85)"/>
                </StackPanel>
            </Expander>

        </StackPanel>
    </ScrollViewer>
</UserControl>
```

---

### Étape 2.5 : Mise à Jour des Descriptions d'Attributs (Jour 4)

#### Fichier : `/src/RingGeneral.UI/Resources/AttributeDescriptions.fr.resx` (MODIFIER)

Ajouter les 30 nouvelles descriptions :

```xml
<!-- IN-RING -->
<data name="Striking" xml:space="preserve">
    <value>Précision et impact des coups (poings, pieds, coudes, genoux). Un score élevé signifie des frappes réalistes et percutantes.</value>
</data>
<data name="Grappling" xml:space="preserve">
    <value>Maîtrise des prises au sol, des soumissions et du catch technique. Important pour les matchs de style "wrestling pur".</value>
</data>
<data name="HighFlying" xml:space="preserve">
    <value>Agilité, acrobaties et prises aériennes. Essentiel pour les cruiserweights et les high-flyers spectaculaires.</value>
</data>
<data name="Powerhouse" xml:space="preserve">
    <value>Capacité à soulever des adversaires lourds et force brute. Clé pour les power moves (chokeslams, powerbombs, etc.).</value>
</data>
<data name="Timing" xml:space="preserve">
    <value>Précision chirurgicale dans l'enchaînement des mouvements. Un bon timing évite les spots maladroits et fluidifie le match.</value>
</data>
<data name="Selling" xml:space="preserve">
    <value>Capacité à rendre les coups de l'adversaire crédibles en vendant la douleur. Un art souvent sous-estimé mais crucial.</value>
</data>
<data name="Psychology" xml:space="preserve">
    <value>Savoir construire un match pour raconter une histoire logique. Quand ralentir, quand accélérer, quand faire le comeback.</value>
</data>
<data name="Stamina" xml:space="preserve">
    <value>Endurance pour maintenir un rythme élevé sur 30+ minutes sans être essoufflé. Vital pour les main events longs.</value>
</data>
<data name="Safety" xml:space="preserve">
    <value>Capacité à protéger son partenaire de ring et à limiter les blessures. La sécurité avant tout dans le catch.</value>
</data>
<data name="HardcoreBrawl" xml:space="preserve">
    <value>Utilisation d'objets (chaises, tables, échelles) et combat de rue. Pour les matchs hardcore, no-DQ et street fights.</value>
</data>

<!-- ENTERTAINMENT -->
<data name="Charisma" xml:space="preserve">
    <value>Magnétisme naturel et présence scénique, même sans parler. Certains wrestlers captent l'attention dès qu'ils apparaissent.</value>
</data>
<data name="MicWork" xml:space="preserve">
    <value>Aisance verbale et capacité à délivrer des promos mémorables. Le micro est l'arme la plus puissante du catch.</value>
</data>
<data name="Acting" xml:space="preserve">
    <value>Crédibilité dans les expressions faciales et les segments backstage. Transforme un script en moment mémorable.</value>
</data>
<data name="CrowdConnection" xml:space="preserve">
    <value>Capacité à faire réagir la foule (heat pour les heels, pops pour les faces). Le vrai baromètre du succès d'un wrestler.</value>
</data>
<data name="StarPower" xml:space="preserve">
    <value>L'aura de "Main Eventer", le look et la prestance. Ce facteur X qui sépare les stars des bons workers.</value>
</data>
<data name="Improvisation" xml:space="preserve">
    <value>Capacité à réagir aux imprévus ou aux chants du public. Parfois le meilleur moment d'une promo n'était pas scripté.</value>
</data>
<data name="Entrance" xml:space="preserve">
    <value>Impact visuel et théâtralité de l'arrivée vers le ring. Une grande entrance peut créer une star (ex: The Undertaker).</value>
</data>
<data name="SexAppeal" xml:space="preserve">
    <value>Attrait esthétique ou facteur "cool". Pas seulement physique, c'est aussi le swagger et l'attitude.</value>
</data>
<data name="MerchandiseAppeal" xml:space="preserve">
    <value>Potentiel de vente de produits dérivés (t-shirts, masques, logos). Un gimmick vendable génère des millions.</value>
</data>
<data name="CrossoverPotential" xml:space="preserve">
    <value>Capacité à attirer un public hors-catch (cinéma, TV, mainstream). The Rock et John Cena en sont les exemples parfaits.</value>
</data>

<!-- STORY -->
<data name="CharacterDepth" xml:space="preserve">
    <value>Complexité et nuances du personnage. Les meilleurs gimmicks ne sont pas juste "gentil" ou "méchant", ils ont des layers.</value>
</data>
<data name="Consistency" xml:space="preserve">
    <value>Fidélité au personnage sur le long terme. Un worker cohérent ne casse jamais son caractère sans raison.</value>
</data>
<data name="HeelPerformance" xml:space="preserve">
    <value>Efficacité dans le rôle de l'antagoniste (heel). Générer de la heat authentique est un art difficile.</value>
</data>
<data name="BabyfacePerformance" xml:space="preserve">
    <value>Efficacité dans le rôle du héros (babyface). Faire aimer le public sans être fade demande du talent.</value>
</data>
<data name="StorytellingLongTerm" xml:space="preserve">
    <value>Capacité à porter une rivalité sur plusieurs mois avec cohérence. Les meilleures feuds se construisent lentement.</value>
</data>
<data name="EmotionalRange" xml:space="preserve">
    <value>Capacité à générer des émotions variées : tristesse, peur, joie, rage. Un wrestler complet doit tout maîtriser.</value>
</data>
<data name="Adaptability" xml:space="preserve">
    <value>Facilité à changer de gimmick ou à évoluer. Certains wrestlers sont figés dans un rôle, d'autres sont caméléons.</value>
</data>
<data name="RivalryChemistry" xml:space="preserve">
    <value>Capacité naturelle à créer une étincelle avec n'importe quel adversaire. Certaines paires de wrestlers ont une alchimie magique.</value>
</data>
<data name="CreativeInput" xml:space="preserve">
    <value>L'implication du wrestler dans ses propres idées de storylines. Les meilleurs angles viennent souvent des workers eux-mêmes.</value>
</data>
<data name="MoralAlignment" xml:space="preserve">
    <value>Capacité à jouer les "Tweener" (zone grise morale). Certains wrestlers excellent dans l'ambiguïté (ex: Stone Cold, CM Punk).</value>
</data>
```

---

### Étape 2.6 : Mise à Jour du Simulateur (Jour 5)

#### Fichier : `/src/RingGeneral.Core/Simulation/ShowSimulationEngine.cs` (MODIFIER)

Adapter les formules de calcul pour utiliser les attributs granulaires :

```csharp
private int CalculerQualiteMatch(SegmentDefinition segment, ShowContext context)
{
    var participants = segment.Participants
        .Select(id => context.Workers.First(w => w.WorkerId == id))
        .ToList();

    // AVANT (simplifié) :
    // int moyenneInRing = participants.Average(w => w.InRing);

    // APRÈS (granulaire selon le type de match) :
    int qualiteInRing = segment.TypeSegment switch
    {
        "Striking Match" => CalculerQualiteStriking(participants),
        "Technical Match" => CalculerQualiteTechnical(participants),
        "High-Flying Spot" => CalculerQualiteHighFlying(participants),
        "Hardcore Match" => CalculerQualiteHardcore(participants),
        "Powerhouse Brawl" => CalculerQualitePowerhouse(participants),
        _ => CalculerQualiteStandard(participants)
    };

    int qualiteEntertainment = (int)participants.Average(w => w.Attributs.EntertainmentGlobal);
    int qualiteStory = segment.StorylineId != null
        ? (int)participants.Average(w => w.Attributs.StoryGlobal)
        : 0;

    return (qualiteInRing * 50 + qualiteEntertainment * 30 + qualiteStory * 20) / 100;
}

private int CalculerQualiteStriking(List<WorkerSnapshot> workers)
{
    // Moyenne pondérée : Striking (60%) + Timing (20%) + Selling (20%)
    return workers.Average(w =>
        w.Attributs.InRing.Striking * 0.6 +
        w.Attributs.InRing.Timing * 0.2 +
        w.Attributs.InRing.Selling * 0.2
    );
}

private int CalculerQualiteTechnical(List<WorkerSnapshot> workers)
{
    // Grappling (70%) + Psychology (20%) + Stamina (10%)
    return workers.Average(w =>
        w.Attributs.InRing.Grappling * 0.7 +
        w.Attributs.InRing.Psychology * 0.2 +
        w.Attributs.InRing.Stamina * 0.1
    );
}

private int CalculerQualiteHighFlying(List<WorkerSnapshot> workers)
{
    // HighFlying (80%) + Safety (15%) + Timing (5%)
    return workers.Average(w =>
        w.Attributs.InRing.HighFlying * 0.8 +
        w.Attributs.InRing.Safety * 0.15 +
        w.Attributs.InRing.Timing * 0.05
    );
}

private int CalculerQualiteHardcore(List<WorkerSnapshot> workers)
{
    // HardcoreBrawl (70%) + Safety (20%) + Selling (10%)
    return workers.Average(w =>
        w.Attributs.InRing.HardcoreBrawl * 0.7 +
        w.Attributs.InRing.Safety * 0.2 +
        w.Attributs.InRing.Selling * 0.1
    );
}

private int CalculerQualitePowerhouse(List<WorkerSnapshot> workers)
{
    // Powerhouse (60%) + Safety (25%) + Selling (15%)
    return workers.Average(w =>
        w.Attributs.InRing.Powerhouse * 0.6 +
        w.Attributs.InRing.Safety * 0.25 +
        w.Attributs.InRing.Selling * 0.15
    );
}

private int CalculerQualiteStandard(List<WorkerSnapshot> workers)
{
    // Moyenne équilibrée de tous les attributs IN-RING
    return workers.Average(w => w.Attributs.InRingGlobal);
}
```

---

## 🧪 PHASE 3 : TESTS

### Test 3.1 : Migration de Base de Données

```csharp
[Fact]
public void Migration_QuandExecutee_DoitPreserverLesScoresGlobaux()
{
    // Arrange
    var dbPath = CreateTestDatabase();
    var factory = new SqliteConnectionFactory($"Data Source={dbPath}");

    // Créer un worker avec InRing=85, Entertainment=92, Story=88
    using (var conn = factory.CreateConnection())
    {
        SeedOldSchemaWorker(conn, "W_TEST", inRing: 85, entertainment: 92, story: 88);
    }

    // Act : Exécuter la migration
    using (var conn = factory.CreateConnection())
    {
        PerformanceAttributesMigration.MigrateToGranularAttributes(conn);
    }

    // Assert : Vérifier que les moyennes sont cohérentes
    using (var conn = factory.CreateConnection())
    {
        var repo = new WorkerRepository(factory);
        var attributs = repo.ChargerAttributsPerformance("W_TEST");

        // Les moyennes doivent être proches des scores originaux (± 5 à cause de la variance)
        Assert.InRange(attributs.InRingGlobal, 80, 90);
        Assert.InRange(attributs.EntertainmentGlobal, 87, 97);
        Assert.InRange(attributs.StoryGlobal, 83, 93);
    }
}
```

### Test 3.2 : Affichage ProfileView

```csharp
[Fact]
public void ProfileAttributesViewModel_QuandCharge_DoitAfficherLes30Attributs()
{
    // Arrange
    var dbPath = CreateTestDatabase();
    var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
    var repo = new WorkerRepository(factory);

    // Créer un worker de test avec migration
    using (var conn = factory.CreateConnection())
    {
        PerformanceAttributesMigration.MigrateToGranularAttributes(conn);
        SeedWorker(conn, "W_CENA", "John Cena");
    }

    // Act
    var viewModel = new ProfileAttributesViewModel(repo, "W_CENA");

    // Assert : Vérifier que les 30 attributs sont chargés
    Assert.InRange(viewModel.Striking, 1, 100);
    Assert.InRange(viewModel.Charisma, 1, 100);
    Assert.InRange(viewModel.CharacterDepth, 1, 100);

    // Vérifier que les moyennes sont calculées
    Assert.True(viewModel.InRingMoyenne > 0);
    Assert.True(viewModel.EntertainmentMoyenne > 0);
    Assert.True(viewModel.StoryMoyenne > 0);
}
```

### Test 3.3 : Simulation avec Attributs Granulaires

```csharp
[Fact]
public void ShowSimulation_AvecAttributsGranulaires_DoitDonnerResultatsPlusPrecis()
{
    // Arrange
    var context = CreateShowContext();
    var engine = new ShowSimulationEngine();

    // Créer un segment Striking Match avec 2 strikers excellents
    var segment = new SegmentDefinition(
        SegmentId: "SEG_1",
        TypeSegment: "Striking Match",
        Participants: new[] { "W_NAKAMURA", "W_CESARO" },
        DureeMinutes: 15,
        EstMainEvent: false,
        StorylineId: null,
        TitreId: null,
        Intensite: 80,
        VainqueurId: "W_NAKAMURA",
        PerdantId: "W_CESARO"
    );

    // Act
    var resultat = engine.SimulerSegment(segment, context);

    // Assert : Un match de strikers doit avoir une meilleure note
    Assert.True(resultat.Note >= 80); // Attendu : note élevée pour un match adapté aux forces des workers
}
```

---

## 📅 PLANNING D'EXÉCUTION

| Jour | Tâche | Livrables | Statut |
|------|-------|-----------|--------|
| **J1** | Migration DB + Modèles Core | DbMigrations.cs, PerformanceAttributes.cs | ⏳ À faire |
| **J2** | Repositories + Tests Migration | WorkerRepository.cs, tests migration | ⏳ À faire |
| **J3** | ProfileAttributesViewModel | ViewModel avec 30 propriétés | ⏳ À faire |
| **J4** | ProfileAttributesView + Descriptions | View AXAML + 30 descriptions .resx | ⏳ À faire |
| **J5** | Simulation + Tests E2E | ShowSimulationEngine.cs, tests | ⏳ À faire |

**Durée totale** : 5 jours (1 semaine)

---

## 🎯 CRITÈRES DE SUCCÈS

### Critères Fonctionnels
- [ ] Migration s'exécute sans erreur sur une DB existante
- [ ] Les 30 attributs sont visibles dans ProfileView
- [ ] Les descriptions s'affichent en tooltip
- [ ] Les moyennes sont calculées correctement
- [ ] La simulation utilise les attributs granulaires
- [ ] Les anciens scores (InRing, Entertainment, Story) sont préservés pour compatibilité

### Critères Techniques
- [ ] Aucune régression sur les tests existants
- [ ] Tous les nouveaux tests passent (minimum 10 tests)
- [ ] Pas de fuite mémoire (test avec 1000 workers)
- [ ] Performance acceptable (chargement < 200ms pour un profil)

### Critères UX
- [ ] L'UI reste responsive même avec 30 barres d'attributs
- [ ] Les tooltips sont informatifs et en français
- [ ] Le layout est cohérent avec le mockup fourni
- [ ] Les couleurs de barres sont lisibles (rouge/orange/vert)

---

## ⚠️ RISQUES & MITIGATIONS

### Risque 1 : Migration échoue sur DB production
**Mitigation** :
- Créer un backup automatique avant migration
- Tester la migration sur une copie de BAKI1.1.db
- Rollback automatique en cas d'erreur

### Risque 2 : Performance dégradée (30 colonnes vs 3)
**Mitigation** :
- Indexer les colonnes les plus utilisées
- Charger les attributs uniquement quand nécessaire (lazy loading)
- Utiliser des projections SQL pour ne charger que ce qui est affiché

### Risque 3 : Simulation devient trop complexe
**Mitigation** :
- Commencer par des formules simples (moyennes pondérées)
- Raffiner progressivement avec des tests A/B
- Permettre de désactiver la granularité (mode "legacy")

---

## 🔄 IMPACTS SUR LES DOCUMENTS EXISTANTS

### PLAN_SPRINT_REVISE.md
**Section à modifier** : Sprint 2 - ProfileView Universel

**Avant** :
```markdown
#### Tâche 2.1 : AttributesTabViewModel
- Afficher InRing, Entertainment, Story (3 attributs)
```

**Après** :
```markdown
#### Tâche 2.1 : ProfileAttributesViewModel
- Afficher 30 attributs granulaires (10 IN-RING + 10 ENTERTAINMENT + 10 STORY)
- Ajouter la page profile en main page avec mockup John Cena
- Calcul des moyennes par catégorie
```

### PLAN_IMPLEMENTATION_TECHNIQUE.md
**Section à ajouter** : Phase 1 - Tâche 1.3.5 (nouveau)

```markdown
#### Tâche 1.3.5 : Refonte des Attributs de Performance (5 jours) 🔴 CRITIQUE

**Objectif** : Remplacer les 3 attributs simples par 30 attributs granulaires

**Fichiers modifiés** :
- Migration : `/src/RingGeneral.Data/Database/DbMigrations.cs`
- Modèles : `/src/RingGeneral.Core/Models/PerformanceAttributes.cs`
- Repository : `/src/RingGeneral.Data/Repositories/WorkerRepository.cs`
- Simulation : `/src/RingGeneral.Core/Simulation/ShowSimulationEngine.cs`
- UI : `/src/RingGeneral.UI/Views/Profile/ProfileAttributesView.axaml`

**Livrables** :
- ✅ 30 attributs en base de données
- ✅ Migration des données existantes
- ✅ ProfileView avec affichage des 30 attributs
- ✅ Simulation adaptée aux attributs granulaires
- ✅ Tests complets (migration + simulation + UI)
```

---

## 📚 DOCUMENTATION COMPLÉMENTAIRE

### Noms des Attributs (Français)
Pour les bindings UI, utiliser les noms français :

| Attribut (Code) | Nom Français (UI) |
|-----------------|-------------------|
| Striking | Striking |
| Grappling | Grappling |
| HighFlying | High-Flying |
| Powerhouse | Force Brute |
| Timing | Timing |
| Selling | Selling |
| Psychology | Psychologie |
| Stamina | Stamina |
| Safety | Sécurité |
| HardcoreBrawl | Hardcore/Brawl |
| Charisma | Charisme |
| MicWork | Mic Work |
| Acting | Acting |
| CrowdConnection | Connexion Crowd |
| StarPower | Star Power |
| Improvisation | Improvisation |
| Entrance | Entrée |
| SexAppeal | Sex Appeal |
| MerchandiseAppeal | Merchandising |
| CrossoverPotential | Aura |
| CharacterDepth | Prof. Perso |
| Consistency | Cohérence |
| HeelPerformance | Perf. Heel |
| BabyfacePerformance | Perf. Face |
| StorytellingLongTerm | Storytelling |
| EmotionalRange | Émotion |
| Adaptability | Adaptabilité |
| RivalryChemistry | Alchimie |
| CreativeInput | Vision Créative |
| MoralAlignment | Nuances (Moral) |

---

## ✅ CHECKLIST AVANT DE COMMENCER

- [ ] Créer un backup de la base de données actuelle
- [ ] Créer une branche Git : `feature/performance-attributes-rework`
- [ ] Lire ce document en entier
- [ ] Confirmer que Sprint 1 (composants UI) est terminé
- [ ] Vérifier que AttributeBar.axaml est fonctionnel
- [ ] Préparer les données de test (copie de BAKI1.1.db)

---

## 🚀 PROCHAINE ACTION

**Démarrer Jour 1** : Migration de la base de données

1. Créer le fichier `/src/RingGeneral.Data/Database/DbMigrations.cs`
2. Implémenter `PerformanceAttributesMigration.MigrateToGranularAttributes()`
3. Exécuter la migration sur une DB de test
4. Valider que les 30 colonnes sont ajoutées
5. Vérifier que les données migrées ont des scores cohérents

**Commande pour démarrer** :
```bash
git checkout -b feature/performance-attributes-rework
dotnet build RingGeneral.sln
dotnet test tests/RingGeneral.Tests/RingGeneral.Tests.csproj
```

---

**Prêt à démarrer ce rework majeur ? 🎯**
