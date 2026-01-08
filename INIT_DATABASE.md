# 🗄️ Ring General - Guide d'Initialisation de la Base de Données

**Version:** 1.0.0
**Date:** 2026-01-08
**Auteur:** Claude

---

## 📋 Vue d'Ensemble

Ce guide explique comment initialiser complètement la base de données Ring General avec :
- ✅ Schéma complet (60+ tables)
- ✅ 30 attributs de performance (In-Ring, Entertainment, Story)
- ✅ 10 attributs mentaux (Système Personnalités - Phase 8)
- ✅ Import automatique depuis BAKI1.1.db
- ✅ Génération intelligente des attributs mentaux

---

## 🎯 Méthodes d'Initialisation

### Méthode 1: Script Automatique (Recommandé) 🚀

```bash
# Depuis la racine du projet
chmod +x init.sh
./init.sh
```

Cette méthode exécute automatiquement toutes les étapes ci-dessous.

### Méthode 2: Manuelle (Étape par Étape) 🔧

Pour une compréhension complète ou pour déboguer, suivez les étapes manuelles ci-dessous.

---

## 📂 Fichiers Nécessaires

| Fichier | Description | Requis |
|---------|-------------|--------|
| `src/RingGeneral.Data/Migrations/Base_Schema.sql` | Schéma complet (60+ tables) | ✅ Oui |
| `src/RingGeneral.Data/Migrations/ImportWorkersFromBaki.sql` | Import depuis BAKI1.1.db | ✅ Oui |
| `BAKI1.1.db` | Base de données source | ✅ Oui |

---

## 🔧 Étape 1: Vérification des Prérequis

### 1.1. Vérifier SQLite

```bash
sqlite3 --version
# Doit afficher: 3.x.x ou supérieur
```

Si SQLite n'est pas installé :

```bash
# Ubuntu/Debian
sudo apt-get install sqlite3

# macOS
brew install sqlite

# Windows
# Télécharger depuis https://www.sqlite.org/download.html
```

### 1.2. Vérifier BAKI1.1.db

```bash
# Vérifier que le fichier existe
ls -lh BAKI1.1.db

# Vérifier que la DB est valide
sqlite3 BAKI1.1.db "SELECT COUNT(*) FROM workers;"
# Doit afficher un nombre (ex: 500)
```

**⚠️ Important:** Le fichier `BAKI1.1.db` doit être à la racine du projet.

---

## 🏗️ Étape 2: Création du Schéma de Base

### 2.1. Supprimer l'ancienne DB (si elle existe)

```bash
# ⚠️ ATTENTION: Cela supprime toutes les données existantes !
rm -f ringgeneral.db
```

### 2.2. Créer la nouvelle DB avec le schéma complet

```bash
sqlite3 ringgeneral.db < src/RingGeneral.Data/Migrations/Base_Schema.sql
```

### 2.3. Vérifier la création

```bash
sqlite3 ringgeneral.db "
SELECT
    '✅ Tables créées' AS Status,
    COUNT(*) AS Count
FROM sqlite_master
WHERE type='table';
"
```

**Résultat attendu:** `Count` devrait être >= 60

### 2.4. Vérifier les tables critiques

```bash
sqlite3 ringgeneral.db "
SELECT name FROM sqlite_master
WHERE type='table'
  AND name IN (
    'Workers',
    'WorkerInRingAttributes',
    'WorkerEntertainmentAttributes',
    'WorkerStoryAttributes',
    'WorkerMentalAttributes'
  )
ORDER BY name;
"
```

**Résultat attendu:** Les 5 tables doivent être listées.

---

## 📥 Étape 3: Import depuis BAKI1.1.db

### 3.1. Vérifier BAKI1.1.db

```bash
sqlite3 BAKI1.1.db "
SELECT
    COUNT(*) AS WorkerCount,
    MIN(Age) AS MinAge,
    MAX(Age) AS MaxAge,
    ROUND(AVG(in_ring), 1) AS AvgInRing
FROM workers;
"
```

### 3.2. Exécuter l'import complet

```bash
sqlite3 ringgeneral.db < src/RingGeneral.Data/Migrations/ImportWorkersFromBaki.sql
```

**⏱️ Durée estimée:** 5-30 secondes selon le nombre de workers

### 3.3. Vérifier l'import

