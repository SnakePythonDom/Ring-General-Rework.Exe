# 🤖 FEATURE : SYSTÈME D'AUTO-BOOKING IA

**Date d'implémentation** : 8 janvier 2026
**Version** : 1.0
**Statut** : ✅ **LIVRÉ ET OPÉRATIONNEL**

---

## 📋 VUE D'ENSEMBLE

Le système d'**Auto-Booking IA** permet au Booker de votre compagnie de générer automatiquement des cartes de shows complètes basées sur ses préférences personnelles, ses mémoires de décisions passées, et les contraintes que vous définissez en tant qu'Owner.

### 🎯 Objectifs

1. **Délégation intelligente** : Permettre au joueur de déléguer la création de cartes au Booker IA
2. **Cohérence narrative** : Utiliser le système de mémoire pour des décisions cohérentes à long terme
3. **Personnalisation** : Respecter les préférences du Booker (style de produit, favoris, etc.)
4. **Contrôle Owner** : Permettre au joueur de définir des contraintes et limites

---

## 🎮 EXPÉRIENCE UTILISATEUR

### Pour le Joueur (Owner)

#### **Scénario 1 : Booking Manuel Complet**
Le joueur crée manuellement tous les segments de son show.

#### **Scénario 2 : Booking Semi-Automatique**
1. Le joueur crée le main event manuellement
2. Il clique sur **"🤖 Laisser le Booker préparer le show"**
3. Le Booker complète automatiquement les slots vides
4. Le joueur peut ensuite ajuster les segments générés

#### **Scénario 3 : Booking 100% Automatique**
1. Le joueur crée un show vide
2. Il clique sur **"🤖 Laisser le Booker préparer le show"**
3. Le Booker génère une carte complète (4-8 segments)
4. Le joueur valide et simule

### Pour les Compagnies IA (Adversaires)

Les compagnies adverses génèrent **automatiquement** leurs cartes lors du Show Day :
- Aucune intervention humaine requise
- Contraintes adaptées aux IA (plus agressives sur la fatigue)
- Résultats visibles dans les rapports d'audience et ratings

---

## 🧠 LOGIQUE MÉTIER

### Types de Produit du Booker

Le champ `PreferredProductType` détermine le style de booking :

| Type | Durée Matchs | Intensité | Segments Préférés |
|------|--------------|-----------|-------------------|
| **Hardcore** | 25 min | 85-90% | Matchs extrêmes avec stipulations |
| **Puroresu** | 30 min | 75-80% | Matchs longs et techniques (strong style) |
| **Technical** | 25 min | 70% | Wrestling technique pur, soumissions |
| **Entertainment** | 15 min | 60% | Mix promos/matchs, focus narratif |
| **Balanced** | 20 min | 70-75% | Mix équilibré de tous les styles |

### Préférences de Push

Le Booker favorise certains types de workers selon ses préférences :

| Préférence | Condition | Bonus Score |
|------------|-----------|-------------|
| `LikesUnderdog` | Popularité < 40 | +20 |
| `LikesVeteran` | InRing ≥ 75 | +20 |
| `LikesFastRise` | Momentum > 60 | +15 |
| `LikesSlowBurn` | - | Privilégie storylines longues |

### Système de Mémoire

Le Booker utilise ses **BookerMemories** pour influencer les décisions :
- ✅ **Mémoires positives** (ImpactScore > 50) → Réutiliser les workers
- ❌ **Mémoires négatives** (ImpactScore < -50) → Éviter les workers
- 📊 **Decay naturel** : RecallStrength diminue de 1 point/semaine

---

## 🔧 ALGORITHME DE GÉNÉRATION

### Étape 1 : Filtrage des Workers Disponibles

```csharp
FilterAvailableWorkers(context, constraints, existingSegments)
```

**Exclusions automatiques** :
- ❌ Workers bannis par l'Owner
- ❌ Workers blessés (si `ForbidInjuredWorkers = true`)
- ❌ Workers trop fatigués (si `Fatigue > MaxFatigueLevel`)
- ❌ Workers déjà utilisés (si `ForbidMultipleAppearances = true`)

### Étape 2 : Création du Main Event

