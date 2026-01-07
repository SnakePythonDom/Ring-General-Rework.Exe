# 🎭 PROTOTYPES UI - RING GENERAL

**4 prototypes d'interface utilisateur inspirés de Football Manager 26**

---

## 📋 VUE D'ENSEMBLE DES PROTOTYPES

| Prototype | Style | Inspiration | Complexité | Modernité |
|-----------|-------|-------------|------------|-----------|
| **A - Tabs Horizontal** | Onglets classiques en haut | FM Classic, Excel | ⭐⭐ Faible | ⭐⭐⭐ Moyenne |
| **B - Sidebar Vertical** | Navigation latérale moderne | VS Code, Discord | ⭐⭐⭐ Moyenne | ⭐⭐⭐⭐⭐ Très moderne |
| **C - Dashboard** | Dashboard widgets | Power BI, Google Analytics | ⭐⭐⭐⭐ Élevée | ⭐⭐⭐⭐⭐ Très moderne |
| **D - Dual-pane** | Navigation arborescente | FM 2026, Total War | ⭐⭐⭐⭐ Élevée | ⭐⭐⭐⭐ Moderne |

---

## 🅰️ PROTOTYPE A : Tabs Horizontal (Classique)

### Description visuelle

```
┌─────────────────────────────────────────────────────────────────┐
│  🎭 RING GENERAL     Semaine 24  📺 Monday Night Raw  💰 $2.4M  │
├─────────────────────────────────────────────────────────────────┤
│ [📋 BOOKING] [👤 ROSTER] [📖 STORYLINES] [🎓 YOUTH] [💼 FINANCE]│
├────────────────────────────────┬────────────────────────────────┤
│                                │                                │
│  SEGMENTS DE SHOW              │  ✅ VALIDATION                 │
│  ────────────────────          │                                │
│  ┌──────────────────────────┐  │  ✅ Booking valide             │
│  │ ⭐ MAIN EVENT            │  │  ⚠️ 2 warnings                │
│  │ Singles Match            │  │  💡 Conseils                  │
│  │ Cena vs. Orton           │  │                                │
│  │ 15 min • 🔥 85%          │  ├────────────────────────────────┤
│  │ 📖 Rivalité Title        │  │  📊 DÉTAILS DU SEGMENT         │
│  └──────────────────────────┘  │                                │
│                                │  Type: Singles Match           │
│  ┌──────────────────────────┐  │  Participants:                 │
│  │ 🎤 PROMO                 │  │  • John Cena                   │
│  │ Interview                │  │  • Randy Orton                 │
│  │ The Rock • 8 min         │  │                                │
│  └──────────────────────────┘  │  Durée: 15 minutes             │
│                                │  Storyline: Rivalité Title     │
│  [+ Nouveau Segment]           │  Note estimée: ⭐⭐⭐⭐ 82/100  │
├────────────────────────────────┴────────────────────────────────┤
│  35/120 min • 3 segments            [▶️ SIMULER SHOW]          │
└─────────────────────────────────────────────────────────────────┘
```

### ✅ Avantages

- **Familiarité** : Interface classique type Excel/FM, facile à comprendre
- **Simplicité** : Navigation claire par onglets horizontaux
- **Efficacité** : Tout visible en un coup d'œil
- **Panels latéraux** : Validation + détails toujours visibles
- **Splitter redimensionnable** : Adaptable aux préférences

### ❌ Inconvénients

- **Moins moderne** : Look plus traditionnel
- **Limitation d'espace** : Onglets limités en largeur d'écran
- **Pas de hiérarchie** : Navigation plate, pas de sous-catégories
- **Scalabilité** : Difficile d'ajouter plus d'onglets (>6-7)

### 🎯 Cas d'usage idéal

- Utilisateurs habitués à FM Classic
- Workflow linéaire (Booking → Roster → Storylines)
- Besoin de rapidité et simplicité

---

