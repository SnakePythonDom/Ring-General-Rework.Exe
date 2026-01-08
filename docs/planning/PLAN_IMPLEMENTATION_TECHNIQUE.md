# Plan d'Implémentation Technique - Ring General

**Version:** 1.1 (Révisé)
**Date:** 7 janvier 2026
**Branche:** `claude/ring-general-implementation-plan-QS8kR`

---

## ⚠️ AVERTISSEMENT IMPORTANT

Ce document représente un **plan aspirationnel** détaillant la vision complète du projet Ring General sur 12-18 mois.

**Pour l'état FACTUEL et ACTUEL du projet**, consultez plutôt :
- 📊 **[CURRENT_STATE.md](./CURRENT_STATE.md)** - État réel basé sur audit du code (7 jan 2026)
- 🗺️ **[ROADMAP_MISE_A_JOUR.md](./ROADMAP_MISE_A_JOUR.md)** - Roadmap court terme mise à jour

**Découverte Clé (7 jan 2026)** : Suite à un audit exhaustif, le projet est **plus avancé que prévu** :
- ViewModels : **92%** complétés (pas 20%)
- Views : **65%** complétées (pas 10%)
- Navigation : **95%** complète (pas 80%)
- Seed Data : **100%** implémenté (pas 0%)

Voir section "État Actuel Révisé" ci-dessous pour les corrections.

---

## Table des Matières

