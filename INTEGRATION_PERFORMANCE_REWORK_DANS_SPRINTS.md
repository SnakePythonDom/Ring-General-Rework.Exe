# 🔄 Intégration du Rework des Attributs de Performance dans les Sprints

**Date** : 7 janvier 2026
**Contexte** : Refonte majeure des attributs (3 → 30 attributs granulaires) + Page Profile principale

---

## 📋 Résumé Exécutif

### Changements Demandés
1. **Attributs de Performance** : Passer de 3 attributs simples à 30 attributs granulaires
   - IN-RING : 10 attributs (Striking, Grappling, High-Flying, etc.)
   - ENTERTAINMENT : 10 attributs (Charisma, Mic Work, Acting, etc.)
   - STORY : 10 attributs (Character Depth, Consistency, Heel/Face Performance, etc.)

2. **Page Profile Principale** : Ajouter une page de profil complète AVANT les tabs avec :
   - Header avec photo/avatar + infos clés
   - Barres de condition (Condition, Forme, Fatigue, Pop)
   - 3 colonnes d'attributs avec moyennes
   - Historique des performances récentes

### Impact sur les Sprints Existants

| Sprint Impacté | Modifications | Durée Ajoutée |
|----------------|---------------|---------------|
| **Sprint 1** | ✅ Aucune (AttributeBar déjà créé) | +0 jour |
| **Sprint 2** | 🔴 Refonte complète (ProfileView) | +2-3 jours |
| **Sprint 3** | 🟡 Adaptation simulation | +1 jour |
| **Nouveau Sprint 2.5** | 🆕 Migration DB + Modèles | +5 jours |

**Impact total** : +7 à 9 jours (1.5 semaines)

---

## 🗓️ Nouveau Planning Révisé

### Option 1 : Sprint 2.5 Intercalé (Recommandé)

```
SPRINT 0 : Infrastructure DI                    [✅ TERMINÉ]
SPRINT 1 : Composants UI Réutilisables          [✅ TERMINÉ]
  │
  ├─→ SPRINT 2.5 : Migration Attributs (NOUVEAU) [🆕 5 jours]
  │    ├─ Jour 1 : Migration DB + Tests
  │    ├─ Jour 2 : Modèles Core + Repositories
  │    ├─ Jour 3 : ProfileAttributesViewModel
  │    ├─ Jour 4 : ProfileAttributesView + Descriptions
  │    └─ Jour 5 : Adaptation Simulation + Tests E2E
  │
  └─→ SPRINT 2 : ProfileView Complet (MODIFIÉ)   [⏳ 4-5 jours]
       ├─ Jour 1-2 : Page Profile Principale (Header + 3 colonnes)
       ├─ Jour 3 : Tabs (Contrats, Gimmick, Relations)
       ├─ Jour 4 : Tabs (History, Notes)
       └─ Jour 5 : Intégration + Tests

SPRINT 3 : Résultats de Simulation              [⏳ 3 jours]
SPRINT 4 : Inbox & Actualités                   [⏳ 2-3 jours]
SPRINT 5 : Calendrier & Création Shows          [⏳ 2-3 jours]
SPRINT 6 : Boucle de Jeu Complète               [⏳ 5-7 jours]
```

**Durée totale** : 23-32 jours (vs 18-27 jours avant)
**Delta** : +5 jours

---

### Option 2 : Sprint 2 Fusionné (Plus Rapide mais Plus Dense)

```
SPRINT 0 : Infrastructure DI                    [✅ TERMINÉ]
SPRINT 1 : Composants UI Réutilisables          [✅ TERMINÉ]
  │
  └─→ SPRINT 2 : ProfileView + Attributs (FUSIONNÉ) [⏳ 7-8 jours]
       ├─ Jour 1 : Migration DB + Tests
       ├─ Jour 2 : Modèles Core + Repositories
       ├─ Jour 3 : ProfileAttributesViewModel
       ├─ Jour 4 : Page Profile Principale (Header + 3 colonnes)
       ├─ Jour 5 : Tabs (Contrats, Gimmick, Relations)
       ├─ Jour 6 : Tabs (History, Notes) + Descriptions
       └─ Jour 7-8 : Adaptation Simulation + Tests E2E + Intégration

SPRINT 3 : Résultats de Simulation              [⏳ 3 jours]
SPRINT 4 : Inbox & Actualités                   [⏳ 2-3 jours]
SPRINT 5 : Calendrier & Création Shows          [⏳ 2-3 jours]
SPRINT 6 : Boucle de Jeu Complète               [⏳ 5-7 jours]
```

