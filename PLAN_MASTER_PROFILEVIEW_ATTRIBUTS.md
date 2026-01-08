# 🎯 PLAN MASTER : ProfileView Universel + Rework Attributs Performance

**Date** : 7 janvier 2026
**Chef de Projet** : Claude DevOps
**Version** : 1.0 - Plan Combiné
**Branche** : `claude/rework-performance-attributes-YBXRx`
**Priorité** : 🔴 CRITIQUE
**Durée Estimée** : 4-5 semaines

---

## 📋 SYNTHÈSE EXÉCUTIVE

### Objectif Global

Créer le **système de profil complet de nouvelle génération** en combinant :
1. **Rework des Attributs** : 30 attributs professionnels (10 × 3 catégories)
2. **ProfileView Universel** : 6 onglets fonctionnels avec fiche personnage complète

### Pourquoi Combiner ?

**Synergies identifiées** :
- ✅ **Tab Attributs** : Fiche personnage + 30 attributs = une seule implémentation
- ✅ **Migration SQL** : Une seule migration pour tout (11 tables au lieu de 2 migrations séparées)
- ✅ **Workers table** : Modifications groupées (géo + gimmick + spécialisations)
- ✅ **AttributesTabViewModel** : Implémentation unique avec les 30 attributs
- ✅ **Tests** : Suite de tests combinée et cohérente

**Gains** :
- 🚀 **Temps** : ~4-5 semaines au lieu de ~5-6 semaines séparées
- 🎯 **Cohérence** : Architecture unifiée dès le départ
- 🔧 **Moins de refactoring** : Pas besoin de revenir sur le code
- ✅ **Validation complète** : Tout testé ensemble

---

## 🎯 PÉRIMÈTRE COMPLET

### 1. Rework Attributs Performance (30 attributs)

**IN-RING** (10 attributs) :
- Striking, Grappling, High-Flying, Powerhouse
- Timing, Selling, Psychology, Stamina
- Safety, Hardcore/Brawl

**ENTERTAINMENT** (10 attributs) :
- Charisma, Mic Work, Acting, Crowd Connection
- Star Power, Improvisation, Entrance
- Sex Appeal, Merchandise Appeal, Crossover Potential

**STORY** (10 attributs) :
- Character Depth, Consistency, Heel/Babyface Performance
- Storytelling, Emotional Range, Adaptability
- Rivalry Chemistry, Creative Input, Moral Alignment

### 2. ProfileView avec 6 Onglets

**📊 Tab 1 : ATTRIBUTS**
- **Fiche Personnage** (Photo, Identité, Spécialisations, Géographie)
- **30 Attributs** avec AttributeBar (Universels + In-Ring + Entertainment + Story)
- **Indicateurs de changement** (↑↓)

**📝 Tab 2 : CONTRATS**
- Contrat actuel (dates, salaire, type, clauses)
- Historique des contrats
- Actions (Renégocier, Prolonger, Libérer)

**🎭 Tab 3 : GIMMICK/PUSH**
- Gimmick actuel + historique
- Alignment (Face/Heel/Tweener)
- Push Level + TV Role + Booking Intent
- Finishers et Signatures

**👥 Tab 4 : RELATIONS**
- **Relations 1-à-1** : Amitié 🤝, Couple ❤, Fraternité 👊, Rivalité ⚔
- **Factions** : Tag Team 🤜🤛, Trio 🎯, Faction 👊
- Gestion complète (add/edit/delete)

**📖 Tab 5 : HISTORIQUE**
- Biographie complète
- Historique des matchs avec notes
- Historique des titres
- Historique des blessures
- Statistiques (W/L, %, titres)

**📌 Tab 6 : NOTES**
- Notes personnalisables avec catégories
- Add/Edit/Delete
- Timestamps automatiques

---

## 🗂️ ARCHITECTURE COMBINÉE

### Base de Données (11 tables)

#### Tables du Rework Attributs (3)
1. **WorkerInRingAttributes** - 10 attributs In-Ring + moyenne calculée
2. **WorkerEntertainmentAttributes** - 10 attributs Entertainment + moyenne
3. **WorkerStoryAttributes** - 10 attributs Story + moyenne

#### Tables du ProfileView (7)
4. **WorkerSpecializations** - Spécialisations (Brawler, Technical, etc.)
5. **WorkerRelations** - Relations 1-à-1 entre workers
6. **Factions** - Groupes (Tag Team, Trio, Faction)
7. **FactionMembers** - Membres des factions
8. **WorkerNotes** - Notes personnalisables
9. **ContractHistory** - Historique des contrats
10. **MatchHistory** - Historique des matchs (si pas déjà existant)
11. **TitleReigns** - Historique des titres (si pas déjà existant)

#### Modifications Workers Table
```sql
ALTER TABLE Workers ADD COLUMN BirthCity TEXT;
ALTER TABLE Workers ADD COLUMN BirthCountry TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceCity TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceState TEXT;
ALTER TABLE Workers ADD COLUMN ResidenceCountry TEXT;
ALTER TABLE Workers ADD COLUMN PhotoPath TEXT;
ALTER TABLE Workers ADD COLUMN Handedness TEXT DEFAULT 'Right';
ALTER TABLE Workers ADD COLUMN FightingStance TEXT DEFAULT 'Orthodox';
ALTER TABLE Workers ADD COLUMN CurrentGimmick TEXT;
ALTER TABLE Workers ADD COLUMN Alignment TEXT DEFAULT 'Face';
ALTER TABLE Workers ADD COLUMN PushLevel TEXT DEFAULT 'MidCard';
ALTER TABLE Workers ADD COLUMN TvRole INTEGER DEFAULT 50;
ALTER TABLE Workers ADD COLUMN BookingIntent TEXT;
```

