# Plan Sprint Révisé - Ring General

**Date** : 7 janvier 2026
**Basé sur** : Audit exhaustif du code + État réel documenté dans CURRENT_STATE.md
**Statut Projet** : Phase 0 Complète (95%), Phase 1 En Cours (40%)

---

## 🎯 OBJECTIF

Adapter le plan d'implémentation à la **réalité actuelle du projet** suite à la découverte que nous sommes plus avancés que prévu, et prioriser les tâches qui débloquent la boucle de jeu complète.

---

## 📊 ANALYSE DE L'ÉCART

### Ce qui est MIEUX que prévu
- ✅ ViewModels : **92%** (vs 20% pensé)
- ✅ Views : **65%** (vs 10% pensé)
- ✅ Navigation : **95%** (vs 80% pensé)
- ✅ Seed Data : **100%** (vs 0% pensé)

### Ce qui BLOQUE le progrès
1. ❌ Composants UI réutilisables (0%) - **Bloque tout le développement UI rapide**
2. ❌ Boucle de jeu complète (0%) - **Bloque la jouabilité**
3. ⚠️ Repositories en DI (12%) - **Complexifie l'injection**
4. ❌ UI des résultats de simulation - **Bloque la validation du booking**

---

## 🚀 PLAN PAR SPRINTS (Révisé et Adapté)

### SPRINT 0 : Finalisation Infrastructure ✅ TERMINÉ (7 janvier 2026)

**Objectif** : Terminer les 5% restants de la Phase 0

**Tâches** :
1. ✅ Enregistrer les 11 repositories manquants dans le DI
   - Ajouté TitleRepository et MedicalRepository au RepositoryContainer
   - Tous les 11 repositories enregistrés dans App.axaml.cs
   - Fichiers modifiés :
     * `/src/RingGeneral.Data/Repositories/RepositoryFactory.cs`
     * `/src/RingGeneral.UI/App.axaml.cs`

2. ✅ Tests de résolution réussis

**Livrables** :
- ✅ Tous les repositories accessibles via DI (11/11)
- ✅ Infrastructure 100% complète
- ✅ Phase 0 fermée définitivement

**Commit** : `51d0b77` - "Sprint 0: Register all repositories in DI container"

**Durée réelle** : < 1 jour

---

### SPRINT 1 : Composants UI Réutilisables (3-5 jours) 🔴 CRITIQUE

**Objectif** : Créer les composants qui accéléreront tous les développements futurs

**Justification** : Ces composants débloquent :
- ProfileView
- ShowResultsView
- InboxView
- Tous les écrans de Phase 1

#### Tâche 1.1 : AttributeBar Component (Jour 1)

**Fichiers à créer** :
```
/src/RingGeneral.UI/
├── Components/
│   ├── AttributeBar.axaml
│   └── AttributeBar.axaml.cs
└── Resources/
    └── AttributeDescriptions.fr.resx
```

**Spécifications** :
- Barre visuelle de stat (échelle 1-20 ou 0-100)
- Couleur graduée :
  - Rouge : < 50
  - Orange : 50-70
  - Vert : > 70
- Label avec nom de l'attribut
- Valeur numérique affichée
- Tooltip avec description (chargée depuis `AttributeDescriptions.fr.resx`)
- Flèche ↑↓ si changement récent (optionnel)

**Propriétés** :
```csharp
public string AttributeName { get; set; }
public int Value { get; set; }
public int? PreviousValue { get; set; }
public string Description { get; set; }
public int MaxValue { get; set; } = 100
```

**Attributs à documenter dans les ressources** (50+) :
- Universels : ConditionPhysique, Moral
- In-Ring : Timing, Psychology, Selling, Stamina, Safety, Technique
- Entertainment : Charisma, Promo, CrowdConnection, StarPower
- Story : Storytelling, CharacterWork, Versatility
- Backstage (Staff) : Respect, Politicking, Credibility, EyeForTalent
- Coaching (Staff) : TechniqueTeaching, PsychologyTeaching, PromoTeaching
- Potentiel (Trainee) : InRingCeiling, CharismaCeiling, Athleticism, LearningSpeed, WorkEthic

---

#### Tâche 1.2 : DetailPanel Component (Jour 2)