**Durée totale** : 21-29 jours (vs 18-27 jours avant)
**Delta** : +3 jours

**Recommandation** : Option 1 (Sprint 2.5 séparé) pour plus de clarté et moins de risque

---

## 📝 Modifications à Apporter aux Documents

### 1. PLAN_SPRINT_REVISE.md

#### Section à Modifier : Sprint 2 - ProfileView Universel

**AVANT** (lignes 215-718) :
```markdown
### SPRINT 2 : ProfileView Universel (4-5 jours) 🔴 HAUTE

**Objectif** : Créer la fiche de profil complète avec 6 onglets pour Workers, Staff et Trainees

#### Tâche 2.1 : ViewModels de Profil (Jours 1-2)
...
AttributesTabViewModel :
    - Universels : ConditionPhysique, Moral, Popularite, Fatigue, Momentum
    - In-Ring (si Worker) : InRing, Timing, Psychology, Selling, Stamina, Safety
    - Entertainment (si Worker) : Entertainment, Charisma, Promo, CrowdConnection, StarPower
    - Story (si Worker) : Story, Storytelling, CharacterWork
```

**APRÈS** :
```markdown
### SPRINT 2.5 : Migration des Attributs de Performance (5 jours) 🔴 CRITIQUE
**Dépendances** : Sprint 1 (AttributeBar terminé ✅)

**Objectif** : Remplacer les 3 attributs simples (InRing, Entertainment, Story) par 30 attributs granulaires

**Détails** : Voir [FEATURE_PLAN_PERFORMANCE_ATTRIBUTES_REWORK.md](./FEATURE_PLAN_PERFORMANCE_ATTRIBUTES_REWORK.md)

#### Tâche 2.5.1 : Migration Base de Données (Jour 1)
- Créer `DbMigrations.cs`
- Ajouter 30 nouvelles colonnes à la table Workers
- Migrer les données existantes avec variance
- Tests de migration

#### Tâche 2.5.2 : Modèles Core + Repositories (Jour 2)
- Créer `PerformanceAttributes.cs` (3 records : InRingAttributes, EntertainmentAttributes, StoryAttributes)
- Mettre à jour `WorkerSnapshot` avec le nouveau modèle
- Adapter `WorkerRepository.ChargerAttributsPerformance()`
- Tests unitaires des repositories

#### Tâche 2.5.3 : ProfileAttributesViewModel (Jour 3)
- Créer ViewModel avec 30 propriétés + 3 moyennes
- Binding avec WorkerRepository
- Calcul des moyennes par catégorie
- Tests du ViewModel

#### Tâche 2.5.4 : ProfileAttributesView + Descriptions (Jour 4)
- Créer la View AXAML avec 3 colonnes
- Header avec photo + infos clés (comme mockup John Cena)
- Barres de condition (4 barres)
- Historique des performances (Expander)
- Ajouter 30 descriptions dans `AttributeDescriptions.fr.resx`

#### Tâche 2.5.5 : Adaptation Simulation (Jour 5)
- Modifier `ShowSimulationEngine` pour utiliser attributs granulaires
- Formules spécifiques par type de match (Striking, Technical, High-Flying, Hardcore, etc.)
- Tests de simulation
- Tests E2E complets

**Livrables Sprint 2.5** :
- ✅ 30 attributs en base de données
- ✅ Migration des données existantes réussie
- ✅ Page Profile Principale fonctionnelle (Header + 3 colonnes)
- ✅ Simulation adaptée aux attributs granulaires
- ✅ Tests complets (10+ tests)

**Durée** : 5 jours

---

### SPRINT 2 : ProfileView Complet (4-5 jours) 🔴 HAUTE
**Dépendances** : Sprint 2.5 (Attributs migrés ✅)

**Objectif** : Ajouter les 5 onglets restants (Contrats, Gimmick, Relations, History, Notes)

**Modifications par rapport au plan initial** :
- ✅ Tab ATTRIBUTS → Déjà créé dans Sprint 2.5 (ProfileAttributesView)
- ⏳ Tab CONTRATS → À créer
- ⏳ Tab GIMMICK/PUSH → À créer
- ⏳ Tab RELATIONS → À créer (avec factions)
- ⏳ Tab HISTORY → À créer
- ⏳ Tab NOTES → À créer

#### Tâche 2.1 : TabControl Principal + Navigation (Jour 1)
- Créer `ProfileView.axaml` avec TabControl à 6 onglets
- Intégrer `ProfileAttributesView` (déjà créé) dans le premier tab
- Navigation depuis RosterView vers ProfileView
- Header principal avec photo + actions (Éditer, Libérer)

#### Tâche 2.2 : ContractsTab + GimmickTab (Jour 2)
- `ContractsTabViewModel` + `ContractsTabView`
- `GimmickTabViewModel` + `GimmickTabView`
- Affichage des infos de contrat
- Gestion du push level et alignment

#### Tâche 2.3 : RelationsTab (Jour 3)
- `RelationsTabViewModel` + `RelationsTabView`
- Système de relations 1-à-1 (Amitié, Couple, Fraternité, Rivalité)
- Système de factions (TagTeam, Trio, Faction)
- Actions : Ajouter/Modifier/Supprimer relation

#### Tâche 2.4 : HistoryTab + NotesTab (Jour 4)
- `HistoryTabViewModel` + `HistoryTabView`
- `NotesTabViewModel` + `NotesTabView`
- Affichage des title reigns, match history, injuries
- Système de notes avec catégories

#### Tâche 2.5 : Intégration et Tests (Jour 5)
- Enregistrer tous les ViewModels dans DI
- Tests de navigation
- Tests de chargement des données
- Validation complète du ProfileView

**Livrables Sprint 2** :
- ✅ ProfileView complet avec 6 onglets fonctionnels
- ✅ Navigation fluide entre les tabs
- ✅ Système de relations + factions
- ✅ Tests validés

**Durée** : 4-5 jours
```