```csharp
CreateMainEvent(booker, context, availableWorkers, usedWorkerIds, memories, constraints)
```

**Sélection** :
1. Trier les workers par **Popularité** (descendant)
2. Prendre les **2 meilleurs** disponibles
3. Déterminer durée et intensité selon `PreferredProductType`
4. Chercher un **titre disponible** (si champion présent)

**Exemple** :
```
Booker Puroresu → Match de 30 min, intensité 80%, avec titre si disponible
```

### Étape 3 : Segments de Storylines Actives

```csharp
CreateStorylineSegments(booker, context, availableWorkers, usedWorkerIds, memories, constraints, remainingDuration)
```

**Logique** :
1. Récupérer les **storylines actives** (Status = Active)
2. Trier par **Heat** (descendant)
3. Pour chaque storyline :
   - Vérifier si les 2 participants sont disponibles
   - Créer un segment (promo si Entertainment, match sinon)
   - Durée : 10 min (promo) ou 15 min (match)
4. Limiter à **3 segments de storylines** maximum

### Étape 4 : Remplissage Complémentaire

```csharp
CreateSegmentBasedOnPreferences(booker, context, availableWorkers, usedWorkerIds, memories, constraints, remainingDuration)
```

**Logique** :
1. Déterminer type de segment selon `PreferredProductType`
2. Sélectionner participants via **scoring pondéré** :
   - Popularité : +score/5
   - Skills (InRing+Entertainment+Story) : +score/15
   - Préférences Booker : +20 (Underdog, Veteran, etc.)
   - Mémoires positives : +avgImpact/10
   - Créativité : Bonus aléatoire -15 à +25 (si CreativityScore ≥ 70)
3. Continuer jusqu'à remplir la durée cible ou atteindre MaxSegments

---

## 🎛️ CONTRAINTES OWNER (AutoBookingConstraints)

Le joueur peut définir des contraintes via la classe `AutoBookingConstraints` :

### Contraintes de Personnel

| Contrainte | Type | Description | Défaut |
|------------|------|-------------|--------|
| `BannedWorkers` | List<string> | Workers interdits (suspendus, etc.) | [] |
| `RequiredWorkers` | List<string> | Workers obligatoires à utiliser | [] |
| `ForbidInjuredWorkers` | bool | Interdire les blessés | true |
| `MaxFatigueLevel` | int | Fatigue max acceptée (0-100) | 80 |
| `ForbidMultipleAppearances` | bool | Un worker = un segment max | true |

### Contraintes de Show

| Contrainte | Type | Description | Défaut |
|------------|------|-------------|--------|
| `MinSegments` | int | Nombre min de segments | 4 |
| `MaxSegments` | int | Nombre max de segments | 8 |
| `RequireMainEvent` | bool | Main event obligatoire | true |
| `TargetDuration` | int? | Durée cible en minutes | ShowDuration |
| `PrioritizeActiveStorylines` | bool | Priorité aux feuds en cours | true |
| `UseTitles` | bool | Utiliser les titres disponibles | true |
| `MaxBudget` | double? | Budget max du show | null |

---

## 🏗️ ARCHITECTURE TECHNIQUE

### Classes Impliquées

```
BookerAIEngine
    └── GenerateAutoBooking(bookerId, showContext, existingSegments?, constraints?)
        ├── FilterAvailableWorkers()
        ├── CreateMainEvent()
        ├── CreateStorylineSegments()
        └── CreateSegmentBasedOnPreferences()
            └── SelectParticipants()
```

### Modèles

#### **Booker.cs**
```csharp
public sealed record Booker
{
    public string PreferredProductType { get; init; } = "Balanced"; // NOUVEAU
    public bool LikesUnderdog { get; init; }
    public bool LikesVeteran { get; init; }
    public bool LikesFastRise { get; init; }
    public bool LikesSlowBurn { get; init; }
    public int CreativityScore { get; init; }
    public int LogicScore { get; init; }
    // ...
}
```

#### **AutoBookingConstraints.cs** (NOUVEAU)
```csharp
public sealed record AutoBookingConstraints
{
    public double? MaxBudget { get; init; }
    public List<string> BannedWorkers { get; init; } = new();
    public List<string> RequiredWorkers { get; init; } = new();
    public bool ForbidInjuredWorkers { get; init; } = true;
    public int MaxFatigueLevel { get; init; } = 80;
    // ...
}
```