**Fichiers à créer** :
```
/src/RingGeneral.UI/Components/
├── DetailPanel.axaml
└── DetailPanel.axaml.cs
```

**Spécifications** :
- Conteneur pour le Context Panel (colonne droite)
- Sections collapsibles (Expander)
- Header customisable
- Support de différents types de contenu
- Style cohérent avec le thème FM26

**Sections types** :
- Validation (pour Booking)
- Détails Segment
- Profil Worker
- Statistiques

---

#### Tâche 1.3 : SortableDataGrid Component (Jour 3)

**Fichiers à créer** :
```
/src/RingGeneral.UI/Components/
├── SortableDataGrid.axaml
└── SortableDataGrid.axaml.cs
```

**Spécifications** :
- DataGrid avec tri multi-colonnes
- Filtrage avancé :
  - Texte (recherche)
  - Plages (min-max)
  - Checkboxes (multi-sélection)
- Export CSV
- Sélection multiple
- Pagination (optionnel si > 100 items)

---

#### Tâche 1.4 : NewsCard Component (Jour 4)

**Fichiers à créer** :
```
/src/RingGeneral.UI/Components/
├── NewsCard.axaml
└── NewsCard.axaml.cs
```

**Spécifications** :
- Carte de message pour l'Inbox
- Icône par type de message :
  - 📝 Contrat
  - 🏥 Blessure
  - 🔍 Scout Report
  - 📈 Progression
  - 💰 Finance
  - ⚠️ Alerte
- Badge "Non lu"
- Actions rapides (Marquer lu, Archiver, Supprimer)
- Timestamp relatif ("Il y a 2 jours")

---

#### Tâche 1.5 : Thème Unifié (Jour 5)

**Fichiers à créer** :
```
/src/RingGeneral.UI/Styles/
└── RingGeneralTheme.axaml
```

**Contenu** :
- Couleurs (palette FM26 complète)
- Styles de boutons
- Styles de TextBlock
- Styles de Border
- Styles de DataGrid
- Animations (transitions, hover)

**Livrables Sprint 1** :
- ✅ 4 composants réutilisables fonctionnels
- ✅ 50+ descriptions d'attributs
- ✅ Thème unifié
- ✅ Documentation des composants

**Durée** : 3-5 jours

---

### SPRINT 2 : ProfileView Universel (3-4 jours) 🔴 HAUTE

**Objectif** : Créer la fiche de profil complète qui servira pour Workers, Staff et Trainees

**Dépendances** : Sprint 1 (composants)

#### Tâche 2.1 : ViewModels de Profil (Jour 1)

**Fichiers à créer** :
```
/src/RingGeneral.UI/ViewModels/Profile/
├── ProfileViewModel.cs
├── ProfileTabViewModel.cs
├── AttributesTabViewModel.cs
├── HistoryTabViewModel.cs
└── ContractTabViewModel.cs
```

**ProfileViewModel** :
```csharp
public class ProfileViewModel : ViewModelBase
{
    public string ProfileType { get; } // Worker, Staff, Trainee
    public ProfileTabViewModel ProfileTab { get; }
    public AttributesTabViewModel AttributesTab { get; }
    public HistoryTabViewModel HistoryTab { get; }
    public ContractTabViewModel ContractTab { get; }

    public ReactiveCommand<string, Unit> SwitchTabCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> ReleaseCommand { get; }
}
```

---

#### Tâche 2.2 : Views de Profil (Jours 2-3)

**Fichiers à créer** :
```
/src/RingGeneral.UI/Views/Profile/
├── ProfileView.axaml
├── ProfileTabView.axaml
├── AttributesTabView.axaml
├── HistoryTabView.axaml
└── ContractTabView.axaml
```

**Structure ProfileView** :
```xml
<Grid RowDefinitions="Auto,*">
    <!-- Header : Photo + Nom + Stats clés -->
    <Border Grid.Row="0">
        <StackPanel>
            <Image Source="{Binding PhotoPath}" Width="120"/>
            <TextBlock Text="{Binding FullName}" FontSize="24"/>
            <TextBlock Text="{Binding Role}"/>
        </StackPanel>
    </Border>

    <!-- Tabs -->
    <TabControl Grid.Row="1">
        <TabItem Header="PROFIL">
            <ProfileTabView DataContext="{Binding ProfileTab}"/>
        </TabItem>
        <TabItem Header="ATTRIBUTS">
            <AttributesTabView DataContext="{Binding AttributesTab}"/>
        </TabItem>
        <TabItem Header="HISTORIQUE">
            <HistoryTabView DataContext="{Binding HistoryTab}"/>
        </TabItem>
        <TabItem Header="CONTRAT">
            <ContractTabView DataContext="{Binding ContractTab}"/>
        </TabItem>
    </TabControl>
</Grid>
```