1. [Vue d'Ensemble](#vue-densemble)
2. [État Actuel du Projet](#état-actuel-du-projet)
3. [Phase 1 : Le Socle Jouable](#phase-1--le-socle-jouable)
4. [Phase 2 : La Profondeur Stratégique](#phase-2--la-profondeur-stratégique)
5. [Dépendances et Ordre d'Implémentation](#dépendances-et-ordre-dimplémentation)
6. [Critères de Validation](#critères-de-validation)

---

## Vue d'Ensemble

### Objectif
Transformer la vision ambitieuse du projet "Ring General" en un plan d'implémentation pragmatique et séquentiel, avec comme priorité absolue la création d'un **socle jouable** avant d'ajouter les systèmes de simulation profonde.

### Philosophie de Développement
- **Phase 1** : Boucle de jeu complète et fonctionnelle (3-6 mois)
- **Phase 2** : Systèmes de simulation avancés et profondeur stratégique (6-12 mois)
- **Approche** : Itérative, testable à chaque étape, dérisquée

### Stack Technique Confirmée
- **UI** : Avalonia UI 11.0.6 (cross-platform)
- **Architecture** : MVVM + ReactiveUI
- **Runtime** : .NET 8.0
- **Base de données** : SQLite avec migrations ADO.NET
- **DI** : Microsoft.Extensions.DependencyInjection
- **Langage** : C# 12

---

## État Actuel Révisé du Projet

⚠️ **RÉVISION POST-AUDIT (7 janvier 2026)** : Cette section a été entièrement revue suite à un audit exhaustif du code source.

### ✅ Éléments Déjà Implémentés (35-40% de complétion MVP - RÉVISÉ)

#### Infrastructure (95% COMPLET - au lieu de 80%)
- ✅ Architecture MVVM avec ReactiveUI
- ✅ Système de navigation à 3 colonnes **FONCTIONNEL** (TreeNav + Content + Context)
- ✅ Dependency Injection configurée (App.axaml.cs)
- ✅ Event Aggregator (Pub/Sub messaging)
- ✅ **DbSeeder complet avec import BAKI** (non documenté précédemment)
- ✅ SaveGameManager (sauvegarde/chargement)
- ✅ **13 Views créées et câblées** (vs 1 documenté)
- ✅ **12 ViewModels principaux** (vs 2 documentés)
- ✅ **33 ViewModels de support** (non documentés)

#### Base de Données (90% COMPLET)
- ✅ Schéma complet (30+ tables)
- ✅ **17 Repositories créés** (GameRepository, ShowRepository, WorkerRepository, TitleRepository, ContractRepository, BackstageRepository, CompanyRepository, MedicalRepository, YouthRepository, ScoutingRepository, etc.)
- ⚠️ Seulement 2/17 enregistrés directement dans DI (GameRepository, ScoutingRepository) - **Reste à faire**
- ✅ Modèles de domaine complets (26 fichiers dans /Models/)
- ✅ **DbSeeder implémenté** (non documenté avant)
- ✅ **Import automatique depuis BAKI1.1.db** (non documenté avant)

#### Moteur de Simulation (90% COMPLET - Backend)
- ✅ ShowSimulationEngine (435 lignes - très sophistiqué)
  - Tous les calculs mentionnés sont implémentés
- ❌ **UI des résultats manquante** (ShowResultsView à créer)

#### UI - ViewModels (92% COMPLET - Major Révision!)
**12 ViewModels Principaux ✅** :
- ShellViewModel, StartViewModel, CompanySelectorViewModel, CreateCompanyViewModel
- DashboardViewModel, BookingViewModel, RosterViewModel, WorkerDetailViewModel
- TitlesViewModel, StorylinesViewModel, YouthViewModel, FinanceViewModel, CalendarViewModel

**33 ViewModels de Support ✅** :
- SegmentViewModel, ParticipantViewModel, SegmentTypeCatalog, etc.

**Total : 46 fichiers ViewModels** (vs 2 documentés initialement)

#### UI - Views (65% COMPLET - Major Révision!)
**13 Views Créées ✅** :
- MainWindow, StartView, CompanySelectorView, CreateCompanyView
- DashboardView, BookingView, RosterView, WorkerDetailView, TitlesView
- StorylinesView, YouthView, FinanceView, CalendarView

**Toutes câblées avec DataTemplates ✅**

#### Services Implémentés
**Services Core** (6/20):
- ✅ BookingBuilderService, ContenderService, ShowSchedulerService
- ✅ StorylineService, TemplateService, TitleService

**Services UI** (7/10):
- ✅ NavigationService, EventAggregator, SaveStorageService
- ✅ HelpContentProvider, TooltipHelper, UiPageSpecsProvider, NavigationSpecMapper

**Services Data**:
- ✅ WorkerService

### ⚠️ Éléments Partiellement Implémentés (Confirmé)

- ⚠️ Système de contrats (modèles + repo ✅, UI ❌)
- ⚠️ Développement jeunes (logique ✅, UI basique)
- ⚠️ Storylines (logique ✅, UI basique)
- ⚠️ Médical/Blessures (calcul ✅, UI ❌)
- ⚠️ Scouting (service + repo ✅, UI ❌)
- ⚠️ Booking (fonctionnel ✅, features avancées ❌)

### ❌ Éléments Non Implémentés (Confirmé)

- ❌ Composants UI réutilisables (AttributeBar, SortableDataGrid, DetailPanel, NewsCard)
- ❌ ProfileView universel (Worker/Staff/Trainee)
- ❌ InboxViewModel/View
- ❌ ShowCreationDialog
- ❌ ShowResultsView
- ❌ ContractNegotiationDialog
- ❌ Négociation de contrats (logique)
- ❌ Gestion des deals TV (UI)
- ❌ Finance avancée (budget allocation UI)
- ❌ Simulation du monde (LOD, IA)
- ❌ Gestion des coulisses (UI)
- ❌ Encyclopedia/Tutoriels
- ❌ Outils de modding (UI)
- ❌ **Boucle de jeu complète end-to-end** (critique)

### ✅ Corrections Majeures vs Documentation Initiale

| Élément | Docs Disaient | **Réalité** | Écart |
|---------|---------------|-------------|-------|
| ViewModels | 20% (2/10) | **92%** (46 fichiers) | +72% ⬆️ |
| Views | 10% (1/10) | **65%** (13/20) | +55% ⬆️ |
| Navigation | 80% | **95%** | +15% ⬆️ |
| Seed Data | 0% (base vide) | **100%** (DbSeeder complet) | +100% ⬆️ |
| Repositories | Partiels | **100%** créés (12% en DI) | Nuancé |
| Services Core | 70% | **30%** | -40% ⬇️ |

### 🐛 Dette Technique Identifiée

1. GameRepository trop large (1675 lignes) - en cours de refactoring
2. Duplication du schéma (migrations vs code)
3. ViewModels monolithiques à découper
4. Tests unitaires désynchronisés
5. Context panel (colonne droite) non implémenté
6. DataTemplates manquants pour certains ViewModels

---

## Phase 1 : Le Socle Jouable

**Objectif** : Livrer une boucle de jeu complète, stable et testable en **3 à 6 mois**.

**Critère de succès** : Un joueur peut :
1. Créer une nouvelle partie
2. Signer un catcheur
3. Booker un show complet
4. Le simuler
5. Analyser les résultats
6. Passer à la semaine suivante
7. Répéter le cycle

---

### 1.1 Infrastructure Technique et UI/UX

**Priorité** : CRITIQUE
**Dépendances** : Aucune
**Durée estimée** : 2-3 semaines

#### Tâche 1.1.1 : Finaliser la Localisation Française
**Statut** : ⚠️ Partiel (UI en français, formats à vérifier)

**Actions** :
- [ ] Audit de tous les libellés UI (vérifier la cohérence)
- [ ] Implémenter le formatage des dates (format français : dd/MM/yyyy)
- [ ] Implémenter le formatage des devises (€ avec espace insécable)
- [ ] Créer un fichier de ressources centralisé (`Strings.fr.resx`)
- [ ] Ajouter des tooltips en français sur tous les composants UI

**Livrables** :
- Fichier `Resources/Strings.fr.resx`
- Service de localisation `ILocalizationService`
- Documentation des conventions de formatage

---

#### Tâche 1.1.2 : Compléter le Kit d'Interface "FM26-style"
**Statut** : ⚠️ Partiel (Navigation OK, composants réutilisables manquants)

**Actions** :
- [ ] Créer un composant `SortableDataGrid` réutilisable
  - Tri multi-colonnes
  - Filtrage avancé (texte, plages, checkboxes)
  - Export CSV
  - Sélection multiple
- [ ] Créer un composant `DetailPanel` pour le context panel (colonne droite)
  - Design "sticky" avec sections collapsibles
  - Gestion des profils (Worker, Show, Storyline, Title)
- [ ] Créer un composant `AttributeBar` pour afficher les stats (1-20)
  - Barre visuelle avec couleur graduée
  - Tooltip avec description de l'attribut
  - Flèches de progression (↑↓) si changement récent
- [ ] Créer un composant `NewsCard` pour l'inbox
  - Icône par type de message
  - Badge "Non lu"
  - Actions rapides (Archiver, Répondre)
- [ ] Créer un style guide unifié (`Styles/RingGeneralTheme.axaml`)

**Livrables** :
- `/src/RingGeneral.UI/Components/SortableDataGrid.axaml`
- `/src/RingGeneral.UI/Components/DetailPanel.axaml`
- `/src/RingGeneral.UI/Components/AttributeBar.axaml`
- `/src/RingGeneral.UI/Components/NewsCard.axaml`
- `/src/RingGeneral.UI/Styles/RingGeneralTheme.axaml`
- Documentation des composants

---

#### Tâche 1.1.3 : Système de Persistance Production-Ready
**Statut** : ✅ Implémenté (SaveGameManager existe, tests manquants)

**Actions** :
- [ ] Ajouter la gestion des erreurs robuste dans `SaveGameManager`
  - Try-catch avec logging détaillé
  - Validation de l'intégrité du fichier de sauvegarde
  - Backup automatique avant écrasement
- [ ] Implémenter le versioning des sauvegardes
  - Ajout d'un champ `Version` dans les métadonnées
  - Migration automatique si version antérieure détectée
- [ ] Créer une UI de gestion des sauvegardes
  - Liste des sauvegardes avec métadonnées (date, version, compagnie)
  - Prévisualisation (stats de la compagnie)
  - Suppression avec confirmation
- [ ] Créer des tests d'intégration pour Save/Load
  - Test de round-trip (Save → Load → Compare)
  - Test de corruption (fichier tronqué)
  - Test de backward compatibility

**Livrables** :
- `SaveGameManager.cs` renforcé
- `SaveGameMetadata.cs` (modèle)
- `/Views/Start/SaveGameListView.axaml`
- `/tests/RingGeneral.Tests/Integration/SaveGameTests.cs`

---

#### Tâche 1.1.4 : Fiche de Profil Universelle
**Statut** : ⚠️ Partiel (WorkerDetailView existe, incomplet)

**Actions** :
- [ ] Refactoriser `WorkerDetailView` en composant générique `ProfileView`
  - Support Worker, Staff, Trainee
  - Onglets : Profil, Attributs, Historique, Contrat
- [ ] Onglet "Profil"
  - Photo (placeholder si absente)
  - Infos générales (âge, nationalité, taille, poids)
  - Gimmick actuel
  - Statut (actif, blessé, suspendu)
- [ ] Onglet "Attributs"
  - **Universels** : Condition Physique, Moral
  - **Catcheurs** : In-Ring (6 stats), Entertainment (4 stats), Story (3 stats)
  - **Staff** : Backstage (4 stats), Coaching (3 stats)
  - **Trainees** : Potentiel (5 stats)
  - Chaque stat avec `AttributeBar` + tooltip détaillé
- [ ] Onglet "Historique"
  - Derniers matchs (5) avec notes
  - Dernières storylines
  - Titres détenus (actuels et passés)
- [ ] Onglet "Contrat"
  - Termes du contrat (salaire, dates, clauses)
  - Bouton "Renégocier" ou "Libérer"

**Livrables** :
- `/Views/Shared/ProfileView.axaml`
- `/ViewModels/Shared/ProfileViewModel.cs`
- Tooltips pour tous les attributs (fichier `AttributeDescriptions.fr.resx`)

---

### 1.2 Cœur de la Boucle de Jeu Hebdomadaire

**Priorité** : CRITIQUE
**Dépendances** : 1.1 (UI components)
**Durée estimée** : 4-6 semaines

---

#### Tâche 1.2.1 : Inbox et Actualités (v1)
**Statut** : ❌ Non implémenté (tables DB existent)

**Actions** :
- [ ] Créer `InboxService` avec générateurs de messages
  - **Type 1** : Fin de contrat imminente (30 jours avant expiration)
  - **Type 2** : Blessure confirmée (après simulation de show)
  - **Type 3** : Scout report disponible
  - **Type 4** : Progression notable d'un trainee
  - **Type 5** : Alerte financière (trésorerie < seuil)
- [ ] Implémenter le tri et filtrage des messages
  - Par type (dropdown)
  - Par statut (Non lu / Lu / Archivé)
  - Par date
- [ ] Créer `InboxViewModel` et `InboxView`
  - Liste avec `NewsCard` component
  - Détail du message dans le context panel
  - Actions : Marquer lu, Archiver, Supprimer
- [ ] Intégrer les triggers de génération de messages
  - Hook dans `WeeklyLoopService` pour fin de contrats
  - Hook dans `ShowSimulationEngine` pour blessures
  - Hook dans `ScoutingService` pour rapports

**Livrables** :
- `/src/RingGeneral.Core/Services/InboxService.cs`
- `/src/RingGeneral.UI/ViewModels/Inbox/InboxViewModel.cs`
- `/src/RingGeneral.UI/Views/Inbox/InboxView.axaml`
- Tests unitaires pour générateurs de messages

---

#### Tâche 1.2.2 : Calendrier et Création de Shows (v1)
**Statut** : ⚠️ Partiel (CalendarView existe, création manquante)

**Actions** :
- [ ] Créer `ShowCreationDialog` (popup ou wizard)
  - Champ : Nom du show
  - Champ : Date (DatePicker avec contraintes : pas dans le passé, pas le même jour qu'un autre show)
  - Dropdown : Région
  - Dropdown : Venue (chargé depuis DB selon région)
  - Slider : Durée estimée (1h à 4h)
  - Checkbox : Broadcast (Oui/Non)
  - Bouton : Créer
- [ ] Implémenter la validation de création
  - Vérifier pas de conflit de date
  - Vérifier budget suffisant pour louer la venue
  - Vérifier effectif disponible (au moins 6 workers non blessés)
- [ ] Mettre à jour `CalendarView` pour afficher les shows créés
  - Vue mensuelle (calendrier) avec shows en "cards"
  - Vue liste avec tri/filtrage
  - Clic sur un show → ouvre BookingView pour ce show
- [ ] Intégrer dans le flux de jeu
  - Bouton "Créer un Show" dans CalendarView
  - Validation → Show ajouté en BDD (statut "Scheduled")

**Livrables** :
- `/src/RingGeneral.UI/Views/Calendar/ShowCreationDialog.axaml`
- `/src/RingGeneral.UI/ViewModels/Calendar/ShowCreationViewModel.cs`
- Mise à jour de `CalendarViewModel` et `CalendarView`
- Tests de validation

---

#### Tâche 1.2.3 : Booking (v1 - Cœur de jeu)
**Statut** : ⚠️ Partiel (BookingView existe, fonctionnalités manquantes)

**Actions** :
- [ ] Améliorer `BookingView` pour la v1
  - **Section 1** : Entête du show (nom, date, durée totale, durée utilisée)
  - **Section 2** : Liste des segments (drag-and-drop pour réordonner)
  - **Section 3** : Détail du segment sélectionné (dans context panel)
  - Bouton "Ajouter un Segment" → ouvre `SegmentEditorDialog`
- [ ] Créer `SegmentEditorDialog` (popup ou inline)
  - Dropdown : Type de segment (Match, Promo, Interview, Vignette)
  - **Si Match** :
    - Dropdown : Type de match (Singles, Tag Team, Multi-Man, Title Match)
    - Participant picker (2+ workers selon type)
    - Slider : Durée (5-30 min)
    - Slider : Intensité (1-5)
    - Dropdown : Vainqueur
    - Checkbox : Main Event
  - **Si Promo/Interview** :
    - Participant picker (1-4 workers)
    - Slider : Durée (3-15 min)
  - **Si Vignette** :
    - Text field : Description
    - Slider : Durée (1-5 min)
  - Bouton : Ajouter / Modifier
- [ ] Implémenter `BookingValidator` (déjà créé, à compléter)
  - Validation 1 : Durée totale <= durée max du show
  - Validation 2 : Chaque worker apparaît max 2 fois
  - Validation 3 : Pas de back-to-back matches pour un worker
  - Validation 4 : Main event obligatoire
  - Validation 5 : Title match nécessite un titre actif
  - Affichage des erreurs en rouge dans l'UI
- [ ] Créer le bouton "Valider la Carte"
  - Lance `BookingValidator`
  - Si OK : bouton "Simuler le Show" devient actif
  - Si erreurs : affiche la liste des problèmes
- [ ] Sauvegarder l'état du booking
  - Auto-save toutes les 30 secondes (draft)
  - Save définitif au clic sur "Valider"

**Livrables** :
- Mise à jour de `BookingView.axaml` et `BookingViewModel.cs`
- `/Views/Booking/SegmentEditorDialog.axaml`
- `/ViewModels/Booking/SegmentEditorViewModel.cs`
- Amélioration de `BookingValidator.cs`
- Tests de validation exhaustifs

---

#### Tâche 1.2.4 : Simulation de Show et Résultats (v1)
**Statut** : ✅ Moteur implémenté, ❌ UI des résultats manquante

**Actions** :
- [ ] Créer `ShowResultsView` (nouvelle vue)
  - **Section 1** : Résumé global
    - Note globale du show (A+ à F)
    - Audience totale (estimée vs réelle)
    - Revenus totaux (tickets + merch + TV)
  - **Section 2** : Détail par segment
    - Tableau avec colonnes : Type, Participants, Durée, Note, Crowd Heat
    - Clic sur segment → détail dans context panel
  - **Section 3** : Impacts sur le roster
    - Liste des workers avec changements (Fatigue ↑, Momentum ↑↓, Blessures)
    - Tri par impact (blessures en premier)
  - **Section 4** : Progression des storylines
    - Storylines actives avec changement de heat
  - Bouton : "Retour au Dashboard" ou "Passer à la semaine suivante"
- [ ] Intégrer le bouton "Simuler le Show" dans `BookingView`
  - Confirmation dialog (car irréversible)
  - Appel à `ShowSimulationEngine.SimulateShow()`
  - Appel à `ImpactApplier.ApplyImpacts()` pour persister les résultats
  - Navigation vers `ShowResultsView` avec les résultats
- [ ] Créer `ShowResultsViewModel`
  - Chargement des résultats depuis `ShowRepository`
  - Mapping vers des DTOs affichables
  - Calcul des deltas (avant/après) pour Fatigue, Momentum, Popularity
- [ ] Sauvegarder les résultats dans `ShowHistory`
  - Insertion dans table `ShowHistory`
  - Archivage du show (statut "Completed")

**Livrables** :
- `/Views/Results/ShowResultsView.axaml`
- `/ViewModels/Results/ShowResultsViewModel.cs`
- Mise à jour de `BookingViewModel` (bouton Simuler)
- Tests d'intégration pour le flux Booking → Simulation → Résultats

---

### 1.3 Gestion Fondamentale du Roster

**Priorité** : HAUTE
**Dépendances** : 1.1 (ProfileView), 1.2.1 (Inbox)
**Durée estimée** : 3-4 semaines

---

#### Tâche 1.3.1 : Système de Contrats (v1)
**Statut** : ⚠️ Partiel (modèles + repo OK, UI manquante)

**Actions** :
- [ ] Créer `ContractNegotiationDialog`
  - **Étape 1** : Sélection du worker (depuis scouting ou free agents)
  - **Étape 2** : Offre initiale
    - Slider : Salaire annuel (min basé sur popularité du worker)
    - Slider : Durée (1-5 ans)
    - Checkbox : Exclusivité (oui/non)
    - Dropdown : Rôle (Main Event, Mid-Card, Undercard, Trainee)
  - **Étape 3** : Réponse du worker (simulée)
    - Accepté → contrat signé
    - Refusé → affiche raisons (salaire trop bas, rôle insuffisant)
    - Contre-offre → affiche demandes du worker
  - **Étape 4** : Contre-négociation (si contre-offre)
    - Ajustement de l'offre
    - Retour à Étape 3 (max 3 rounds)
  - Bouton : Abandonner / Signer
- [ ] Implémenter `ContractNegotiationService`
  - Calcul du salaire minimum acceptable (basé sur Popularity + InRing)
  - Calcul de la probabilité d'acceptation (fonction de l'écart offre/demande)
  - Génération de contre-offres réalistes
  - Historique des négociations (sauvegarde dans InboxItems)
- [ ] Créer `ContractsView` (nouvelle vue dans section Roster)
  - Tableau des contrats actifs avec colonnes :
    - Worker, Salaire, Début, Fin, Rôle, Statut (Actif, Expirant bientôt)
  - Filtres : Rôle, Statut
  - Tri par date d'expiration
  - Clic sur contrat → détail dans context panel
  - Bouton : "Renégocier" ou "Résilier" (avec pénalité financière)
- [ ] Intégrer dans le weekly loop
  - Génération d'alerte Inbox 30 jours avant expiration
  - Libération automatique si contrat expiré et non renouvelé
  - Déduction automatique des salaires chaque semaine

**Livrables** :
- `/Views/Contracts/ContractNegotiationDialog.axaml`
- `/ViewModels/Contracts/ContractNegotiationViewModel.cs`
- `/Services/ContractNegotiationService.cs`
- `/Views/Roster/ContractsView.axaml`
- `/ViewModels/Roster/ContractsViewModel.cs`
- Tests de logique de négociation

---

#### Tâche 1.3.2 : Système d'Attributs "Mix FM + TEW"
**Statut** : ✅ Modèles OK, ⚠️ UI incomplète (AttributeBar manquant)

**Actions** :
- [ ] Créer le fichier de ressources `AttributeDescriptions.fr.resx`
  - Description détaillée de chaque attribut (150-200 mots)
  - Exemple d'impact en jeu
  - Facteurs qui influencent la progression
- [ ] Implémenter le composant `AttributeBar` (si pas fait en 1.1.2)
  - Affichage visuel de 1 à 20
  - Couleur graduée (rouge < 50, orange 50-70, vert > 70)
  - Tooltip avec description depuis `AttributeDescriptions.fr.resx`
  - Flèche de tendance si changement dans les 4 dernières semaines
- [ ] Créer `AttributeCategoryPanel` (groupement d'attributs)
  - **Universels** : Condition Physique, Moral
  - **Catcheurs - In-Ring** : Timing, Psychology, Selling, Stamina, Safety, Technique
  - **Catcheurs - Entertainment** : Charisma, Promo, Crowd Connection, Star Power
  - **Catcheurs - Story** : Storytelling, Character Work, Versatility
  - **Staff - Backstage** : Respect, Politicking, Credibility, Eye for Talent
  - **Staff - Coaching** : Technique Teaching, Psychology Teaching, Promo Teaching
  - **Trainees - Potentiel** : In-Ring Ceiling, Charisma Ceiling, Athleticism, Learning Speed, Work Ethic
- [ ] Intégrer dans `ProfileView` (onglet Attributs)
  - Un `AttributeCategoryPanel` par catégorie
  - Sections collapsibles
- [ ] Ajouter l'historique de progression
  - Graphique linéaire (4 dernières semaines) par attribut
  - Tooltip avec événements clés (match important, coaching, blessure)

**Livrables** :
- `/Resources/AttributeDescriptions.fr.resx`
- `/Components/AttributeBar.axaml` et `/Components/AttributeCategoryPanel.axaml`
- Mise à jour de `ProfileView` (onglet Attributs)
- Documentation des attributs

---

### 1.4 Objectifs et Livrables de la Phase 1

**Critères de Validation** :

1. **Stabilité et Performance**
   - [ ] Application démarre sans crash en < 3 secondes
   - [ ] Sauvegarde/chargement fonctionne sans perte de données (100% de réussite sur 100 tests)
   - [ ] Navigation fluide entre toutes les vues (< 200ms de latence)
   - [ ] Aucune fuite mémoire détectable (test de 100 cycles de jeu)

2. **Boucle Jouable Complète**
   - [ ] Création de nouvelle partie fonctionnelle (avec seed data)
   - [ ] Signature d'un catcheur via négociation
   - [ ] Création d'un show dans le calendrier
   - [ ] Booking d'un show complet (min 5 segments) avec validation
   - [ ] Simulation du show avec génération de résultats
   - [ ] Affichage des résultats (notes, audience, impacts)
   - [ ] Passage à la semaine suivante avec mise à jour automatique (salaires, fatigue, inbox)
   - [ ] Répétabilité du cycle (minimum 10 semaines jouables sans bug)

3. **Validation du Gameplay de Base**
   - [ ] 10 testeurs alpha valident que le jeu est "engageant" (questionnaire)
   - [ ] Temps moyen pour booker un show < 10 minutes
   - [ ] Taux de complétion d'une partie de 10 semaines > 80%
   - [ ] Aucun bug bloquant (severity 1) détecté
   - [ ] Documentation utilisateur basique disponible (Quick Start Guide)

**Date cible de complétion** : T0 + 3 mois (MVP), T0 + 6 mois (polissage)

---

## Phase 2 : La Profondeur Stratégique

**Objectif** : Transformer le socle jouable en simulation profonde et immersive.
**Prérequis** : Phase 1 complète et validée.
**Durée estimée** : 6-12 mois

---

### 2.1 L'Écosystème de Développement des Talents

**Priorité** : HAUTE (cœur de la différenciation)
**Dépendances** : Phase 1 complète
**Durée estimée** : 10-14 semaines

---

#### Tâche 2.1.1 : Philosophies de Recrutement
**Statut** : ❌ Non implémenté

**Actions** :
- [ ] Créer `RecruitmentPhilosophyService`
  - Modélisation des 3 philosophies : Athlète d'Élite, Artisan Indépendant, Discipline et Tradition
  - Calcul de l'affinité entre philosophie et worker (matching score)
  - Impact sur le coût de recrutement et la durée de développement
- [ ] Créer `RecruitmentPhilosophySelector` (écran de configuration de compagnie)
  - Sélection de la philosophie principale (1 obligatoire)
  - Possibilité de philosophie secondaire (déblocable via upgrade)
  - Explication détaillée des impacts de chaque philosophie
- [ ] Implémenter les mécaniques de scouting spécifiques
  - **Athlète d'Élite** : Scan automatique des athlètes NCAA/MMA/NFL (génération procédurale)
  - **Artisan Indépendant** : Partenariats avec promotions indépendantes (liste de promotions affiliables)
  - **Discipline et Tradition** : Recrutement de jeunes 16-18 ans (génération avec attributs bruts)
- [ ] Créer les profiles types générés
  - **Athlète d'Élite** : Athleticism 80+, Technique 30-50, Charisma 40-60, Durée dev : 3-5 ans
  - **Artisan Indépendant** : Technique 70+, InRing 65+, Star Power 40-60, Durée dev : 1-3 ans
  - **Discipline et Tradition** : Tous attributs 40-60, Learning Speed 70+, Loyalty 90+, Durée dev : 4-7 ans
- [ ] Intégrer dans `ScoutingService` et `WorkerGenerationService`

**Livrables** :
- `/Services/Recruitment/RecruitmentPhilosophyService.cs`
- `/ViewModels/Company/RecruitmentPhilosophyViewModel.cs`
- `/Views/Company/RecruitmentPhilosophySelector.axaml`
- Mise à jour de `ScoutingService` et `WorkerGenerationService`
- Documentation des philosophies

---

#### Tâche 2.1.2 : Structures de Formation (Dojo, Performance Center, Club)
**Statut** : ⚠️ Partiel (YouthStructures en DB, logique manquante)

**Actions** :
- [ ] Créer `YouthStructureManager` (écran de gestion des structures)
  - Liste des structures possédées
  - Bouton "Créer une nouvelle structure"
  - Détail de la structure sélectionnée (dans context panel)
- [ ] Implémenter `YouthStructureCreationDialog`
  - Dropdown : Type (Dojo, Performance Center, Club)
  - Champ : Nom
  - Champ : Lieu (région)
  - Affichage du coût de création (variable selon type)
  - Affichage du coût d'exploitation mensuel
  - Confirmation → Création en BDD
- [ ] Modéliser les leviers spécifiques de chaque structure

  **Le Dojo ("The Forge")** :
  - Levier 1 : **Tâches Subalternes** (Oui/Non)
    - Si Oui : +10% Work Ethic, +5% Resilience, -5% Morale
  - Levier 2 : **Restriction Technique** (Oui/Non)
    - Si Oui : +15% Psychology, +10% Fundamentals, -10% Versatility
  - Levier 3 : **Intensité du Conditionnement** (Slider 1-5)
    - Impact : +5% Stamina par niveau, +2% Injury Risk par niveau au-dessus de 3
  - Sortie : Wrestlers avec haute Resilience, excellente Psychology, style "épuré"

  **Le Performance Center ("The Lab")** :
  - Levier 1 : **Focus Promo Lab** (Slider 0-100%)
    - Impact : +X% Charisma/Promo progression, -X% InRing progression
  - Levier 2 : **Production Training** (Oui/Non)
    - Si Oui : +20% Hard Cam Awareness, +10% Crowd Connection
  - Levier 3 : **Star Treatment** (Oui/Non)
    - Si Oui : +15% Star Power, +10% Confidence, -10% Work Ethic, Coût +50%
  - Sortie : Superstars TV-ready avec haut Charisma et Star Power

  **Le Club ("The Wild")** :
  - Levier 1 : **Affiliation Ouverte** (Oui/Non)
    - Si Oui : +50% de recrutement de membres amateurs, +20% Incident Risk
  - Levier 2 : **Encadrement Minimal** (Slider 0-100%)
    - Impact : -X% Coaching Cost, +X% Bad Habits Risk, +X% Improvisation
  - Levier 3 : **Shows Locaux** (Fréquence : 0-4/mois)
    - Impact : +Y% InRing progression, +Y% Fatigue, Revenus +Z€/show
  - Sortie : Workers bruts, excellents improvisateurs, risque de bad habits

- [ ] Implémenter `YouthStructureDetailPanel` (context panel)
  - Affichage des leviers avec sliders/checkboxes
  - Affichage des trainees actuels dans la structure (liste)
  - Statistiques : Coût mensuel, Nombre de trainees, Taux de réussite
  - Bouton : "Assigner un Trainee" / "Fermer la Structure"
- [ ] Créer `YouthStructureService` pour gérer les impacts
  - Calcul des modificateurs de progression selon les leviers
  - Application des modificateurs dans `YouthProgressionService`
  - Génération d'incidents (Club) ou de burnout (Performance Center)

**Livrables** :
- `/Views/Youth/YouthStructureManager.axaml`
- `/ViewModels/Youth/YouthStructureManagerViewModel.cs`
- `/Views/Youth/YouthStructureDetailPanel.axaml`
- `/Services/Youth/YouthStructureService.cs`
- Mise à jour de `YouthProgressionService`
- Tests de calcul des modificateurs

---

#### Tâche 2.1.3 : Pipeline de Développement (Child Companies, Excursions)
**Statut** : ❌ Non implémenté

**Actions** :
- [ ] Créer le système de **Child Companies**
  - Modèle : `ChildCompany` (nom, région, relation avec compagnie mère, niveau de développement)
  - Repository : `ChildCompanyRepository`
  - Vue : `ChildCompaniesView` (liste des compagnies affiliées)
- [ ] Implémenter la mécanique de **Territoire de Développement**
  - Upgrade d'une Child Company en "Official Development Territory"
  - Coût : Investissement initial + mensuel
  - Avantages :
    - Flux automatique de workers (Child → Parent)
    - Contrôle du booking de la Child
    - Sharing de staff (coaches)
- [ ] Créer le système d'**Excursions**
  - `ExcursionService` : Gestion des envois à l'étranger
  - Dialogue : `SendOnExcursionDialog`
    - Sélection du trainee
    - Sélection de la destination (Japon, Mexique, Europe, Indés US)
    - Durée (3-12 mois)
    - Objectif (Technique, Charisma, Polyvalence)
  - Résultats possibles :
    - **Succès Commercial** : Victoires, titres → +Popularity, +Confidence, Gimmick potentiel
    - **Échec Commercial, Gain Artistique** : Peu de victoires MAIS déblocage de Gimmick transformateur (ex: "Rainmaker")
    - **Échec Total** : Blessure, perte de confiance → -Morale, +Injury Risk
  - Probabilités : Succès 40%, Échec Artistique 35%, Échec Total 25%
- [ ] Implémenter le **Gimmick Unlock System**
  - Table `GimmickLibrary` (liste de gimmicks possibles)
  - Trigger : Excursion réussie → propose 2-3 gimmicks selon destination
  - Application : Le joueur choisit → update du worker
  - Impact : +Stats selon gimmick, +Popularity si bon match avec personnalité
- [ ] Créer le flux UI
  - Vue : `DevelopmentPipelineView` (diagramme du pipeline)
    - Club/Dojo → Child Company → Main Roster
    - Affichage des workers dans chaque étape
    - Bouton "Promouvoir" ou "Envoyer en Excursion"
  - Context panel : Détail du worker avec historique de développement

**Livrables** :
- `/Models/Youth/ChildCompany.cs`, `/Models/Youth/Excursion.cs`, `/Models/Youth/Gimmick.cs`
- `/Repositories/ChildCompanyRepository.cs`
- `/Services/Youth/ExcursionService.cs`
- `/Views/Youth/DevelopmentPipelineView.axaml`
- `/ViewModels/Youth/DevelopmentPipelineViewModel.cs`
- `/Views/Youth/SendOnExcursionDialog.axaml`
- Table `GimmickLibrary` (seed avec 50+ gimmicks)
- Tests de flux de développement

---

#### Tâche 2.1.4 : Mécaniques de Carrière et d'Échec
**Statut** : ❌ Non implémenté

**Actions** :
- [ ] Implémenter la mécanique de **Push Prématuré**
  - Ajout d'un attribut `MaturityInRing` (caché, calculé)
    - Formule : `(Experience * 0.6) + (Psychology * 0.3) + (Age * 0.1)`
    - Seuil : 60
  - Hook dans `ShowSimulationEngine`
    - Si worker avec `MaturityInRing < 60` dans Main Event ou Title Match
    - Appliquer malus -25% sur la note du match
    - Génération d'un message Inbox : "X semblait dépassé lors du main event"
    - Impact : -10 Confidence, -5 Morale
  - Warning dans `BookingView`
    - Si le joueur place un worker immature en Main Event
    - Affichage d'une icône d'alerte avec tooltip explicatif
- [ ] Implémenter la mécanique de **Burnout**
  - Ajout d'un attribut caché `PressureThreshold`
    - Basé sur Mental Strength et Morale
  - Calcul de la pression actuelle
    - Formule : `(Popularity * 0.5) + (Push Level * 0.3) + (Media Appearances * 0.2)`
  - Trigger de Burnout
    - Si Pression > PressureThreshold pendant 4 semaines consécutives
    - Événement : "X semble épuisé mentalement"
    - Conséquence : Arrêt de la progression de TOUS les attributs pendant 12 semaines
    - Option : Donner du temps off (4 semaines sans booking) → réduit la durée à 6 semaines
  - UI : Indicateur de pression dans `ProfileView`
    - Barre de pression (vert/orange/rouge)
    - Tooltip avec conseil
- [ ] Implémenter la mécanique de **Reconversion**
  - Trigger :
    - Worker > 35 ans avec stagnation de progression (0 amélioration en 24 semaines)
    - OU Worker avec 3+ blessures graves en 2 ans
  - Dialogue : `ReconversionProposalDialog`
    - Option 1 : Coach (si Psychology > 70)
    - Option 2 : Agent (si Charisma > 70)
    - Option 3 : Producteur (si Story > 70)
    - Option 4 : Refuser (worker continue mais avec risque de blessure accru)
  - Conversion :
    - Worker devient Staff avec attributs mappés
    - Exemple : InRing → Technique Teaching, Psychology → Psychology Teaching
    - Salaire réduit de 40%
    - Loyalty +20 (reconnaissance pour la reconversion)
  - UI : Bouton "Proposer une reconversion" dans `ProfileView` (onglet Contrat)

**Livrables** :
- Calcul de `MaturityInRing` dans `WorkerSnapshot`
- Hook dans `ShowSimulationEngine` pour Push Prématuré
- `/Services/Career/BurnoutService.cs`
- `/Services/Career/ReconversionService.cs`
- `/Views/Roster/ReconversionProposalDialog.axaml`
- Indicateur de pression dans `ProfileView`
- Tests de scénarios d'échec

---

### 2.2 Simulation Approfondie du Ring et des Coulisses

**Priorité** : MOYENNE-HAUTE
**Dépendances** : 2.1 (Développement)
**Durée estimée** : 8-10 semaines

---

#### Tâche 2.2.1 : Narration de Match et Psychologie du Ring
**Statut** : ❌ Non implémenté (ShowSimulationEngine basique existe)

**Actions** :
- [ ] Décomposer la simulation de match en **6 phases narratives**
  - Modèle : `MatchPhase` (enum : Establishment, Shine, Cutoff, Heat, Comeback, Finish)
  - Chaque phase a :
    - Durée (% de la durée totale du match)
    - Objectif narratif (string)
    - Impact sur crowd heat (modificateur)
- [ ] Implémenter `MatchNarrativeEngine`
  - Génération d'un "script" de match automatique basé sur :
    - Types de workers (Face/Heel)
    - Chemistry
    - Type de match (Title, Grudge, Exhibition)
  - Sortie : Séquence de phases avec durées et beats narratifs
- [ ] Ajouter les **notes de match** (Match Notes) dans `BookingView`
  - Champs optionnels par match :
    - Dropdown : "Structure narrative" (Standard, Heat prolongé, Comeback rapide, Finish surprise)
    - Slider : "Durée du Heat" (10%-50% du match)
    - Checkbox : "Ref Bump" (arbitre mis KO → permet interférence)
    - Checkbox : "False Finish" (nearfall à 2.9)
    - Text field : "Note libre" (ex: "Heel doit dominer")
  - Impact : Modificateurs sur le calcul de la note du match
- [ ] Implémenter les modificateurs narratifs
  - **Heat prolongé** : +10% Note si Psychology > 70, -15% si < 50 (risque de boring)
  - **Finish surprise** : +15% Note, +10% Crowd Pop
  - **False Finish** : +5% Note si exécuté dans les 2 dernières minutes
  - **Ref Bump** : Permet interférence → +20% Note si Heel, -10% si Face (dépend du contexte)
- [ ] Créer `MatchNarrativeReport` (dans ShowResultsView)
  - Affichage du déroulé du match phase par phase
  - Timeline visuelle avec crowd heat à chaque phase
  - Highlight des moments clés (False Finish, Ref Bump, Interference)
- [ ] Intégrer dans `ShowSimulationEngine`
  - Remplacement du calcul de note simpliste par `MatchNarrativeEngine`
  - Application des notes de match du joueur

**Livrables** :
- `/Services/Simulation/MatchNarrativeEngine.cs`
- `/Models/Simulation/MatchPhase.cs`, `/Models/Simulation/MatchNotes.cs`
- Ajout de "Match Notes" dans `SegmentEditorDialog`
- `/ViewModels/Results/MatchNarrativeReportViewModel.cs`
- Mise à jour de `ShowSimulationEngine`
- Tests de génération narrative

---

#### Tâche 2.2.2 : Facteur Humain et Culture des Vestiaires
**Statut** : ⚠️ Partiel (BackstageService existe, événements manquants)

**Actions** :
- [ ] Créer le système de **Morale de Vestiaire**
  - Modèle : `LockerRoomMorale` (global à la compagnie)
    - Score : 0-100
    - Facteurs : Respect des codes, Équité du booking, Salaires compétitifs, Résultats de la compagnie
  - Repository : `LockerRoomRepository`
- [ ] Implémenter les **événements backstage**

  **Événements Négatifs** (triggered aléatoirement ou par conditions) :
  - **"Jeune oublie de serrer les mains"**
    - Condition : Trainee promu au main roster < 4 semaines
    - Probabilité : 10% par show si le trainee est booké
    - Conséquence : -5 Morale (trainee), -3 Locker Room Morale, +Tension avec 2-3 veterans
  - **"Refus de mettre over un opponent"**
    - Condition : Worker avec Ego > 80 perd un match contre worker avec Popularity < son Popularity -20
    - Probabilité : 20%
    - Conséquence : Incident backstage, -10 Morale (les deux), -5 Locker Room Morale
  - **"Politicking pour un push"**
    - Condition : Worker avec Politicking > 70 et pas de storyline active depuis 8 semaines
    - Probabilité : 15% par semaine
    - Conséquence : Demande de meeting, pression sur le joueur pour le booker

  **Événements Positifs** (rituels) :
  - **"Rituel du pied essuyé"**
    - Condition : Worker avec Respect > 80
    - Probabilité : 5% par show
    - Conséquence : +2 Locker Room Morale, +5 Respect pour tous les jeunes présents
  - **"Veteran coach un jeune"**
    - Condition : Veteran avec Eye for Talent > 70 dans le même match qu'un Trainee
    - Probabilité : 30%
    - Conséquence : +10% progression InRing pour le Trainee pendant 4 semaines
  - **"Celebration d'équipe après un excellent show"**
    - Condition : Show avec note globale A ou A+
    - Probabilité : 80%
    - Conséquence : +10 Locker Room Morale, +5 Morale pour tous les workers bookés

- [ ] Créer `BackstageEventService`
  - Hook dans `ShowSimulationEngine` (après simulation)
  - Hook dans `WeeklyLoopService`
  - Génération des événements selon probabilités
  - Sauvegarde dans `BackstageIncidents` table
  - Génération d'un message Inbox pour notifier le joueur
- [ ] Créer `LockerRoomView` (nouvelle vue dans section Roster)
  - Affichage du score de Morale global (jauge)
  - Liste des incidents récents (4 dernières semaines)
  - Liste des "cliques" (groupes de workers avec affinité)
  - Liste des tensions actives (worker A vs worker B)
  - Bouton "Organiser une réunion de vestiaire" (coûte du temps, +5 Morale si réussie)
- [ ] Implémenter l'impact sur les matchs
  - Si tension entre deux workers → -15% Chemistry dans leurs matchs
  - Si bonne entente → +10% Chemistry
  - Calcul de la Chemistry dynamique basé sur l'historique backstage

**Livrables** :
- `/Models/Backstage/LockerRoomMorale.cs`, `/Models/Backstage/BackstageEvent.cs`
- `/Repositories/LockerRoomRepository.cs`
- `/Services/Backstage/BackstageEventService.cs`
- `/Views/Roster/LockerRoomView.axaml`
- `/ViewModels/Roster/LockerRoomViewModel.cs`
- Mise à jour de `ShowSimulationEngine` et `WeeklyLoopService`
- Tests de génération d'événements

---

#### Tâche 2.2.3 : Production et Gestion Médicale
**Statut** : ⚠️ Partiel (Injuries calculées, UI de gestion manquante)

**Actions** :
- [ ] Implémenter **"Conscience de la Hard Cam"**
  - Ajout d'un attribut `HardCamAwareness` (0-100)
  - Impact sur la note de match (Production Score)
    - Si HardCamAwareness > 70 → +10% Production Score
    - Si < 30 → -20% Production Score (mauvais cadrage, dos à la caméra)
  - Progression :
    - +5 par show si worker booké dans un match télévisé
    - +20 si formé dans un Performance Center avec "Production Training"
  - Affichage dans `ProfileView` (onglet Attributs, catégorie "Production")
- [ ] Créer le **Protocole Commotion**
  - Détection automatique pendant la simulation
    - Si Injury Type = "Concussion" pendant un match
    - Automatiquement : Match arrêté prématurément (si Safety Protocol activé dans Company Settings)
    - Génération d'un message Inbox : "Match arrêté - Protocole Commotion"
  - Conséquences :
    - Worker placé en "Concussion Protocol" (statut spécial)
    - Minimum 4 semaines d'absence (non négociable)
    - Tests hebdomadaires (simulated) avant clearance
    - Si retour prématuré forcé → risque de 2nd concussion (career-ending à 80%)
  - UI : Indicateur de statut dans `RosterView` et `ProfileView`
    - Icône spéciale pour "Concussion Protocol"
    - Tooltip avec semaines restantes
    - Bouton "Forcer le retour" (warning critique)
- [ ] Créer `MedicalManagementView` (nouvelle vue dans section Roster)
  - **Section 1** : Workers blessés actuellement
    - Tableau : Worker, Injury Type, Severity, Weeks Out, Return Date
    - Tri par gravité
  - **Section 2** : Historique médical
    - Filtre par worker
    - Liste des blessures passées avec durées
  - **Section 3** : Injury Risk Dashboard
    - Liste des workers avec Injury Risk > 60 (zone de danger)
    - Recommandation : "Donner du repos" ou "Réduire l'intensité des matchs"
  - Bouton : "Forcer un retour anticipé" (avec confirmation et warning)
- [ ] Implémenter le **Injury Prevention System**
  - Ajout d'un setting : "Injury Prevention Mode" (Off, Moderate, Strict)
    - **Moderate** : Alerte si Injury Risk > 70
    - **Strict** : Bloque le booking si Injury Risk > 80 (sauf override manuel)
  - Calcul du Injury Risk dynamique
    - Facteurs : Fatigue, Age, Safety (opponent), Match Intensity, Injury History
    - Formule : `(Fatigue * 0.4) + (Age * 0.2) + (100 - Safety) * 0.3 + (IntensityLevel * 10)`
  - Affichage dans `BookingView`
    - Icône de warning si Injury Risk > 70 pour un worker dans un segment

**Livrables** :
- Ajout de `HardCamAwareness` dans `WorkerSnapshot`
- `/Services/Medical/ConcussionProtocolService.cs`
- `/Services/Medical/InjuryPreventionService.cs`
- `/Views/Roster/MedicalManagementView.axaml`
- `/ViewModels/Roster/MedicalManagementViewModel.cs`
- Ajout de settings dans `CompanySettings`
- Mise à jour de `ShowSimulationEngine` pour Concussion Protocol
- Tests de protocole médical

---

### 2.3 Expansion des Systèmes de Gestion et du Monde

**Priorité** : MOYENNE
**Dépendances** : 2.1, 2.2
**Durée estimée** : 12-16 semaines

---

#### Tâche 2.3.1 : Finances Avancées
**Statut** : ⚠️ Partiel (FinanceEngine basique, UI manquante)

**Actions** :
- [ ] Créer `BudgetAllocationView`
  - **Section 1** : Budget Annuel
    - Revenus estimés (TV Deal + Tickets + Merch + Misc)
    - Dépenses fixes (Salaires, Venues, Production, Youth Structures)
    - Solde prévisionnel
  - **Section 2** : Allocation par Département
    - Sliders : % pour Talent (salaires), Production, Youth Dev, Marketing, Medical, Misc
    - Total doit = 100%
    - Impact de l'allocation affiché en temps réel (ex: +10% Youth Dev → +15% progression des trainees)
  - **Section 3** : Prévisions
    - Graphique de trésorerie projetée (12 prochains mois)
    - Seuils d'alerte (rouge si trésorerie < 100k dans les 3 mois)
  - Bouton : "Appliquer le Budget"
- [ ] Implémenter `RevenueProjectionService`
  - Calcul des revenus projetés basé sur :
    - Contrat TV (fixe + variable selon audience)
    - Tickets (basé sur venue capacity * fill rate estimé * prix moyen)
    - Merch (basé sur Popularity du roster * taux de conversion)
  - API : `GetProjectedRevenue(int weeksAhead)`
- [ ] Créer `ExpenseBreakdownView`
  - Tableau détaillé de toutes les dépenses mensuelles
  - Catégories : Salaires (detail par worker), Venues, Production, Travel, Medical, Misc
  - Graphique en camembert
  - Export CSV
- [ ] Implémenter le système de **Merchandising Personnalisé**
  - Table : `MerchandiseItems` (worker_id, item_type, price, popularity_threshold)
  - UI : `MerchandiseManagerView`
    - Création d'items de merch pour un worker (T-shirt, Poster, Action Figure)
    - Prix personnalisable
    - Seuil de popularité requis (minimum 60 pour vendre)
  - Calcul des ventes :
    - Formule : `(Worker Popularity * Fill Rate * 0.15) * Item Price * Margin`
  - Revenus ajoutés dans `FinanceTransactions`
- [ ] Créer le système de **Ticketing Dynamique**
  - Modèle : `TicketPricing` (venue_id, section, base_price, dynamic_multiplier)
  - Calcul du multiplicateur dynamique :
    - Basé sur Popularity du main event, Prestige du show, Demande historique
  - UI : `TicketPricingView` (pour chaque show)
    - Affichage des sections de la venue (Floor, Lower Bowl, Upper Bowl)
    - Prix suggéré vs Prix actuel
    - Projection de fill rate et revenus
  - Simulation de la vente :
    - Fill rate calculé selon `(Demand - AvgPrice) / Price Sensitivity`
    - Revenus = `Sum(Section Capacity * Fill Rate * Price)`

**Livrables** :
- `/Views/Finance/BudgetAllocationView.axaml`
- `/ViewModels/Finance/BudgetAllocationViewModel.cs`
- `/Services/Finance/RevenueProjectionService.cs`
- `/Views/Finance/ExpenseBreakdownView.axaml`
- `/Views/Finance/MerchandiseManagerView.axaml`
- `/Views/Finance/TicketPricingView.axaml`
- Tables : `MerchandiseItems`, `TicketPricing`
- Tests de projections financières

---

#### Tâche 2.3.2 : Diffusion et Contrats TV (Broadcasting)
**Statut** : ⚠️ Partiel (TVDeals table existe, UI manquante)

**Actions** :
- [ ] Créer `TVDealNegotiationView`
  - **Étape 1** : Sélection du network (liste des networks disponibles)
    - Affichage : Prestige du network, Reach potentiel, Exigences (min show quality, min roster size)
  - **Étape 2** : Termes du deal
    - Slider : Durée du contrat (1-5 ans)
    - Checkbox : Exclusivité (Oui/Non) → Impact sur le montant
    - Dropdown : Nombre de shows/an (12, 24, 52, 104)
    - Affichage de l'offre du network (calculée dynamiquement)
  - **Étape 3** : Négociation
    - Le joueur peut demander +10% / +20% / +30%
    - Probabilité d'acceptation basée sur Prestige de la compagnie
    - Contre-offre possible du network
  - **Étape 4** : Signature
    - Confirmation → Deal enregistré en BDD
- [ ] Implémenter `TVDealService`
  - Calcul de l'offre initiale
    - Formule : `(Network Prestige * 10k) + (Company Prestige * 5k) + (Avg Show Quality * 2k)`
  - Calcul des probabilités de négociation
    - +10% : 70% si Prestige > 60, 40% sinon
    - +20% : 40% si Prestige > 75, 15% sinon
    - +30% : 10% si Prestige > 85, 0% sinon
  - Application des clauses
    - **Exclusivité** : Si Oui, aucun autre deal TV possible, +30% montant
    - **Quality Clause** : Si moyenne des shows < seuil (ex: 70) pendant 3 mois → pénalité -20% revenus
- [ ] Créer `AudienceAnalyticsView`
  - **Section 1** : Audience Trends
    - Graphique linéaire : Audience moyenne par show (12 derniers mois)
    - Comparaison avec la moyenne de l'industrie
  - **Section 2** : Demographics
    - Graphique en barres : Répartition par âge (18-24, 25-34, 35-49, 50+)
    - Répartition par région
  - **Section 3** : Performance par Segment
    - Tableau : Type de segment, Audience moyenne, Rating
    - Identification des types qui "tirent" l'audience (ex: Main Event Title Matches)
  - Bouton : "Export Report" (PDF ou CSV)
- [ ] Implémenter `AudienceModelService`
  - Calcul de l'audience dynamique par segment
    - Formule de base : `BaseAudience * (SegmentQuality / 100) * (Star Power Factor)`
  - Simulation du "tune-in" et "tune-out"
    - Tune-in : +5% si segment avec high star power après un weak segment
    - Tune-out : -10% si 2 weak segments consécutifs
  - Calcul du Rating global (audience / population disponible)
- [ ] Créer les **Production Constraints** (clauses TV)
  - Clause : "Minimum 1 Title Match par show"
  - Clause : "Minimum 30 minutes de Main Event segment"
  - Clause : "Minimum 3 segments avec Star Power > 70"
  - Validation dans `BookingValidator`
    - Si clause non respectée → Warning + Pénalité de revenus TV (-15%)

**Livrables** :
- `/Views/Broadcasting/TVDealNegotiationView.axaml`
- `/ViewModels/Broadcasting/TVDealNegotiationViewModel.cs`
- `/Services/Broadcasting/TVDealService.cs`
- `/Views/Broadcasting/AudienceAnalyticsView.axaml`
- `/ViewModels/Broadcasting/AudienceAnalyticsViewModel.cs`
- `/Services/Simulation/AudienceModelService.cs`
- Mise à jour de `BookingValidator` pour Production Constraints
- Tests de négociation et d'audience

---

#### Tâche 2.3.3 : Storylines Avancées
**Statut** : ⚠️ Partiel (modèles + basique UI, gestion avancée manquante)

**Actions** :
- [ ] Créer `StorylineBuilderView`
  - **Étape 1** : Création
    - Champ : Titre de la storyline
    - Dropdown : Type (Feud, Alliance, Stable, Tournament, Title Hunt)
    - Participant Picker (2-8 workers selon type)
    - Dropdown : Intensité (Low, Medium, High, Blood Feud)
  - **Étape 2** : Arc Narratif
    - Timeline visuelle des phases
      - Phase 1 : Introduction (4 semaines)
      - Phase 2 : Escalation (4-8 semaines)
      - Phase 3 : Peak/Climax (2 semaines)
      - Phase 4 : Resolution (1 semaine)
    - Assignation de segments à chaque phase
      - Drag & drop depuis un booking vers une phase de storyline
  - **Étape 3** : Objectifs et Payoff
    - Dropdown : Payoff (Title Change, Heel Turn, Face Turn, Split, Retirement)
    - Slider : Heat Target (objectif de heat à atteindre pour le climax)
  - Bouton : "Lancer la Storyline"
- [ ] Implémenter `StorylineProgressionService`
  - Calcul du Heat dynamique
    - Formule : `Base Heat + (Segment Quality * 5) + (Crowd Reaction * 3) - (Time Decay * 2)`
    - Time Decay : -5 heat par semaine sans segment lié à la storyline
  - Calcul de la Completion
    - Basé sur le nombre de segments complétés vs planifiés
    - Basé sur le Heat atteint vs Heat Target
  - Trigger de payoff automatique
    - Si Heat > Heat Target et Phase = Climax → Suggest Payoff
    - Génération d'un message Inbox : "Storyline X prête pour le payoff"
- [ ] Créer `StorylineManagerView` (refonte de l'actuelle)
  - **Section 1** : Storylines Actives
    - Tableau : Titre, Participants, Phase, Heat, Completion %
    - Tri par Heat (descendant)
  - **Section 2** : Détail de la storyline sélectionnée (context panel)
    - Timeline des segments passés
    - Heat graph (évolution sur les 12 dernières semaines)
    - Prochains segments suggérés
    - Bouton : "Modifier" / "Terminer Prématurément" / "Trigger Payoff"
  - **Section 3** : Archive
    - Storylines terminées avec notes finales
- [ ] Implémenter les **Storyline Effects**
  - Impact sur les workers
    - Participation à une storyline High Heat → +5 Popularity/semaine
    - Participation à une storyline Low Heat → -2 Popularity/semaine
    - Payoff réussi (Heat > 80) → +10 Popularity, +15 Momentum
    - Payoff raté (Heat < 40) → -5 Popularity, -10 Morale
  - Impact sur les matchs
    - Workers dans une storyline commune → +20% Chemistry
    - Match "Payoff" (climax) → Bonus de +30% sur la note globale si bien exécuté
- [ ] Créer le système de **Turns** (Heel/Face)
  - Trigger manuel dans `StorylineBuilderView` (Payoff = Heel Turn ou Face Turn)
  - Calcul de l'impact
    - Turn réussi si Heat > 70 et Charisma > 65 → +20 Popularity
    - Turn raté si Heat < 50 → -10 Popularity, Confusion du public (malus temporaire)
  - Update de l'alignement du worker en BDD
  - Impact sur la réaction de la foule dans les shows suivants

**Livrables** :
- `/Views/Storylines/StorylineBuilderView.axaml`
- `/ViewModels/Storylines/StorylineBuilderViewModel.cs`
- `/Services/Storylines/StorylineProgressionService.cs`
- Refonte de `/Views/Storylines/StorylineManagerView.axaml`
- `/Models/Storylines/StorylinePhase.cs`, `/Models/Storylines/Turn.cs`
- Mise à jour de `ShowSimulationEngine` pour Storyline Effects
- Tests de progression de storyline

---

#### Tâche 2.3.4 : Titres Avancés
**Statut** : ⚠️ Partiel (TitlesView basique, gestion avancée manquante)

**Actions** :
- [ ] Créer `TitleManagementView` (refonte)
  - **Section 1** : Liste des Titres
    - Tableau : Titre, Champion Actuel, Prestige, Jours de Règne, Division
    - Bouton : "Créer un Nouveau Titre"
  - **Section 2** : Détail du titre sélectionné (context panel)
    - Historique des règnes (liste avec durées)
    - Statistiques : Nombre de défenses, Règne le plus long, Règne le plus court
    - Prestige graph (évolution sur 52 semaines)
    - Contenders ranking (top 5)
  - **Section 3** : Actions
    - Bouton : "Créer un Contender Ranking"
    - Bouton : "Retirer le Titre" (vacancy)
    - Bouton : "Modifier les Propriétés"
- [ ] Implémenter `TitlePrestigeService`
  - Calcul dynamique du prestige
    - Formule : `Base Prestige + (Champion Popularity * 0.3) + (Avg Match Quality * 0.5) + (Defense Frequency * 0.2)`
    - Déclin : -1 prestige par semaine sans défense
  - Impact des Title Matches
    - Match de qualité A+ → +5 Prestige
    - Match de qualité D ou moins → -3 Prestige
    - Changement de champion avec Popularity > ancien champion → +10 Prestige
    - "Hot Potato" (changement de champion < 2 semaines après le précédent) → -15 Prestige
- [ ] Créer le **Contender Ranking System**
  - Modèle : `ContenderRanking` (title_id, worker_id, rank, points)
  - Calcul des points
    - Victoire dans un match simple : +5 points
    - Victoire dans un #1 Contender Match : +20 points
    - Défaite : -2 points
    - Participation à une storyline liée au titre : +3 points/semaine
  - UI : `ContenderRankingView`
    - Liste des contenders avec points et rank
    - Bouton : "Organiser un #1 Contender Match" (entre les top 2 ou top 4)
  - Auto-suggestion
    - Si le champion n'a pas défendu depuis 4 semaines → Message Inbox : "Suggérer un challenger depuis le ranking"
- [ ] Implémenter le système de **Tournois**
  - Modèle : `Tournament` (nom, title_id, participants[], bracket, status)
  - UI : `TournamentCreatorView`
    - Dropdown : Nombre de participants (4, 8, 16, 32)
    - Participant Picker
    - Génération automatique du bracket
    - Affichage visuel du bracket
  - Intégration dans le booking
    - Chaque match du tournoi doit être booké dans un show
    - Progression automatique du bracket après chaque match
    - Finaliste automatiquement devient #1 Contender ou champion (si pour un titre vacant)
  - UI : `TournamentProgressView`
    - Bracket interactif
    - Résultats des matchs complétés
    - Prochains matchs à booker
- [ ] Créer l'historique détaillé (`TitleHistoryView`)
  - Timeline des règnes avec photos des champions
  - Statistiques par règne : Durée, Nombre de défenses, Meilleur match
  - Graphique de prestige sur toute l'histoire du titre
  - Export en PDF (pour partage communautaire)

**Livrables** :
- Refonte de `/Views/Roster/TitlesView.axaml` → `/Views/Titles/TitleManagementView.axaml`
- `/ViewModels/Titles/TitleManagementViewModel.cs`
- `/Services/Titles/TitlePrestigeService.cs`
- `/Models/Titles/ContenderRanking.cs`, `/Models/Titles/Tournament.cs`
- `/Views/Titles/ContenderRankingView.axaml`
- `/Views/Titles/TournamentCreatorView.axaml`
- `/Views/Titles/TournamentProgressView.axaml`
- `/Views/Titles/TitleHistoryView.axaml`
- Tables : `ContenderRankings`, `Tournaments`, `TournamentMatches`
- Tests de calcul de prestige et de tournois

---

#### Tâche 2.3.5 : Monde Vivant (LOD) et IA des Compagnies
**Statut** : ❌ Non implémenté (WorldSimScheduler existe mais inactif)

**Actions** :
- [ ] Activer et finaliser `WorldSimScheduler`
  - Hook dans `WeeklyLoopService`
  - Appel à la simulation de toutes les compagnies non-jouées
  - Utilisation d'un système de Level of Detail (LOD) :
    - **LOD 0** (Compagnie jouée) : Simulation complète, segment par segment
    - **LOD 1** (Compagnies rivales directes) : Simulation simplifiée par show (notes globales, pas de détail par segment)
    - **LOD 2** (Compagnies lointaines) : Simulation ultra-simplifiée (1 calcul par mois, progression générale)
- [ ] Implémenter `AICompanyService`
  - Génération de booking automatique pour les compagnies IA
    - Algorithme : Prioriser les workers avec haute Popularity
    - Créer des matchs Title si >4 semaines sans défense
    - Générer des storylines basiques (2 workers avec Heat potentiel)
  - Gestion du roster IA
    - Signature de nouveaux workers si effectif < 20
    - Libération de workers si Morale < 30 ou Popularity en baisse depuis 12 semaines
  - Gestion financière IA
    - Budget simplifié : 70% salaires, 20% production, 10% développement
    - Bankruptcy si trésorerie < 0 pendant 12 semaines → Compagnie ferme
- [ ] Créer `WorldOverviewView`
  - **Section 1** : Carte du Monde
    - Vue géographique avec icônes de compagnies par région
    - Taille de l'icône proportionnelle au Prestige
  - **Section 2** : Classement Global
    - Tableau : Compagnie, Prestige, Revenus Annuels, Taille du Roster, Region
    - Tri par Prestige
  - **Section 3** : Détail d'une compagnie sélectionnée (context panel)
    - Top 5 workers
    - Champions actuels
    - Prochains shows
    - Relation avec la compagnie jouée (Allié, Neutre, Rival)
  - Bouton : "Proposer un Partenariat" ou "Déclarer Rivalité"
- [ ] Implémenter le système de **Mouvement de Workers**
  - Modèle : `TransferOffer` (worker_id, from_company, to_company, salary_offer, status)
  - UI : `TransferMarketView`
    - Liste des workers disponibles (contrat expirant ou free agents)
    - Filtres : Région, Popularité, Salaire
    - Bouton : "Faire une Offre"
  - Négociation
    - Si worker sous contrat → nécessite accord de la compagnie actuelle
    - Calcul de la probabilité d'acceptation (similaire aux contrats v1)
  - AI Behavior
    - Compagnies IA peuvent faire des offres aux workers de la compagnie jouée
    - Message Inbox : "X a reçu une offre de Y Company"
    - Le joueur peut matcher ou laisser partir
- [ ] Créer le système de **Relations entre Compagnies**
  - Modèle : `CompanyRelation` (company_a, company_b, relation_type, strength)
  - Types : Partnership (partage de talents), Rivalry (compétition), War (super-rivalry)
  - Impact :
    - Partnership → Possibilité d'envoyer des workers en excursion
    - Rivalry → Bonus de Heat si workers de compagnies rivales s'affrontent (invasion angle)
    - War → Events inter-promotions (nécessite accord mutuel)
  - UI : `CompanyRelationsView`
    - Liste des relations actives
    - Bouton : "Proposer un Partenariat" / "Déclarer une Rivalité"
    - Résultat : Accepté (si affinité), Refusé, Contre-offre

**Livrables** :
- Activation et finalisation de `/Services/Simulation/WorldSimScheduler.cs`
- `/Services/AI/AICompanyService.cs`
- `/Views/World/WorldOverviewView.axaml`
- `/ViewModels/World/WorldOverviewViewModel.cs`
- `/Views/World/TransferMarketView.axaml`
- `/ViewModels/World/TransferMarketViewModel.cs`
- `/Models/World/TransferOffer.cs`, `/Models/World/CompanyRelation.cs`
- `/Views/World/CompanyRelationsView.axaml`
- Tables : `TransferOffers`, `CompanyRelations`
- Tests de simulation IA et de mouvements de workers

---

#### Tâche 2.3.6 : Système d'Aide et Encyclopedia
**Statut** : ❌ Non implémenté

**Actions** :
- [ ] Créer `EncyclopediaView`
  - Navigation par catégories :
    - Gameplay (Booking, Simulation, Contrats, etc.)
    - Attributs (description de chaque stat)
    - Mécaniques (Heat, Momentum, Prestige, etc.)
    - Histoire (Historique des titres, légendes)
  - Recherche par mot-clé
  - Affichage en markdown avec images
- [ ] Créer le contenu de l'Encyclopedia
  - Rédaction de 50+ articles (500-800 mots chacun)
  - Couverture de tous les systèmes du jeu
  - Exemples concrets et captures d'écran
  - Fichiers markdown dans `/docs/encyclopedia/`
- [ ] Implémenter le système de **Tooltips Contextuels**
  - Service : `TooltipService`
  - Chargement des tooltips depuis un fichier JSON (`tooltips.json`)
  - Affichage automatique au survol (250ms de délai)
  - Contenu riche (texte + icônes + valeurs dynamiques)
- [ ] Créer le **Tutorial System**
  - Modèle : `TutorialStep` (id, title, description, target_ui_element, action_required)
  - Séquences de tutoriels :
    - Tutorial 1 : "Première Partie" (7 steps)
    - Tutorial 2 : "Booker votre Premier Show" (10 steps)
    - Tutorial 3 : "Gérer votre Roster" (8 steps)
  - UI : Overlay avec highlight de l'élément UI ciblé
  - Progression sauvegardée (skip possible)
  - Bouton : "Rejouer les Tutoriels" dans le menu Settings

**Livrables** :
- `/Views/Help/EncyclopediaView.axaml`
- `/ViewModels/Help/EncyclopediaViewModel.cs`
- `/docs/encyclopedia/` (50+ fichiers .md)
- `/Services/UI/TooltipService.cs`
- `/Resources/tooltips.json`
- `/Services/Tutorial/TutorialService.cs`
- `/Models/Tutorial/TutorialStep.cs`
- Tests d'affichage de tooltips et de tutoriels

---

#### Tâche 2.3.7 : Outils de Modding et Import/Export
**Statut** : ⚠️ Partiel (BakiImporter existe, outils UI manquants)

**Actions** :
- [ ] Créer `DatabaseEditorView` (outil intégré)
  - **Section 1** : Sélection de table
    - Dropdown : Liste de toutes les tables (Workers, Companies, Titles, etc.)
    - Bouton : "Charger"
  - **Section 2** : Éditeur de données
    - DataGrid éditable avec toutes les colonnes de la table
    - Bouton : "Ajouter une Ligne" / "Supprimer la Ligne"
    - Validation en temps réel (types, contraintes)
  - **Section 3** : Actions
    - Bouton : "Sauvegarder" (commit en BDD)
    - Bouton : "Annuler" (rollback)
    - Bouton : "Exporter cette Table" (CSV ou JSON)
- [ ] Créer `DataPackImporterView`
  - Support de packs communautaires (format ZIP)
  - Contenu d'un pack :
    - `/data/workers.json`
    - `/data/companies.json`
    - `/data/titles.json`
    - `/images/` (photos des workers)
    - `manifest.json` (métadonnées : nom, auteur, version, description)
  - UI :
    - Bouton : "Importer un Pack"
    - Sélection du fichier ZIP
    - Prévisualisation du contenu (liste des workers, companies, etc.)
    - Checkbox : "Remplacer les données existantes" ou "Fusionner"
    - Bouton : "Confirmer l'Import"
  - Validation :
    - Vérification de l'intégrité du pack (manifest, schéma JSON)
    - Détection de conflits (IDs existants)
    - Résolution : Générer de nouveaux IDs ou écraser
- [ ] Créer `DataPackExporterView`
  - Sélection des données à exporter
    - Checkbox par table (Workers, Companies, Titles, etc.)
    - Filtres optionnels (ex: Workers avec Popularity > 70)
  - Génération du pack
    - Création du ZIP avec structure normalisée
    - Génération du `manifest.json`
    - Copie des images depuis `/assets/images/`
  - Bouton : "Exporter" → Sauvegarde du fichier ZIP
- [ ] Documenter le format de modding
  - Création de `/docs/MODDING_GUIDE.md`
  - Explication du schéma JSON pour chaque table
  - Exemples de packs
  - Bonnes pratiques (IDs, relations, images)
- [ ] Créer une galerie de mods communautaires
  - Page web simple (statique) : `mods.ringgeneral.com`
  - Upload de packs par la communauté (via formulaire)
  - Download direct
  - Ratings et commentaires

**Livrables** :
- `/Views/Modding/DatabaseEditorView.axaml`
- `/ViewModels/Modding/DatabaseEditorViewModel.cs`
- `/Views/Modding/DataPackImporterView.axaml`
- `/ViewModels/Modding/DataPackImporterViewModel.cs`
- `/Views/Modding/DataPackExporterView.axaml`
- `/ViewModels/Modding/DataPackExporterViewModel.cs`
- `/Services/Modding/DataPackService.cs`
- `/docs/MODDING_GUIDE.md`
- Tests d'import/export
- (Optionnel) Site web `mods.ringgeneral.com`

---

### 2.4 Objectifs et Livrables de la Phase 2

**Critères de Validation** :

1. **Profondeur Stratégique Complète**
   - [ ] Les 3 philosophies de recrutement sont fonctionnelles et impactent le gameplay
   - [ ] Les 3 structures de formation (Dojo, Performance Center, Club) sont opérationnelles avec leurs leviers
   - [ ] Le pipeline de développement (Club/Dojo → Child Company → Main Roster) fonctionne
   - [ ] Les mécaniques d'échec (Push Prématuré, Burnout, Reconversion) sont implémentées et testées
   - [ ] Le système de narration de match (6 phases) génère des rapports cohérents
   - [ ] La culture des vestiaires génère des événements organiques (min 10 types)
   - [ ] Le protocole commotion et le système médical sont opérationnels

2. **Monde de Jeu Dynamique**
   - [ ] Au moins 10 compagnies IA sont simulées (LOD 1 ou 2)
   - [ ] Le mouvement de workers entre compagnies est fonctionnel
   - [ ] Les relations entre compagnies (Partnership, Rivalry, War) impactent le jeu
   - [ ] Le système de LOD maintient des performances acceptables (< 500ms par tick hebdomadaire)

3. **Expérience "Feature-Complete"**
   - [ ] Tous les systèmes de la vision produit sont implémentés
   - [ ] Encyclopedia avec 50+ articles est complète
   - [ ] Système de tooltips couvre 100% des attributs et mécaniques
   - [ ] Tutoriels guidés pour les 3 premiers shows
   - [ ] Outils de modding (import/export) fonctionnels et documentés
   - [ ] Au moins 5 packs de mods communautaires disponibles au lancement

4. **Qualité et Stabilité**
   - [ ] 100 heures de jeu en alpha sans crash
   - [ ] 20 testeurs valident que le jeu est "profond et rejouable"
   - [ ] Performance : 60 FPS constant sur hardware cible (mid-range PC 2024)
   - [ ] Aucun bug de severity 1 ou 2 en production

**Date cible de complétion** : T0 + 12 mois (Phase 1) + 9 mois

---

## Dépendances et Ordre d'Implémentation

### Graphe de Dépendances (Phase 1)

```
1.1 Infrastructure & UI/UX (Fondation)
  ├─→ 1.1.1 Localisation
  ├─→ 1.1.2 Kit d'Interface (SortableDataGrid, AttributeBar, etc.)
  ├─→ 1.1.3 Système de Persistance
  └─→ 1.1.4 Fiche de Profil Universelle
      ↓
1.2 Boucle de Jeu
  ├─→ 1.2.1 Inbox (dépend de 1.1.2 pour NewsCard)
  ├─→ 1.2.2 Calendrier et Création de Shows (dépend de 1.1.2)
  ├─→ 1.2.3 Booking v1 (dépend de 1.1.2, 1.2.2)
  └─→ 1.2.4 Simulation et Résultats (dépend de 1.2.3)
      ↓
1.3 Gestion du Roster
  ├─→ 1.3.1 Contrats (dépend de 1.1.4, 1.2.1)
  └─→ 1.3.2 Attributs (dépend de 1.1.4)
```

### Graphe de Dépendances (Phase 2)

```
Phase 1 Complète
  ↓
2.1 Développement des Talents
  ├─→ 2.1.1 Philosophies (peut démarrer en parallèle)
  ├─→ 2.1.2 Structures (dépend de 2.1.1)
  ├─→ 2.1.3 Pipeline (dépend de 2.1.2)
  └─→ 2.1.4 Mécaniques d'Échec (dépend de 2.1.2)
      ↓
2.2 Simulation Approfondie (parallèle à 2.1)
  ├─→ 2.2.1 Narration de Match (peut démarrer en parallèle)
  ├─→ 2.2.2 Coulisses (dépend de 2.1.2 pour Youth impacts)
  └─→ 2.2.3 Production & Médical (peut démarrer en parallèle)
      ↓
2.3 Expansion
  ├─→ 2.3.1 Finances (peut démarrer après Phase 1)
  ├─→ 2.3.2 Broadcasting (dépend de 2.2.1 pour Audience)
  ├─→ 2.3.3 Storylines (dépend de 2.2.1)
  ├─→ 2.3.4 Titres (dépend de 2.3.3)
  ├─→ 2.3.5 Monde Vivant (dépend de 2.1, 2.2, 2.3.1-4)
  ├─→ 2.3.6 Encyclopedia (peut démarrer en parallèle, finir en dernier)
  └─→ 2.3.7 Modding (peut démarrer en parallèle)
```

### Ordre de Priorisation Recommandé

**Sprint 1-2 (Phase 1 - Semaines 1-6)** :
1. 1.1.1 Localisation
2. 1.1.2 Kit d'Interface
3. 1.1.3 Persistance
4. 1.1.4 Profil Universel

**Sprint 3-5 (Phase 1 - Semaines 7-14)** :
5. 1.2.2 Calendrier
6. 1.2.3 Booking v1
7. 1.2.4 Simulation et Résultats
8. 1.2.1 Inbox

**Sprint 6-7 (Phase 1 - Semaines 15-20)** :
9. 1.3.1 Contrats
10. 1.3.2 Attributs

**Sprint 8-10 (Phase 2 - Semaines 21-32)** :
11. 2.1.1 Philosophies de Recrutement
12. 2.1.2 Structures de Formation
13. 2.2.1 Narration de Match (parallèle)

**Sprint 11-13 (Phase 2 - Semaines 33-44)** :
14. 2.1.3 Pipeline de Développement
15. 2.1.4 Mécaniques d'Échec
16. 2.2.2 Coulisses
17. 2.2.3 Production & Médical

**Sprint 14-18 (Phase 2 - Semaines 45-60)** :
18. 2.3.1 Finances Avancées
19. 2.3.2 Broadcasting
20. 2.3.3 Storylines Avancées
21. 2.3.4 Titres Avancés
22. 2.3.5 Monde Vivant

**Sprint 19-20 (Phase 2 - Semaines 61-68)** :
23. 2.3.6 Encyclopedia
24. 2.3.7 Modding
25. QA Finale et Polissage

---

## Critères de Validation

### Phase 1 : Validation du Socle Jouable

**Technique** :
- [ ] 100% des tests unitaires passent
- [ ] 0 warning de compilation
- [ ] 0 fuite mémoire détectée (profiling sur 1000 semaines simulées)
- [ ] Temps de chargement < 3 secondes
- [ ] Temps de sauvegarde < 2 secondes

**Fonctionnel** :
- [ ] Boucle de jeu complète de 52 semaines sans crash
- [ ] Toutes les vues accessibles et fonctionnelles
- [ ] Validation du booking empêche les erreurs critiques
- [ ] Simulation produit des résultats cohérents (notes, audience, impacts)
- [ ] Inbox génère des messages pertinents

**Utilisateur** :
- [ ] 10 testeurs alpha complètent une partie de 20 semaines
- [ ] Taux de satisfaction > 70% (questionnaire)
- [ ] 0 bug bloquant remonté
- [ ] Temps moyen pour booker un show < 10 minutes

### Phase 2 : Validation de la Profondeur

**Technique** :
- [ ] Performance : 60 FPS constant sur hardware cible
- [ ] Simulation de 10 compagnies IA < 500ms par tick hebdomadaire
- [ ] Base de données : < 100 MB pour 10 ans de jeu simulé
- [ ] Pas de ralentissement après 500 semaines jouées

**Fonctionnel** :
- [ ] Tous les systèmes de développement de talents fonctionnent
- [ ] LOD des compagnies IA génère des résultats crédibles
- [ ] Mouvements de workers entre compagnies cohérents
- [ ] Storylines génèrent du heat et des payoffs satisfaisants
- [ ] Système médical empêche les abus

**Utilisateur** :
- [ ] 20 testeurs beta jouent 100+ heures sans se lasser
- [ ] Taux de complétion d'une carrière de 5 ans > 60%
- [ ] Note métacritique simulée > 80/100 (basée sur feedbacks)
- [ ] Communauté de modding active (5+ packs disponibles)

---

## Conclusion

Ce plan d'implémentation technique traduit la vision ambitieuse de "Ring General" en **tâches concrètes et actionables**, structurées en **deux phases séquentielles** :

1. **Phase 1** (3-6 mois) : Construction d'un **socle jouable** stable avec une boucle de jeu complète
2. **Phase 2** (6-12 mois) : Ajout de la **profondeur stratégique** et des systèmes de simulation avancés

L'approche priorise le **dérisquage** en garantissant un produit jouable et testable avant d'investir dans les couches de complexité. Chaque tâche est détaillée avec :
- Statut actuel (✅ ⚠️ ❌)
- Actions concrètes (checkboxes)
- Livrables attendus (fichiers, tests, documentation)

En suivant ce plan, le projet "Ring General" livrera non seulement un jeu complet, mais une **simulation de gestion de catch d'une profondeur sans précédent**, fidèle à la vision initiale tout en garantissant la qualité et la stabilité du produit final.

---

**Prochaines Étapes Immédiates** :
1. Validation de ce plan par l'équipe de développement
2. Création des issues GitHub correspondantes (1 issue par tâche)
3. Estimation détaillée des charges (en heures-dev)
4. Démarrage du Sprint 1 : Tâche 1.1.1 (Localisation)

**Auteur** : Claude (Anthropic)
**Révision** : À valider par l'équipe
**Statut** : DRAFT v1.0
