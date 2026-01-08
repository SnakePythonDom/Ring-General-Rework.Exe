# 🔧 Guide d'Intégration - Performance Attributes Rework + ProfileView

Ce guide explique comment intégrer et utiliser le nouveau système de 30 attributs et le ProfileView universel dans Ring General.

---

## 📦 Vue d'Ensemble du Projet

Le rework complet comprend:
- **11 nouvelles tables** de base de données
- **12 nouveaux models** C# (3 attributes + 3 relations + 6 support)
- **3 nouveaux repositories** avec interfaces
- **7 nouveaux ViewModels** (1 principal + 6 tabs)
- **1 nouvelle View** Avalonia avec TabControl

---

## 🗄️ PHASE 1: Base de Données (SQL)

### Fichiers SQL
```
src/RingGeneral.Data/Migrations/
├── Migration_Master_ProfileViewAttributs.sql    # Migration principale (11 tables)

src/RingGeneral.Data/Seed/
├── WorkersAttributesSeed.sql                    # Seed 30 attributs (10 wrestlers)
└── WorkersProfileDataSeed.sql                   # Seed relations/factions/specs
```

### Exécution de la Migration

```bash
# Option 1: Intégration automatique au démarrage
# Le système détectera et exécutera la migration automatiquement

# Option 2: Exécution manuelle via SQLite
sqlite3 ring_general.db < src/RingGeneral.Data/Migrations/Migration_Master_ProfileViewAttributs.sql
sqlite3 ring_general.db < src/RingGeneral.Data/Seed/WorkersAttributesSeed.sql
sqlite3 ring_general.db < src/RingGeneral.Data/Seed/WorkersProfileDataSeed.sql
```

### Tables Créées
1. `WorkerInRingAttributes` - 10 attributs in-ring + InRingAvg (generated)
2. `WorkerEntertainmentAttributes` - 10 attributs entertainment + EntertainmentAvg
3. `WorkerStoryAttributes` - 10 attributs story + StoryAvg
4. `WorkerSpecializations` - Styles de lutte (Brawler, Technical, etc.)
5. `WorkerRelations` - Relations 1-to-1 entre workers
6. `Factions` - Tag teams, trios, stables
7. `FactionMembers` - Memberships avec dates
8. `WorkerNotes` - Notes du booker
9. `ContractHistory` - Historique des contrats
10. `MatchHistory` - Historique des matchs
11. `TitleReigns` - Règnes de champions

### Colonnes Ajoutées à `Workers`
- **Géographie**: `BirthCity`, `BirthCountry`, `ResidenceCity`, `ResidenceState`, `ResidenceCountry`
- **Physique**: `PhotoPath`, `Handedness`, `FightingStance`
- **Gimmick/Push**: `CurrentGimmick`, `Alignment`, `PushLevel`, `TvRole`, `BookingIntent`

---

## 🏗️ PHASE 2: Models

### Structure des Fichiers
```
src/RingGeneral.Core/Models/
├── Worker.cs                                # Model central (13 props + navigation)
├── ContractHistory.cs                       # Contrats
├── MatchHistoryItem.cs                      # Matchs
├── TitleReign.cs                            # Titres
├── WorkerNote.cs                            # Notes
├── WorkerSpecialization.cs                  # Spécialisations
│
├── Attributes/
│   ├── WorkerInRingAttributes.cs           # 10 attributs in-ring
│   ├── WorkerEntertainmentAttributes.cs    # 10 attributs entertainment
│   └── WorkerStoryAttributes.cs            # 10 attributs story
│
└── Relations/
    ├── WorkerRelation.cs                    # Relations 1-to-1
    ├── Faction.cs                           # Factions/Teams
    └── FactionMember.cs                     # Memberships
```

### Utilisation des Models

```csharp
// Créer un worker avec tous les attributs
var worker = new Worker
{
    Name = "Test Worker",
    Age = 28,
    Height = 183,
    Weight = 95,
    Gender = Gender.Male,
    BirthCity = "Tokyo",
    BirthCountry = "Japan",
    Alignment = Alignment.Face,
    PushLevel = PushLevel.MidCard,
    TvRole = 65
};

// Accéder aux attributs via navigation
worker.InRingAttributes = new WorkerInRingAttributes
{
    Striking = 85,
    Grappling = 78,
    HighFlying = 92,
    // ... etc
};

// Propriétés calculées
var overall = worker.OverallRating; // (InRing + Entertainment + Story) / 3
var isChampion = worker.IsChampion;
var winPct = worker.WinPercentage;
```