**AttributesTabView** - Utilise `AttributeBar` :
```xml
<ScrollViewer>
    <StackPanel>
        <Expander Header="ATTRIBUTS UNIVERSELS" IsExpanded="True">
            <StackPanel>
                <components:AttributeBar
                    AttributeName="Condition Physique"
                    Value="{Binding ConditionPhysique}"/>
                <components:AttributeBar
                    AttributeName="Moral"
                    Value="{Binding Moral}"/>
            </StackPanel>
        </Expander>

        <Expander Header="IN-RING" IsExpanded="True"
                  IsVisible="{Binding IsWorker}">
            <!-- 6 AttributeBars -->
        </Expander>

        <!-- Autres sections -->
    </StackPanel>
</ScrollViewer>
```

---

#### Tâche 2.3 : Intégration et Tests (Jour 4)

- Enregistrer ProfileViewModel dans DI
- Ajouter DataTemplate
- Navigation depuis RosterView
- Tests avec données réelles (DbSeeder)

**Livrables Sprint 2** :
- ✅ ProfileView complet avec 4 onglets
- ✅ Support Worker/Staff/Trainee
- ✅ Affichage de tous les attributs
- ✅ Navigation fonctionnelle
- ✅ Tests validés

**Durée** : 3-4 jours

---

### SPRINT 3 : Résultats de Simulation (2-3 jours) 🔴 HAUTE

**Objectif** : Créer l'UI pour afficher les résultats de simulation (le backend existe déjà !)

**Dépendances** : Sprint 1 (composants)

#### Tâche 3.1 : ShowResultsViewModel (Jour 1)

**Fichier** : `/ViewModels/Results/ShowResultsViewModel.cs`

```csharp
public class ShowResultsViewModel : ViewModelBase
{
    public string ShowName { get; set; }
    public string OverallRating { get; set; } // A+, A, B, etc.
    public int EstimatedAudience { get; set; }
    public int ActualAudience { get; set; }
    public decimal TotalRevenue { get; set; }

    public ObservableCollection<SegmentResultViewModel> SegmentResults { get; }
    public ObservableCollection<WorkerImpactViewModel> WorkerImpacts { get; }
    public ObservableCollection<StorylineProgressionViewModel> StorylineProgressions { get; }

    public ReactiveCommand<Unit, Unit> ReturnToDashboardCommand { get; }
    public ReactiveCommand<Unit, Unit> ContinueToNextWeekCommand { get; }
}
```

---

#### Tâche 3.2 : ShowResultsView (Jours 2-3)

**Fichier** : `/Views/Results/ShowResultsView.axaml`

**Structure** :
```xml
<ScrollViewer>
    <StackPanel>
        <!-- Section 1 : Résumé Global -->
        <Border>
            <StackPanel>
                <TextBlock Text="{Binding ShowName}" FontSize="24"/>
                <TextBlock Text="{Binding OverallRating}" FontSize="48"/>
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="Audience :"/>
                    <TextBlock Text="{Binding ActualAudience}"/>
                    <TextBlock Text="(estimé : {EstimatedAudience})"/>
                </StackPanel>
                <TextBlock Text="Revenus : {TotalRevenue:C}"/>
            </StackPanel>
        </Border>

        <!-- Section 2 : Détail par Segment -->
        <Expander Header="DÉTAIL PAR SEGMENT" IsExpanded="True">
            <DataGrid ItemsSource="{Binding SegmentResults}">
                <!-- Colonnes : Type, Participants, Durée, Note, Crowd Heat -->
            </DataGrid>
        </Expander>

        <!-- Section 3 : Impacts sur le Roster -->
        <Expander Header="IMPACTS SUR LE ROSTER">
            <DataGrid ItemsSource="{Binding WorkerImpacts}">
                <!-- Colonnes : Worker, Fatigue, Momentum, Popularity, Blessures -->
            </DataGrid>
        </Expander>

        <!-- Section 4 : Progression des Storylines -->
        <Expander Header="PROGRESSION DES STORYLINES">
            <ItemsControl ItemsSource="{Binding StorylineProgressions}"/>
        </Expander>

        <!-- Boutons -->
        <StackPanel Orientation="Horizontal">
            <Button Content="Retour au Dashboard"
                    Command="{Binding ReturnToDashboardCommand}"/>
            <Button Content="Passer à la semaine suivante"
                    Command="{Binding ContinueToNextWeekCommand}"/>
        </StackPanel>
    </StackPanel>
</ScrollViewer>
```