### Models (11 nouveaux)

**Attributs** (3) :
1. `WorkerInRingAttributes.cs`
2. `WorkerEntertainmentAttributes.cs`
3. `WorkerStoryAttributes.cs`

**Relations & Factions** (3) :
4. `WorkerRelation.cs`
5. `Faction.cs`
6. `FactionMember.cs`

**Autres** (5) :
7. `WorkerSpecialization.cs`
8. `WorkerNote.cs`
9. `ContractHistory.cs`
10. `MatchHistoryItem.cs` (si pas existant)
11. `TitleReign.cs` (si pas existant)

### Repositories (3 nouveaux)

1. **WorkerAttributesRepository** - CRUD pour les 30 attributs
2. **RelationsRepository** - Relations + Factions + Members
3. **NotesRepository** - Notes personnalisables

### ViewModels (10 nouveaux)

**ProfileView Shell** (1) :
1. `ProfileViewModel` - Coordination des 6 tabs

**ProfileMain** (1) :
2. `ProfileMainViewModel` - Fiche personnage (photo, identité, quick stats)

**6 Tabs** (6) :
3. `AttributesTabViewModel` - **30 attributs** + Fiche personnage
4. `ContractsTabViewModel` - Contrats + historique
5. `GimmickTabViewModel` - Gimmick + Alignment + Push
6. `RelationsTabViewModel` - Relations + Factions
7. `HistoryTabViewModel` - Bio + Matchs + Titres + Stats
8. `NotesTabViewModel` - Notes avec catégories

**Nested VMs** (2) :
9. `WorkerRelationViewModel` - Pour chaque relation 1-à-1
10. `FactionViewModel` - Pour chaque faction

### Views (8 nouvelles)

1. **ProfileView.axaml** - Shell (Header + TabControl)
2. **ProfileMainView.axaml** - Fiche personnage (intégré dans tab Attributs)
3. **AttributesTabView.axaml** - Fiche + 30 attributs en 3 catégories
4. **ContractsTabView.axaml**
5. **GimmickTabView.axaml**
6. **RelationsTabView.axaml** - Relations + Factions
7. **HistoryTabView.axaml**
8. **NotesTabView.axaml**

### Resources

1. **AttributeDescriptions.fr.resx** - 30 descriptions détaillées en français
2. **WorkersAttributesSeed.sql** - Seed data pour 50+ workers
3. **WorkersSpecializationsSeed.sql** - Spécialisations

---

## 🗓️ PLANNING COMBINÉ (4-5 SEMAINES)

### PHASE 1 : Base de Données Complète (3-4 jours)

**Agent** : Systems Architect

**Objectif** : Créer toute l'infrastructure DB en une seule migration

#### Jour 1 : Migration SQL (Matin)
- [ ] Créer `Migration_Master_ProfileViewAttributs.sql`
- [ ] **3 tables attributs** avec colonnes calculées (moyennes)
- [ ] **7 tables ProfileView** (Relations, Factions, Notes, etc.)
- [ ] **13 colonnes ajoutées** à Workers

#### Jour 1 : Migration SQL (Après-midi)
- [ ] Tester migration sur copie DB
- [ ] Script de rollback
- [ ] Validation de l'intégrité

#### Jour 2 : Data Seeding (Matin)
- [ ] Seed attributs pour 50+ workers (30 attributs × 50)
- [ ] Seed spécialisations (2-3 par worker)
- [ ] Seed géographie (villes, pays)

#### Jour 2 : Data Seeding (Après-midi)
- [ ] Seed relations de test (10+ relations)
- [ ] Seed factions de test (5+ factions)
- [ ] Seed notes de test
- [ ] Validation qualité des données

#### Jour 3 : Tests & Validation
- [ ] Tests d'intégrité référentielle
- [ ] Tests de performance (chargement profil < 500ms)
- [ ] Backup de la DB avec données seed
- [ ] Documentation de la structure

**Livrables Phase 1** :
- ✅ 11 tables créées et testées
- ✅ Workers table enrichie (13 colonnes)
- ✅ Données seed pour 50+ workers
- ✅ Migration réversible
- ✅ Performance validée

---

### PHASE 2 : Models Complets (4-5 jours)

**Agent** : Systems Architect

**Objectif** : Créer les 11 Models avec navigation properties

#### Jour 4 : Models Attributs (Matin)
- [ ] `WorkerInRingAttributes.cs` (10 props + moyenne calculée)
- [ ] `WorkerEntertainmentAttributes.cs` (10 props + moyenne)
- [ ] `WorkerStoryAttributes.cs` (10 props + moyenne)

#### Jour 4 : Models Attributs (Après-midi)
- [ ] `WorkerSpecialization.cs` avec enum
- [ ] Tests unitaires des calculs de moyennes
- [ ] Validation des ranges (0-100)

#### Jour 5 : Models Relations (Matin)
- [ ] `WorkerRelation.cs` avec enum RelationType
- [ ] `Faction.cs` avec enum FactionType et FactionStatus
- [ ] `FactionMember.cs` avec dates join/left