#### Section à Ajouter : Après Sprint 6 (Nouvelles Opportunités)

```markdown
### 🎁 BONUS : Nouvelles Opportunités grâce aux Attributs Granulaires

Une fois les attributs granulaires en place, de nouvelles features deviennent possibles :

#### Future Feature : Training System
- Entraînement ciblé d'attributs spécifiques (ex: "Améliorer le Mic Work")
- Coût et durée variables selon l'attribut
- Plafond de progression basé sur le potentiel

#### Future Feature : Match Type Recommendations
- L'IA recommande le meilleur type de match selon les attributs des workers
- Ex: 2 High-Flyers → recommander un "Ladder Match"
- Ex: 2 Brawlers → recommander un "Street Fight"

#### Future Feature : Scouting Avancé
- Rapports de scouting détaillés avec breakdown des 30 attributs
- Comparaison avec le roster actuel
- Identification des lacunes (ex: "Manque de High-Flyers")

#### Future Feature : Worker Archetypes
- Détection automatique de l'archétype (Striker, Grappler, High-Flyer, etc.)
- Suggestions de rivalités basées sur les styles complémentaires
- Templates de booking optimisés par archétype
```

---

### 2. PLAN_IMPLEMENTATION_TECHNIQUE.md

#### Section à Ajouter : Phase 1, Tâche 1.3.5 (Nouvelle)

Insérer après la Tâche 1.3.4 (ShowCreationDialog) :

