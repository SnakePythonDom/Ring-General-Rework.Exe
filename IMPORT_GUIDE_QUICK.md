# 🚀 GUIDE RAPIDE - Import Workers depuis BAKI1.1.db

**Objectif**: Importer tous vos wrestlers existants vers le nouveau système avec 30 attributs

---

## ⚡ Démarrage Rapide (5 minutes)

### Prérequis
- ✅ BAKI1.1.db présent dans le répertoire du projet
- ✅ Nouveau schéma créé (Migration_Master_ProfileViewAttributs.sql exécuté)
- ✅ Backup de votre base actuelle

### Étapes

#### 1. Backup (OBLIGATOIRE !)
```bash
# Linux/Mac
cp ring_general.db ring_general_backup_$(date +%Y%m%d_%H%M%S).db

# Windows PowerShell
Copy-Item ring_general.db -Destination "ring_general_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
```

#### 2. Exécuter le script d'import

**Option A: Avec sqlite3 (ligne de commande)**
```bash
sqlite3 ring_general.db < src/RingGeneral.Data/Migrations/ImportWorkersFromBaki.sql
```

**Option B: Avec DB Browser for SQLite (GUI)**
1. Ouvrir `ring_general.db` dans DB Browser
2. Onglet "Execute SQL"
3. Charger le fichier `ImportWorkersFromBaki.sql`
4. Cliquer "Execute"

**Option C: Depuis l'application C# (automatique)**
```csharp
// Dans Program.cs ou au démarrage
var importer = serviceProvider.GetRequiredService<WorkerImporter>();
await importer.ImportFromLegacyDb("BAKI1.1.db");
```

#### 3. Vérifier le résultat

Le script affichera automatiquement:
- ✅ Nombre de workers importés
- ✅ Nombre d'attributs créés (30 par worker)
- ✅ Top 10 workers par overall rating
- ✅ Statistiques par PushLevel

### Résultat Attendu

Tous vos workers seront importés avec:
- **30 attributs détaillés** (générés intelligemment depuis in_ring/entertainment/story)
- **Alignment** (Face/Heel/Tweener)
- **PushLevel** (MainEvent → Jobber)
- **Spécialisations** (Brawler, Technical, HighFlyer, etc.)
- **Toutes les données de base** préservées

---

## 🎯 Algorithme de Génération

### Comment les 30 attributs sont créés ?

**Ancien système** (3 attributs agrégés):
- `in_ring` = 75
- `entertainment` = 82
- `story` = 68

**Nouveau système** (30 attributs détaillés):

#### In-Ring (10 attributs)
Base: `in_ring` = 75

- Striking: 75 + variation (-12 à +12) = **68-87**
- Grappling: 75 + variation = **63-87**
- HighFlying: 75 + variation + bonus jeune âge = **73-97** si < 30 ans
- Powerhouse: 75 + variation + bonus poids = **85-100** si > 100kg
- Timing: 75 + variation + bonus experience = **85-97** si > 10 ans
- Etc.

#### Corrélations Intelligentes

| Attribut | Corrélé avec |
|----------|--------------|
| **HighFlying** | ↑ si jeune, ↓ si lourd |
| **Powerhouse** | ↑ si lourd, ↓ si léger |
| **Timing** | ↑ si expérimenté |
| **Psychology** | ↑ si expérimenté + âgé |
| **Stamina** | ↑ si jeune, ↓ si vieux |
| **Safety** | ↑ si expérimenté |
| **Charisma** | ↑ si populaire |
| **CrowdConnection** | ↑ si très populaire |
| **StarPower** | ↑ si populaire + momentum |
| **HeelPerformance** | ↑ si Alignment=Heel |
| **BabyfacePerformance** | ↑ si Alignment=Face |
| **MoralAlignment** | ↑ si Alignment=Tweener |
| **CreativeInput** | ↑ si expérimenté + populaire |

### Exemple Concret

**John Cena** (ancien système):
- in_ring: 80
- entertainment: 95
- story: 90
- popularité: 98
- age: 35
- experience: 15 ans

**Résultat généré** (nouveau système):
- Striking: 75 ✅
- Grappling: 78 ✅
- HighFlying: 45 ✅ (malus âge)
- Powerhouse: 90 ✅ (bon poids)
- Timing: 95 ✅ (bonus experience)
- Charisma: 100 ✅ (popularité élevée)
- MicWork: 92 ✅
- StarPower: 98 ✅ (popularité + momentum)
- BabyfacePerformance: 96 ✅ (Face alignment)
- CreativeInput: 88 ✅ (expérimenté + populaire)

**InRingAvg**: 82
**EntertainmentAvg**: 96
**StoryAvg**: 92
**Overall**: 90 ✅

---

## 🔧 Personnalisation Post-Import

### Ajuster les Attributs Manuellement

Après l'import, vous pouvez affiner dans ProfileView:

1. Ouvrir ProfileView pour un worker
2. Onglet "Attributs"
3. Cliquer "Modifier"
4. Ajuster les 30 valeurs
5. Sauvegarder

### Enrichir les Données

Données à compléter manuellement (NULL après import):
- **Géographie**: `BirthCity`, `BirthCountry`, `ResidenceCity`
- **RealName**: Nom réel du wrestler
- **PhotoPath**: Chemin vers photo
- **CurrentGimmick**: Description du gimmick
- **BookingIntent**: Plans du booker

---

## 📊 Validation

### Requêtes SQL Utiles

```sql
-- Top 10 overall
SELECT Name, (InRingAvg + EntertainmentAvg + StoryAvg)/3 AS Overall
FROM Workers w
JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
JOIN WorkerEntertainmentAttributes wea ON w.Id = wea.WorkerId
JOIN WorkerStoryAttributes wsa ON w.Id = wsa.WorkerId
ORDER BY Overall DESC LIMIT 10;

-- Workers sans attributs (anomalies)
SELECT w.Name FROM Workers w
LEFT JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
WHERE wir.WorkerId IS NULL;

-- Moyenne par PushLevel
SELECT PushLevel, AVG(InRingAvg) FROM Workers w
JOIN WorkerInRingAttributes wir ON w.Id = wir.WorkerId
GROUP BY PushLevel;
```

---

## ⚠️ Troubleshooting

### Erreur: "no such table: legacy.workers"
**Solution**: Vérifier que BAKI1.1.db est dans le bon répertoire

### Erreur: "FOREIGN KEY constraint failed"
**Solution**: Exécuter d'abord Migration_Master_ProfileViewAttributs.sql

### Import réussi mais moyennes incohérentes
**Solution**: Normal, variation aléatoire ±12 points. Ajuster manuellement si nécessaire.

### Tous les attributs sont identiques
**Solution**: Relancer l'import avec RANDOM() correctement initialisé

---

## 📞 Support

Pour plus de détails, consulter:
- `PLAN_IMPORT_WORKERS.md` - Plan complet
- `ATTRIBUTS_DESCRIPTIONS.md` - Description des 30 attributs
- `INTEGRATION_GUIDE.md` - Guide d'intégration général

---

**Version**: 1.0
**Date**: 2026-01-08
**Temps estimé**: 5 minutes ⚡
**Status**: ✅ Production Ready