#### Jour 5 : Models Relations (Après-midi)
- [ ] `WorkerNote.cs` avec enum NoteCategory
- [ ] `ContractHistory.cs`
- [ ] Tests unitaires

#### Jour 6 : Models History (Matin)
- [ ] `MatchHistoryItem.cs` (si pas existant)
- [ ] `TitleReign.cs` (si pas existant)
- [ ] `InjuryHistoryItem.cs` (si pas existant)

#### Jour 6 : Worker Model Update (Après-midi)
- [ ] Ajouter 13 propriétés à `Worker.cs`
- [ ] Ajouter navigation properties pour les 11 models
- [ ] Tests de navigation
- [ ] Validation des contraintes

#### Jour 7 : Tests & Documentation
- [ ] Suite de tests complète (11 Models)
- [ ] Tests de relations bidirectionnelles
- [ ] Documentation des enums
- [ ] Diagramme de classes UML

**Livrables Phase 2** :
- ✅ 11 Models créés et testés
- ✅ Worker.cs enrichi avec 13 props + navigation
- ✅ Tests unitaires passants (100% coverage)
- ✅ Documentation complète

---

### PHASE 3 : Repositories Complets (5-6 jours)

**Agent** : Systems Architect

**Objectif** : Créer 3 Repositories complets avec tests

#### Jour 8 : WorkerAttributesRepository (Matin)
- [ ] `IWorkerAttributesRepository.cs` (interface)
- [ ] Méthodes CRUD In-Ring (10 attributs)
- [ ] Méthodes CRUD Entertainment (10 attributs)
- [ ] Méthodes CRUD Story (10 attributs)

#### Jour 8 : WorkerAttributesRepository (Après-midi)
- [ ] Implémentation ADO.NET complète
- [ ] Requêtes SQL paramétrées
- [ ] Gestion des transactions
- [ ] Tests unitaires

#### Jour 9 : RelationsRepository (Matin)
- [ ] `IRelationsRepository.cs` (interface)
- [ ] CRUD WorkerRelations (bidirectionnelles)
- [ ] CRUD Factions
- [ ] CRUD FactionMembers

#### Jour 9 : RelationsRepository (Après-midi)
- [ ] Implémentation ADO.NET
- [ ] Gestion des contraintes uniques
- [ ] Validation des relations bidirectionnelles
- [ ] Tests unitaires

#### Jour 10 : NotesRepository (Matin)
- [ ] `INotesRepository.cs` (interface)
- [ ] CRUD WorkerNotes
- [ ] Filtrage par catégorie
- [ ] Tri par date

#### Jour 10 : NotesRepository (Après-midi)
- [ ] Implémentation ADO.NET
- [ ] Tests unitaires
- [ ] Tests de filtrage et tri

#### Jour 11 : Specializations & History (Matin)
- [ ] Méthodes Specializations dans WorkerAttributesRepository
- [ ] Méthodes MatchHistory dans WorkerRepository (ou nouveau repo)
- [ ] Méthodes TitleReigns

#### Jour 11 : Dependency Injection (Après-midi)
- [ ] Enregistrer 3 repositories dans `App.axaml.cs`
- [ ] Tests de résolution DI
- [ ] Validation des dépendances

#### Jour 12 : Tests d'Intégration
- [ ] Tests de chargement complet profil (30 attributs + relations + notes)
- [ ] Tests de performance (< 500ms)
- [ ] Tests de transactions (rollback si erreur)
- [ ] Documentation des repositories

**Livrables Phase 3** :
- ✅ 3 Repositories complets
- ✅ DI configuré et testé
- ✅ Tests unitaires + intégration
- ✅ Performance < 500ms

---

### PHASE 4 : ViewModels Complets (6-8 jours)

**Agent** : Systems Architect

**Objectif** : Créer les 10 ViewModels avec data binding

#### Jour 13 : ProfileViewModel (Shell)
- [ ] `ProfileViewModel.cs` avec coordination des 6 tabs
- [ ] Gestion de la navigation entre tabs
- [ ] Commands Edit et Release
- [ ] Tests de navigation

#### Jour 14 : ProfileMainViewModel
- [ ] `ProfileMainViewModel.cs` (Fiche personnage)
- [ ] Photo, identité, spécialisations, géographie
- [ ] Quick stats (Condition, Forme, Fatigue, Pop)
- [ ] Commands ChangePhoto et GenerateAvatar
- [ ] Tests

#### Jour 15 : AttributesTabViewModel (Jour complet)
- [ ] **Fiche personnage intégrée** (même que ProfileMain)
- [ ] **30 propriétés d'attributs** (In-Ring, Entertainment, Story)
- [ ] **3 moyennes calculées** (InRingAvg, EntertainmentAvg, StoryAvg)
- [ ] **PreviousValues** dictionary pour indicateurs ↑↓
- [ ] Data binding bidirectionnel
- [ ] Tests de calcul des moyennes
- [ ] Tests de tracking des changements

#### Jour 16 : ContractsTabViewModel
- [ ] Contrat actuel (8 propriétés)
- [ ] ContractHistory collection
- [ ] Commands Renegotiate, Release, Extend
- [ ] Tests

#### Jour 17 : GimmickTabViewModel
- [ ] Gimmick, Alignment, PushLevel, TvRole, BookingIntent
- [ ] GimmickHistory, FinishingMoves, Signatures collections
- [ ] Commands ChangeGimmick, ToggleAlignment, AdjustPush
- [ ] Tests