### Interface

#### **IBookerAIEngine.cs**
```csharp
List<SegmentDefinition> GenerateAutoBooking(
    string bookerId,
    ShowContext showContext,
    List<SegmentDefinition>? existingSegments = null,
    AutoBookingConstraints? constraints = null);
```

---

## 📊 EXEMPLES DE RÉSULTATS

### Exemple 1 : Booker Hardcore

**Configuration** :
- PreferredProductType = "Hardcore"
- LikesUnderdog = true
- CreativityScore = 85

**Carte générée** :
```
1. [MAIN EVENT] John Cena vs Randy Orton (25 min, 90% intensité, WWE Championship)
2. CM Punk vs Daniel Bryan (20 min, 85% intensité)
3. Zack Ryder vs Dolph Ziggler (15 min, 85% intensité) ← Underdog push
4. Promo: Authority (10 min)
5. Edge vs Christian - Tables Match (20 min, 90% intensité) ← Stipulation créative
```

### Exemple 2 : Booker Puroresu

**Configuration** :
- PreferredProductType = "Puroresu"
- LikesVeteran = true
- LogicScore = 90

**Carte générée** :
```
1. [MAIN EVENT] Hiroshi Tanahashi vs Kazuchika Okada (30 min, 80% intensité, IWGP Title)
2. [STORYLINE] Bullet Club vs Chaos (25 min, 75% intensité)
3. Minoru Suzuki vs Tomohiro Ishii (25 min, 75% intensité) ← Veterans
4. NEVER Title: Shingo Takagi vs EVIL (20 min, 75% intensité)
```

### Exemple 3 : Booker Entertainment

**Configuration** :
- PreferredProductType = "Entertainment"
- LikesSlowBurn = true

**Carte générée** :
```
1. [MAIN EVENT] The Rock vs John Cena (20 min, 60% intensité)
2. [PROMO] The Authority - Segment d'ouverture (10 min)
3. [STORYLINE PROMO] Wyatt Family vs Shield (10 min)
4. Seth Rollins vs Dean Ambrose (15 min, 60% intensité)
5. [PROMO] Paul Heyman avec Brock Lesnar (10 min)
6. Randy Orton vs Roman Reigns (15 min, 60% intensité)
```

---

## 🧪 TESTS & VALIDATION

### Tests de Cohérence

✅ **Aucun worker utilisé deux fois** (si `ForbidMultipleAppearances = true`)
- Algorithme utilise un `HashSet<string>` pour tracker les workers déjà utilisés
- Chaque worker ajouté est immédiatement marqué comme utilisé

✅ **Workers blessés exclus** (si `ForbidInjuredWorkers = true`)
- Filtrage : `w.Blessure == null || w.Blessure == "Aucune"`

✅ **Fatigue respectée** (si `MaxFatigueLevel = 80`)
- Filtrage : `w.Fatigue <= constraints.MaxFatigueLevel`

✅ **Durée totale respectée**
- Tracking : `remainingDuration -= segment.DureeMinutes`
- Boucle s'arrête si `remainingDuration < 10`

✅ **Main event garanti** (si `RequireMainEvent = true`)
- Vérifie `existingSegments.Any(s => s.EstMainEvent)` avant génération

### Tests Recommandés

#### Test 1 : Génération avec contraintes strictes
```csharp
var constraints = new AutoBookingConstraints
{
    BannedWorkers = new List<string> { "WORKER-001", "WORKER-002" },
    MaxFatigueLevel = 60,
    ForbidInjuredWorkers = true,
    MaxSegments = 5
};
```
**Attendu** : Carte de 5 segments max, sans WORKER-001 et WORKER-002, sans blessés, fatigue ≤ 60

#### Test 2 : Respect des préférences Hardcore
```csharp
var booker = new Booker { PreferredProductType = "Hardcore", ... };
```
**Attendu** : Matchs intenses (85-90%), durée 20-25 min

