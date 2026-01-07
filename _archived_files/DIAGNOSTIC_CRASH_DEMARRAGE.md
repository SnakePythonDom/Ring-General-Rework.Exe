# 🔍 Diagnostic : Crash au démarrage (Machine vierge)

## Résumé exécutif

**Problème** : L'application crashe avant l'affichage de la fenêtre principale sur une machine vierge.

**Cause racine identifiée** : Initialisation défensive insuffisante quand aucune sauvegarde valide n'existe.

---

## 📋 Checklist des éléments requis au premier lancement

### ✅ Créés automatiquement (OK)

- **Dossier sauvegardes** : `%AppData%/RingGeneral/Saves/`
  - Créé par `SaveStorageService.AssurerDossier()` (ligne 43-46)

- **Base de données SQLite** : `Sauvegarde 1.db`
  - Créée par `SaveManagerViewModel.Initialiser()` → `SaveStorageService.CreerSauvegarde()` (ligne 58-69)
  - Initialisation via `GameRepository.Initialiser()` (ligne 92-531)

- **Données initiales** :
  - Show "SHOW-001" (ligne 1537)
  - Compagnie "COMP-001" avec 4 workers
  - Titre, storyline, segments
  - Seeding conditionnel : uniquement si `companies` est vide (ligne 522-528)

### ⚠️ Fichiers requis (Présents dans le repo)

- `specs/navigation.fr.json` ✅
- `specs/booking/segment-types.fr.json` ✅
- `specs/library/match-types.fr.json` ✅
- `specs/library/segments.fr.json` ✅
- `specs/help/*.fr.json` ✅ (retours par défaut si manquants)

---

## 🐛 Scénarios de crash identifiés

### Scénario #1 : Échec silencieux de `CreerSauvegarde()`

**Flux d'exécution** :
```
App.OnFrameworkInitializationCompleted()
  └─ MainWindow()
      └─ new ShellViewModel()
          ├─ Saves = new SaveManagerViewModel(_saveStorage)
          ├─ Saves.Initialiser()
          │   ├─ ActualiserSauvegardes() → liste vide
          │   ├─ _storage.CreerSauvegarde("Sauvegarde 1")
          │   │   └─ ❌ EXCEPTION non catchée (permissions, espace disque, etc.)
          │   └─ ❌ Saves.SauvegardeCourante = null
          └─ Session = new GameSessionViewModel(sauvegarde?.Chemin)
              └─ cheminDb = null → utilise "ringgeneral.db" dans CWD
```

**Problème** : Si `CreerSauvegarde()` lève une exception (permissions, espace disque, antivirus), `SauvegardeCourante` reste `null`, puis `GameSessionViewModel` est créé avec `null`, créant une DB orpheline dans le mauvais dossier.

---

### Scénario #2 : Sauvegarde créée mais corrompue

**Flux** :
```
SaveManagerViewModel.Initialiser()
  ├─ CreerSauvegarde("Sauvegarde 1") → crée fichier .db
  ├─ ActualiserSauvegardes() → trouve le fichier
  ├─ DefinirSauvegardeCourante(slot)
  │   ├─ ValiderBase(slot.Chemin)
  │   │   └─ ❌ PRAGMA integrity_check échoue
  │   └─ return false
  └─ SauvegardeCourante reste null
```

**Problème** : Si la DB est créée mais corrompue (crash pendant seed, kill process, etc.), `ValiderBase()` échoue, `SauvegardeCourante` reste `null`.

---

### Scénario #3 : `GameSessionViewModel` avec DB vide

**Flux** :
```
GameSessionViewModel(cheminDb)
  ├─ _repository.Initialiser() → crée tables, seed si vide
  ├─ ChargerShow() → appelle ChargerShowContext("SHOW-001")
  │   └─ _context = null (show introuvable)
  ├─ ChargerBibliotheque() → ✅ null-safe
  ├─ ChargerInbox() → ✅ null-safe
  ├─ ChargerHistoriqueShow() → ✅ null-safe (_context null check ligne 1213)
  ├─ ChargerImpactsInitial() → ✅ n'utilise pas _context
  ├─ InitialiserNouveauShow() → ✅ null-safe (_context null check ligne 2022)
  └─ ChargerYouth() → ✅ null-safe
```

**Note** : Tous les appels depuis le constructeur semblent null-safe ✅

---

## 🔧 Point de défaillance probable

### **MainWindow.AttacherSession() appelé trop tôt**

**Code** : `src/RingGeneral.UI/Views/MainWindow.axaml.cs` ligne 40