#### Jour 18 : RelationsTabViewModel (Jour complet)
- [ ] Relations collection
- [ ] **WorkerRelationViewModel** (nested)
  - RelationType, Strength, Icon, Notes
  - IsStrongRelation, IsMediumRelation
- [ ] Factions collection
- [ ] **FactionViewModel** (nested)
  - FactionType, Status, Members, Leader
  - Commands Edit, Disband, RemoveMember, AddMember
- [ ] Commands AddRelation, EditRelation, DeleteRelation, CreateFaction
- [ ] Tests des relations bidirectionnelles
- [ ] Tests des factions

#### Jour 19 : HistoryTabViewModel
- [ ] Biographie (8 propriétés)
- [ ] TitleReigns collection
- [ ] MatchHistory collection
- [ ] InjuryHistory collection
- [ ] StorylineHistory collection
- [ ] Stats calculées (W/L, %, titres)
- [ ] Tests

#### Jour 20 : NotesTabViewModel
- [ ] Notes collection
- [ ] NewNoteText property
- [ ] Commands AddNote, EditNote, DeleteNote
- [ ] Tri par date (récent en premier)
- [ ] Tests

**Livrables Phase 4** :
- ✅ 10 ViewModels créés et testés
- ✅ Data binding complet pour 30 attributs
- ✅ Commands fonctionnelles
- ✅ Tests de binding

---

### PHASE 5 : Views & UI (7-9 jours)

**Agent** : UI Specialist

**Objectif** : Créer les 8 Views avec XAML

#### Jour 21 : ProfileView (Shell)
- [ ] `ProfileView.axaml` - Structure Grid (Header + TabControl)
- [ ] Header avec Photo (80×80, rond) + Nom + Actions
- [ ] TabControl avec 6 TabItems
- [ ] DataTemplates dans MainWindow.axaml
- [ ] Tests de navigation

#### Jour 22 : ProfileMainView (Intégré dans AttributesTab)
- [ ] Fiche personnage complète (voir mockup John Cena)
- [ ] Layout 2 colonnes (Photo 200×200 | Infos)
- [ ] Section Photo avec boutons (Changer, Générer Avatar)
- [ ] Identité : Nom complet, Type, Rôle TV, Spécialisations
- [ ] Âge et dates (📅 Âge: 46 ans (27 avril 1977))
- [ ] Géographie (🌍 Naissance, 🏠 Résidence)
- [ ] Quick Stats avec barres visuelles
- [ ] Tests de layout responsive

#### Jour 23-24 : AttributesTabView (2 jours)
- [ ] **Intégrer ProfileMainView en haut**
- [ ] **3 Expanders** (IN-RING, ENTERTAINMENT, STORY)
- [ ] **10 AttributeBar par catégorie** (30 total)
- [ ] Affichage des moyennes (Moy: 82/100)
- [ ] Binding vers AttributesTabViewModel
- [ ] Tooltips sur chaque AttributeBar
- [ ] Indicateurs de changement (↑↓ avec couleur)
- [ ] Tests de binding
- [ ] Tests de tooltips

#### Jour 25 : ContractsTabView
- [ ] Section Contrat Actuel avec Grid 2 colonnes
- [ ] Dates (StartDate, EndDate, WeeksRemaining)
- [ ] Salaire (WeeklySalary formaté en €)
- [ ] Type et Clauses
- [ ] Section Historique avec DataGrid
- [ ] Boutons Renégocier, Prolonger, Libérer
- [ ] Tests

#### Jour 26 : GimmickTabView
- [ ] Section Gimmick Actuel (TextBox éditable)
- [ ] Section Alignment avec RadioButtons (Face/Heel/Tweener)
- [ ] Section Push Level avec Slider
- [ ] TV Role gauge (0-100)
- [ ] Booking Intent (TextBox multiligne)
- [ ] Section Finishers/Signatures (listes)
- [ ] Tests

#### Jour 27 : RelationsTabView (Jour complet)
- [ ] **Section Relations 1-à-1**
  - Header avec bouton "+ Ajouter"
  - ItemsControl avec cards
  - Card layout : Icône (32px) | Infos | Actions (✏🗑)
  - Infos : Nom, Type, Force (avec couleur), Notes
- [ ] **Section Factions**
  - Header avec bouton "+ Créer"
  - ItemsControl avec cards
  - Card layout : Icône | Infos | Actions
  - Infos : Nom, Type, Membres, Leader, Status (badge), Dates
- [ ] Styling des badges (couleurs par statut)
- [ ] Tests

#### Jour 28 : HistoryTabView
- [ ] Section Biographie avec Grid 2 colonnes
- [ ] Section Stats (W/L avec pourcentages)
- [ ] Expander "Historique des Matchs" avec DataGrid
- [ ] Expander "Historique des Titres" avec ItemsControl
- [ ] Expander "Historique des Blessures"
- [ ] Expander "Historique des Storylines"
- [ ] Tests

#### Jour 29 : NotesTabView
- [ ] Section "Nouvelle Note" avec TextBox + bouton Ajouter
- [ ] ItemsControl des notes existantes
- [ ] Note card : Texte, Catégorie (badge), Date, Actions (✏🗑)
- [ ] Tri par date (récent en premier)
- [ ] Tests

**Livrables Phase 5** :
- ✅ 8 Views créées (Shell + ProfileMain + 6 tabs)
- ✅ Layout complet pour John Cena mockup
- ✅ 30 AttributeBar affichés
- ✅ Relations + Factions UI
- ✅ Tests de layout

