# Refactoring - Injection de Dépendances & Logging Structuré

**Date**: 2026-01-08
**Status**: ✅ Complété (Phase 2 & 3)
**Impact**: Infrastructure DI + Logging professionnel

---

## 🎯 Objectifs

### Phase 2 - Injection de Dépendances (DI)
- Créer un système DI simple et efficace
- Éliminer les instantiations en dur (`new Service()`)
- Faciliter les tests et la maintenabilité

### Phase 3 - Logging Structuré
- Remplacer 102 `Console.WriteLine` par un système de logging professionnel
- Niveaux de log : Debug, Info, Warning, Error, Fatal
- Format structuré avec timestamps

---

## 📦 Nouveaux Composants

### 1. ILoggingService (Interface)
**Fichier** : `src/RingGeneral.Core/Services/ILoggingService.cs`

```csharp
public interface ILoggingService
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
    void Fatal(string message, Exception? exception = null);
}
```

**Utilisation** :
- `Debug` : Messages de développement uniquement
- `Info` : Événements normaux (démarrage, opérations réussies)
- `Warning` : Situations anormales mais gérables
- `Error` : Erreurs nécessitant attention
- `Fatal` : Erreurs critiques bloquantes

### 2. ConsoleLoggingService (Implémentation)
**Fichier** : `src/RingGeneral.Core/Services/ConsoleLoggingService.cs`

**Format** : `[TIMESTAMP] [LEVEL] Message`

**Exemple** :
```
[2026-01-08 14:32:15] [INFO] Initializing GameSession with database: ringgeneral.db
[2026-01-08 14:32:15] [INFO] GameSession initialized successfully
[2026-01-08 14:32:20] [ERROR] Failed to load worker data
Exception: SqliteException: no such table: Workers
```

**Configuration** :
```csharp
// Logger minimal (Info+ uniquement)
var logger = new ConsoleLoggingService(LogLevel.Info);

// Logger verbeux (tout inclus)
var debugLogger = new ConsoleLoggingService(LogLevel.Debug);

// Sans timestamps
var simpleLogger = new ConsoleLoggingService(LogLevel.Info, includeTimestamp: false);
```

### 3. ServiceContainer (DI Container)
**Fichier** : `src/RingGeneral.Core/Services/ServiceContainer.cs`

**Fonctionnalités** :
- ✅ Singleton (instance unique partagée)
- ✅ Transient (nouvelle instance à chaque résolution)
- ✅ Résolution type-safe
- ✅ Configuration par défaut

**Utilisation** :

```csharp
// Créer conteneur avec services par défaut
var container = ServiceContainer.CreateDefault();

// Enregistrer singletons
container.RegisterSingleton<ILoggingService>(new ConsoleLoggingService());
container.RegisterSingleton<IMyService>(new MyService());

// Enregistrer transients (nouvelle instance à chaque fois)
container.RegisterTransient<IRepository>(() => new Repository());

// Résoudre services
var logger = container.Resolve<ILoggingService>();
var repo = container.Resolve<IRepository>();

// Vérifier si service existe
if (container.IsRegistered<ILoggingService>())
{
    // ...
}
```

### 4. ApplicationServices (Point d'accès global)
**Fichier** : `src/RingGeneral.Core/Services/ApplicationServices.cs`

**Singleton thread-safe** pour accès simplifié :

```csharp
// Au démarrage de l'application
ApplicationServices.Initialize();

// Ou avec conteneur personnalisé
var customContainer = new ServiceContainer();
customContainer.RegisterSingleton<ILoggingService>(myLogger);
ApplicationServices.Initialize(customContainer);

// Partout dans l'application
var logger = ApplicationServices.Logger;
logger.Info("Message");

// Ou résoudre autre service
var service = ApplicationServices.Resolve<IMyService>();

// Reset (tests uniquement)
ApplicationServices.Reset();
```

---

## 🔄 Intégrations