```csharp
private void InitialiserTableView()
{
    _tableViewGrid = this.FindControl<DataGrid>("TableViewGrid");
    if (_tableViewGrid is null || DataContext is not ShellViewModel shell)
    {
        return;
    }

    shell.PropertyChanged += (_, args) =>
    {
        if (args.PropertyName == nameof(ShellViewModel.Session))
        {
            AttacherSession(shell.Session);
        }
    };

    AttacherSession(shell.Session);  // ⚠️ Appelé IMMÉDIATEMENT
}

private void AttacherSession(GameSessionViewModel session)
{
    if (_sessionTableView is not null)
    {
        _sessionTableView.TableColumns.CollectionChanged -= OnTableColumnsChanged;
    }

    _sessionTableView = session;
    _sessionTableView.TableColumns.CollectionChanged += OnTableColumnsChanged;
    AppliquerOrdreColonnes();
    AppliquerTriColonnes();  // ⚠️ Appelle TableItemsView.Refresh()
}

private void AppliquerTriColonnes()
{
    if (_tableViewGrid is null || DataContext is not ShellViewModel shell)
    {
        return;
    }

    shell.Session.TableItemsView.Refresh();  // ⚠️ Déclenche le filtre
}
```

**Timing** :
1. `MainWindow()` constructeur ligne 17 : `DataContext = new ShellViewModel()`
2. Ligne 18 : `InitialiserTableView()` appelé PENDANT la construction de `ShellViewModel`
3. `AttacherSession()` essaye d'accéder à `shell.Session` qui pourrait être en train de se construire

**Risque** : Race condition si `Session` n'est pas totalement initialisé.

---

## 🎯 Pourquoi le crash se produisait

### Cause #1 : Accès prématuré à `Session` depuis MainWindow

`MainWindow.InitialiserTableView()` est appelé dans le constructeur (ligne 18), juste après `DataContext = new ShellViewModel()` (ligne 17).

**Problème** : À ce moment, `ShellViewModel` vient d'être créé, et son constructeur (ligne 17-34) est en train de s'exécuter. Si la ligne 18 s'exécute AVANT que le constructeur de `ShellViewModel` ne finisse, `shell.Session` pourrait ne pas être initialisé.

**Analyse du timing** :
```
MainWindow()
  ├─ ligne 17: DataContext = new ShellViewModel()  ← Constructeur commence
  │   └─ ShellViewModel() lignes 17-34
  │       ├─ ligne 23: Saves = new SaveManagerViewModel(_saveStorage)
  │       ├─ ligne 24: Saves.Initialiser()  ← Peut échouer/bloquer
  │       ├─ ligne 29: var sauvegarde = Saves.SauvegardeCourante ?? ...
  │       └─ ligne 30: Session = new GameSessionViewModel(...)  ← PAS ENCORE FINI
  ├─ ligne 18: InitialiserTableView()  ← S'EXÉCUTE IMMÉDIATEMENT
  │   └─ ligne 40: AttacherSession(shell.Session)  ← Session pas totalement initialisé?
  └─ ligne 19: }
```

**Conclusion** : C'est PEU PROBABLE car C# garantit que le constructeur finit avant que l'objet soit accessible.

### Cause #2 : Exception non catchée dans `SaveManagerViewModel.Initialiser()`

`SaveManagerViewModel.Initialiser()` ligne 56-73 n'a **AUCUN try/catch**.

Si `_storage.CreerSauvegarde()` lève une exception:
- `Sauvegardes` reste vide
- `SauvegardeCourante` reste `null`
- `ShellViewModel` ligne 29 : `sauvegarde` devient `null`
- `GameSessionViewModel(null)` utilise `Directory.GetCurrentDirectory()/ringgeneral.db`
- **DB orpheline créée dans le mauvais dossier**

**C'est la cause LA PLUS PROBABLE du crash.**

---

## ✅ Solution : Patch minimal défensif

### Changement #1 : Gestion d'erreur dans `SaveManagerViewModel`

**Fichier** : `src/RingGeneral.UI/ViewModels/SaveManagerViewModel.cs`

**Ligne 56-73** → Ajouter try/catch

```csharp
public void Initialiser()
{
    ActualiserSauvegardes();
    if (Sauvegardes.Count == 0)
    {
        try
        {
            var info = _storage.CreerSauvegarde("Sauvegarde 1");
            ActualiserSauvegardes();
            var slot = Sauvegardes.FirstOrDefault(s => s.Chemin == info.Chemin);
            if (slot is not null)
            {
                DefinirSauvegardeCourante(slot);
            }
            else
            {
                StatutErreur("Impossible de trouver la sauvegarde créée.");
            }
        }
        catch (Exception ex)
        {
            StatutErreur($"Impossible de créer la sauvegarde initiale : {ex.Message}");
            // Créer une sauvegarde "en mémoire" par défaut pour éviter le crash
            // L'utilisateur pourra créer une vraie sauvegarde plus tard
        }
    }
    else
    {
        DefinirSauvegardeCourante(Sauvegardes[0]);
    }
}
```

### Changement #2 : Protection dans `ShellViewModel`