---

### PHASE 6 : Resources & Localisation (3-4 jours)

**Agent** : Content Creator

**Objectif** : Créer les descriptions et données seed

#### Jour 30 : Descriptions d'Attributs
- [ ] Rédiger **30 descriptions détaillées** en français
- [ ] Format : 1-2 phrases explicatives par attribut
- [ ] Ajouter à `AttributeDescriptions.fr.resx`
- [ ] Validation linguistique
- [ ] Tests des tooltips

#### Jour 31 : Data Seed Attributs (Matin)
- [ ] Générer valeurs pour **John Cena** (voir mockup)
- [ ] Générer valeurs pour **50+ workers** BAKI
- [ ] Cohérence avec personnages réels
- [ ] Assigner spécialisations réalistes

#### Jour 31 : Data Seed Relations (Après-midi)
- [ ] Créer 20+ relations de test
- [ ] Créer 10+ factions de test (Tag Teams, Trios, Factions)
- [ ] Assigner membres aux factions
- [ ] Cohérence avec l'histoire du catch

#### Jour 32 : Data Seed Historique
- [ ] Matchs historiques pour top workers
- [ ] Title reigns historiques
- [ ] Notes de test pour certains workers
- [ ] Validation qualité

**Livrables Phase 6** :
- ✅ 30 descriptions en français
- ✅ Data seed pour 50+ workers
- ✅ Relations + Factions réalistes
- ✅ Historique cohérent

---

### PHASE 7 : Integration & Tests (4-5 jours)

**Agent** : Systems Architect + UI Specialist

**Objectif** : Tests complets et corrections

#### Jour 33 : Tests Unitaires
- [ ] Tests Models (11 models)
- [ ] Tests Repositories (3 repos)
- [ ] Tests ViewModels (10 VMs)
- [ ] Coverage > 80%

#### Jour 34 : Tests d'Intégration
- [ ] Chargement profil complet (30 attributs + 6 tabs)
- [ ] Modification d'attributs et persistance
- [ ] Ajout/Edit/Delete relations
- [ ] Ajout/Edit/Delete factions
- [ ] Ajout/Edit/Delete notes
- [ ] Performance (chargement < 500ms)

#### Jour 35 : Tests UI
- [ ] Affichage correct des 30 attributs
- [ ] Fiche personnage complète (photo, identité, géo)
- [ ] Tooltips fonctionnels
- [ ] Indicateurs de changement (↑↓)
- [ ] Navigation entre tabs
- [ ] Responsive design
- [ ] Thème cohérent

#### Jour 36 : Tests End-to-End
- [ ] Navigation RosterView → ProfileView
- [ ] Sélection de différents workers
- [ ] Modification et sauvegarde
- [ ] Vérification persistance en DB
- [ ] Tests avec Worker/Staff/Trainee

#### Jour 37 : Corrections & Optimisations
- [ ] Corrections de bugs identifiés
- [ ] Optimisations de performance
- [ ] Amélioration UX
- [ ] Re-tests après corrections

**Livrables Phase 7** :
- ✅ Suite de tests complète
- ✅ Bugs corrigés
- ✅ Performance validée
- ✅ UX optimisée

---

### PHASE 8 : Nettoyage & Documentation (2-3 jours)

**Agent** : File Cleaner

**Objectif** : Code propre et documentation complète

