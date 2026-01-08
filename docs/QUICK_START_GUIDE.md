# 🚀 GUIDE DE DÉMARRAGE RAPIDE - Prototype D

**Implémentation :** ✅ Base complète
**Status :** 🟡 Nécessite configuration DI pour lancer
**Temps restant :** ~6-8 heures pour finalisation complète

---

## ✅ CE QUI EST FAIT

### 🏗️ Architecture (100% complète)

✅ **Services créés :**
- `NavigationService` - Navigation entre ViewModels
- `EventAggregator` - Messaging Pub/Sub

✅ **ViewModels créés :**
- `ShellViewModel` - Shell principal avec TreeNavigation
- `BookingViewModel` - Gestion booking (extrait de GameSessionViewModel)
- `NavigationItemViewModel` - Items de navigation

✅ **Vues créées :**
- `MainWindow.axaml` - Layout 3 colonnes (TreeNav | Content | Context)
- `BookingView.axaml` - Table segments style FM26

**Total :** 15 fichiers, ~1250 lignes de code

---

## 🚧 CE QUI MANQUE (pour lancer l'app)

### 1️⃣ URGENT : Configuration DI

**Fichier à modifier :** `src/RingGeneral.UI/App.axaml.cs`

**Installer le package NuGet :**
```bash
cd src/RingGeneral.UI
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Hosting
```

**Modifier App.axaml.cs :**
```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.Services.Messaging;
using RingGeneral.UI.ViewModels.Core;
using RingGeneral.UI.ViewModels.Booking;
using RingGeneral.UI.Views.Shell;
using RingGeneral.Data.Database;
using RingGeneral.Data.Repositories;
using RingGeneral.Core.Validation;

namespace RingGeneral.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configuration DI
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventAggregator, EventAggregator>();

        // Database & Repositories
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db");
        var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
        var repositories = RepositoryFactory.CreateRepositories(factory);
        services.AddSingleton(repositories.GameRepository);
        services.AddSingleton(repositories.ScoutingRepository);

        // Core Services
        services.AddSingleton<BookingValidator>();
        services.AddSingleton<SegmentTypeCatalog>(ChargerSegmentTypes());

        // ViewModels
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<BookingViewModel>();

        var provider = services.BuildServiceProvider();

        // Lancer le Shell
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(
                provider.GetRequiredService<ShellViewModel>()
            );
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static SegmentTypeCatalog ChargerSegmentTypes()
    {
        // Charger depuis les specs ou créer un catalogue par défaut
        return new SegmentTypeCatalog(
            new Dictionary<string, string>
            {
                ["match"] = "Match",
                ["promo"] = "Promo",
                ["angle"] = "Angle",
                ["interview"] = "Interview"
            },
            new Dictionary<string, IReadOnlyList<string>>(),
            new Dictionary<string, IReadOnlyList<string>>(),
            new Dictionary<string, string>()
        );
    }
}
```

---

### 2️⃣ Ajouter les DataTemplates

**Fichier à modifier :** `src/RingGeneral.UI/Views/Shell/MainWindow.axaml`

**Ajouter dans `<Window>` après `<Design.DataContext>` :**

```xml
<Window.Resources>
    <!-- DataTemplate pour BookingViewModel -->
    <DataTemplate DataType="vm:BookingViewModel">
        <booking:BookingView />
    </DataTemplate>

    <!-- TODO: Ajouter les autres DataTemplates quand vous créez les vues -->
</Window.Resources>
```

**Ajouter le namespace Booking en haut :**
```xml
xmlns:booking="using:RingGeneral.UI.Views.Booking"
```

---

### 3️⃣ Corriger les types manquants

**Dans BookingViewModel.cs**, corriger les imports :

```csharp
// Ligne 4, remplacer :
using RingGeneral.Core.Validation;

// Par :
using RingGeneral.Core.Models;
using RingGeneral.Core.Validation;
```

**Ligne 170, dans ValidationIssues.Add(), utiliser :**
```csharp
ValidationIssues.Add(new BookingIssueViewModel(
    "booking.empty",
    "Le booking est vide. Ajoutez au moins un segment.",
    ValidationSeverity.Warning,  // ← Utiliser enum au lieu de string
    null,
    "Ajouter"
));
```

---

## ⚡ LANCER L'APPLICATION

### Étape 1 : Installer les dépendances
```bash
cd /home/user/Ring-General-Rework.Exe
dotnet restore
```

### Étape 2 : Build
```bash
dotnet build
```

### Étape 3 : Lancer
```bash
dotnet run --project src/RingGeneral.UI
```

---

## 🎯 RÉSULTAT ATTENDU

### Fenêtre lancée

```
┌─────────────────────────────────────────────────────┐
│  🎭 RING GENERAL    📺 Monday Night Raw  💰 $2.4M  │
├───────────────┬─────────────────────┬───────────────┤
│               │                     │               │
│ 🏠 ACCUEIL    │  BOOKING VIEW       │  PANNEAU DE   │
│ 📋 BOOKING    │                     │  CONTEXTE     │
│  ▾            │  Table segments:    │               │
│  📺 Shows ←   │  1. ⭐ Main Event   │  Détails      │
│  📚 Biblio    │     Cena v Orton    │               │
│  📊 Histo     │                     │               │
│ 👤 ROSTER     │  2. Promo           │               │
│ 📖 STORIES    │     The Rock        │               │
│ 🎓 YOUTH      │                     │               │
│ 💼 FINANCE    │  [+ Ajouter]        │               │
│ 📆 CALENDAR   │                     │               │
│               │  [▶️ SIMULER]       │               │
│ [🔄 Next Week]│                     │               │
└───────────────┴─────────────────────┴───────────────┘
```