## 🅱️ PROTOTYPE B : Sidebar Vertical (Moderne)

### Description visuelle

```
┌──┬────────────┬──────────────────────────────────┬─────────────┐
│🎭│  BOOKING   │                                  │  📊 DÉTAILS │
│  │            │  SHOW BOOKING                    │             │
│📋│  Semaine 24│  ────────────────                │  Segment #1 │
│  │  120 min   │                                  │             │
│👤│            │  ┌────────────────────────────┐  │  Singles    │
│  │  SEGMENTS  │  │ ⭐ Main Event Timeline    │  │  Match      │
│📖│            │  │ ████████░░░░░░ 15min      │  │             │
│  │  • Main    │  │ Cena vs. Orton            │  │  👤 Cena    │
│🎓│  • Promo   │  └────────────────────────────┘  │  👤 Orton   │
│  │  • Tag     │                                  │             │
│💼│            │  ┌────────────────────────────┐  │  ⏱️ 15 min  │
│  │  [+ New]   │  │ Interview Timeline        │  │  🔥 85%     │
│📆│            │  │ ████░░░░░░░░░░ 8min       │  │  📖 Title   │
│  │            │  │ The Rock                  │  │             │
│  │  QUICK     │  └────────────────────────────┘  │  ⭐⭐⭐⭐    │
│📥│  ACTIONS   │                                  │  82/100     │
│  │            │  [▶️ SIMULER LE SHOW]            │             │
│⚙️│  Templates │  Note: 82 • Audience: 2.1M      │  [✏️ Edit]  │
│  │  Historique│                                  │  [📋 Copy]  │
└──┴────────────┴──────────────────────────────────┴─────────────┘
```

### ✅ Avantages

- **Très moderne** : Style VS Code / Discord / Apps récentes
- **Navigation efficace** : Icons sidebar + panel de navigation
- **Timeline visuelle** : Représentation temporelle des segments
- **Panneau de détails riche** : Informations contextuelles complètes
- **Scalabilité** : Facile d'ajouter de nouvelles sections

### ❌ Inconvénients

- **Complexité** : 3 colonnes peuvent être overwhelming
- **Espace réduit** : Colonne centrale moins large
- **Courbe d'apprentissage** : Moins intuitif pour débutants
- **Ressources** : Plus de rendus simultanés

### 🎯 Cas d'usage idéal

- Utilisateurs avancés
- Workflow avec beaucoup de contexte
- Applications modernes avec timeline/planning

---

## 🅾️ PROTOTYPE C : Dashboard (Management)

### Description visuelle