---

#### Tâche 3.3 : Intégration avec ShowSimulationEngine

- Appeler `ShowSimulationEngine` depuis BookingView (bouton "Simuler")
- Mapper les résultats vers `ShowResultsViewModel`
- Naviguer vers `ShowResultsView`
- Persister dans `ShowHistory`

**Livrables Sprint 3** :
- ✅ Affichage complet des résultats de simulation
- ✅ Notes par segment
- ✅ Impacts sur workers
- ✅ Progression de storylines
- ✅ Bouton "Simuler" fonctionnel

**Durée** : 2-3 jours

---

### SPRINT 4 : Inbox & Actualités v1 (2-3 jours) 🔴 HAUTE

**Objectif** : Implémenter le système de messages automatiques

**Dépendances** : Sprint 1 (NewsCard)

#### Tâche 4.1 : InboxService (Jour 1)

**Fichier** : `/Services/InboxService.cs`

**Générateurs de messages** :
```csharp
public class InboxService
{
    public void GenerateContractExpiringMessage(int workerId, int daysRemaining);
    public void GenerateInjuryMessage(int workerId, string injuryType, int weeksOut);
    public void GenerateScoutReportMessage(int scoutMissionId);
    public void GenerateYouthProgressionMessage(int traineeId, string milestone);
    public void GenerateFinanceAlertMessage(decimal treasury, decimal threshold);
}
```

**Types de messages** :
1. Fin de contrat imminente (30 jours avant)
2. Blessure confirmée
3. Scout report disponible
4. Progression notable d'un trainee
5. Alerte financière (trésorerie < seuil)

---

#### Tâche 4.2 : InboxViewModel et InboxView (Jour 2)

**Fichiers** :
- `/ViewModels/Inbox/InboxViewModel.cs`
- `/Views/Inbox/InboxView.axaml`

**Features** :
- Liste avec `NewsCard` pour chaque message
- Filtres par type (dropdown)
- Filtres par statut (Non lu / Lu / Archivé)
- Tri par date
- Détail du message dans DetailPanel
- Actions : Marquer lu, Archiver, Supprimer

---

#### Tâche 4.3 : Intégration dans WeeklyLoop (Jour 3)

**Hooks** :
- `WeeklyLoopService` → Génération fin de contrats
- `ShowSimulationEngine` → Génération blessures
- `ScoutingService` → Génération rapports

**Livrables Sprint 4** :
- ✅ InboxService fonctionnel
- ✅ Génération automatique de 5 types de messages
- ✅ InboxView avec filtres et tri
- ✅ Intégration dans la boucle de jeu

**Durée** : 2-3 jours

---

### SPRINT 5 : Calendrier & Création de Shows (2-3 jours) 🟡 MOYENNE

**Objectif** : Permettre la création de shows via une UI

**Dépendances** : Aucune (peut être fait en parallèle)

#### Tâche 5.1 : ShowCreationDialog (Jours 1-2)

**Fichiers** :
- `/Views/Calendar/ShowCreationDialog.axaml`
- `/ViewModels/Calendar/ShowCreationViewModel.cs`

**Champs** :
- Nom du show (TextBox)
- Date (DatePicker avec validation : pas dans le passé, pas de conflit)
- Région (Dropdown chargé depuis DB)
- Venue (Dropdown selon région, avec capacité affichée)
- Durée estimée (Slider 1h-4h)
- Broadcast (Checkbox Oui/Non)

**Validation** :
- Pas de conflit de date
- Budget suffisant pour louer la venue
- Effectif disponible (min 6 workers non blessés)

---