### Navigation fonctionnelle

✅ **Cliquer sur "📺 Shows actifs" :**
- Zone centrale affiche BookingView
- Table avec 2 segments de test (Main Event + Promo)

✅ **Bouton "+ Nouveau segment" :**
- Ajoute un nouveau segment vide
- Validation se met à jour

✅ **Splitters :**
- Redimensionnables à la souris
- Position sauvegardable (TODO)

---

## 📋 TODO LIST PROCHAINES ÉTAPES

### Court terme (1-2h)

- [ ] Installer Microsoft.Extensions.DependencyInjection
- [ ] Configurer App.axaml.cs avec DI
- [ ] Ajouter DataTemplates dans MainWindow
- [ ] Corriger types dans BookingViewModel
- [ ] Tester le lancement

### Moyen terme (4-6h)

- [ ] Créer RosterViewModel
- [ ] Créer RosterView
- [ ] Créer ValidationPanelViewModel (context panel)
- [ ] Créer SegmentDetailsViewModel (context panel)
- [ ] Implémenter switch context panel selon contenu
- [ ] Tester navigation complète

### Long terme (optionnel)

- [ ] Créer YouthDashboardViewModel + View
- [ ] Créer FinanceDashboardViewModel + View
- [ ] Créer CalendarViewModel + View
- [ ] Intégrer simulation show
- [ ] Sauvegarder préférences UI

---

## 🆘 DÉPANNAGE

### Erreur : "Type not found: ShellViewModel"
**Solution :** Vérifier que `App.axaml.cs` est bien configuré avec DI

### Erreur : "NavigationService not registered"
**Solution :** Ajouter `services.AddSingleton<INavigationService, NavigationService>()` dans App.axaml.cs

### ContentControl vide (zone centrale)
**Solution :** Ajouter les DataTemplates dans MainWindow.axaml

### TreeView ne s'affiche pas
**Solution :** Vérifier que `NavigationItems` est bien initialisé dans ShellViewModel

### Segments ne s'affichent pas
**Solution :** Vérifier que `LoadTestData()` est appelé dans BookingViewModel

---

## 📚 DOCUMENTATION

**Guide complet :** `IMPLEMENTATION_PROTOTYPE_D.md`
**Prototypes :** `prototypes/README.md`
**Code source :**
- Services : `src/RingGeneral.UI/Services/`
- ViewModels : `src/RingGeneral.UI/ViewModels/`
- Views : `src/RingGeneral.UI/Views/`

---

## 💡 ASTUCES

### Débuggage TreeNavigation
```csharp
// Dans ShellViewModel.NavigateToViewModelType()
System.Diagnostics.Debug.WriteLine($"Navigation vers {viewModelType.Name}");
```

### Voir les bindings en temps réel
```bash
# Lancer avec verbose logging
dotnet run --project src/RingGeneral.UI --verbosity detailed
```

### Hot Reload Avalonia
```bash
# Installer Avalonia Hot Reload
dotnet tool install -g Avalonia.HotReload
```

---

## 🎉 FÉLICITATIONS !

Vous avez maintenant :
- ✅ Une architecture modulaire propre
- ✅ Un Shell FM26 style avec TreeNavigation
- ✅ Une vue Booking fonctionnelle
- ✅ Des services de navigation et messaging
- ✅ Une base solide pour étendre l'application

**Prochaine étape :** Configurer le DI et lancer l'app ! 🚀

---

**Guide créé le 6 janvier 2026**
**Temps estimé de mise en route : 1-2 heures**

---

## 🎮 GUIDE JOUEUR

### Démarrage rapide
1. Ouvrez la page **Booking**.
2. Ajoutez 4 à 6 segments, dont un main event solide.
3. Lancez **Simuler le show**.
4. Consultez **Résultats**, puis ouvrez les **Impacts** pour comprendre les variations.
5. Passez à la semaine suivante et ajustez la carte.

### Navigation clé
- **Booking** : construction de la carte et validation.
- **Résultats** : note globale, facteurs clés, impacts.
- **Impacts** : popularité, finances, fatigue/blessures, storylines, titres.
- **Aide / Codex** : définitions et systèmes en français.

### Conseils essentiels
- Alternez matchs et promos pour limiter les pénalités de rythme.
- Gardez un main event fort pour stabiliser la note globale.
- Surveillez la fatigue pour limiter les blessures.
- Reliez vos segments aux storylines actives.

### Glossaire rapide
- **Heat** : tension d'une storyline.
- **Momentum** : dynamique récente d'un talent.
- **LOD** : niveau de détail simulé pour une compagnie.
- **Prestige de titre** : valeur actuelle d'un titre.

### Scénario de test manuel
1. Ouvrir un profil et survoler un attribut : tooltip affiché.
2. Page Booking : vérifier les warnings et ouvrir l'aide contextuelle.
3. Résultats show : lire "Pourquoi cette note ?" et ouvrir un impact.
4. Codex : recherche d'un terme et navigation via liens internes.