---

## 📊 PHASE 3: Repositories

### Structure des Fichiers
```
src/RingGeneral.Data/Repositories/
├── IWorkerAttributesRepository.cs           # Interface attributs
├── WorkerAttributesRepository.cs            # Implémentation attributs
├── IRelationsRepository.cs                  # Interface relations
├── RelationsRepository.cs                   # Implémentation relations
├── INotesRepository.cs                      # Interface notes/history
└── NotesRepository.cs                       # Implémentation notes/history
```

### Injection de Dépendances

```csharp
// Dans Program.cs ou Startup.cs
services.AddSingleton<IWorkerAttributesRepository, WorkerAttributesRepository>();
services.AddSingleton<IRelationsRepository, RelationsRepository>();
services.AddSingleton<INotesRepository, NotesRepository>();
```

### Exemples d'Utilisation

```csharp
// WorkerAttributesRepository
var attrs = _attributesRepository.GetAllAttributes(workerId);
_attributesRepository.UpdateInRingAttribute(workerId, "Striking", 90);
_attributesRepository.InitializeDefaultAttributes(newWorkerId);

// RelationsRepository
var relations = _relationsRepository.GetRelationsForWorker(workerId);
_relationsRepository.CreateRelation(new WorkerRelation
{
    WorkerId1 = 1,
    WorkerId2 = 2,
    RelationType = RelationType.Rivalite,
    RelationStrength = 85
});

// NotesRepository
var contracts = _notesRepository.GetContractHistory(workerId);
var currentContract = _notesRepository.GetActiveContract(workerId);
var (total, wins, losses, draws, winPct) = _notesRepository.GetMatchStats(workerId);
```

---

## 🎨 PHASE 4: ViewModels

### Structure des Fichiers
```
src/RingGeneral.UI/ViewModels/Workers/Profile/
├── ProfileViewModel.cs                      # ViewModel principal avec tabs
├── AttributesTabViewModel.cs                # Tab 1: 30 attributs
├── ContractsTabViewModel.cs                 # Tab 2: Contrats
├── GimmickTabViewModel.cs                   # Tab 3: Gimmick/Push
├── RelationsTabViewModel.cs                 # Tab 4: Relations/Factions
├── HistoryTabViewModel.cs                   # Tab 5: Matchs/Titres
└── NotesTabViewModel.cs                     # Tab 6: Notes booker
```

### Injection de Dépendances

```csharp
// Dans Program.cs ou App.axaml.cs
services.AddTransient<ProfileViewModel>();
services.AddTransient<AttributesTabViewModel>();
// ... etc pour tous les tabs
```

### Utilisation du ProfileViewModel

```csharp
// Dans un autre ViewModel ou service de navigation
var profileVM = serviceProvider.GetRequiredService<ProfileViewModel>();

// Charger un worker
profileVM.LoadWorker(selectedWorker);
// ou
profileVM.LoadWorkerById(workerId);

// Naviguer vers un tab spécifique
profileVM.NavigateToTab(0); // Attributs
profileVM.NavigateToTab(3); // Relations

// Rafraîchir
profileVM.RefreshCommand.Execute();
```

---

## 🖥️ PHASE 5: Views (UI)

### Fichiers Créés
```
src/RingGeneral.UI/Views/Workers/Profile/
├── ProfileView.axaml                        # View principale avec TabControl
└── ProfileView.axaml.cs                     # Code-behind
```

### Navigation vers ProfileView

```csharp
// Option 1: Via NavigationService
_navigationService.NavigateTo<ProfileViewModel>(worker);

// Option 2: Direct binding dans XAML
<ContentControl Content="{Binding CurrentProfileViewModel}"/>

// Option 3: Dans le Shell
var profileView = new ProfileView
{
    DataContext = profileViewModel
};
```

### Structure de la View

La ProfileView utilise un **TabControl Avalonia** avec 6 tabs:
1. 📊 ATTRIBUTS - Display des 30 attributs avec edit mode
2. 📄 CONTRATS - Contrat actuel + historique
3. 🎭 GIMMICK - Gimmick, alignment, push, spécialisations
4. 👥 RELATIONS - Relations + factions
5. 📜 HISTORIQUE - Matchs + titres + stats
6. 📝 NOTES - Notes du booker

