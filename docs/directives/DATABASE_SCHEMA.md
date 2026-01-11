# 🗄️ SCHEMA DE BASE DE DONNÉES - RING GENERAL 2026

**Dernière mise à jour** : 11 Janvier 2026
**Version du Schéma** : 1.1.0
**Fichier Source** : `src/RingGeneral.Data/Migrations/Base_Schema.sql`

---

## 📋 VUE D'ENSEMBLE

Le schéma de base de données de Ring General est centralisé dans le fichier `Base_Schema.sql`. Il suit une convention de nommage PascalCase (sauf pour quelques tables legacy en snake_case conservées pour compatibilité).

### Conventions
- **Tables** : PascalCase (ex: `Workers`, `Owners`) ou snake_case (legacy, ex: `youth_structures`).
- **Colonnes** : PascalCase préféré (ex: `WorkerId`, `Name`).
- **IDs** : TEXT (Guid ou codes string) ou INTEGER (AutoIncrement) selon le contexte.

---

## 🏗️ STRUCTURE DES TABLES

### 1. Core Game Tables
- **SaveGames** : Gestion des sauvegardes.
- **Countries / Regions** : Données géographiques statiques.
- **companies** : Compagnies de catch.
- **Workers** : Table centrale des catcheurs. Contient les infos de base, stats agrégées et le **PersonalityProfile**.

### 2. Performance Attributes System
- **WorkerInRingAttributes** : 10 attributs techniques (Striking, Grappling...) + Moyenne calculée.
- **WorkerEntertainmentAttributes** : 10 attributs de divertissement (Charisma, MicWork...) + Moyenne calculée.
- **WorkerStoryAttributes** : 10 attributs narratifs (CharacterDepth, Consistency...) + Moyenne calculée.

### 3. Mental Attributes & Personality
- **WorkerMentalAttributes** : 10 attributs mentaux cachés (Ambition, Loyauté...).

### 4. ProfileView Support
- **WorkerSpecializations** : Styles de combat (Primary/Secondary).
- **WorkerRelations** : Relations (Amitié, Rivalité).
- **Factions / FactionMembers** : Groupes et équipes.
- **WorkerNotes** : Notes utilisateurs.

### 5. Owner & Booker System (NEW)
*Ajouté le 11/01/2026*
- **Owners** : Propriétaires IA avec préférences de gestion.
- **Bookers** : Bookers IA avec style de booking.
- **BookerMemory** : Mémoire des événements passés pour l'IA Booker.
- **BookerEmploymentHistory** : Historique d'emploi des bookers.

### 6. Youth Development
- **youth_structures** : Centres de développement.
- **youth_trainees** : Stagiaires.
- **youth_programs** : Programmes d'entraînement.
- **NOTE** : La table `youth_structures` doit contenir une colonne `type TEXT`.

### 7. Medical System
- **Injuries** : Blessures actives et passées.
- **MedicalNotes** : Notes médicales.
- **RecoveryPlans** : Plans de récupération.

### 8. Contracts & Negotiations
- **contracts** : Contrats actifs.
- **contract_offers** : Offres en cours.

---

## 🔄 FLUX DE MIGRATION

Pour mettre à jour la base de données :

1.  **Nouveaux développements** : Ajoutez les `CREATE TABLE` dans `Base_Schema.sql`.
2.  **Migration existante** : Créez un script dans `src/RingGeneral.Data/Migrations/YYYYMMDD_Description.sql`.
3.  **Validation** : Utilisez `Base_Schema.sql` comme référence canonique.

## 🛠️ OUTILS DE MAINTENANCE

- **DbInitializer** : Utilise `Base_Schema.sql` pour initialiser une nouvelle partie.
- **DbValidator** : Peut vérifier l'intégrité du schéma au démarrage.