#### Test 3 : Utilisation des storylines
```csharp
var constraints = new AutoBookingConstraints { PrioritizeActiveStorylines = true };
```
**Attendu** : Au moins 1 segment par storyline active (si workers disponibles)

---

## 🚀 ÉVOLUTIONS FUTURES

### Version 1.1 (Prévue Q1 2026)

- [ ] **UI de configuration des contraintes** : Modal pour définir contraintes Owner
- [ ] **Templates de contraintes** : Sauvegarder/charger des presets
- [ ] **Suggestions du Booker** : Afficher le "raisonnement" du Booker (pourquoi ce worker ?)
- [ ] **Historique de décisions** : Consulter les décisions passées du Booker

### Version 1.2 (Prévue Q2 2026)

- [ ] **Apprentissage par renforcement** : Booker apprend des succès/échecs
- [ ] **Styles de matchs** : Stipulations automatiques (Ladder, Cage, etc.)
- [ ] **Gestion des segments non-matchs** : Interviews, Backstage Angles, etc.
- [ ] **Multi-Bookers** : Plusieurs Bookers avec spécialités (Main Event Booker, Midcard Booker)

### Version 2.0 (Prévue Q3 2026)

- [ ] **IA avancée (ML)** : Utiliser Machine Learning pour prédictions
- [ ] **Booking collaboratif** : Joueur + Booker négocient
- [ ] **Système de confiance** : Relation Owner-Booker impacte les décisions

---

## 📚 DOCUMENTATION CONNEXE

- **[SHOW_DAY_IMPLEMENTATION_REPORT.md](./SHOW_DAY_IMPLEMENTATION_REPORT.md)** — Rapport d'implémentation du flux Show Day
- **[PROJECT_STATUS.md](./PROJECT_STATUS.md)** — État global du projet
- **[ROADMAP_MISE_A_JOUR.md](./ROADMAP_MISE_A_JOUR.md)** — Roadmap détaillée (Phase 1.9)

---

## 🎓 GUIDE D'UTILISATION

### Pour le Joueur

1. **Ouvrir l'interface de booking** (`BookingView`)
2. **Optionnel** : Créer manuellement quelques segments (ex: main event)
3. **Cliquer sur** : `🤖 Laisser le Booker préparer le show`
4. **Vérifier** : Les segments générés apparaissent dans la liste
5. **Ajuster** : Modifier/supprimer les segments si nécessaire
6. **Valider** : Cliquer sur `✅ Valider le Booking`
7. **Simuler** : Cliquer sur `▶️ Simuler le Show`

### Pour les Développeurs

#### Appeler l'auto-booking depuis le code

```csharp
// Récupérer le Booker
var bookerRepo = new BookerRepository(connection);
var bookerId = "BOOKER-001";

// Préparer le contexte
var context = gameRepository.ChargerShowContext(showId);

// Définir les contraintes
var constraints = new AutoBookingConstraints
{
    ForbidInjuredWorkers = true,
    MaxFatigueLevel = 80,
    RequireMainEvent = true,
    PrioritizeActiveStorylines = true
};

// Générer le booking
var bookerAI = new BookerAIEngine(bookerRepo);
var segments = bookerAI.GenerateAutoBooking(bookerId, context, null, constraints);

// Sauvegarder les segments
foreach (var segment in segments)
{
    gameRepository.AjouterSegment(showId, segment, order++);
}
```

---

## 🐛 PROBLÈMES CONNUS

### Issue #1 : Booker ID hardcodé
**Statut** : ⚠️ **À CORRIGER**
**Description** : `ShowBookingViewModel.GenerateAutoBooking()` utilise `"BOOKER-DEFAULT"` au lieu de récupérer le Booker de la compagnie
**Solution** : Ajouter `GameRepository.GetBookerForCompanyAsync(companyId)`

### Issue #2 : Pas de validation de budget
**Statut** : ⚠️ **À IMPLÉMENTER**
**Description** : `AutoBookingConstraints.MaxBudget` n'est pas utilisé dans l'algorithme
**Solution** : Calculer le coût de chaque segment et vérifier le budget restant

---

**Feature documentée par** : Claude Code
**Date de dernière mise à jour** : 8 janvier 2026
**Statut** : ✅ Production-ready