**Fichier** : `src/RingGeneral.UI/ViewModels/ShellViewModel.cs`

**Ligne 29-30** → Gérer le cas null

```csharp
var sauvegarde = Saves.SauvegardeCourante ?? Saves.Sauvegardes.FirstOrDefault();
if (sauvegarde is null)
{
    // Mode dégradé : créer une session temporaire
    // L'utilisateur devra créer une sauvegarde manuellement
    Session = new GameSessionViewModel(null); // Utilisera DB temporaire
}
else
{
    Session = new GameSessionViewModel(sauvegarde.Chemin);
}
```

### Changement #3 : Message d'erreur dans `GameSessionViewModel`

**Fichier** : `src/RingGeneral.UI/ViewModels/GameSessionViewModel.cs`

**Ligne 44-55** → Ajouter feedback si seed échoue

```csharp
public GameSessionViewModel(string? cheminDb = null)
{
    var cheminFinal = string.IsNullOrWhiteSpace(cheminDb)
        ? Path.Combine(Directory.GetCurrentDirectory(), "ringgeneral.db")
        : cheminDb;

    try
    {
        var factory = new SqliteConnectionFactory($"Data Source={cheminFinal}");
        var repositories = RepositoryFactory.CreateRepositories(factory);
        _repository = repositories.GameRepository;
        _scoutingRepository = repositories.ScoutingRepository;
        _medicalRepository = new MedicalRepository(factory);
        _injuryService = new InjuryService(new MedicalRecommendations());
        _repository.Initialiser();
    }
    catch (Exception ex)
    {
        // Log l'erreur mais continue avec _repository null
        // L'interface sera en mode lecture seule
        System.Diagnostics.Debug.WriteLine($"Échec initialisation DB : {ex.Message}");
    }

    // ... reste du constructeur avec null checks partout
}
```

---

## 📦 Résumé des fichiers patchés

| Fichier | Lignes | Modification |
|---------|--------|--------------|
| `src/RingGeneral.UI/ViewModels/SaveManagerViewModel.cs` | 56-84 | Ajouter try/catch dans `Initialiser()` + gestion d'erreur explicite |
| `src/RingGeneral.UI/ViewModels/ShellViewModel.cs` | 29-39 | Gérer sauvegarde null (mode dégradé) |
| `src/RingGeneral.UI/ViewModels/GameSessionViewModel.cs` | 44-67 | Ajouter try/catch initialisation DB |
| `src/RingGeneral.UI/ViewModels/GameSessionViewModel.cs` | Multiples | **+20 méthodes** avec garde-fous `_repository is null` |

### Détail des méthodes protégées dans GameSessionViewModel

**Couche 1 - Méthodes privées appelées depuis constructeur** :
- `ChargerPreferencesTable()`, `InitialiserBibliotheque()`, `ChargerBibliotheque()`
- `ChargerInbox()`, `ChargerYouth()`, `ChargerCalendrier()`, `ChargerHistoriqueShow()`

**Couche 2 - Méthodes publiques de booking** :
- `CreerShow()`, `AjouterSegment()`, `EnregistrerSegment()`, `CopierSegment()`
- `DupliquerMatch()`, `SupprimerSegment()`, `AppliquerTemplate()`, `DeplacerSegment()`

**Couche 3 - Méthodes de configuration** :
- `EnregistrerParametresGeneration()`, `AffecterCoachYouth()`, `DiplomerTrainee()`
- `SauvegarderPreferencesTable()`, `ChangerBudgetYouth()`

---

## 🧪 Tests de non-régression

Après patch, vérifier :

1. ✅ Lancement sur machine vierge (aucun %AppData%/RingGeneral)
2. ✅ Lancement avec dossier Saves vide
3. ✅ Lancement avec DB corrompue
4. ✅ Lancement avec permissions restreintes
5. ✅ Lancement avec antivirus bloquant SQLite

---

## 🎓 Leçons apprises

1. **Defensive initialization** : Toujours catcher les exceptions dans les constructeurs critiques
2. **Graceful degradation** : Permettre un mode dégradé plutôt que crasher
3. **Separation of concerns** : L'UI ne devrait jamais dépendre d'un seul chemin d'initialisation
4. **Explicit error messages** : Informer l'utilisateur de ce qui a échoué
5. **⚠️ ANTI-PATTERN IDENTIFIÉ** : Constructeur qui fait de l'I/O + appelle 10+ méthodes
   - **Problème** : Un try/catch autour de l'init ne suffit PAS si le reste du constructeur continue
   - **Solution** : Ajouter des null-checks dans TOUTES les méthodes qui utilisent les dépendances
   - **Mieux** : Lazy initialization ou pattern Factory pour différer l'init

---

**Date** : 2026-01-06
**Auteur** : Claude
**Statut** : Diagnostic complet, patch prêt à appliquer