```bash
sqlite3 ringgeneral.db "
-- Nombre de workers importés
SELECT '1. Workers importés' AS Check_Name, COUNT(*) AS Count FROM Workers
UNION ALL
-- Attributs In-Ring générés
SELECT '2. In-Ring Attributes', COUNT(*) FROM WorkerInRingAttributes
UNION ALL
-- Attributs Entertainment générés
SELECT '3. Entertainment Attributes', COUNT(*) FROM WorkerEntertainmentAttributes
UNION ALL
-- Attributs Story générés
SELECT '4. Story Attributes', COUNT(*) FROM WorkerStoryAttributes
UNION ALL
-- Attributs Mentaux générés
SELECT '5. Mental Attributes', COUNT(*) FROM WorkerMentalAttributes;
"
```

**Résultat attendu:** Tous les counts doivent être identiques (nombre de workers).

---

## 🎭 Étape 4: Vérification du Système Personnalités

### 4.1. Vérifier les attributs mentaux

```bash
sqlite3 ringgeneral.db "
SELECT
    'Ambition' AS Attribut,
    ROUND(AVG(Ambition), 1) AS Moyenne,
    MIN(Ambition) AS Min,
    MAX(Ambition) AS Max
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Loyauté', ROUND(AVG(Loyauté), 1), MIN(Loyauté), MAX(Loyauté)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Professionnalisme', ROUND(AVG(Professionnalisme), 1), MIN(Professionnalisme), MAX(Professionnalisme)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Égoïsme', ROUND(AVG(Égoïsme), 1), MIN(Égoïsme), MAX(Égoïsme)
FROM WorkerMentalAttributes
UNION ALL
SELECT 'Influence', ROUND(AVG(Influence), 1), MIN(Influence), MAX(Influence)
FROM WorkerMentalAttributes;
"
```

**Résultat attendu:**
- Moyenne entre 9.0 et 13.0 pour chaque attribut
- Min >= 0
- Max <= 20

### 4.2. Top 10 Professionnels

```bash
sqlite3 ringgeneral.db "
SELECT
    w.Name,
    wma.Professionnalisme AS Pro,
    wma.Sportivité AS Sport,
    wma.Loyauté AS Loy,
    ROUND((wma.Professionnalisme + wma.Sportivité + wma.Loyauté) / 3.0, 1) AS ProfScore
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
ORDER BY ProfScore DESC
LIMIT 10;
"
```

### 4.3. Top 10 Égoïstes (Red Flags potentiels)

```bash
sqlite3 ringgeneral.db "
SELECT
    w.Name,
    wma.Égoïsme AS Ego,
    wma.Tempérament AS Temp,
    wma.Sportivité AS Sport,
    wma.Professionnalisme AS Pro
FROM Workers w
INNER JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
ORDER BY wma.Égoïsme DESC, wma.Sportivité ASC
LIMIT 10;
"
```

---

## 🎨 Étape 5: Vérification des Attributs de Performance

### 5.1. Moyennes In-Ring

```bash
sqlite3 ringgeneral.db "
SELECT
    w.Name,
    wir.InRingAvg,
    wir.Striking,
    wir.Grappling,
    wir.HighFlying
FROM Workers w
INNER JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
ORDER BY wir.InRingAvg DESC
LIMIT 10;
"
```

### 5.2. Moyennes Entertainment

```bash
sqlite3 ringgeneral.db "
SELECT
    w.Name,
    wea.EntertainmentAvg,
    wea.Charisma,
    wea.MicWork,
    wea.StarPower
FROM Workers w
INNER JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
ORDER BY wea.EntertainmentAvg DESC
LIMIT 10;
"
```

### 5.3. Moyennes Story

```bash
sqlite3 ringgeneral.db "
SELECT
    w.Name,
    wsa.StoryAvg,
    wsa.CharacterDepth,
    wsa.HeelPerformance,
    wsa.BabyfacePerformance
FROM Workers w
INNER JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
ORDER BY wsa.StoryAvg DESC
LIMIT 10;
"
```

---

## 📊 Étape 6: Tests d'Intégrité

### 6.1. Vérifier les contraintes Foreign Key

```bash
sqlite3 ringgeneral.db "
PRAGMA foreign_key_check;
"
```

**Résultat attendu:** Aucun résultat (vide = OK)

### 6.2. Vérifier workers sans attributs

```bash
sqlite3 ringgeneral.db "
SELECT
    'Workers sans In-Ring' AS Check_Type,
    COUNT(*) AS Count
FROM Workers w
LEFT JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
WHERE wir.WorkerId IS NULL
UNION ALL
SELECT 'Workers sans Entertainment', COUNT(*)
FROM Workers w
LEFT JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
WHERE wea.WorkerId IS NULL
UNION ALL
SELECT 'Workers sans Story', COUNT(*)
FROM Workers w
LEFT JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
WHERE wsa.WorkerId IS NULL
UNION ALL
SELECT 'Workers sans Mental', COUNT(*)
FROM Workers w
LEFT JOIN WorkerMentalAttributes wma ON w.Id = wma.WorkerId
WHERE wma.WorkerId IS NULL;
"
```