```markdown
#### Tâche 1.3.5 : Refonte des Attributs de Performance (5 jours) 🔴 CRITIQUE

**Priorité** : HAUTE (Bloquant pour ProfileView et Simulation)
**Dépendances** : Tâche 1.1.2 (Kit UI - AttributeBar créé)

**Objectif** : Remplacer les 3 attributs simples par 30 attributs granulaires pour une simulation plus réaliste

**Contexte** :
Le système actuel utilise seulement 3 attributs (InRing, Entertainment, Story), ce qui est trop simpliste pour :
- Différencier les styles de wrestlers (Brawler vs High-Flyer vs Technical)
- Calculer des qualités de match adaptées au type de segment
- Offrir de la profondeur aux fans de wrestling simulation

**Nouveau Système** :
- **IN-RING** (10 attributs) : Striking, Grappling, High-Flying, Powerhouse, Timing, Selling, Psychology, Stamina, Safety, Hardcore/Brawl
- **ENTERTAINMENT** (10 attributs) : Charisma, Mic Work, Acting, Crowd Connection, Star Power, Improvisation, Entrance, Sex Appeal, Merchandise Appeal, Crossover Potential
- **STORY** (10 attributs) : Character Depth, Consistency, Heel Performance, Babyface Performance, Storytelling (Long-term), Emotional Range, Adaptability, Rivalry Chemistry, Creative Input, Moral Alignment

**Plan Détaillé** : Voir [FEATURE_PLAN_PERFORMANCE_ATTRIBUTES_REWORK.md](./FEATURE_PLAN_PERFORMANCE_ATTRIBUTES_REWORK.md)

**Fichiers à Créer/Modifier** :
```
CRÉER :
- src/RingGeneral.Data/Database/DbMigrations.cs
- src/RingGeneral.Core/Models/PerformanceAttributes.cs
- src/RingGeneral.UI/ViewModels/Profile/ProfileAttributesViewModel.cs
- src/RingGeneral.UI/Views/Profile/ProfileAttributesView.axaml
- tests/RingGeneral.Tests/Migrations/PerformanceAttributesMigrationTests.cs

MODIFIER :
- src/RingGeneral.Core/Models/DomainModels.cs (WorkerSnapshot)
- src/RingGeneral.Data/Repositories/WorkerRepository.cs
- src/RingGeneral.Core/Simulation/ShowSimulationEngine.cs
- src/RingGeneral.UI/Resources/AttributeDescriptions.fr.resx (+30 descriptions)
```

**Livrables** :
- ✅ Migration DB fonctionnelle avec rollback
- ✅ 30 attributs affichés dans ProfileView
- ✅ Simulation adaptée avec formules par type de match
- ✅ 30 descriptions en français
- ✅ Tests unitaires complets (migration + simulation + UI)
- ✅ Performance acceptable (< 200ms pour charger un profil)

**Durée estimée** : 5 jours (1 semaine)

**Risques** :
- 🟡 Migration échoue sur DB production → **Mitigation** : Backup automatique + rollback
- 🟡 Performance dégradée → **Mitigation** : Indexation + lazy loading
- 🟡 Simulation trop complexe → **Mitigation** : Formules simples d'abord, raffinement progressif

**Validation** :
- [ ] Migration s'exécute sans erreur sur BAKI1.1.db
- [ ] Les 30 attributs sont visibles et lisibles
- [ ] Les moyennes sont calculées correctement
- [ ] La simulation donne des résultats cohérents
- [ ] Aucune régression sur les tests existants
```

---

## 🎯 Checklist de Validation du Rework

### Avant de Commencer
- [ ] Sprint 1 (Composants UI) est terminé
- [ ] AttributeBar.axaml est fonctionnel et testé
- [ ] Backup de la base de données actuelle créé
- [ ] Branche Git créée : `feature/performance-attributes-rework`
- [ ] Document FEATURE_PLAN lu en entier

### Pendant le Développement (Sprint 2.5)

#### Jour 1 : Migration DB
- [ ] `DbMigrations.cs` créé
- [ ] 30 colonnes ajoutées à la table Workers
- [ ] Migration testée sur DB de test
- [ ] Données migrées avec variance cohérente
- [ ] Tests de migration passent