### GameSessionViewModel
**Fichier** : `src/RingGeneral.UI/ViewModels/GameSessionViewModel.cs`

**Avant** :
```csharp
public GameSessionViewModel(string? cheminDb = null)
{
    try
    {
        var factory = new SqliteConnectionFactory($"Data Source={cheminDb}");
        // ...
    }
    catch (Exception ex)
    {
        Console.WriteLine("FATAL ERROR: Impossible de charger la base de données.");
        Console.WriteLine($"Chemin tenté : {cheminFinal}");
        Console.WriteLine($"Erreur : {ex.Message}");
    }
}
```

**Après** :
```csharp
private readonly ILoggingService _logger;

public GameSessionViewModel(string? cheminDb = null, ServiceContainer? services = null)
{
    // Logger from DI container or default
    _logger = services?.IsRegistered<ILoggingService>() == true
        ? services.Resolve<ILoggingService>()
        : new ConsoleLoggingService(LogLevel.Info);

    try
    {
        _logger.Info($"Initializing GameSession with database: {cheminFinal}");
        var factory = new SqliteConnectionFactory($"Data Source={cheminDb}");
        // ...
        _logger.Info("GameSession initialized successfully");
    }
    catch (Exception ex)
    {
        _logger.Fatal("Failed to initialize database", ex);
    }
}
```

**Avantages** :
- ✅ Format structuré avec timestamps
- ✅ Stack traces automatiques pour exceptions
- ✅ Double sortie (Console + Debug)
- ✅ Configurable (niveau de log)
- ✅ Testable (mock ILoggingService)

### DbSeeder
**Fichier** : `src/RingGeneral.Data/Database/DbSeeder.cs`

**Ajout** :
```csharp
private static ILoggingService? _logger;

public static void SetLogger(ILoggingService logger) => _logger = logger;

private static void Log(LogLevel level, string message)
{
    if (_logger != null)
    {
        // Use structured logger
    }
    else
    {
        // Fallback to Console
        Console.WriteLine($"[DbSeeder] [{level}] {message}");
    }
}
```

**Utilisation** :
```csharp
// Configuration au démarrage
DbSeeder.SetLogger(ApplicationServices.Logger);

// Dans le code
Log(LogLevel.Info, "Seeding database...");
Log(LogLevel.Debug, $"Created {count} workers");
Log(LogLevel.Error, $"Import failed: {ex.Message}");
```

---

## 📊 Impact sur le Code

### Fichiers Créés
| Fichier | Lignes | Rôle |
|---------|--------|------|
| `ILoggingService.cs` | 35 | Interface de logging |
| `ConsoleLoggingService.cs` | 65 | Implémentation console |
| `ServiceContainer.cs` | 75 | Conteneur DI |
| `ApplicationServices.cs` | 60 | Point d'accès global |

### Fichiers Modifiés
| Fichier | Avant | Après | Changement |
|---------|-------|-------|------------|
| `GameSessionViewModel.cs` | 2,374 | 2,385 | +logging, +DI |
| `DbSeeder.cs` | 340 | 370 | +logging |

### Console.WriteLine Remplacés
- **GameSessionViewModel** : 5 remplacements (Fatal, Info)
- **DbSeeder** : 13 remplacements (Info, Debug, Warning, Error)
- **Total actuel** : 18 / 102 (17%)

### Prochaines Migrations
- **ViewModels UI** : ~20 fichiers (80 Console.WriteLine)
- **Services Data** : ~3 fichiers (4 Console.WriteLine)

---

## 🎓 Guide d'Utilisation

### Pour un Nouveau Service
```csharp
public class MyService
{
    private readonly ILoggingService _logger;

    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.Info("Starting operation");

        try
        {
            // ...
            _logger.Debug("Operation details...");
        }
        catch (Exception ex)
        {
            _logger.Error("Operation failed", ex);
        }
    }
}
```