**Résultat attendu:** Tous les counts = 0

### 6.3. Vérifier les CHECK constraints

```bash
sqlite3 ringgeneral.db "
-- Vérifier que tous les attributs In-Ring sont dans [0, 100]
SELECT 'In-Ring hors limites' AS Check_Type, COUNT(*) AS Count
FROM WorkerInRingAttributes
WHERE Striking NOT BETWEEN 0 AND 100
   OR Grappling NOT BETWEEN 0 AND 100
   OR HighFlying NOT BETWEEN 0 AND 100
   OR Powerhouse NOT BETWEEN 0 AND 100
   OR Timing NOT BETWEEN 0 AND 100
   OR Selling NOT BETWEEN 0 AND 100
   OR Psychology NOT BETWEEN 0 AND 100
   OR Stamina NOT BETWEEN 0 AND 100
   OR Safety NOT BETWEEN 0 AND 100
   OR HardcoreBrawl NOT BETWEEN 0 AND 100

UNION ALL

-- Vérifier que tous les attributs mentaux sont dans [0, 20]
SELECT 'Mental hors limites', COUNT(*)
FROM WorkerMentalAttributes
WHERE Ambition NOT BETWEEN 0 AND 20
   OR Loyauté NOT BETWEEN 0 AND 20
   OR Professionnalisme NOT BETWEEN 0 AND 20
   OR Pression NOT BETWEEN 0 AND 20
   OR Tempérament NOT BETWEEN 0 AND 20
   OR Égoïsme NOT BETWEEN 0 AND 20
   OR Détermination NOT BETWEEN 0 AND 20
   OR Adaptabilité NOT BETWEEN 0 AND 20
   OR Influence NOT BETWEEN 0 AND 20
   OR Sportivité NOT BETWEEN 0 AND 20;
"
```

**Résultat attendu:** Tous les counts = 0

---

## 🎯 Étape 7: Rapport Final de Validation

### 7.1. Exécuter le rapport complet

```bash
sqlite3 ringgeneral.db "
SELECT '========================================' AS '';
SELECT '🗄️  RING GENERAL DATABASE REPORT' AS '';
SELECT '========================================' AS '';
SELECT '' AS '';

SELECT '📊 TABLES' AS '';
SELECT '--------' AS '';
SELECT 'Total tables: ' || COUNT(*) AS '' FROM sqlite_master WHERE type='table';
SELECT '' AS '';

SELECT '👤 WORKERS' AS '';
SELECT '--------' AS '';
SELECT 'Total workers: ' || COUNT(*) AS '' FROM Workers;
SELECT 'Average Age: ' || ROUND(AVG(Age), 1) AS '' FROM Workers WHERE Age IS NOT NULL;
SELECT 'Average Popularity: ' || ROUND(AVG(Popularity), 1) AS '' FROM Workers;
SELECT '' AS '';

SELECT '🥊 PERFORMANCE ATTRIBUTES' AS '';
SELECT '-------------------------' AS '';
SELECT 'Average In-Ring: ' || ROUND(AVG(InRingAvg), 1) AS '' FROM WorkerInRingAttributes;
SELECT 'Average Entertainment: ' || ROUND(AVG(EntertainmentAvg), 1) AS '' FROM WorkerEntertainmentAttributes;
SELECT 'Average Story: ' || ROUND(AVG(StoryAvg), 1) AS '' FROM WorkerStoryAttributes;
SELECT '' AS '';

SELECT '🎭 MENTAL ATTRIBUTES (Phase 8)' AS '';
SELECT '------------------------------' AS '';
SELECT 'Workers with Mental Attributes: ' || COUNT(*) AS '' FROM WorkerMentalAttributes;
SELECT 'Average Professionnalisme: ' || ROUND(AVG(Professionnalisme), 1) AS '' FROM WorkerMentalAttributes;
SELECT 'Average Égoïsme: ' || ROUND(AVG(Égoïsme), 1) AS '' FROM WorkerMentalAttributes;
SELECT 'Average Influence: ' || ROUND(AVG(Influence), 1) AS '' FROM WorkerMentalAttributes;
SELECT '' AS '';

SELECT '✅ INTEGRITY CHECKS' AS '';
SELECT '------------------' AS '';
SELECT CASE
    WHEN (SELECT COUNT(*) FROM Workers w LEFT JOIN WorkerInRingAttributes a ON w.Id = a.WorkerId WHERE a.WorkerId IS NULL) = 0
    THEN '✅ All workers have In-Ring attributes'
    ELSE '❌ Some workers missing In-Ring attributes'
END AS '';
SELECT CASE
    WHEN (SELECT COUNT(*) FROM Workers w LEFT JOIN WorkerMentalAttributes a ON w.Id = a.WorkerId WHERE a.WorkerId IS NULL) = 0
    THEN '✅ All workers have Mental attributes'
    ELSE '❌ Some workers missing Mental attributes'
END AS '';
SELECT '' AS '';

SELECT '========================================' AS '';
SELECT '✅ DATABASE INITIALIZATION COMPLETE!' AS '';
SELECT '========================================' AS '';
"
```