#### Tâche 5.2 : Amélioration CalendarView (Jour 3)

- Vue mensuelle (calendrier visuel)
- Vue liste avec tri/filtrage
- Clic sur show → ouvre BookingView
- Bouton "Créer un Show" → ouvre ShowCreationDialog

**Livrables Sprint 5** :
- ✅ Création de shows via UI
- ✅ Validation complète
- ✅ Calendrier visuel amélioré
- ✅ Navigation vers booking

**Durée** : 2-3 jours

---

### SPRINT 6 : Boucle de Jeu Complète (5-7 jours) 🔴 CRITIQUE

**Objectif** : Connecter tous les éléments pour rendre le jeu jouable end-to-end

**Dépendances** : Sprints 1-5

#### Tâche 6.1 : Bouton "Passer à la Semaine Suivante" (Jour 1)

**Emplacement** : DashboardView (ou topbar)

**Action** :
```csharp
public async Task PasserSemaineCommand()
{
    // 1. Vérifier qu'il n'y a pas de show non simulé
    if (HasPendingShows())
    {
        ShowWarning("Des shows n'ont pas été simulés");
        return;
    }

    // 2. Appeler WeeklyLoopService
    await _weeklyLoopService.ProcessWeek();

    // 3. Rafraîchir tous les ViewModels
    RefreshAll();

    // 4. Générer messages inbox
    await _inboxService.GenerateWeeklyMessages();

    // 5. Naviguer vers Inbox pour afficher les nouveaux messages
    NavigateTo<InboxViewModel>();
}
```

---

#### Tâche 6.2 : Finalisation WeeklyLoopService (Jours 2-4)

**Fichier** : `/Services/WeeklyLoopService.cs` (existe déjà, à compléter)