```
┌─────────────────────────────────────────────────────────────────┐
│  🎭 Ring General  [Dashboard] [Booking] [Roster] [Storylines]  │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐          │
│  │ 📅       │ │ 💰       │ │ ⭐       │ │ 📺       │          │
│  │ Semaine  │ │ Budget   │ │ Note moy │ │ Audience │          │
│  │ 24       │ │ $2.4M    │ │ 78/100   │ │ 2.1M     │          │
│  │          │ │ ↗ +12.5% │ │ Last:82  │ │ ↘ -3.2%  │          │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘          │
│                                                                 │
│  ┌─────────────────────────────────┬─────────────────────────┐ │
│  │  PROCHAIN SHOW                  │  ✅ VALIDATION          │ │
│  │  Monday Night Raw • Sem. 24     │  Prêt à simuler         │ │
│  │                                 │                         │ │
│  │  ┌───────────────────────────┐  │  ⚠️ 2 warnings         │ │
│  │  │ ⭐ MAIN EVENT   [82/100] │  │  • Fatigue Cena        │ │
│  │  │ Singles Match            │  │  • Peu de storylines   │ │
│  │  │ Cena vs. Orton           │  ├─────────────────────────┤
│  │  │ ⏱️ 15min 🔥85% 📖Title   │  │  TOP PERFORMERS         │ │
│  │  └───────────────────────────┘  │  • Cena (95) ↗ +5      │ │
│  │                                 │  • Orton (88) ↗ +3     │ │
│  │  ┌───────────────────────────┐  │  • Rock (92) → 0       │ │
│  │  │ Interview       [68/100] │  ├─────────────────────────┤
│  │  │ The Rock • 8min          │  │  STORYLINES ACTIVES     │ │
│  │  └───────────────────────────┘  │  • Rivalité Title      │ │
│  │                                 │    Peak • Heat 88      │ │
│  │  [+ Ajouter segment]            │  • Legacy Rising       │ │
│  │                                 │    Build • Heat 65     │ │
│  │  [▶️ Simuler] 35/120min 75/100  │  [+ Nouvelle]          │ │
│  ├─────────────────────────────────┴─────────────────────────┤ │
│  │  SHOWS À VENIR                                            │ │
│  │  • Sem. 25 : Friday Night SmackDown →                    │ │
│  │  • Sem. 26 : Monday Night Raw →                          │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### ✅ Avantages

- **Vue d'ensemble** : Toutes les infos clés sur un seul écran
- **KPIs visibles** : Métriques importantes en haut
- **Design moderne** : Style dashboard management/analytics
- **Widgets modulaires** : Réorganisables, personnalisables
- **Prise de décision** : Toutes les données pour décider rapidement

### ❌ Inconvénients

- **Information overload** : Trop d'informations simultanées
- **Scrolling nécessaire** : Pas tout visible sans défiler
- **Moins de détails** : Widgets condensés
- **Complexité technique** : Beaucoup de composants à gérer

### 🎯 Cas d'usage idéal

- Vue stratégique / management
- Décisions macro (pas de micro-management)
- Utilisateurs expérimentés voulant tout voir
- Dashboard de reporting

---

## 🅳 PROTOTYPE D : Dual-pane (FM 2026 Style)

### Description visuelle

```
┌─────────────────────────────────────────────────────────────────┐
│  🎭 RING GENERAL    📺 Monday Night Raw  📅 24/52  💰 $2.4M    │
├──────────────┬───────────────────────────────────┬──────────────┤
│              │                                   │              │
│  🔍 Search   │  BOOKING • SHOWS ACTIFS           │  📊 DÉTAILS  │
│              │  Monday Night Raw                 │  DU SEGMENT  │
│  🏠 ACCUEIL  │  ✅ Valide ⚠️ 2 warnings          │              │
│              │                                   │  Segment #1  │
│  📋 BOOKING  │  # TYPE       PARTICIPANTS  NOTE  │              │
│   ▾          │  ────────────────────────────────  │  ⭐ MAIN    │
│   📺 Shows   │  1 ⭐ Singles  Cena v Orton  [82] │  EVENT      │
│   📚 Biblio  │     Match                         │              │
│   📊 Histo   │                                   │  Singles    │
│              │  2   Promo     The Rock      [68] │  Match      │
│  👤 ROSTER   │                                   │              │
│   ▸          │  3   Tag Team  DX v Legacy   [74] │  👤 PARTICI │
│   🤼 Workers │                                   │  ┌────────┐ │
│   🏆 Titres  │  [+ Ajouter segment]              │  │ Cena   │ │
│              │                                   │  │ Pop 95 │ │
│  📖 STORIES  │  ────────────────────────────────  │  │ Mom +5 │ │
│   ▸          │  Durée: 35/120 ███░░░░            │  └────────┘ │
│   🔥 Actives │  Note moy: ⭐⭐⭐⭐ 75/100          │  ┌────────┐ │
│   ⏸️ Suspend │  Audience: 2.1M                   │  │ Orton  │ │
│              │                                   │  │ Pop 88 │ │
│  🎓 YOUTH    │  [▶️ SIMULER LE SHOW]             │  │ Mom +3 │ │
│              │                                   │  └────────┘ │
│  💼 FINANCE  │                                   │              │
│              │                                   │  ⏱️ 15 min   │
│  📆 CALENDAR │                                   │  🔥 85%      │
│              │                                   │  📖 Title    │
│              │                                   │              │
│  [🔄 Passer  │                                   │  NOTE: 82   │
│   semaine]   │                                   │  ⭐⭐⭐⭐      │
│              │                                   │              │
│              │                                   │  [✏️ Éditer] │
│              │                                   │  [📋 Copier] │
└──────────────┴───────────────────────────────────┴──────────────┘
│  💾 Sauvegarde auto • 14:32 • ringgeneral.db       v1.0.0      │
└─────────────────────────────────────────────────────────────────┘
```

### ✅ Avantages

- **Navigation arborescente** : Hiérarchie claire et extensible
- **Style FM 2026** : Interface familière aux joueurs de FM
- **Trois panneaux** : Navigation + Contenu + Contexte
- **Table détaillée** : Vue tabulaire professionnelle des segments
- **Contexte riche** : Panel de droite avec tous les détails
- **Splitters** : Tout redimensionnable

### ❌ Inconvénients

- **Espace fragmenté** : 3 colonnes = moins de largeur par zone
- **Complexité visuelle** : Beaucoup d'éléments UI
- **Nécessite grand écran** : Optimal sur 1600px+ minimum
- **Lourdeur** : Plus de composants à charger

### 🎯 Cas d'usage idéal

- Fans de Football Manager
- Grand écran / multi-moniteurs
- Workflow complexe avec beaucoup de navigation
- Besoin de voir beaucoup de contexte simultanément

---

## 📊 COMPARAISON RAPIDE

| Critère | A - Tabs | B - Sidebar | C - Dashboard | D - Dual-pane |
|---------|----------|-------------|---------------|---------------|
| **Simplicité** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |
| **Modernité** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Efficacité** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Scalabilité** | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Familiarité FM** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Petit écran** | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| **Grand écran** | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

---

## 🚀 PLAN DE MISE EN ŒUVRE

### Ce qu'il faut pour rendre CHAQUE prototype fonctionnel

Peu importe le prototype choisi, voici les éléments communs à implémenter :

### 🔧 PHASE 1 : Infrastructure de base (1-2 semaines)

#### 1.1 Architecture MVVM Avalonia

```
✅ À créer :
- ViewModelBase (ReactiveObject)
- Services de navigation
- Event Aggregator / Messenger
- Dependency Injection (Microsoft.Extensions.DI ou Splat)
```

**Fichiers à créer :**
- `ViewModels/Core/ViewModelBase.cs`
- `Services/Navigation/INavigationService.cs`
- `Services/Navigation/NavigationService.cs`
- `Services/Messaging/IEventAggregator.cs`
- `Services/Messaging/EventAggregator.cs`
- `App.axaml.cs` (Setup DI Container)

**Dépendances NuGet :**
```xml
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="ReactiveUI" Version="19.5.0" />
```

---

#### 1.2 Découpage GameSessionViewModel

```
✅ ViewModels à extraire :