#### Jour 2 : Modèles Core
- [ ] `PerformanceAttributes.cs` créé avec 3 records
- [ ] `WorkerSnapshot` mis à jour
- [ ] `WorkerRepository.ChargerAttributsPerformance()` implémenté
- [ ] Tests unitaires des repositories passent

#### Jour 3 : ViewModel
- [ ] `ProfileAttributesViewModel` créé avec 30 propriétés
- [ ] Binding avec repository fonctionnel
- [ ] Moyennes calculées correctement
- [ ] Tests du ViewModel passent

#### Jour 4 : View + Descriptions
- [ ] `ProfileAttributesView.axaml` créé
- [ ] Header avec photo + infos clés affiché
- [ ] 3 colonnes d'attributs avec barres
- [ ] 30 descriptions ajoutées à `.resx`
- [ ] Tooltips s'affichent correctement

#### Jour 5 : Simulation
- [ ] `ShowSimulationEngine` adapté
- [ ] Formules spécifiques par type de match implémentées
- [ ] Tests de simulation passent
- [ ] Tests E2E complets validés

### Après Sprint 2.5
- [ ] Aucune régression sur les tests existants
- [ ] Performance acceptable (< 200ms pour ProfileView)
- [ ] Documentation mise à jour
- [ ] Pull Request créée avec description détaillée
- [ ] Review de code complétée
- [ ] Merge dans la branche principale

---

## 📊 Tableau de Bord des Modifications

| Document | Section | Action | Statut |
|----------|---------|--------|--------|
| PLAN_SPRINT_REVISE.md | Sprint 2 | Diviser en Sprint 2.5 + Sprint 2 | ⏳ À faire |
| PLAN_IMPLEMENTATION_TECHNIQUE.md | Phase 1, Tâche 1.3.5 | Ajouter nouvelle tâche | ⏳ À faire |
| FEATURE_PLAN_PERFORMANCE_ATTRIBUTES_REWORK.md | - | Document créé | ✅ Fait |
| INTEGRATION_PERFORMANCE_REWORK_DANS_SPRINTS.md | - | Document créé | ✅ Fait |

---

## 🚀 Prochaines Actions Recommandées

### Immédiat (Aujourd'hui)
1. **Valider l'approche** : Relire les 2 documents créés et confirmer que c'est ce que vous voulez
2. **Choisir l'option** : Sprint 2.5 séparé (Option 1) ou Sprint 2 fusionné (Option 2)
3. **Créer la branche Git** : `git checkout -b feature/performance-attributes-rework`

### Court Terme (Cette Semaine)
4. **Modifier PLAN_SPRINT_REVISE.md** : Intégrer les changements décrits ci-dessus
5. **Modifier PLAN_IMPLEMENTATION_TECHNIQUE.md** : Ajouter la Tâche 1.3.5
6. **Démarrer Sprint 2.5 Jour 1** : Migration de la base de données

### Moyen Terme (Semaine Prochaine)
7. **Compléter Sprint 2.5** : 5 jours de développement
8. **Tester sur BAKI1.1.db** : Validation avec données réelles
9. **Démarrer Sprint 2** : ProfileView complet avec les 5 tabs restants

---

## ✅ Validation Finale

**Ce rework est-il aligné avec vos attentes ?**
- ✅ 30 attributs granulaires (10 IN-RING + 10 ENTERTAINMENT + 10 STORY)
- ✅ Page Profile Principale avant les tabs (comme mockup John Cena)
- ✅ Migration des données existantes
- ✅ Simulation adaptée
- ✅ Plan détaillé sur 5 jours

**Questions à clarifier** :
1. Voulez-vous garder les anciennes colonnes (InRing, Entertainment, Story) pour compatibilité ou les supprimer ?
2. Préférez-vous l'Option 1 (Sprint 2.5 séparé) ou l'Option 2 (Sprint 2 fusionné) ?
3. Faut-il ajouter une feature de "Worker Archetypes" dès maintenant ou plus tard ?
4. Les 30 descriptions en français sont-elles suffisantes ou voulez-vous plus de détails ?

---

**Prêt à intégrer ce rework dans les sprints ? 🚀**