#### Jour 38 : Nettoyage du Code
- [ ] Vérifier tous les namespaces
- [ ] Supprimer fichiers obsolètes
- [ ] Nettoyer using inutilisés
- [ ] Organiser dossiers (Models/Relations/, etc.)
- [ ] Formater le code (conventions C#)

#### Jour 39 : Documentation
- [ ] **Guide Utilisateur** : Utilisation du ProfileView
- [ ] **Guide Développeur** : Architecture du système d'attributs
- [ ] **Guide de Migration** : Pour les développeurs futurs
- [ ] **API Documentation** : Repositories et Services
- [ ] Update `CURRENT_STATE.md`
- [ ] Update `PLAN_SPRINT_REVISE.md`

#### Jour 40 : Validation Finale
- [ ] Compilation sans warnings
- [ ] Tous les tests passent
- [ ] Documentation complète
- [ ] Prêt pour merge dans main

**Livrables Phase 8** :
- ✅ Code propre et organisé
- ✅ Documentation complète
- ✅ Guides utilisateur/développeur
- ✅ Prêt pour production

---

## 📊 PLANNING RÉCAPITULATIF

| Phase | Durée | Agent | Livrables Clés |
|-------|-------|-------|----------------|
| **Phase 1** : Base de Données | 3-4j | Systems Architect | 11 tables, Migration SQL, Data seed |
| **Phase 2** : Models | 4-5j | Systems Architect | 11 Models, Worker enrichi |
| **Phase 3** : Repositories | 5-6j | Systems Architect | 3 Repos, DI configuré |
| **Phase 4** : ViewModels | 6-8j | Systems Architect | 10 VMs avec 30 attributs |
| **Phase 5** : Views & UI | 7-9j | UI Specialist | 8 Views, ProfileMain, 6 tabs |
| **Phase 6** : Resources | 3-4j | Content Creator | 30 descriptions, Data seed |
| **Phase 7** : Integration | 4-5j | Architect + UI | Tests, Corrections, Validation |
| **Phase 8** : Nettoyage | 2-3j | File Cleaner | Code propre, Documentation |

**Durée Totale** : **34-44 jours** (environ **4-5 semaines** avec parallélisation)

### Parallélisation Possible

- **Phase 6** (Resources) peut se faire en parallèle de **Phase 5** (Views)
- Gain : **~3 jours**
- **Durée optimale** : **31-41 jours** (~**4-5 semaines**)

---

## 📦 LIVRABLES TOTAUX

### Base de Données
- ✅ **11 nouvelles tables**
- ✅ **13 colonnes ajoutées** à Workers
- ✅ **1 migration SQL unique** et testée
- ✅ **Data seed** pour 50+ workers

### Models (11)
1. WorkerInRingAttributes
2. WorkerEntertainmentAttributes
3. WorkerStoryAttributes
4. WorkerSpecialization
5. WorkerRelation
6. Faction
7. FactionMember
8. WorkerNote
9. ContractHistory
10. MatchHistoryItem
11. TitleReign

### Repositories (3)
1. WorkerAttributesRepository (30 attributs + spécialisations)
2. RelationsRepository (Relations + Factions)
3. NotesRepository

### ViewModels (10)
1. ProfileViewModel (Shell)
2. ProfileMainViewModel (Fiche personnage)
3. AttributesTabViewModel (30 attributs + fiche)
4. ContractsTabViewModel
5. GimmickTabViewModel
6. RelationsTabViewModel
7. WorkerRelationViewModel (nested)
8. FactionViewModel (nested)
9. HistoryTabViewModel
10. NotesTabViewModel

### Views (8)
1. ProfileView.axaml (Shell)
2. ProfileMainView.axaml
3. AttributesTabView.axaml (30 AttributeBar)
4. ContractsTabView.axaml
5. GimmickTabView.axaml
6. RelationsTabView.axaml
7. HistoryTabView.axaml
8. NotesTabView.axaml

### Resources
- ✅ 30 descriptions détaillées (FR)
- ✅ Data seed 50+ workers
- ✅ Spécialisations seed
- ✅ Relations/Factions seed

### Documentation
- ✅ Guide Utilisateur ProfileView
- ✅ Guide Développeur Attributs
- ✅ Guide de Migration
- ✅ API Documentation

**Total** : **~60 fichiers** (50+ nouveaux + 10 modifiés)

---

## ✅ CRITÈRES DE VALIDATION GLOBAUX

### Critères Techniques

- [ ] 11 tables créées et migration réussie
- [ ] 11 Models créés et testés
- [ ] 3 Repositories fonctionnels
- [ ] 10 ViewModels créés et testés
- [ ] 8 Views créées et stylées
- [ ] 30 attributs affichés correctement
- [ ] 6 tabs fonctionnels
- [ ] Navigation ProfileView opérationnelle
- [ ] DataTemplates enregistrés
- [ ] DI configuré
- [ ] Tous les tests passent (>80% coverage)
- [ ] Compilation sans warnings
- [ ] Performance < 500ms (chargement profil)

### Critères Fonctionnels

**Tab Attributs** :
- [ ] Fiche personnage complète (photo, identité, géo, spécialisations)
- [ ] 30 attributs affichés avec AttributeBar
- [ ] 3 moyennes calculées (In-Ring, Entertainment, Story)
- [ ] Indicateurs de changement (↑↓)
- [ ] Tooltips sur tous les attributs

**Tab Contrats** :
- [ ] Contrat actuel affiché
- [ ] Historique visible
- [ ] Actions fonctionnelles (Renégocier, Prolonger, Libérer)

**Tab Gimmick** :
- [ ] Gimmick éditable
- [ ] Alignment modifiable (Face/Heel/Tweener)
- [ ] Push Level ajustable
- [ ] Finishers/Signatures listés

**Tab Relations** :
- [ ] Relations 1-à-1 affichées avec icônes
- [ ] Factions affichées avec membres
- [ ] CRUD complet (Add/Edit/Delete)
- [ ] Badges colorés par statut

**Tab Historique** :
- [ ] Biographie complète
- [ ] Matchs historiques avec notes
- [ ] Titres historiques
- [ ] Blessures historiques
- [ ] Stats W/L affichées

**Tab Notes** :
- [ ] Notes affichées triées par date
- [ ] Ajout de notes fonctionnel
- [ ] Édition/Suppression fonctionnelles
- [ ] Catégories visibles

**Général** :
- [ ] Support Worker/Staff/Trainee
- [ ] Navigation depuis RosterView
- [ ] Persistance en DB
- [ ] Responsive design

### Critères Qualité

- [ ] Code respecte MVVM
- [ ] Namespaces corrects
- [ ] Pas de code dupliqué
- [ ] UI cohérente avec RingGeneralTheme
- [ ] Tooltips partout où nécessaire
- [ ] Performance optimale
- [ ] Documentation complète

---

## 🎯 EXEMPLE VISUEL : Résultat Final

### Page Profil John Cena (Mockup)

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ JOHN CENA [USA] [■■■■■] PROFIL                                                         │
├───────────────────┬─────────────────────────────────────────────────────────────────────┤
│ ┌─────────┐       │ Rôle : Main Eventer (Star)    Contrat : 3,500,000 € / an          │
│ │         │       │ Style : Brawler / Powerhouse   Moral : Excellent                   │
│ │  PHOTO  │       │ Poids : 114 kg  Taille : 185 cm                                    │
│ │ 200x200 │       │ Droitier (Pied/Poing)  Exp. : 24 ans                               │
│ └─────────┘       │ ─────────────────────────────────────────────────────────────────  │
│ 46 ans            │ [ Condition: 78% ] [ Forme: 88% ] [ Fatigue: 35% ] [ Pop: 95 ]    │
│                   │                                                                     │
│ [📁 Changer]     │ 📅 Âge: 46 ans (27 avril 1977)                                     │
│ [🎨 Avatar]      │ 🌍 Naissance: West Newbury, Massachusetts, USA                     │
│                   │ 🏠 Résidence: Tampa, Floride, USA                                  │
└───────────────────┴─────────────────────────────────────────────────────────────────────┘

┌─ TAB CONTROL ───────────────────────────────────────────────────────────────────────────┐
│ [📊 ATTRIBUTS] [📝 CONTRATS] [🎭 GIMMICK] [👥 RELATIONS] [📖 HISTORIQUE] [📌 NOTES]   │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                          │
│ IN-RING (Moy: 82)          ENTERTAINMENT (Moy: 88)      STORY (Moy: 80)                │
│ ┌───────────────────────┐  ┌───────────────────────┐    ┌───────────────────────┐     │
│ │ Striking       │ 75   │  │ Charisme       │ 92   │    │ Prof. Perso    │ 84   │     │
│ │ Grappling      │ 78   │  │ Mic Work       │ 95   │    │ Cohérence      │ 90   │     │
│ │ High-Flying    │ 45   │  │ Acting         │ 88   │    │ Perf. Heel     │ 80   │     │
│ │ Force Brute    │ 90   │  │ Connexion      │ 98   │    │ Perf. Face     │ 95   │     │
│ │ Timing         │ 85   │  │ Star Power     │ 95   │    │ Storytelling   │ 88   │     │
│ │ Selling        │ 82   │  │ Improvisation  │ 90   │    │ Émotion        │ 85   │     │
│ │ Psychologie    │ 88   │  │ Entrée         │ 92   │    │ Adaptabilité   │ 75   │     │
│ │ Stamina        │ 85   │  │ Sex Appeal     │ 85   │    │ Alchimie       │ 82   │     │
│ │ Sécurité       │ 94   │  │ Merchandising  │ 96   │    │ Vision Créative│ 78   │     │
│ │ Hardcore/Brawl │ 80   │  │ Aura           │ 94   │    │ Nuances        │ 72   │     │
│ └───────────────────────┘  └───────────────────────┘    └───────────────────────┘     │
│                                                                                          │
│ ┌─ HISTORIQUE DES PERFORMANCES ─────────────────────────────────────────────────────┐  │
│ │ [RAW] vs Randy Orton ⭐⭐⭐⭐½ (92) | [SD!] vs AJ Styles ⭐⭐⭐⭐⭐ (97)           │  │
│ │ [PPV] vs Kevin Owens ⭐⭐⭐⭐ (85)  | [RAW] vs Solo Sikoa ⭐⭐⭐½ (72)            │  │
│ └────────────────────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## ⚠️ RISQUES ET MITIGATION

### Risque 1 : Complexité de l'Intégration

**Impact** : Combiner 2 plans = plus de points de friction

**Mitigation** :
- Tests d'intégration rigoureux (Phase 7)
- Validation après chaque phase
- Rollback possible (migration réversible)

### Risque 2 : Performance avec 30 Attributs

**Impact** : Chargement lent si mal optimisé

**Mitigation** :
- Colonnes calculées en SQL pour moyennes
- Index sur clés étrangères
- Lazy loading si nécessaire
- Tests de performance < 500ms

### Risque 3 : UI Surchargée

**Impact** : 30 attributs + 6 tabs = beaucoup de contenu

**Mitigation** :
- Expanders collapsibles
- Lazy loading des tabs
- Groupement par catégorie claire
- Tooltips pour éviter surcharge visuelle

### Risque 4 : Durée du Projet (4-5 semaines)

**Impact** : Projet long = risque de dérive

**Mitigation** :
- Revues hebdomadaires avec Chef de Projet
- Validation après chaque phase
- Livrables clairs et mesurables
- Parallélisation (Phase 6 + Phase 5)

---

## 📁 FICHIERS IMPACTÉS (Total : ~60 fichiers)

### Nouveaux Fichiers (~50)

**Base de Données** (1) :
1. `/src/RingGeneral.Data/Migrations/Migration_Master_ProfileViewAttributs.sql`

**Models** (11) :
2-4. WorkerInRingAttributes.cs, WorkerEntertainmentAttributes.cs, WorkerStoryAttributes.cs
5-7. WorkerRelation.cs, Faction.cs, FactionMember.cs
8-12. WorkerSpecialization.cs, WorkerNote.cs, ContractHistory.cs, MatchHistoryItem.cs, TitleReign.cs

**Repositories** (6) :
13-14. IWorkerAttributesRepository.cs, WorkerAttributesRepository.cs
15-16. IRelationsRepository.cs, RelationsRepository.cs
17-18. INotesRepository.cs, NotesRepository.cs

**ViewModels** (10) :
19-28. ProfileViewModel, ProfileMainViewModel, AttributesTabViewModel, ContractsTabViewModel, GimmickTabViewModel, RelationsTabViewModel, WorkerRelationViewModel, FactionViewModel, HistoryTabViewModel, NotesTabViewModel

**Views** (16) :
29-44. ProfileView.axaml/.cs, ProfileMainView.axaml/.cs, AttributesTabView.axaml/.cs, ContractsTabView.axaml/.cs, GimmickTabView.axaml/.cs, RelationsTabView.axaml/.cs, HistoryTabView.axaml/.cs, NotesTabView.axaml/.cs

**Resources** (3) :
45-47. AttributeDescriptions.fr.resx, WorkersAttributesSeed.sql, WorkersSpecializationsSeed.sql

**Tests** (~10) :
48-57. Tests pour Models, Repositories, ViewModels, Integration

**Documentation** (3) :
58-60. Guide Utilisateur, Guide Développeur, Guide Migration

### Fichiers Modifiés (~10)
- Worker.cs (13 props + navigation)
- App.axaml.cs (DI)
- MainWindow.axaml (DataTemplates)
- CURRENT_STATE.md
- PLAN_SPRINT_REVISE.md
- Etc.

---

## 🚀 WORKFLOW DE COORDINATION

### Communication Entre Sous-Agents

```
Phase 1-3 : Systems Architect (Solo)
    ↓
Phase 4 : Systems Architect (Solo)
    ↓
Phase 5 : UI Specialist (Solo) ←→ Phase 6 : Content Creator (Parallèle)
    ↓
Phase 7 : Systems Architect + UI Specialist (Collaboration)
    ↓
Phase 8 : File Cleaner (Solo)
```

### Points de Synchronisation

- **Fin Phase 1** : Validation DB par Chef de Projet
- **Fin Phase 2** : Validation Models par Chef de Projet
- **Fin Phase 3** : Validation Repos par Chef de Projet
- **Fin Phase 4** : Validation VMs + Handoff vers UI Specialist
- **Fin Phase 5** : Validation Views + Début Phase 7 (collaboration)
- **Fin Phase 7** : Validation Tests + Handoff vers File Cleaner
- **Fin Phase 8** : Validation Finale + Merge

### Réunions Hebdomadaires

- **Semaine 1** (Phases 1-2) : Lundi - Status DB, Vendredi - Status Models
- **Semaine 2** (Phase 3) : Lundi - Status Repos, Vendredi - Status DI
- **Semaine 3** (Phase 4) : Lundi - Status VMs Part 1, Vendredi - Status VMs Part 2
- **Semaine 4** (Phases 5-6) : Lundi - Status Views, Vendredi - Status Resources
- **Semaine 5** (Phases 7-8) : Lundi - Status Tests, Vendredi - Demo Finale

---

## ✅ CHECKLIST DE DÉMARRAGE

Avant de lancer le projet :

- [ ] Approuver ce plan master
- [ ] Créer backup complet de la base de données
- [ ] Valider que Sprint 1 (Composants UI) est terminé ✅
- [ ] Vérifier que AttributeBar component fonctionne ✅
- [ ] Vérifier que RingGeneralTheme.axaml est prêt ✅
- [ ] Assigner Systems Architect (Phases 1-4)
- [ ] Assigner UI Specialist (Phase 5)
- [ ] Assigner Content Creator (Phase 6)
- [ ] Assigner File Cleaner (Phase 8)
- [ ] Configurer environnement de test
- [ ] Préparer outils de suivi (Kanban, etc.)

---

## 📊 MÉTRIQUES DE SUCCÈS

### Métriques Techniques
- **Performance** : Chargement profil < 500ms
- **Coverage** : Tests > 80%
- **Compilation** : 0 warnings
- **Documentation** : 100% des APIs documentées

### Métriques Fonctionnelles
- **30 attributs** affichés et fonctionnels
- **6 tabs** complets et navigables
- **Fiche personnage** avec photo et géographie
- **Relations + Factions** CRUD complet

### Métriques Qualité
- **Code** : MVVM strict, namespaces corrects
- **UI** : Thème cohérent, responsive
- **UX** : Tooltips partout, navigation fluide

---

## 🎯 APRÈS LE PLAN MASTER

Une fois ce plan master complété, débloquer :

**Sprint 3** : Résultats de Simulation
- Utiliser historique des matchs (Tab Historique)
- Afficher impacts sur attributs (Tab Attributs)

**Sprint 4** : Inbox & Actualités
- Générer alertes fins de contrat (Tab Contrats)
- Messages progression attributs (Tab Attributs)

**Sprint 6** : Boucle de Jeu Complète
- ProfileView utilisé après chaque simulation
- Tracking complet de l'évolution

---

## 📞 CONTACT CHEF DE PROJET

**En cas de questions ou blocages** :
- Phase bloquante : Remonter immédiatement
- Besoin de clarification : Demander au Chef de Projet
- Découverte de risque : Alerter et proposer mitigation

---

**Version** : 1.0 - Plan Master Combiné
**Auteur** : Chef de Projet DevOps (Claude)
**Date de création** : 7 janvier 2026
**Statut** : ⏸️ EN ATTENTE DE VALIDATION CLIENT

---

## 🎯 PROCHAINE ÉTAPE

**Chef de Projet** → **Vous (Client)**

Je reviens vers toi avec ce plan master combiné de **60 fichiers** et **4-5 semaines**.

**Questions pour validation** :

1. ✅ **Approuves-tu la durée** de 4-5 semaines ?
2. ✅ **Approuves-tu le périmètre** (30 attributs + 6 tabs) ?
3. ✅ **Approuves-tu l'ordre des phases** (DB → Models → Repos → VMs → Views) ?
4. ✅ **Veux-tu ajuster quelque chose** avant le lancement ?

**Si tu valides, je lance Phase 1 immédiatement !** 🚀