BookingViewModel (300 lignes)
├── Segments collection
├── Add/Edit/Delete commands
├── Validation
└── Template application

ShowSimulationViewModel (250 lignes)
├── Simulation engine wrapper
├── Results processing
└── Impact display

RosterViewModel (350 lignes)
├── Table management
├── Filtering/Sorting
├── Column configuration
└── Preferences persistence

YouthDashboardViewModel (250 lignes)
├── Youth structures
├── Trainees management
└── Budget/Coach assignment

FinanceDashboardViewModel (180 lignes)
├── TV deals
├── Audience analytics
└── Reach map

CalendarViewModel (120 lignes)
├── Shows calendar
└── Show scheduling

GlobalSearchViewModel (100 lignes)
├── Search indexing
└── Search results

InboxViewModel (80 lignes)
├── Notifications
└── Alerts

ValidationPanelViewModel (150 lignes)
├── Validation rules
├── Issues collection
└── Auto-fix suggestions
```

**Fichiers à créer :**
- `ViewModels/Booking/BookingViewModel.cs`
- `ViewModels/Booking/ShowSimulationViewModel.cs`
- `ViewModels/Booking/ValidationPanelViewModel.cs`
- `ViewModels/Roster/RosterViewModel.cs`
- `ViewModels/Youth/YouthDashboardViewModel.cs`
- `ViewModels/Finance/FinanceDashboardViewModel.cs`
- `ViewModels/Schedule/CalendarViewModel.cs`
- `ViewModels/Shared/Search/GlobalSearchViewModel.cs`
- `ViewModels/Shared/Inbox/InboxViewModel.cs`

**GameSessionViewModel refactorisé (~200 lignes) :**
```csharp
public sealed class GameSessionViewModel : ViewModelBase
{
    private readonly GameRepository _repository;