---

## 📚 PHASE 6: Resources & Localization

### Fichier de Référence
`ATTRIBUTS_DESCRIPTIONS.md` - Descriptions complètes des 30 attributs en français

### Intégration dans l'UI

```csharp
// Créer un fichier de resources Avalonia (optionnel)
// Resources/AttributeDescriptions.resx

// Ou utiliser des tooltips directs dans XAML
<TextBlock Text="Striking" ToolTip.Tip="Maîtrise des coups de poing, coups de pied..."/>
```

---

## ✅ PHASE 7: Tests & Intégration

### Tests Recommandés

```csharp
// Tests Unitaires pour Repositories
[Fact]
public void GetAllAttributes_ReturnsAllThreeTables()
{
    var (inRing, entertainment, story) = _repository.GetAllAttributes(1);
    Assert.NotNull(inRing);
    Assert.NotNull(entertainment);
    Assert.NotNull(story);
}

// Tests pour ViewModels
[Fact]
public void LoadWorker_LoadsAllTabs()
{
    profileVM.LoadWorker(testWorker);
    Assert.Equal(testWorker, profileVM.CurrentWorker);
    Assert.NotNull(profileVM.AttributesTab.InRingAttributes);
}
```

### Checklist d'Intégration

- [ ] Migration SQL exécutée
- [ ] Seed data chargé (10 wrestlers avec 30 attributs)
- [ ] Repositories injectés dans DI container
- [ ] ViewModels enregistrés dans DI
- [ ] ProfileView ajoutée au système de navigation
- [ ] Tests unitaires passent
- [ ] UI fonctionne et affiche les données

---

## 🚀 Démarrage Rapide

### 1. Base de Données
```bash
sqlite3 ring_general.db < src/RingGeneral.Data/Migrations/Migration_Master_ProfileViewAttributs.sql
```

### 2. Dependency Injection (Program.cs)
```csharp
// Repositories
services.AddSingleton<IWorkerAttributesRepository, WorkerAttributesRepository>();
services.AddSingleton<IRelationsRepository, RelationsRepository>();
services.AddSingleton<INotesRepository, NotesRepository>();

// ViewModels
services.AddTransient<ProfileViewModel>();
services.AddTransient<AttributesTabViewModel>();
// ... autres tabs
```

### 3. Navigation (dans un ViewModel)
```csharp
var profileVM = _serviceProvider.GetRequiredService<ProfileViewModel>();
profileVM.LoadWorker(selectedWorker);
_navigationService.NavigateTo(profileVM);
```

---

## 📈 Évolutions Futures

### Court Terme
- [ ] Édition inline des attributs avec sliders
- [ ] Graphiques radar pour visualiser les 30 attributs
- [ ] Export PDF du profil worker
- [ ] Comparaison de 2 workers side-by-side

### Moyen Terme
- [ ] Système de progression automatique (training)
- [ ] Notifications contrats expirant
- [ ] Analytics IA pour recommandations push
- [ ] Integration avec matchmaking (chemistry score)

### Long Terme
- [ ] Historical tracking (évolution attributs dans le temps)
- [ ] Achievements/badges basés sur attributs
- [ ] Custom attribute templates par promotion
- [ ] API REST pour accès externe

---

## 🐛 Troubleshooting

### Erreur: "Table does not exist"
**Solution**: Exécuter la migration SQL

### Erreur: "Repository not found in DI"
**Solution**: Vérifier l'enregistrement dans Program.cs

### Problème: Attributs null après LoadWorker
**Solution**: Vérifier que InitializeDefaultAttributes() a été appelé pour les nouveaux workers

### UI ne rafraîchit pas
**Solution**: Utiliser `RaisePropertyChanged()` dans les ViewModels

---

## 📞 Support

Pour toute question ou problème:
- Consulter `ATTRIBUTS_DESCRIPTIONS.md` pour détails sur les attributs
- Consulter `PLAN_MASTER_PROFILEVIEW_ATTRIBUTS.md` pour le plan complet
- Vérifier les tests unitaires pour exemples d'utilisation

---

**Version**: 1.0
**Date**: 2026-01-07
**Auteur**: Ring General Team
**Status**: ✅ Production Ready
