# Guide d'import de base SQLite (FR)

## Méthodes recommandées
- **DB Browser for SQLite** : édition manuelle ou import CSV.
- **Scripts SQL** : batch d'INSERT/UPDATE exécutés via un client SQLite.
- **Import CSV** : possible si un outil d'import est ajouté plus tard.

## Exemples SQL

### Insérer une compagnie
```sql
INSERT INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige, Treasury, AverageAudience, Reach)
VALUES ('COMP-001', 'Ma Compagnie', 'FR', 'FR-IDF', 50, 200000, 12000, 4);
```

### Insérer un worker
```sql
INSERT INTO Workers (WorkerId, Name, FirstName, LastName, Nationality, CompanyId, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv)
VALUES ('W-001', 'Jean Dupont', 'Jean', 'Dupont', 'FR', 'COMP-001', 60, 55, 50, 45, 0, 'AUCUNE', 0, 'MID');
```

### Insérer un contrat
```sql
INSERT INTO Contracts (WorkerId, CompanyId, StartDate, EndDate, Salary, IsExclusive)
VALUES ('W-001', 'COMP-001', 1, 52, 1500, 1);
```

## Erreurs fréquentes + solutions
- **"Table obligatoire manquante"** : la base ne suit pas le schéma officiel. Repartir du fichier vierge ou appliquer les migrations.
- **"Colonne obligatoire manquante"** : le schéma est incomplet. Appliquer les migrations (`db upgrade`).
- **"Base importée invalide"** : le fichier n'est pas une SQLite valide ou n'est pas conforme.
- **Clés étrangères manquantes** : vérifier l'ordre d'import (pays/régions avant compagnies, etc.).

---

Pour comprendre l'ordre de remplissage et les bonnes pratiques, consultez : **docs/DATABASE_GUIDE_FR.md**.