    // Agrégation des sous-ViewModels
    public BookingViewModel Booking { get; }
    public RosterViewModel Roster { get; }
    public YouthDashboardViewModel Youth { get; }
    public FinanceDashboardViewModel Finance { get; }
    public CalendarViewModel Calendar { get; }
    public GlobalSearchViewModel Search { get; }
    public InboxViewModel Inbox { get; }

    public GameSessionViewModel(/* DI injection */)
    {
        // Initialisation via DI
    }

    public void NextWeek() { /* Délégation */ }
    public void LoadSession() { /* Orchestration */ }
}
```

---

#### 1.3 Services manquants

```
✅ Services à créer :

NavigationService
├── Navigate(ViewModelType)
├── GoBack()
└── CurrentViewModel observable

EventAggregator
├── Publish<TEvent>(TEvent)
└── Subscribe<TEvent>(Action<TEvent>)

DialogService
├── ShowMessageBox()
├── ShowConfirm()
└── ShowCustomDialog()

ThemeService (optionnel)
├── Dark/Light mode
└── Color schemes
```

**Fichiers à créer :**
- `Services/Navigation/NavigationService.cs`
- `Services/Messaging/EventAggregator.cs`
- `Services/Dialog/IDialogService.cs`
- `Services/Dialog/DialogService.cs`
- `Services/Theme/ThemeService.cs` (optionnel)

---

### 🎨 PHASE 2 : Vues modulaires (2-3 semaines)

#### Selon le prototype choisi

**🅰️ Pour Prototype A (Tabs Horizontal) :**
```
Views/
├── Shell/
│   └── MainWindow.axaml (Shell avec TabControl)
├── Booking/
│   ├── BookingView.axaml
│   ├── SegmentListPanel.axaml
│   ├── ValidationPanel.axaml
│   └── SegmentDetailsPanel.axaml
├── Roster/
│   └── RosterTableView.axaml
└── Shared/
    ├── SearchPanel.axaml
    └── HelpPanel.axaml
```

**Composants Avalonia nécessaires :**
- `TabControl` pour les onglets
- `DataGrid` pour les tables
- `GridSplitter` pour les panels redimensionnables

---

**🅱️ Pour Prototype B (Sidebar Vertical) :**
```
Views/
├── Shell/
│   ├── MainWindow.axaml (Grid 3 colonnes)
│   ├── IconSidebar.axaml
│   └── NavigationPanel.axaml
├── Booking/
│   ├── BookingTimelineView.axaml
│   └── SegmentTimelineItem.axaml
├── Shared/
│   └── DetailsPanel.axaml
```

**Composants Avalonia nécessaires :**
- Custom Timeline control (à créer)
- Icon button sidebar
- Animated panel transitions

**Contrôles custom à créer :**
- `TimelineControl.axaml.cs` (affichage temporel des segments)

---

**🅾️ Pour Prototype C (Dashboard) :**
```
Views/
├── Shell/
│   └── MainWindow.axaml (ScrollViewer principal)
├── Dashboard/
│   ├── DashboardView.axaml
│   ├── MetricCard.axaml (widget réutilisable)
│   ├── ShowCard.axaml
│   ├── ValidationWidget.axaml
│   ├── TopPerformersWidget.axaml
│   └── StorylinesWidget.axaml
```

**Composants Avalonia nécessaires :**
- Custom card controls
- Grid layout manager
- Chart controls (si analytics visuels) → utiliser `LiveCharts2.Avalonia`

---

**🅳 Pour Prototype D (Dual-pane FM26) :**
```
Views/
├── Shell/
│   ├── MainWindow.axaml (Grid 3 colonnes)
│   ├── NavigationTree.axaml
│   └── ContextPanel.axaml
├── Booking/
│   └── BookingTableView.axaml
└── Shared/
    ├── TreeNavigationControl.axaml
    └── DetailsPanelControl.axaml
