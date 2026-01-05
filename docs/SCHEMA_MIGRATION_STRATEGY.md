# Stratégie de Migration du Schéma de Base de Données

**Date**: 2026-01-05
**Statut**: En attente (Phase 1)
**Priorité**: Haute

---

## Contexte du Problème

Le projet contient actuellement **deux systèmes de création de tables** qui coexistent:

### Système 1: `GameRepository.Initialiser()` (Legacy)

**Fichier**: `src/RingGeneral.Data/Repositories/GameRepository.cs:43`

```sql
CREATE TABLE IF NOT EXISTS companies (
    company_id TEXT PRIMARY KEY,
    nom TEXT NOT NULL,
    ...
);
CREATE TABLE IF NOT EXISTS workers (
    worker_id TEXT PRIMARY KEY,
    ...
);
```

**Caractéristiques**:
- Nommage **snake_case** (companies, workers, contracts, etc.)
- Colonnes en **snake_case** (company_id, worker_id, etc.)
- ~40 tables créées inline dans le code C#
- Pas de versioning
- Pas de foreign keys

### Système 2: `DbInitializer.ApplyMigrations()` (Moderne)

**Dossier**: `data/migrations/`

```sql
CREATE TABLE IF NOT EXISTS Companies (
    CompanyId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    ...
);
CREATE TABLE IF NOT EXISTS Workers (
    WorkerId TEXT PRIMARY KEY,
    ...
);
```

**Caractéristiques**:
- Nommage **PascalCase** (Companies, Workers, Contracts, etc.)
- Colonnes en **PascalCase** (CompanyId, WorkerId, etc.)
- ~16 fichiers de migration avec versioning
- Foreign keys et contraintes
- Table `SchemaVersion` pour tracking

---

## Impact du Problème

### Problèmes Actuels

1. **Tables dupliquées**: `workers` ET `Workers` peuvent coexister
2. **Confusion des requêtes**: Certaines requêtes pointent vers des tables inexistantes
3. **Tests incohérents**: Les tests créent parfois un schéma, parfois l'autre
4. **Maintenance difficile**: Modifications à faire dans deux endroits

### Risques

| Risque | Probabilité | Impact |
|--------|-------------|--------|
| Données écrites dans mauvaise table | Moyenne | Élevé |
| Requêtes échouant silencieusement | Élevée | Moyen |
| Migrations corrompant anciennes saves | Faible | Élevé |

---

## Stratégie de Migration Recommandée

### Phase 1: Préparation (Sans casser l'existant)

1. **Documenter toutes les requêtes SQL** dans `GameRepository`
2. **Créer un mapping** snake_case → PascalCase
3. **Ajouter des tests** vérifiant les deux schémas

### Phase 2: Migration Progressive

```
Étape 2.1: Ajouter des vues de compatibilité
────────────────────────────────────────────
CREATE VIEW IF NOT EXISTS workers AS SELECT * FROM Workers;
CREATE VIEW IF NOT EXISTS companies AS SELECT * FROM Companies;
...

Avantage: Le code legacy continue de fonctionner
```

```
Étape 2.2: Mettre à jour les requêtes
─────────────────────────────────────
Modifier progressivement les requêtes pour utiliser les noms PascalCase.
Une méthode à la fois, avec tests.
```

```
Étape 2.3: Supprimer les CREATE TABLE legacy
────────────────────────────────────────────
Une fois toutes les requêtes migrées, supprimer le code dans Initialiser()
```

### Phase 3: Nettoyage

1. **Supprimer les vues de compatibilité**
2. **Supprimer les tables legacy** des anciennes saves (migration)
3. **Documenter le schéma final**

---

## Mapping des Tables

| Legacy (snake_case) | Moderne (PascalCase) | Statut |
|---------------------|----------------------|--------|
| `companies` | `Companies` | Migration requise |
| `workers` | `Workers` | Migration requise |
| `contracts` | `Contracts` | Migration requise |
| `titles` | `Titles` | Migration requise |
| `storylines` | `Storylines` | Migration requise |
| `shows` | `Shows` | Migration requise |
| `segments` | `ShowSegments` | Renommage + migration |
| `tv_deals` | `TVDeals` | Migration requise |
| `chimies` | `Chimies` | Identique |
| `finances` | `FinanceTransactions` | Renommage + migration |
| `inbox_items` | `InboxItems` | Migration requise |
| `Injuries` | `Injuries` | Déjà PascalCase |
| `RecoveryPlans` | `RecoveryPlans` | Déjà PascalCase |
| `MedicalNotes` | `MedicalNotes` | Déjà PascalCase |

---

## Scripts de Migration

### Script: `003_schema_compatibility_views.sql`

```sql
-- Vues de compatibilité pour transition progressive
-- Ces vues permettent au code legacy de continuer à fonctionner

CREATE VIEW IF NOT EXISTS companies AS
SELECT
    CompanyId AS company_id,
    Name AS nom,
    RegionId AS region,
    Prestige AS prestige,
    Treasury AS tresorerie,
    AverageAudience AS audience_moyenne,
    Reach AS reach
FROM Companies;

CREATE VIEW IF NOT EXISTS workers AS
SELECT
    WorkerId AS worker_id,
    Name AS nom,
    FirstName AS prenom,
    CompanyId AS company_id,
    InRing AS in_ring,
    Entertainment AS entertainment,
    Story AS story,
    Popularity AS popularite,
    Fatigue AS fatigue,
    InjuryStatus AS blessure,
    Momentum AS momentum,
    RoleTv AS role_tv,
    60 AS morale  -- Valeur par défaut car absente dans Workers moderne
FROM Workers;

-- Ajouter d'autres vues selon besoin...
```

---

## Checklist de Migration

### Pré-migration
- [ ] Sauvegarder toutes les bases de test
- [ ] Documenter l'état actuel des tables
- [ ] Créer des tests de non-régression

### Migration
- [ ] Créer migration `003_schema_compatibility_views.sql`
- [ ] Tester avec bases existantes
- [ ] Mettre à jour requêtes dans `GameRepository`
- [ ] Tester chaque modification individuellement

### Post-migration
- [ ] Supprimer CREATE TABLE de `Initialiser()`
- [ ] Supprimer vues de compatibilité
- [ ] Documenter schéma final
- [ ] Mettre à jour tests

---

## Références

- `docs/PLAN_ACTION_FR.md` - Plan d'action complet
- `docs/ARCHITECTURE_REVIEW_FR.md` - Revue d'architecture
- `src/RingGeneral.Data/Repositories/GameRepository.cs` - Code legacy
- `data/migrations/` - Migrations modernes