---

## 🚨 Dépannage

### Problème 1: "BAKI1.1.db not found"

**Solution:**
```bash
# Vérifier l'emplacement du fichier
find . -name "BAKI1.1.db"

# Si trouvé ailleurs, copier à la racine
cp /chemin/vers/BAKI1.1.db .
```

### Problème 2: "table Workers already exists"

**Solution:**
```bash
# Supprimer complètement la DB et recommencer
rm -f ringgeneral.db
sqlite3 ringgeneral.db < src/RingGeneral.Data/Migrations/Base_Schema.sql
sqlite3 ringgeneral.db < src/RingGeneral.Data/Migrations/ImportWorkersFromBaki.sql
```

### Problème 3: "no such column: w.Versatility"

**Cause:** Le script ImportWorkersFromBaki.sql référence une colonne qui n'existe pas dans BAKI1.1.db

**Solution:**
```bash
# Vérifier les colonnes disponibles dans BAKI1.1.db
sqlite3 BAKI1.1.db "PRAGMA table_info(workers);"

# Si Versatility n'existe pas, éditez ImportWorkersFromBaki.sql
# et remplacez les références à Versatility par une constante (ex: 50)
```

### Problème 4: Attributs mentaux tous à 10

**Cause:** Les corrélations n'ont pas fonctionné correctement

**Solution:**
```bash
# Vérifier que les tables d'attributs de performance existent et sont remplies
sqlite3 ringgeneral.db "
SELECT COUNT(*) FROM WorkerInRingAttributes;
SELECT COUNT(*) FROM WorkerEntertainmentAttributes;
"

# Si count = 0, l'import BAKI n'a pas fonctionné
# Relancer l'import
sqlite3 ringgeneral.db < src/RingGeneral.Data/Migrations/ImportWorkersFromBaki.sql
```

### Problème 5: "FOREIGN KEY constraint failed"

**Solution:**
```bash
# Activer les foreign keys
sqlite3 ringgeneral.db "PRAGMA foreign_keys = ON;"

# Vérifier les violations
sqlite3 ringgeneral.db "PRAGMA foreign_key_check;"
```

---

## 📁 Structure Finale

Après initialisation, votre projet devrait contenir :

```
Ring-General-Rework.Exe/
├── ringgeneral.db                    ← Base de données créée ✅
├── BAKI1.1.db                        ← Source d'import ✅
├── INIT_DATABASE.md                  ← Ce fichier
├── init.sh                           ← Script automatique
└── src/
    └── RingGeneral.Data/
        └── Migrations/
            ├── Base_Schema.sql       ← Schéma complet
            └── ImportWorkersFromBaki.sql ← Import BAKI
```

---

## 🎓 Étapes Suivantes

1. **Lancer l'application:**
   ```bash
   dotnet run --project src/RingGeneral.UI
   ```

2. **Vérifier dans l'UI:**
   - Ouvrir un profil worker
   - Naviguer vers l'onglet "🎭 PERSONNALITÉ"
   - Vérifier que les 4 piliers sont affichés
   - Tester le bouton "🔍 Scouting Complet"

3. **Développement:**
   - Les services `PersonalityDetectorService` et `AgentReportGeneratorService` sont déjà enregistrés dans le DI
   - Les ViewModels sont connectés
   - L'UI est prête

---

## 📞 Support

En cas de problème :

1. Vérifier les logs dans la console
2. Vérifier les contraintes SQL avec `PRAGMA foreign_key_check;`
3. Consulter `PLAN_PERSONALITY_SYSTEM.md` pour la documentation complète
4. Vérifier les commits récents pour les changements

---

**✅ Vous êtes prêt à utiliser Ring General avec le système complet de personnalités !**