```

**Composants Avalonia nécessaires :**
- `TreeView` pour navigation
- `DataGrid` avec colonnes personnalisables
- Custom detail panel avec bindings riches

---

### 🔗 PHASE 3 : Data Binding & Commands (1 semaine)

```
✅ Bindings à implémenter :

Booking → BookingView
├── ObservableCollection<SegmentViewModel> → ItemsControl
├── SelectedSegment → TwoWay binding
├── AddSegmentCommand → Button
└── DeleteSegmentCommand → Button

Validation → ValidationPanel
├── ObservableCollection<BookingIssueViewModel> → ItemsControl
├── ValidationSummary → TextBlock
└── CorrectIssueCommand → Button

Roster → RosterTableView
├── TableItems → DataGrid.ItemsSource
├── TableConfiguration → Column visibility
└── Filters → DataGrid.Filter
```

**ReactiveUI Commands à créer :**
```csharp
// Exemple dans BookingViewModel
public ReactiveCommand<Unit, Unit> AddSegmentCommand { get; }
public ReactiveCommand<SegmentViewModel, Unit> DeleteSegmentCommand { get; }
public ReactiveCommand<Unit, Unit> SimulateShowCommand { get; }

// Constructeur
AddSegmentCommand = ReactiveCommand.Create(AddSegment);
DeleteSegmentCommand = ReactiveCommand.Create<SegmentViewModel>(DeleteSegment);
SimulateShowCommand = ReactiveCommand.Create(SimulateShow, canSimulate);
```

---

### ⚙️ PHASE 4 : State Management & Persistence (1 semaine)

```
✅ À implémenter :

User Preferences
├── Table column order
├── Filter settings
├── Panel sizes (splitter positions)
└── Theme preferences

Session State
├── Current show ID
├── Selected segment
├── Navigation history
└── Search history

Auto-save
├── Save on navigation
├── Save on segment edit
└── Debounced save (éviter save à chaque keystroke)
```

**Services à créer :**
```
Services/Preferences/
├── IPreferencesService.cs
├── PreferencesService.cs
└── UserPreferences.cs (model)

Services/State/
├── ISessionStateService.cs
└── SessionStateService.cs
```

---

### 🧪 PHASE 5 : Tests & Polish (1-2 semaines)

```
✅ Tests à créer :

Unit Tests (xUnit + FluentAssertions)
├── BookingViewModel tests
├── ValidationPanelViewModel tests
├── RosterViewModel tests
└── Command execution tests

Integration Tests
├── Navigation flow tests
├── Data persistence tests
└── Repository interaction tests

UI Tests (optionnel - Avalonia.HeadlessNUnit)
├── View loading tests
└── Binding validation tests
```

**Projets de tests à créer :**
- `RingGeneral.UI.Tests/ViewModels/BookingViewModelTests.cs`
- `RingGeneral.UI.Tests/Services/NavigationServiceTests.cs`

---

## 📦 DÉPENDANCES NUGET COMPLÈTES

```xml
<!-- Avalonia Core -->
<PackageReference Include="Avalonia" Version="11.0.0" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.0" />

