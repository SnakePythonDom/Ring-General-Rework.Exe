# Configuration de la Base de Données - Ring General

## Correction appliquée

### Problème identifié
L'application chargeait la **mauvaise base de données** au démarrage :
- ? **Avant** : `ring_general.db` (base de sauvegarde vide)
- ? **Après** : `ring_world.db` (base de données monde avec Companies, Workers, etc.)

### Changement dans `App.axaml.cs`
```csharp
// AVANT (incorrect)
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "ring_general.db");
var factory = new SqliteConnectionFactory($"Data Source={dbPath}");

// APRÈS (correct)
var factory = new SqliteConnectionFactory();
```

## Configuration de la base de données World

La `SqliteConnectionFactory` cherche la base de données monde selon cette hiérarchie :

### 1?? Variable d'environnement (override maximum)
```
RINGGENERAL_WORLD_DB_PATH = "C:\path\to\ring_world.db"
```
À définir pour un contrôle expert ou les environnements personnalisés.

### 2?? Chemin par défaut (recommandé)
```
{AppContext.BaseDirectory}/data/ring_world.db
```
Pour une application compilée en Release/Debug, ce chemin est :
```
bin/Debug/net8.0/data/ring_world.db     (développement)
bin/Release/net8.0/data/ring_world.db   (production)
```

## Structure attendue

### Dossiers
```
src/RingGeneral.UI/bin/Debug/net8.0/
??? data/
?   ??? ring_world.db          ? Base de données monde (OBLIGATOIRE)
??? ring_general.exe
??? RingGeneral.UI.dll
??? [autres assemblies]
```

### Tables requises dans `ring_world.db`
La base de données doit contenir **au minimum** :
- `companies` (ou `Companies`) - Liste des compagnies de catch
- `workers` (ou `Workers`) - Liste des lutteurs

**Validation effectuée au démarrage** : `CreateWorldConnection()` lève une exception si ces tables ne sont pas trouvées.

## Configuration recommandée

### Pour le développement
Placer `ring_world.db` dans : `src/RingGeneral.UI/bin/Debug/net8.0/data/`

### Pour la distribution
Placer `ring_world.db` dans le répertoire binaire de l'application avec le même chemin relatif `data/ring_world.db`

### Pour les environnements personnalisés
Définir la variable d'environnement avant de lancer l'application :
```powershell
# PowerShell
$env:RINGGENERAL_WORLD_DB_PATH = "C:\custom\path\ring_world.db"

# Windows CMD
set RINGGENERAL_WORLD_DB_PATH=C:\custom\path\ring_world.db
```

## Bases de données séparées

L'application gère deux bases distinctes :

| Base | Fichier | Localisation | Contenu |
|------|---------|--------------|---------|
| **World DB** | `ring_world.db` | `bin/[config]/net8.0/data/` | Companies, Workers, Shows, Regions, Countries, Titles, Storylines, etc. |
| **Save DB** | `ring_save.db` | `%APPDATA%/RingGeneral/` | SaveGames, état de partie active (créée automatiquement) |

## Résolution des erreurs

### ? Erreur : "Table 'Companies' introuvable"
**Cause** : `ring_world.db` est manquant ou vide
**Solution** :
1. Vérifier que `ring_world.db` existe dans le répertoire `data/`
2. Vérifier que le fichier n'est pas vide (> 0 octets)
3. Exécuter une requête pour confirmer les tables : `SELECT name FROM sqlite_master WHERE type='table';`

### ?? Avertissement : Variable d'env `RINGGENERAL_WORLD_DB_PATH` définie ?
Si une variable d'environnement était définie, elle prendra la priorité.
Vérifier avec : `echo %RINGGENERAL_WORLD_DB_PATH%` (CMD) ou `$env:RINGGENERAL_WORLD_DB_PATH` (PowerShell)

## Logs de diagnostic

L'application enregistre le chemin réel utilisé dans les logs au démarrage.
Rechercher dans la sortie de debug :
```
Chemin attendu : C:\Users\...\bin\Debug\net8.0\data\ring_world.db
```
