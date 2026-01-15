# Youth Development Center Directive

## 1. Vue Shell : Navigation Centrale
Dans la sidebar (NavigationView), nous regroupons tout sous une structure unique.

```plaintext
┌──────────────────────────┐
│ ...                      │
│ 🎓 CENTRE DE DÉVELOPPEMENT│
│   ├ 🏛️ Structures        │
│   ├ 👨‍🏫 Staff & Coachs    │
│   ├ 🤼 Trainees          │
│   └ 🏢 Prêts Filiales     │
│ ...                      │
└──────────────────────────┘
```

## 2. Mockups des Onglets (Architecture Shell View)

### Onglet 1 : 🏛️ STRUCTURES (Gestion des Centres)
Cet onglet permet de gérer les Dojos indépendants ou les centres liés aux filiales.

```plaintext
┌─────────────────────────────────────────────────────────────────────────────┐
│ 🏛️ GESTION DES STRUCTURES                                    [+ Créer Dojo] │
├─────────────────────────────────────────────────────────────────────────────┤
│ LISTE DES STRUCTURES                                    DÉTAILS SÉLECTIONNÉS│
│ ┌────────────────────────┐ │ ┌────────────────────────────────────────────┐ │
│ │ 🥋 Dojo Phoenix (Indep)│ │ │ DOJO PHOENIX                               │ │
│ │ Niveau 2 • 15 Trainees │ │ │ Type: DOJO • Statut: ACTIF                 │ │
│ │                        │ │ │ ────────────────────────────────────────── │ │
│ │ 🏢 Performance Center  │ │ │ 🛠️ INFRASTRUCTURES: Niveau 2 [Améliorer]    │ │
│ │ (Filiale: NXT)         │ │ │ 💰 BUDGET ANNUEL: 150 000€  [Modifier]     │ │
│ │                        │ │ │ 💡 PHILOSOPHIE: PURE [▼]                   │ │
│ │                        │ │ │ 📅 PROCHAINE GÉNÉRATION: Sem. 48           │ │
│ └────────────────────────┘ │ └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```
**Logique Core** : Gère le `NiveauEquipements` et le `BudgetAnnuel` pour les bonus de génération.

### Onglet 2 : 👨‍🏫 STAFF & COACHS
Gestion des entraîneurs qui influencent les stats des futurs trainees.

```plaintext
┌─────────────────────────────────────────────────────────────────────────────┐
│ 👨‍🏫 STAFF & COACHING                                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│ COACHS ASSIGNÉS                     RECRUTEMENT / DISPONIBLES               │
│ ┌────────────────────────┐ │ ┌────────────────────────────────────────────┐ │
│ │ 🥋 Dojo Phoenix        │ │ │ [Rechercher un coach...]                     │ │
│ │ Coach: John Doe        │ │ │                                            │ │
│ │ Qualité: ★★★★☆         │ │ │ 👤 Bret Hart        [Assigner à Phoenix]   │ │
│ │                        │ │ │    Exp: 95 • Technique: 99                 │ │
│ │ 🏢 Performance Center  │ │ │                                            │ │
│ │ Coach: Dusty R.        │ │ │ 👤 Shawn M.         [Assigner à PC Miami]  │ │
│ │ Qualité: ★★★★★         │ │ │    Exp: 92 • Entertainment: 98            │ │
│ └────────────────────────┘ │ └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```
**Logique Core** : La `QualiteCoaching` est utilisée dans `CalculerBaseQuantite` pour la génération.

### Onglet 3 : 🤼 TRAINEES (Liste de Génération)
Intégration de la vue actuelle de suivi des jeunes en cours de formation.

```plaintext
┌─────────────────────────────────────────────────────────────────────────────┐
│ 🤼 TRAINEES EN FORMATION                                    [Total: 15]      │
├─────────────────────────────────────────────────────────────────────────────┤
│ NOM            │ STRUCTURE       │ PROGRESSION │ ACTIONS                    │
├────────────────┼─────────────────┼─────────────┼────────────────────────────┤
│ Mike Thunder   │ Dojo Phoenix    │ [█████░░] 68%│ [🎓 Graduer]               │
│ Sarah Phoenix  │ Dojo Phoenix    │ [███░░░░] 45%│ [🎓 Graduer]               │
│ John Morrison  │ PC Miami        │ [████░░░] 52%│ [🎓 Graduer]               │
└────────────────┴─────────────────┴─────────────┴────────────────────────────┘
```
**Logique Core** : Les nouveaux travailleurs sont créés comme `TRAINEE` et liés via la table `YouthTrainees` (ou concept équivalent).

### Onglet 4 : 🏢 FILIALES DE DÉVELOPPEMENT (Prêts)
Gestion des workers du roster principal envoyés en filiale pour "développement".

```plaintext
┌─────────────────────────────────────────────────────────────────────────────┐
│ 🏢 PRÊTS DÉVELOPPEMENTAUX (LOANED)                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│ FILIALE : NXT (Objectif: Développement)                    [+ Envoyer Worker]│
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ WORKER         │ TEMPS PASSÉ │ PROGRESSION POP │ STATUT      │ ACTIONS     │ │
│ ├────────────────┼─────────────┼─────────────────┼─────────────┼─────────────┤ │
│ │ Cody Rhodes    │ 4 Semaines  │ +15%            │ Main Event  │ [Rappeler]  │ │
│ │ Seth Rollins   │ 2 Semaines  │ +5%             │ Midcard     │ [Rappeler]  │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```
**Logique Core** : Utilise `ChildCompaniesExtended` pour identifier les filiales avec l'objectif `Development`.

## 3. Plan d'implémentation (3-Layer Architecture)

### Layer 3 : Execution (RingGeneral.Core)
- **Service** : `WorkerGenerationService` pour la boucle hebdomadaire.
- **Service** : `ChildCompanyService` pour lier automatiquement une nouvelle structure lors de la création d'une filiale de développement.
- **Repository** : `YouthRepository` pour enregistrer les structures indépendantes et les coachs.

### Layer 2 : Orchestration (ViewModels)
- **YouthHubViewModel** : Gère la navigation entre les onglets et le rafraîchissement des données globales.
- **StructureManagementViewModel** : Commandes pour l'upgrade du budget et des équipements.
- **LoanManagementViewModel** : Gère le mouvement des workers entre ParentCompany et ChildCompany.