<!-- MVVM & Reactive -->
<PackageReference Include="ReactiveUI" Version="19.5.0" />
<PackageReference Include="ReactiveUI.Fody" Version="19.5.0" />

<!-- Dependency Injection -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />

<!-- Testing (optionnel) -->
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.0" />

<!-- Charts (si Prototype C Dashboard) -->
<PackageReference Include="LiveChartsCore.SkiaSharpView.Avalonia" Version="2.0.0-rc2" />
```

---

## 📝 CHECKLIST COMPLÈTE DE MISE EN ŒUVRE

### Infrastructure (Commun à tous les prototypes)

- [ ] Installer Avalonia.ReactiveUI
- [ ] Configurer DI Container dans App.axaml.cs
- [ ] Créer ViewModelBase
- [ ] Créer NavigationService
- [ ] Créer EventAggregator
- [ ] Découper GameSessionViewModel en 8-10 ViewModels

### Vues (Spécifique au prototype choisi)

**Si Prototype A :**
- [ ] Créer MainWindow avec TabControl
- [ ] Créer BookingView avec GridSplitter
- [ ] Créer ValidationPanel
- [ ] Créer SegmentDetailsPanel

**Si Prototype B :**
- [ ] Créer IconSidebar
- [ ] Créer NavigationPanel
- [ ] Créer TimelineControl custom
- [ ] Créer DetailsPanel

**Si Prototype C :**
- [ ] Créer DashboardView
- [ ] Créer MetricCard widgets
- [ ] Créer ShowCard
- [ ] Intégrer LiveCharts2 (optionnel)

**Si Prototype D :**
- [ ] Créer NavigationTreeView
- [ ] Créer BookingTableView (DataGrid)
- [ ] Créer ContextPanel
- [ ] Implémenter Expanders pour navigation

### Bindings & Commands

- [ ] Implémenter ReactiveCommands pour toutes les actions
- [ ] Configurer bindings TwoWay pour édition
- [ ] Implémenter validation avec INotifyDataErrorInfo
- [ ] Tester tous les bindings

### Persistence & State

- [ ] Implémenter PreferencesService
- [ ] Sauvegarder positions des splitters
- [ ] Sauvegarder configuration de colonnes
- [ ] Auto-save avec debouncing

### Polish & Tests

- [ ] Écrire tests unitaires ViewModels
- [ ] Tester navigation flows
- [ ] Optimiser performance (virtualisation pour grandes listes)
- [ ] Ajouter animations/transitions (optionnel)

---

## 🎯 RECOMMANDATION PERSONNELLE

### Si vous voulez la **modernité** : Prototype B (Sidebar Vertical)
- Interface très moderne type VS Code
- Timeline visuelle pour les segments
- Navigation efficace
- Bon compromis complexité/modernité

### Si vous voulez du **FM26 pur** : Prototype D (Dual-pane)
- Style Football Manager 2026
- Navigation arborescente
- Table détaillée professionnelle
- Panel de contexte riche

### Si vous voulez la **simplicité** : Prototype A (Tabs Horizontal)
- Interface classique et efficace
- Facile à comprendre
- Rapide à implémenter
- Bon pour MVP/prototype rapide

### Si vous voulez du **management stratégique** : Prototype C (Dashboard)
- Vue d'ensemble stratégique
- KPIs visibles
- Widgets modulaires
- Meilleur pour prise de décision macro

---

## 📧 PROCHAINES ÉTAPES

1. **Choisissez votre prototype préféré** (A, B, C ou D)
2. Je créerai le code complet pour ce prototype :
   - MainWindow.axaml fonctionnel
   - ViewModels découpés
   - Services de navigation
   - Bindings configurés
3. On testera et itérera

**Quel prototype préférez-vous ?** 🚀