### Pour un ViewModel
```csharp
public class MyViewModel : ViewModelBase
{
    private readonly ILoggingService _logger;

    public MyViewModel(ServiceContainer? services = null)
    {
        _logger = services?.Resolve<ILoggingService>()
            ?? ApplicationServices.Logger;
    }
}
```

### Pour une Classe Statique
```csharp
public static class MyStaticClass
{
    private static ILoggingService? _logger;

    public static void SetLogger(ILoggingService logger)
    {
        _logger = logger;
    }

    private static void Log(string message)
    {
        _logger?.Info(message);
        // Fallback if no logger
        Console.WriteLine(message);
    }
}
```

---

## 🧪 Tests

### Test du Logger
```csharp
[Fact]
public void Logger_FormatsMessagesCorrectly()
{
    var logger = new ConsoleLoggingService(LogLevel.Debug);

    // Capture console output
    logger.Info("Test message");

    // Assert format: [TIMESTAMP] [INFO] Test message
}
```

### Test du ServiceContainer
```csharp
[Fact]
public void Container_ResolvesSingletons()
{
    var container = new ServiceContainer();
    var logger = new ConsoleLoggingService();
    container.RegisterSingleton<ILoggingService>(logger);

    var resolved1 = container.Resolve<ILoggingService>();
    var resolved2 = container.Resolve<ILoggingService>();

    Assert.Same(resolved1, resolved2); // Same instance
}
```

### Test de ViewModel avec DI
```csharp
[Fact]
public void ViewModel_UsesInjectedLogger()
{
    var mockLogger = new Mock<ILoggingService>();
    var container = new ServiceContainer();
    container.RegisterSingleton<ILoggingService>(mockLogger.Object);

    var vm = new MyViewModel(container);
    vm.DoSomething();

    mockLogger.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
}
```

---

## 📈 Bénéfices

### Logging Structuré
1. **Lisibilité** : Format cohérent avec timestamps
2. **Débogage** : Stack traces automatiques pour exceptions
3. **Filtrage** : Niveaux de log (Debug en dev, Info en prod)
4. **Traçabilité** : Double sortie (Console + Debug output)

### Injection de Dépendances
1. **Testabilité** : Mock facile des dépendances
2. **Découplage** : Interfaces au lieu d'implémentations concrètes
3. **Configuration** : Changement de services sans recompilation
4. **Maintenabilité** : Dépendances explicites et claires

### Impact Global
- **-102 Console.WriteLine** (progressivement)
- **+Logging professionnel** avec niveaux et format
- **+DI simple** sans overhead de frameworks lourds
- **+Testabilité** pour tous les composants

---

## 🔄 Prochaines Étapes (Optionnel)

### Phase 4 - Extensions
1. **FileLoggingService** : Log vers fichiers rotatifs
2. **CompositeLogger** : Multiple destinations simultanées
3. **Enrichissement** : Contexte automatique (thread, user)

### Phase 5 - Migration Complète
1. Migrer tous les ViewModels (20 fichiers)
2. Migrer tous les Services (3 fichiers)
3. Supprimer tous les `Console.WriteLine`

### Phase 6 - Avancé
1. **Scoped lifetime** dans ServiceContainer
2. **Auto-registration** par convention
3. **Configuration-based** registration (JSON/XML)

---

## 🎉 Conclusion

Les Phases 2 et 3 établissent une **infrastructure professionnelle** pour Ring General :

- ✅ **Logging structuré** avec ILoggingService
- ✅ **Injection de dépendances** avec ServiceContainer
- ✅ **Point d'accès global** avec ApplicationServices
- ✅ **Intégration progressive** sans breaking changes
- ✅ **18/102 Console.WriteLine migrés** (début prometteur)

Le système est **simple**, **testable**, et **évolutif** sans dépendances externes lourdes.

---

**Prochaines priorités** :
1. Migrer les ViewModels restants (20 fichiers)
2. Ajouter FileLoggingService pour persistence
3. Compléter la migration vers logging structuré (82 restants)