**Actions hebdomadaires** :
1. Déduction des salaires (via FinanceEngine)
2. Progression de la fatigue (-5 par semaine si repos)
3. Guérison des blessures (-1 semaine pour chaque injury)
4. Progression des trainees (YouthProgressionService)
5. Vieillissement des workers (+1 semaine age)
6. Progression des storylines (si pas d'update récent)
7. Détection fins de contrat imminentes
8. Génération de messages inbox

---

#### Tâche 6.3 : Tests End-to-End (Jours 5-7)

**Scénario de test complet** :
1. Créer nouvelle partie
2. Signer un worker
3. Créer un show
4. Booker le show (minimum 5 segments)
5. Valider la carte
6. Simuler le show
7. Voir les résultats
8. Passer à la semaine suivante
9. Vérifier :
   - Salaires déduits
   - Fatigue mise à jour
   - Messages inbox générés
   - Show archivé
10. Répéter le cycle (min 10 semaines)

**Tests de non-régression** :
- Navigation ne casse pas
- Seed data toujours OK
- Sauvegardes fonctionnent
- Pas de fuite mémoire

**Livrables Sprint 6** :
- ✅ Boucle de jeu 100% fonctionnelle
- ✅ WeeklyLoopService complet
- ✅ Bouton "Passer semaine" opérationnel
- ✅ 10 semaines jouables sans bug
- ✅ Tests end-to-end validés

**Durée** : 5-7 jours

---

## 📅 PLANNING GLOBAL

| Sprint | Nom | Durée | Dépendances | Statut |
|--------|-----|-------|-------------|--------|
| **Sprint 0** | Finalisation Infrastructure | 1-2 jours | - | À faire |
| **Sprint 1** | Composants UI Réutilisables | 3-5 jours | Sprint 0 | À faire |
| **Sprint 2** | ProfileView Universel | 3-4 jours | Sprint 1 | À faire |
| **Sprint 3** | Résultats de Simulation | 2-3 jours | Sprint 1 | À faire |
| **Sprint 4** | Inbox & Actualités v1 | 2-3 jours | Sprint 1 | À faire |
| **Sprint 5** | Calendrier & Création Shows | 2-3 jours | - (parallèle) | À faire |
| **Sprint 6** | Boucle de Jeu Complète | 5-7 jours | Sprints 1-5 | À faire |

**Durée totale estimée** : 18-27 jours (3.5-5.5 semaines)

**Avec parallélisation** : Sprint 5 peut être fait en parallèle de Sprint 2/3/4 → **~4 semaines**

---

## 🎯 CRITÈRES DE SUCCÈS (Phase 1 Complète)

À la fin de Sprint 6, le projet doit valider :

### 1. Stabilité et Performance
- [ ] Application démarre en < 3 secondes
- [ ] Sauvegarde/chargement 100% fonctionnel
- [ ] Navigation fluide (< 200ms)
- [ ] Aucune fuite mémoire (test 100 cycles)

### 2. Boucle Jouable Complète
- [ ] Création de partie fonctionnelle
- [ ] Signature de worker (via contrats simplifié ou direct)
- [ ] Création de show
- [ ] Booking de show (min 5 segments)
- [ ] Validation de la carte
- [ ] Simulation du show
- [ ] Affichage des résultats complets
- [ ] Passage à la semaine suivante
- [ ] Répétabilité du cycle (min 10 semaines)

### 3. Validation du Gameplay
- [ ] 10 testeurs valident "engageant"
- [ ] Temps pour booker un show < 10 min
- [ ] Taux de complétion 10 semaines > 80%
- [ ] Aucun bug bloquant
- [ ] Quick Start Guide disponible

---

## 🔄 APRÈS SPRINT 6 : PROCHAINES ÉTAPES

Une fois la boucle de jeu fonctionnelle, on pourra attaquer :

### Court Terme (Sprints 7-9)
- **Sprint 7** : SegmentEditorDialog (édition détaillée des segments)
- **Sprint 8** : ContractNegotiationDialog (offre/contre-offre)
- **Sprint 9** : Amélioration Booking (templates, bibliothèque)

### Moyen Terme (Sprints 10-15)
- Fonctionnalités Phase 1 restantes (selon PLAN_IMPLEMENTATION_TECHNIQUE.md)
- Titres avancés (ranking, tournois)
- Storylines avancées (builder, arc narratif)
- Youth avancé (structures, pipeline)

### Long Terme
- Phase 2 : Profondeur Stratégique (6-12 mois)

---

## 📊 COMPARAISON AVEC LE PLAN INITIAL

| Aspect | Plan Initial | Plan Révisé | Raison |
|--------|--------------|-------------|--------|
| **Démarrage** | Tâche 1.1.1 Localisation | **Sprint 0 : DI** | Infrastructure à finaliser d'abord |
| **Première Feature** | Tâche 1.1.2 Kit UI | **Sprint 1 : Composants** | Idem, mais adapté |
| **ProfileView** | Sprint 1 proposé | **Sprint 2** | Dépend des composants |
| **Focus** | Tout Phase 1 | **Boucle de jeu d'abord** | Dérisquer, jouable plus vite |
| **Durée Phase 1** | 3-6 mois | **4-6 semaines** (base) | On est plus avancés ! |

---

## ✅ RECOMMANDATIONS

1. **Démarrer IMMÉDIATEMENT Sprint 0** (enregistrement DI)
   - Bloquant pour tout le reste
   - Rapide (1-2 jours)

2. **Enchaîner directement Sprint 1** (composants UI)
   - Débloque Sprints 2, 3, 4
   - Investissement qui paye

3. **Paralléliser Sprint 5** (Calendrier) avec Sprints 2-4
   - Pas de dépendance
   - Gain de temps

4. **Focus absolu sur Sprint 6** (Boucle complète)
   - C'est le vrai objectif de Phase 1
   - Tests rigoureux requis

5. **Ne PAS se laisser distraire** par Phase 2
   - Phase 2 n'a de sens que si Phase 1 marche
   - Boucle de jeu jouable = validation du concept

---

## 🎯 CONCLUSION

**Le projet est en excellente position** :
- Infrastructure complète à 95%
- UI/ViewModels beaucoup plus avancés que pensé
- Simulation backend puissante déjà là

**Il manque seulement 4-6 semaines de travail concentré** pour avoir un jeu **jouable de bout en bout**.

**Priorité absolue** : Composants UI → Résultats → Inbox → Boucle complète

**Une fois la boucle fonctionnelle**, on peut enrichir à l'infini avec Phase 1 avancée et Phase 2.

---

**Prêt à démarrer Sprint 0 ? 🚀**

**Prochaine action recommandée** : Ouvrir `/src/RingGeneral.UI/App.axaml.cs` et enregistrer les 15 repositories manquants dans le DI.
