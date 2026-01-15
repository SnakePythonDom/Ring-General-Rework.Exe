# 🏢 Company Hub - Plan d'Implémentation Complet

**Branche** : `claude/company-hub-governance-lPkmH`
**Date** : 2026-01-08
**Architecte** : Claude (Lead Software Architect & Fullstack Engineer)

---

## 📋 TABLE DES MATIÈRES

1. [Vue d'Ensemble](#vue-densemble)
2. [Architecture Technique](#architecture-technique)
3. [Phase 1 : Infrastructure DB & Modèles](#phase-1--infrastructure-db--modèles-) ✅
4. [Phase 2 : Flux de Création Sécurisé](#phase-2--flux-de-création-sécurisé-) ⚠️
5. [Phase 3 : Company Hub UI](#phase-3--company-hub-ui-) 🔜
6. [Tests & Validation](#tests--validation-)
7. [Fichiers Modifiés/Créés](#fichiers-modifiéscréés)

---

## 🎯 VUE D'ENSEMBLE

### Objectifs

Implémenter un système complet de gestion des compagnies incluant :

1. **Identité de la Compagnie** : Pays, Année, Taille, Era, Style de Catch
2. **Gouvernance** : Owner (décideur stratégique) + Booker (directeur créatif)
3. **Syst\u00e8me de Mémoire du Booker** : Tracking des événements et biais
4. **Interface Company Hub** : Navigation multi-tabs pour gérer sa compagnie ET observer les rivales
5. **Flux de Création Sécurisé** : Éviter les erreurs `null` et garantir la compatibilité ascendante

### Problème Critique Identifié ❌

Le flux de création de partie **ne générait jamais d'Owner ni de Booker**, causant :
- Données orphelines dans la DB
- Navigation vers OwnerBookerViewModel plantée (null references)
- Système de gouvernance inaccessible

---

## 🏗️ ARCHITECTURE TECHNIQUE

### Stack Technologique

- **UI** : Avalonia 11.0.6 + ReactiveUI (MVVM pattern)
- **DB** : SQLite avec migrations versionnées
- **Pattern** : Service-based navigation + Repository Pattern
- **DI** : Microsoft.Extensions.DependencyInjection

### Diagramme d'Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         COMPANY HUB SYSTEM                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │   IDENTITY   │    │  GOVERNANCE  │    │  CATCH STYLE │      │
│  ├──────────────┤    ├──────────────┤    ├──────────────┤      │
│  │ FoundedYear  │    │    Owner     │    │ WrestlingPurity│    │
│  │ CompanySize  │    │   Booker     │    │ Entertainment │     │
│  │ CurrentEra   │    │ BookerMemory │    │ Hardcore      │     │
│  │ CountryId    │    │ Employment   │    │ LuchaInfluence│     │
│  └──────────────┘    └──────────────┘    └──────────────┘      │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐     │
│  │              COMPANY HUB VIEW (Multi-Tabs)              │     │
│  ├──────┬──────┬──────┬──────┬────────┬────────────────┐  │     │
│  │Profile│Staff│Roster│Teams │History │   Rivals       │  │     │
│  └──────┴──────┴──────┴──────┴────────┴────────────────┘  │     │
│                                                              │     │
└──────────────────────────────────────────────────────────────┘
```

---

## ✅ PHASE 1 : INFRASTRUCTURE DB & MODÈLES

### 1.1 Migrations SQL Créées

#### `004_owner_booker_governance.sql`
**Localisation** : `/data/migrations/004_owner_booker_governance.sql`

**Tables créées** :
- `Owners` (OwnerId, CompanyId, VisionType, RiskTolerance, PreferredProductType, etc.)
- `Bookers` (BookerId, CompanyId, CreativityScore, LogicScore, PreferredStyle, etc.)
- `BookerMemory` (MemoryId, EventType, ImpactScore, RecallStrength, WorkerId)
- `BookerEmploymentHistory` (HistoryId, StartDate, EndDate, PerformanceScore)

**Caractéristiques** :
- ✅ Contraintes CHECK pour validation des énums
- ✅ Index sur CompanyId, EmploymentStatus, EventType, EventDate
- ✅ Foreign Keys avec CASCADE DELETE
- ✅ Données de référence en commentaires (exemples d'INSERT)

#### `005_company_identity.sql`
**Localisation** : `/data/migrations/005_company_identity.sql`

**Modifications Companies** :
```sql
ALTER TABLE Companies ADD COLUMN FoundedYear INTEGER DEFAULT 2024;
ALTER TABLE Companies ADD COLUMN CompanySize TEXT DEFAULT 'Local';
ALTER TABLE Companies ADD COLUMN CurrentEra TEXT DEFAULT 'Foundation Era';
ALTER TABLE Companies ADD COLUMN CatchStyleId TEXT;
ALTER TABLE Companies ADD COLUMN IsPlayerControlled INTEGER DEFAULT 0;
ALTER TABLE Companies ADD COLUMN MonthlyBurnRate REAL DEFAULT 0.0;
```

**Nouvelles tables** :
- `CompanyEras` (historique des eras, similaire à WWE Attitude Era)
- `CompanyMilestones` (jalons : FirstShow, FirstTitle, First100kAudience, etc.)

**Vues créées** :
- `vw_CompanyGovernance` : JOIN Company + Owner + Booker actif
- `vw_RivalCompanies` : Toutes les compagnies sauf celle du joueur

**Triggers** :
- `trg_create_initial_era` : Crée automatiquement "Foundation Era" lors de la création d'une Company

**Migration de données** :
```sql
UPDATE Companies
SET FoundedYear = 2024, CompanySize = 'Local', CurrentEra = 'Foundation Era'
WHERE FoundedYear IS NULL;

-- Ajustement auto de la taille selon le Reach
UPDATE Companies SET CompanySize = CASE
    WHEN Reach >= 10000000 THEN 'Global'
    WHEN Reach >= 1000000 THEN 'International'
    ...
END;
```

#### `006_catch_styles.sql`
**Localisation** : `/data/migrations/006_catch_styles.sql`

**Table CatchStyles** :
```sql
CREATE TABLE CatchStyles (
    CatchStyleId TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    WrestlingPurity INTEGER,
    EntertainmentFocus INTEGER,
    HardcoreIntensity INTEGER,
    LuchaInfluence INTEGER,
    StrongStyleInfluence INTEGER,
    FanExpectationMatchQuality INTEGER,
    FanExpectationStorylines INTEGER,
    FanExpectationPromos INTEGER,
    FanExpectationSpectacle INTEGER,
    MatchRatingMultiplier REAL,
    PromoRatingMultiplier REAL,
    IconName TEXT,
    AccentColor TEXT
);
```

**Styles prédéfinis** (8 styles) :
1. **Pure Wrestling** 🥋 : Workrate technique, peu de storylines (1.3x match rating)
2. **Sports Entertainment** 🎭 : Équilibre wrestling/spectacle (1.2x promo rating)
3. **Hardcore Wrestling** 💀 : Violence extrême, spots dangereux
4. **Lucha Libre** 🎪 : High-flying, masques, tradition mexicaine
5. **Strong Style** ⚔️ : Puroresu japonais, fighting spirit
6. **Hybrid Wrestling** 🌐 : Mix équilibré (DEFAULT)
7. **Family-Friendly** 👨‍👩‍👧‍👦 : Tous publics, heroes vs villains
8. **Indie Wrestling** 💎 : Innovation, passion, petit budget

**Table CompanyStyleEvolution** : Tracking des changements de style (avec raison et FanReactionScore)

**Migration de données** :
```sql
UPDATE Companies SET CatchStyleId = 'STYLE_HYBRID' WHERE CatchStyleId IS NULL;
```

### 1.2 Modèles C# Mis à Jour

#### `DomainModels.cs`
**Localisation** : `/src/RingGeneral.Core/Models/DomainModels.cs`

**Records ajoutés** :

```csharp
// CompanyState enrichi avec 8 nouveaux champs
public sealed record CompanyState(
    string CompagnieId,
    string Nom,
    string Region,
    int Prestige,
    double Tresorerie,
    int AudienceMoyenne,
    int Reach,
    int FoundedYear = 2024,
    string CompanySize = "Local",
    string CurrentEra = "Foundation Era",
    string? CatchStyleId = null,
    bool IsPlayerControlled = false,
    double MonthlyBurnRate = 0.0,
    string? OwnerId = null,
    string? BookerId = null);

// Nouveaux records
public sealed record CatchStyle(...);
public sealed record CompanyEra(...);
public sealed record CompanyMilestone(...);
public sealed record OwnerSnapshot(...);
public sealed record BookerSnapshot(...);
public sealed record BookerMemoryEntry(...);
public sealed record CompanyGovernanceView(...); // Vue combinée
public sealed record CompanyMainStar(...);       // Top workers
```

### 1.3 Repositories Créés

#### `ICatchStyleRepository.cs` + `CatchStyleRepository.cs`
**Localisation** : `/src/RingGeneral.Data/Repositories/`

**Méthodes** :
```csharp
Task<IReadOnlyList<CatchStyle>> GetAllActiveStylesAsync();
Task<CatchStyle?> GetStyleByIdAsync(string styleId);
Task<IReadOnlyList<CatchStyle>> GetCompatibleStylesAsync(string preferredProductType);
double CalculateStyleMatchBonus(CatchStyle style, int workrate, int entertainment, int hardcore);
```

**Logique de compatibilité** :
- `Technical` → Pure Wrestling, Strong Style, Hybrid, Indie
- `Entertainment` → Sports Entertainment, Hybrid, Family-Friendly, Lucha
- `Hardcore` → Hardcore, Strong Style, Indie
- `Family-Friendly` → Family-Friendly, Lucha, Sports Entertainment

**Calcul de bonus** :
```
Alignment = 1.0 - |StylePurity - MatchWorkrate| / 100.0
Multiplier = 0.8 + (AverageAlignment * 0.5)  // Range: 0.8x à 1.3x
```

### 1.4 Intégration DI

#### `RepositoryFactory.cs`
**Modifications** :
- Ajout `IOwnerRepository`, `IBookerRepository`, `ICatchStyleRepository` au `RepositoryContainer`
- Instanciation dans `CreateRepositories()` avec `factory.GetConnectionString()`

#### `SqliteConnectionFactory.cs`
**Ajout** :
```csharp
public string GetConnectionString() => _connectionString;
```

#### `App.axaml.cs`
**Ajout** :
```csharp
// Company Governance & Identity
services.AddSingleton(repositories.OwnerRepository);
services.AddSingleton(repositories.BookerRepository);
services.AddSingleton(repositories.CatchStyleRepository);
```

---

## ⚠️ PHASE 2 : FLUX DE CRÉATION SÉCURISÉ

### État Actuel

Le fichier `CreateCompanyViewModel.cs` **N'A PAS ENCORE ÉTÉ MODIFIÉ**.

### Modifications Requises

#### 2.1 Injecter les Repositories

**Avant** :
```csharp
public CreateCompanyViewModel(
    GameRepository? repository = null,
    INavigationService? navigationService = null)
```

**Après** :
```csharp
private readonly IOwnerRepository _ownerRepository;
private readonly IBookerRepository _bookerRepository;
private readonly ICatchStyleRepository _catchStyleRepository;
private CatchStyle? _selectedCatchStyle;
private int _foundedYear = 2024;

public CreateCompanyViewModel(
    GameRepository? repository = null,
    INavigationService? navigationService = null,
    IOwnerRepository? ownerRepository = null,
    IBookerRepository? bookerRepository = null,
    ICatchStyleRepository? catchStyleRepository = null)
{
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
    _bookerRepository = bookerRepository ?? throw new ArgumentNullException(nameof(bookerRepository));
    _catchStyleRepository = catchStyleRepository ?? throw new ArgumentNullException(nameof(catchStyleRepository));

    AvailableRegions = new ObservableCollection<RegionInfo>();
    AvailableCatchStyles = new ObservableCollection<CatchStyle>(); // NOUVEAU
    LoadRegionsFromDatabase();
    LoadCatchStylesFromDatabase(); // NOUVEAU

    CreateCompanyCommand = ReactiveCommand.Create(CreateCompany);
    CancelCommand = ReactiveCommand.Create(Cancel);
}
```

#### 2.2 Charger les CatchStyles

```csharp
private async void LoadCatchStylesFromDatabase()
{
    try
    {
        var styles = await _catchStyleRepository.GetAllActiveStylesAsync();
        foreach (var style in styles)
        {
            AvailableCatchStyles.Add(style);
        }

        // Sélectionner "Hybrid" par défaut (le plus équilibré)
        SelectedCatchStyle = AvailableCatchStyles.FirstOrDefault(s => s.CatchStyleId == "STYLE_HYBRID")
                          ?? AvailableCatchStyles.FirstOrDefault();
    }
    catch (Exception ex)
    {
        Logger.Error($"Erreur chargement styles: {ex.Message}");
        // Créer style par défaut en fallback
        var defaultStyle = new CatchStyle(
            "STYLE_HYBRID", "Hybrid Wrestling", "Style équilibré",
            60, 60, 20, 30, 30, // Characteristics
            65, 65, 60, 65,     // Expectations
            1.0, 1.0, "🌐", "#607D8B", true);
        AvailableCatchStyles.Add(defaultStyle);
        SelectedCatchStyle = defaultStyle;
    }
}
```

#### 2.3 Modifier CreateCompany() - CRITIQUE

**Ancien INSERT** :
```csharp
insertCmd.CommandText = @"
    INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury)
    VALUES (@companyId, @name, @countryId, @regionId, @prestige, @treasury)";
```

**Nouveau INSERT** (avec tous les champs) :
```csharp
insertCmd.CommandText = @"
    INSERT INTO Companies (
        CompanyId, Name, CountryId, RegionId, Prestige, Treasury,
        FoundedYear, CompanySize, CurrentEra, CatchStyleId, IsPlayerControlled, MonthlyBurnRate
    ) VALUES (
        @companyId, @name, @countryId, @regionId, @prestige, @treasury,
        @foundedYear, @companySize, @currentEra, @catchStyleId, @isPlayerControlled, @burnRate
    )";

insertCmd.Parameters.AddWithValue("@companyId", companyId);
insertCmd.Parameters.AddWithValue("@name", CompanyName.Trim());
insertCmd.Parameters.AddWithValue("@countryId", countryId);
insertCmd.Parameters.AddWithValue("@regionId", SelectedRegion!.RegionId);
insertCmd.Parameters.AddWithValue("@prestige", StartingPrestige);
insertCmd.Parameters.AddWithValue("@treasury", StartingTreasury);
insertCmd.Parameters.AddWithValue("@foundedYear", FoundedYear);
insertCmd.Parameters.AddWithValue("@companySize", "Local"); // Taille initiale
insertCmd.Parameters.AddWithValue("@currentEra", "Foundation Era");
insertCmd.Parameters.AddWithValue("@catchStyleId", SelectedCatchStyle!.CatchStyleId);
insertCmd.Parameters.AddWithValue("@isPlayerControlled", 1); // C'est la compagnie du joueur
insertCmd.Parameters.AddWithValue("@burnRate", 5000.0); // Burn rate initial modéré
```

#### 2.4 Créer l'Owner Automatiquement

**AJOUTER** après l'INSERT Company (ligne ~210) :

```csharp
Logger.Info($"Compagnie créée: {CompanyName} ({companyId})");

// ===== NOUVEAU CODE =====

// Créer l'Owner (contrôleur stratégique)
var ownerId = $"OWN_{Guid.NewGuid():N}".Substring(0, 16);
await CreateDefaultOwner(companyId, ownerId);
Logger.Info($"Owner créé: {ownerId}");

// Créer le Booker (directeur créatif)
var bookerId = $"BOOK_{Guid.NewGuid():N}".Substring(0, 16);
await CreateDefaultBooker(companyId, bookerId);
Logger.Info($"Booker créé: {bookerId}");

// ===== FIN NOUVEAU CODE =====

// Créer la sauvegarde
CreateSaveGame(connection, companyId);
```

#### 2.5 Méthodes de Création Owner/Booker

**AJOUTER** à la fin de la classe (avant Cancel) :

```csharp
private async System.Threading.Tasks.Task CreateDefaultOwner(string companyId, string ownerId)
{
    // Mapper le CatchStyle vers PreferredProductType
    var productType = SelectedCatchStyle!.Name switch
    {
        "Pure Wrestling" or "Strong Style" => "Technical",
        "Sports Entertainment" or "Family-Friendly" => "Entertainment",
        "Hardcore Wrestling" => "Hardcore",
        "Lucha Libre" => "Entertainment",
        _ => "Entertainment"
    };

    var owner = new Owner
    {
        OwnerId = ownerId,
        CompanyId = companyId,
        Name = "Owner",  // Le joueur pourra personnaliser plus tard
        VisionType = "Balanced",
        RiskTolerance = 50,
        PreferredProductType = productType,
        ShowFrequencyPreference = "Weekly",
        TalentDevelopmentFocus = 50,
        FinancialPriority = 50,
        FanSatisfactionPriority = 50,
        CreatedAt = DateTime.Now
    };

    await _ownerRepository.SaveOwnerAsync(owner);
}

private async System.Threading.Tasks.Task CreateDefaultBooker(string companyId, string bookerId)
{
    var booker = new RingGeneral.Core.Models.Booker.Booker
    {
        BookerId = bookerId,
        CompanyId = companyId,
        Name = "Head Booker",
        CreativityScore = 60,
        LogicScore = 70,
        BiasResistance = 60,
        PreferredStyle = "Flexible",
        LikesUnderdog = true,
        LikesVeteran = false,
        LikesFastRise = false,
        LikesSlowBurn = true,
        IsAutoBookingEnabled = false,  // Désactivé par défaut
        EmploymentStatus = "Active",
        HireDate = DateTime.Now,
        CreatedAt = DateTime.Now
    };

    await _bookerRepository.SaveBookerAsync(booker);
}
```

#### 2.6 Propriétés UI Ajoutées

```csharp
public CatchStyle? SelectedCatchStyle
{
    get => _selectedCatchStyle;
    set => this.RaiseAndSetIfChanged(ref _selectedCatchStyle, value);
}

public int FoundedYear
{
    get => _foundedYear;
    set => this.RaiseAndSetIfChanged(ref _foundedYear, Math.Clamp(value, 1950, 2100));
}

public ObservableCollection<CatchStyle> AvailableCatchStyles { get; }
```

### Fichier à Modifier

**Fichier** : `/src/RingGeneral.UI/ViewModels/Start/CreateCompanyViewModel.cs`
**Lignes à modifier** : 27-41 (constructeur), 86+ (ajouter LoadCatchStyles), 195-224 (CreateCompany)

### Validation

✅ Le flux garantira :
1. Chaque Company créée aura **TOUJOURS** un Owner et un Booker
2. Les valeurs par défaut sont équilibrées (Balanced, 50/50/50)
3. Le CatchStyle est lié correctement au PreferredProductType de l'Owner
4. IsPlayerControlled = 1 pour distinguer la compagnie du joueur
5. FoundedYear personnalisable (défaut : 2024)

---

## 🔜 PHASE 3 : COMPANY HUB UI

### 3.1 Architecture UI

```
CompanyHubViewModel (Parent avec TabControl)
├── CompanyProfileTabViewModel
│   ├── Header (Logo, Pays, Année, Taille, Trésorerie, Era)
│   ├── Direction (Cards Owner + Booker cliquables)
│   └── Main Stars (Top 3/5 workers avec avatars)
├── CompanyStaffTabViewModel
│   ├── Créatif (Bookers, Writers)
│   ├── Structurel (Trainers, Medics, Scouts)
│   └── Services (Security, Catering, etc.)
├── CompanyRosterTabViewModel
│   └── Tableau (Nom, Stats, Moral, Push, Contrat)
├── CompanyTeamsTabViewModel
│   ├── Tag Teams
│   ├── Trios
│   └── Factions/Stables
└── CompanyHistoryTabViewModel
    ├── Titres (Champions actuels, historique règnes)
    └── Eras Timeline
```

### 3.2 Fichiers à Créer

#### ViewModels

1. `/src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs`
   - Navigation entre onglets (SelectedTabIndex)
   - Chargement CompanyGovernanceView
   - Switch entre "Ma Compagnie" / "Compagnies Rivales"

2. `/src/RingGeneral.UI/ViewModels/CompanyHub/CompanyProfileTabViewModel.cs`
   - Propriétés : CompanyName, CountryName, FoundedYear, CompanySize, Era
   - OwnerSnapshot, BookerSnapshot, CatchStyle
   - ObservableCollection<CompanyMainStar> TopWorkers (Top 5 par popularité)
   - Commands : NavigateToOwnerDetail, NavigateToBookerDetail

3. `/src/RingGeneral.UI/ViewModels/CompanyHub/CompanyStaffTabViewModel.cs`
   - Staff groupé par pôle (Créatif, Structurel, Trainers)

4. `/src/RingGeneral.UI/ViewModels/CompanyHub/CompanyRosterTabViewModel.cs`
   - DataGrid avec Workers
   - Filtres : Actif/Blessé, Push Level, Contrat expiring

5. `/src/RingGeneral.UI/ViewModels/CompanyHub/CompanyTeamsTabViewModel.cs`
   - Liste Tag Teams, Trios, Factions

6. `/src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHistoryTabViewModel.cs`
   - Titres + Champions
   - Eras Timeline (avec dates)

#### Views (AXAML)

1. `/src/RingGeneral.UI/Views/CompanyHub/CompanyHubView.axaml`
   - TabControl parent avec 2 sections : "Ma Compagnie" | "Rivales"
   - Border scannables

2. `/src/RingGeneral.UI/Views/CompanyHub/CompanyProfileView.axaml`
   - Header avec icônes + textes
   - 2 Cards côte à côte : Owner | Booker (cliquables avec hover effect)
   - Grid 3 colonnes pour Top Stars (Photo + Nom + Stats)

3. `/src/RingGeneral.UI/Views/CompanyHub/CompanyStaffView.axaml`
   - Grille groupée par pôle

4. `/src/RingGeneral.UI/Views/CompanyHub/CompanyRosterView.axaml`
   - SortableDataGrid réutilisable

5. `/src/RingGeneral.UI/Views/CompanyHub/CompanyTeamsView.axaml`
   - Liste avec avatars workers

6. `/src/RingGeneral.UI/Views/CompanyHub/CompanyHistoryView.axaml`
   - Accordion pour chaque titre + Timeline visuelle

### 3.3 Pattern TabControl (Exemple ProfileView.axaml)

```xaml
<TabControl Grid.Row="2" SelectedIndex="{Binding SelectedTabIndex}">
    <!-- Tab 1: Profil -->
    <TabItem Header="📊 PROFIL">
        <ScrollViewer>
            <StackPanel Margin="20" Spacing="25">
                <!-- Header Company -->
                <Border Background="#1e293b" CornerRadius="12" Padding="30">
                    <Grid ColumnDefinitions="Auto,*,Auto">
                        <Image Source="{Binding LogoPath}" Width="80" Height="80"/>
                        <StackPanel Grid.Column="1" Margin="20,0">
                            <TextBlock Text="{Binding CompanyName}" FontSize="28" FontWeight="Bold"/>
                            <TextBlock Text="{Binding CountryName}" FontSize="16" Foreground="#94a3b8"/>
                        </StackPanel>
                        <StackPanel Grid.Column="2">
                            <TextBlock Text="{Binding FoundedYear}" FontSize="18"/>
                            <TextBlock Text="{Binding CompanySize}" FontSize="14" Foreground="#888"/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Direction (Owner + Booker) -->
                <WrapPanel>
                    <Border Background="#1e293b" CornerRadius="12" Padding="20" Width="300" Cursor="Hand">
                        <StackPanel>
                            <TextBlock Text="👔 OWNER" FontSize="14" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding OwnerName}" FontSize="20" FontWeight="Bold"/>
                            <TextBlock Text="{Binding VisionType}" FontSize="14"/>
                        </StackPanel>
                    </Border>

                    <Border Background="#1e293b" CornerRadius="12" Padding="20" Width="300" Cursor="Hand" Margin="20,0,0,0">
                        <StackPanel>
                            <TextBlock Text="📋 BOOKER" FontSize="14" Foreground="#94a3b8"/>
                            <TextBlock Text="{Binding BookerName}" FontSize="20" FontWeight="Bold"/>
                            <TextBlock Text="{Binding BookerPreferredStyle}" FontSize="14"/>
                        </StackPanel>
                    </Border>
                </WrapPanel>

                <!-- Main Stars (Top 5) -->
                <StackPanel>
                    <TextBlock Text="⭐ MAIN STARS" FontSize="18" FontWeight="Bold" Margin="0,0,0,15"/>
                    <ItemsControl ItemsSource="{Binding TopWorkers}">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate>
                                <WrapPanel/>
                            </ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Background="#2d2d2d" CornerRadius="8" Padding="15" Width="180" Margin="0,0,10,10">
                                    <StackPanel>
                                        <!-- Avatar worker -->
                                        <Ellipse Width="60" Height="60" Fill="#3b82f6"/>
                                        <TextBlock Text="{Binding NomComplet}" FontSize="14" FontWeight="Bold" TextAlignment="Center" Margin="0,10,0,0"/>
                                        <TextBlock Text="{Binding Popularite}" FontSize="12" Foreground="#10b981" TextAlignment="Center"/>
                                    </StackPanel>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </StackPanel>
            </StackPanel>
        </ScrollViewer>
    </TabItem>

    <!-- Tab 2-6: Autres onglets -->
</TabControl>
```

### 3.4 Enregistrement DI

**Dans `App.axaml.cs`** (après ligne 130) :

```csharp
// Company Hub ViewModels
services.AddTransient<CompanyHubViewModel>();
services.AddTransient<CompanyProfileTabViewModel>();
services.AddTransient<CompanyStaffTabViewModel>();
services.AddTransient<CompanyRosterTabViewModel>();
services.AddTransient<CompanyTeamsTabViewModel>();
services.AddTransient<CompanyHistoryTabViewModel>();
```

**Dans `MainWindow.axaml`** (DataTemplates) :

```xaml
<DataTemplate DataType="vmCompanyHub:CompanyHubViewModel">
    <companyhub:CompanyHubView />
</DataTemplate>
```

### 3.5 Navigation ShellViewModel

**Dans `ShellViewModel.BuildNavigationTree()`** :

```csharp
var companyHub = new NavigationItemViewModel(
    "companyhub",
    "COMPANY HUB",
    "🏢",
    typeof(CompanyHubViewModel)
);
root.Add(companyHub);
```

### 3.6 Chargement des Données

**Dans `CompanyHubViewModel.OnNavigatedTo()` :**

```csharp
public void OnNavigatedTo(object? parameter)
{
    // Charger la compagnie du joueur
    var playerCompany = await _gameRepository.GetPlayerCompanyAsync();

    // Charger la vue combinée
    var governanceView = await LoadCompanyGovernanceView(playerCompany.CompanyId);

    // Propager aux sous-ViewModels
    _profileTab.LoadData(governanceView);
    _staffTab.LoadData(playerCompany.CompanyId);
    _rosterTab.LoadWorkers(playerCompany.CompanyId);
    _teamsTab.LoadTeams(playerCompany.CompanyId);
    _historyTab.LoadHistory(playerCompany.CompanyId);
}

private async Task<CompanyGovernanceView> LoadCompanyGovernanceView(string companyId)
{
    using var connection = _repository.CreateConnection();
    using var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT * FROM vw_CompanyGovernance WHERE CompanyId = @id";
    cmd.Parameters.AddWithValue("@id", companyId);

    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return MapCompanyGovernanceView(reader);
    }

    throw new InvalidOperationException($"Company {companyId} not found");
}
```

### 3.7 Navigation vers Rivales

**Switch Button dans CompanyHubView** :

```xaml
<ToggleButton IsChecked="{Binding IsViewingRival}" Content="{Binding RivalButtonText}"/>
```

**Logique dans CompanyHubViewModel** :

```csharp
private bool _isViewingRival = false;
private string _currentCompanyId;

public bool IsViewingRival
{
    get => _isViewingRival;
    set
    {
        this.RaiseAndSetIfChanged(ref _isViewingRival, value);
        if (value)
        {
            // Charger liste des rivales
            LoadRivalCompanies();
        }
        else
        {
            // Retour à la compagnie du joueur
            LoadPlayerCompany();
        }
    }
}

private async void LoadRivalCompanies()
{
    var rivals = await _companyRepository.GetRivalCompaniesAsync();
    RivalCompanies = new ObservableCollection<CompanyState>(rivals);
}
```

---

## 🧪 TESTS & VALIDATION

### Tests Unitaires Requis

1. **CatchStyleRepository**
   - ✅ GetAllActiveStylesAsync() retourne 8 styles
   - ✅ GetCompatibleStylesAsync("Technical") retourne Pure Wrestling, Strong Style, Hybrid, Indie
   - ✅ CalculateStyleMatchBonus() retourne 1.3x pour match parfaitement aligné
   - ✅ CalculateStyleMatchBonus() retourne 0.8x pour match opposé

2. **CreateCompanyViewModel**
   - ✅ CreateCompany() insère Company avec tous les champs
   - ✅ CreateCompany() crée Owner avec PreferredProductType aligné
   - ✅ CreateCompany() crée Booker avec EmploymentStatus = "Active"
   - ✅ Validation échoue si CompanyName < 3 caractères
   - ✅ Validation échoue si SelectedCatchStyle == null

3. **Migrations SQL**
   - ✅ Migration 004 crée tables Owners, Bookers, BookerMemory, BookerEmploymentHistory
   - ✅ Migration 005 ajoute colonnes à Companies + crée vues
   - ✅ Migration 006 crée table CatchStyles + insère 8 styles
   - ✅ Trigger trg_create_initial_era s'exécute lors de INSERT Company

### Tests d'Intégration

1. **Flux Complet Création de Partie**
   - Créer nouvelle compagnie via UI
   - Vérifier Owner créé en DB
   - Vérifier Booker créé en DB
   - Vérifier Era créée automatiquement
   - Naviguer vers Dashboard sans erreur

2. **Company Hub Navigation**
   - Naviguer vers Company Hub
   - Charger vw_CompanyGovernance sans null
   - Afficher Owner + Booker correctement
   - Switcher vers Rival Company
   - Retour vers Player Company

3. **Style System**
   - Créer Company avec style "Pure Wrestling"
   - Vérifier Owner.PreferredProductType = "Technical"
   - Simuler match technique → Bonus 1.2x
   - Simuler match hardcore → Malus 0.9x

### Tests UI

1. **CreateCompanyView**
   - ✅ Dropdown CatchStyles affiche 8 options
   - ✅ Sélection style change icône + description
   - ✅ Validation affiche erreur si champ vide
   - ✅ Bouton "Créer" désactivé si formulaire invalide

2. **CompanyHubView**
   - ✅ Tabs s'affichent correctement
   - ✅ Cards Owner/Booker sont cliquables
   - ✅ Top Stars affiche 5 workers maximum
   - ✅ Switch "Voir Rivales" fonctionne

---

## 📁 FICHIERS MODIFIÉS/CRÉÉS

### ✅ Déjà Complétés

**Migrations SQL** :
- `data/migrations/004_owner_booker_governance.sql` (170 lignes)
- `data/migrations/005_company_identity.sql` (240 lignes)
- `data/migrations/006_catch_styles.sql` (280 lignes)

**Modèles** :
- `src/RingGeneral.Core/Models/DomainModels.cs` (modifié : +100 lignes)

**Repositories** :
- `src/RingGeneral.Data/Repositories/ICatchStyleRepository.cs` (créé : 25 lignes)
- `src/RingGeneral.Data/Repositories/CatchStyleRepository.cs` (créé : 180 lignes)
- `src/RingGeneral.Data/Repositories/RepositoryFactory.cs` (modifié : +15 lignes)
- `src/RingGeneral.Data/Database/SqliteConnectionFactory.cs` (modifié : +1 ligne)

**DI** :
- `src/RingGeneral.UI/App.axaml.cs` (modifié : +4 lignes)

### ⚠️ En Attente de Modification

**ViewModels** :
- `src/RingGeneral.UI/ViewModels/Start/CreateCompanyViewModel.cs` (270 lignes → 400 lignes estimé)

### 🔜 À Créer (Phase 3)

**ViewModels** (6 fichiers) :
- `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs` (~200 lignes)
- `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyProfileTabViewModel.cs` (~150 lignes)
- `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyStaffTabViewModel.cs` (~100 lignes)
- `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyRosterTabViewModel.cs` (~120 lignes)
- `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyTeamsTabViewModel.cs` (~100 lignes)
- `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHistoryTabViewModel.cs` (~150 lignes)

**Views AXAML** (6 fichiers) :
- `src/RingGeneral.UI/Views/CompanyHub/CompanyHubView.axaml` (~150 lignes)
- `src/RingGeneral.UI/Views/CompanyHub/CompanyProfileView.axaml` (~200 lignes)
- `src/RingGeneral.UI/Views/CompanyHub/CompanyStaffView.axaml` (~100 lignes)
- `src/RingGeneral.UI/Views/CompanyHub/CompanyRosterView.axaml` (~150 lignes)
- `src/RingGeneral.UI/Views/CompanyHub/CompanyTeamsView.axaml` (~100 lignes)
- `src/RingGeneral.UI/Views/CompanyHub/CompanyHistoryView.axaml` (~150 lignes)

**Estimations** :
- ViewModels : ~820 lignes
- Views : ~850 lignes
- **Total Phase 3** : ~1670 lignes de code

---

## 🚀 PROCHAINES ÉTAPES

### Immédiat (Critique)

1. ✅ **Commit Phase 1** : Migrations + Repositories + DI
2. ⚠️ **Compléter CreateCompanyViewModel** : Ajouter initialisation Owner/Booker
3. 🧪 **Tester flux de création** : Créer partie + vérifier DB
4. ✅ **Commit Phase 2** : Flux de création sécurisé

### Moyen Terme (UI)

5. 🎨 **Créer CompanyHub ViewModels** : 6 fichiers
6. 🎨 **Créer CompanyHub Views** : 6 fichiers AXAML
7. 🔌 **Intégrer navigation** : ShellViewModel + DI
8. 🧪 **Tests UI manuels** : Navigation + affichage

### Long Terme (Polish)

9. 📊 **Dashboard intégration** : Ajouter bouton "Company Hub" sur Dashboard
10. 🎨 **UI Polish** : Animations, hover effects, loading states
11. 📚 **Documentation** : User guide pour Company Hub
12. 🌍 **Localisation** : Traductions FR/EN

---

## 📝 NOTES TECHNIQUES

### Compatibilité Ascendante

**Sauvegardes existantes** :
- Les migrations 005/006 incluent des `UPDATE` pour les Companies existantes
- Valeurs par défaut : FoundedYear=2024, CompanySize='Local', CatchStyleId='STYLE_HYBRID'
- ✅ **Aucune sauvegarde ne sera corrompue**

**Fallback** :
Si un champ est NULL malgré les migrations :
```csharp
var foundedYear = company.FoundedYear ?? 2024;
var catchStyleId = company.CatchStyleId ?? "STYLE_HYBRID";
```

### Performance

**Vues SQL** :
- `vw_CompanyGovernance` : JOIN Company + Owner + Booker (1 query au lieu de 3)
- Index sur `CompanyId`, `EmploymentStatus`, `EventType` pour accès rapide

**Lazy Loading** :
- CompanyHub charge uniquement l'onglet actif (SelectedTabIndex)
- Top Stars limités à 5 workers (pas de pagination nécessaire)

### Sécurité

**Validation** :
- CHECK constraints en DB pour enum values
- Validation C# dans `Owner.IsValid()` et `Booker.IsValid()`
- Clamp sur les valeurs UI (Prestige: 0-100, FoundedYear: 1950-2100)

**SQL Injection** :
- Utilisation exclusive de paramètres `@name` (pas de string concat)

---

## 🔗 RÉFÉRENCES

**Documentation Avalonia** :
- TabControl : https://docs.avaloniaui.net/docs/controls/tabcontrol
- DataTemplates : https://docs.avaloniaui.net/docs/templates/data-templates

**Patterns utilisés** :
- MVVM (Model-View-ViewModel)
- Repository Pattern
- Dependency Injection
- Observer Pattern (ReactiveUI)

**Fichiers de référence** :
- `/src/RingGeneral.UI/Views/Workers/Profile/ProfileView.axaml` (exemple TabControl)
- `/src/RingGeneral.UI/Views/OwnerBooker/OwnerBookerView.axaml` (exemple Owner/Booker)

---

## ✅ CHECKLIST VALIDATION

### Phase 1 : Infrastructure ✅
- [x] Migration 004 créée (Owner, Booker, Memory)
- [x] Migration 005 créée (Company Identity, Eras, Milestones)
- [x] Migration 006 créée (CatchStyles + 8 styles prédéfinis)
- [x] DomainModels.cs mis à jour (12 nouveaux records)
- [x] CatchStyleRepository créé + Interface
- [x] RepositoryFactory enrichi
- [x] App.axaml.cs : DI configuré

### Phase 2 : Flux Création ⚠️
- [ ] CreateCompanyViewModel : Repositories injectés
- [ ] CreateCompanyViewModel : LoadCatchStylesFromDatabase()
- [ ] CreateCompanyViewModel : INSERT Company avec nouveaux champs
- [ ] CreateCompanyViewModel : CreateDefaultOwner()
- [ ] CreateCompanyViewModel : CreateDefaultBooker()
- [ ] Test : Créer partie → Owner + Booker dans DB
- [ ] Test : Aucune erreur null sur Dashboard

### Phase 3 : Company Hub 🔜
- [ ] CompanyHubViewModel créé
- [ ] 5 sous-ViewModels créés
- [ ] CompanyHubView.axaml créé
- [ ] 5 sous-Views créées
- [ ] DI : Enregistrement ViewModels
- [ ] MainWindow.axaml : DataTemplate
- [ ] ShellViewModel : Navigation item
- [ ] Test : Navigation vers Company Hub
- [ ] Test : Switch Rival Company

### Final 🎯
- [ ] Tests unitaires passés
- [ ] Tests d'intégration passés
- [ ] Documentation utilisateur
- [ ] Commit final + Push

---

**Dernière mise à jour** : 2026-01-08 par Claude
**Version** : 1.0
**Statut** : Phase 1 Complétée ✅ | Phase 2 En Cours ⚠️ | Phase 3 À Venir 🔜
