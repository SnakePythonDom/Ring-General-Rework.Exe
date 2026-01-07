# Comparaison des Plans et Prochaines Étapes

**Date** : 7 janvier 2026
**Auteur** : Claude

---

## 📊 Comparaison des Documents Existants

### 1. ROADMAP_MISE_A_JOUR.md (Court Terme)

**Horizon** : 3-4 mois (Phase 0-2)
**Focus** : Stabilisation technique et infrastructure
**État** : Phase 0 à 80%, Phase 1-2 à 0%

**Phases** :
- ✅ **Phase 0** (80%) : Stabilisation critique (architecture, navigation, DI)
- 🟡 **Phase 1** (0%) : Fondations UI/UX (tous les ViewModels/Views)
- 🟡 **Phase 2** (0%) : Intégration données (seed DB, mapping)
- 🟡 **Phase 3** (0%) : Fonctionnalités métier (contrats, inbox, booking complet)

**Forces** :
- Très pragmatique et orienté "faire marcher le code"
- Estimations réalistes (jours/semaines)
- Identifie les blocages techniques actuels

**Faiblesses** :
- Peu de détails sur les mécaniques de gameplay
- Ne couvre pas la vision long terme

---

### 2. PLAN_IMPLEMENTATION_TECHNIQUE.md (Long Terme)

**Horizon** : 12-18 mois (Phase 1-2)
**Focus** : Features gameplay et simulation profonde
**État** : 30-40% de complétion MVP

**Phases** :
- 🟡 **Phase 1** (3-6 mois) : Le Socle Jouable
  - Infrastructure & UI/UX
  - Boucle de jeu hebdomadaire
  - Gestion du roster
- 🟡 **Phase 2** (6-12 mois) : La Profondeur Stratégique
  - Écosystème de développement des talents (Dojo, Performance Center, Club)
  - Simulation approfondie (narration, coulisses)
  - Expansion des systèmes (finances, broadcasting, monde vivant)

**Forces** :
- Vision complète du produit final
- Détails exhaustifs des mécaniques (18 000 mots)
- Critères de validation clairs
- Dépendances entre tâches identifiées

**Faiblesses** :
- Suppose que l'infrastructure est déjà stable
- Peut sembler intimidant par son ampleur

---

### 3. IMPLEMENTATION_PROTOTYPE_D.md (État Actuel)

**Statut** : ✅ Prototype D implémenté (navigation à 3 colonnes)

**Ce qui existe** :
- ✅ Architecture MVVM avec ReactiveUI
- ✅ Navigation TreeView fonctionnelle
- ✅ BookingView avec table FM26
- ✅ Services (NavigationService, EventAggregator)

**Ce qui manque** :
- ❌ Configuration DI complète
- ❌ DataTemplates pour toutes les vues
- ❌ ViewModels (Roster, Dashboard, Youth, Finance, Calendar)
- ❌ Context Panel fonctionnel
- ❌ Seed de la base de données

---

## 🎯 Synthèse : Où en Sommes-Nous ?

| Aspect | Progression | Prochain Besoin |
|--------|-------------|-----------------|
| **Architecture UI** | 80% ✅ | Finaliser DI + DataTemplates |
| **Navigation** | 90% ✅ | Créer les vues manquantes |
| **Base de Données** | 90% ✅ | Seed automatique |
| **Simulation** | 70% ✅ | UI des résultats |
| **Booking** | 60% ⚠️ | Validation + templates |
| **Roster Management** | 20% ❌ | UI complète |
| **Contrats** | 10% ❌ | Tout |
| **Youth/Finance** | 30% ⚠️ | UI complète |
| **Boucle de jeu** | 0% ❌ | Connecter tous les éléments |

**Constat** : On est à la **transition entre Phase 0 et Phase 1**

---

## 🔄 Plan Unifié : Pont Entre Court et Long Terme

### Étape Actuelle : **Phase 0.5 - Finalisation de l'Infrastructure**

**Objectif** : Terminer Phase 0 (ROADMAP) ET poser les bases de Phase 1 (PLAN_TECHNIQUE)

**Durée** : 2-3 semaines

**Tâches** :

1. ✅ **Finaliser la configuration (déjà fait à 80%)**
   - DI dans App.axaml.cs
   - Enregistrement de tous les services

2. 🎨 **Créer les composants UI réutilisables** (NOUVEAU - du PLAN_TECHNIQUE)
   - `AttributeBar.axaml` (barre de stat 1-20 avec tooltip)
   - `SortableDataGrid.axaml` (DataGrid avec tri/filtres)
   - `DetailPanel.axaml` (panneau de contexte structuré)
   - `NewsCard.axaml` (carte de message inbox)

3. 📄 **Créer la Fiche de Profil Universelle** (PRIORITÉ #1)
   - `ProfileView.axaml` (avec onglets : Profil, Attributs, Historique, Contrat)
   - `ProfileViewModel.cs` (support Worker, Staff, Trainee)
   - Utilise les composants créés en #2

4. 📊 **Seed automatique de la DB** (du ROADMAP)
   - `DbSeeder.cs` avec import depuis BAKI1.1.db
   - Appel automatique au premier lancement

5. 🖼️ **Compléter les ViewModels/Views manquants** (du ROADMAP)
   - RosterViewModel + RosterView
   - DashboardViewModel + DashboardView
   - TitlesViewModel + TitlesView
   - CalendarViewModel + CalendarView

---

## 🚀 PROPOSITION : Première Partie à Implémenter

### 🎯 **SPRINT 1 : "La Fiche de Profil Universelle"**

**Durée** : 3-5 jours
**Priorité** : 🔴 CRITIQUE
**Dépendances** : Aucune (peut démarrer immédiatement)

---

#### Pourquoi Commencer Par Là ?

1. ✅ **Visible et testable** : On voit immédiatement le résultat
2. ✅ **Bloque le reste** : Contrats, Booking, Youth ont tous besoin de profils
3. ✅ **Pose les standards** : Établit le pattern pour tous les autres écrans
4. ✅ **Crée les composants réutilisables** : Utilisés partout ensuite
5. ✅ **Pas de dépendance** : Peut se faire même si la DB est vide (données de test)

---

#### Objectif du Sprint 1

**À la fin du sprint, on doit avoir :**

1. ✅ Un système de composants réutilisables (AttributeBar, DetailPanel, etc.)
2. ✅ Une fiche de profil complète et fonctionnelle
3. ✅ Affichage de tous les attributs avec tooltips
4. ✅ Navigation depuis RosterView vers ProfileView
5. ✅ Données de test pour un worker (John Cena par exemple)

---

#### Plan Détaillé du Sprint 1

##### **Jour 1 : Composants Réutilisables (6-8h)**

**Tâche 1.1 : Créer AttributeBar.axaml**
```
Composant : Barre visuelle pour afficher une stat (1-20)
Features :
- Barre de progression avec couleur graduée (rouge < 50, orange 50-70, vert > 70)
- Label avec nom de l'attribut
- Valeur numérique affichée
- Tooltip avec description détaillée (chargée depuis ressources)
- Flèche ↑↓ si changement récent

Livrables :
- /src/RingGeneral.UI/Components/AttributeBar.axaml
- /src/RingGeneral.UI/Components/AttributeBar.axaml.cs
- /src/RingGeneral.UI/ViewModels/Shared/AttributeBarViewModel.cs
```

**Tâche 1.2 : Créer AttributeDescriptions.fr.resx**
```
Fichier de ressources avec descriptions de tous les attributs :
- Universels : ConditionPhysique, Moral
- In-Ring : Timing, Psychology, Selling, Stamina, Safety, Technique
- Entertainment : Charisma, Promo, CrowdConnection, StarPower
- Story : Storytelling, CharacterWork, Versatility

Chaque description : 100-150 mots

Livrables :
- /src/RingGeneral.UI/Resources/AttributeDescriptions.fr.resx
```

**Tâche 1.3 : Créer DetailPanel.axaml**
```
Composant : Panneau de détail pour le context panel (colonne droite)
Features :
- Sections collapsibles
- Support de différents types de contenu (texte, stats, actions)
- Style cohérent avec le thème FM26

Livrables :
- /src/RingGeneral.UI/Components/DetailPanel.axaml
```

---

##### **Jour 2 : ViewModel de Profil (6-8h)**

**Tâche 2.1 : Créer ProfileViewModel.cs**
```csharp
public class ProfileViewModel : ViewModelBase
{
    // Profil Type (Worker, Staff, Trainee)
    public string ProfileType { get; }

    // Onglets
    public ProfileTabViewModel ProfileTab { get; }
    public AttributesTabViewModel AttributesTab { get; }
    public HistoryTabViewModel HistoryTab { get; }
    public ContractTabViewModel ContractTab { get; }

    // Navigation entre onglets
    public ReactiveCommand<string, Unit> SwitchTabCommand { get; }

    // Actions
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> ReleaseCommand { get; }
}
```

**Tâche 2.2 : Créer les ViewModels d'onglets**
```
- ProfileTabViewModel : Infos générales (photo, nom, âge, etc.)
- AttributesTabViewModel : Tous les attributs avec AttributeBar
- HistoryTabViewModel : Derniers matchs, storylines, titres
- ContractTabViewModel : Détails du contrat actuel
```

**Livrables :**
```
- /src/RingGeneral.UI/ViewModels/Profile/ProfileViewModel.cs
- /src/RingGeneral.UI/ViewModels/Profile/ProfileTabViewModel.cs
- /src/RingGeneral.UI/ViewModels/Profile/AttributesTabViewModel.cs
- /src/RingGeneral.UI/ViewModels/Profile/HistoryTabViewModel.cs
- /src/RingGeneral.UI/ViewModels/Profile/ContractTabViewModel.cs
```

---

##### **Jour 3 : Vue de Profil - Partie 1 (6-8h)**

**Tâche 3.1 : Créer ProfileView.axaml - Structure**
```xml
<UserControl>
    <Grid RowDefinitions="Auto,*">
        <!-- Header : Photo + Nom + Stats clés -->
        <Border Grid.Row="0">
            <StackPanel>
                <Image Source="{Binding PhotoPath}" Width="120" Height="120"/>
                <TextBlock Text="{Binding FullName}" FontSize="24"/>
                <TextBlock Text="{Binding Role}" FontSize="14"/>
            </StackPanel>
        </Border>

        <!-- Tabs : Profil | Attributs | Historique | Contrat -->
        <TabControl Grid.Row="1" SelectedIndex="{Binding SelectedTabIndex}">
            <TabItem Header="PROFIL">
                <views:ProfileTabView DataContext="{Binding ProfileTab}"/>
            </TabItem>
            <TabItem Header="ATTRIBUTS">
                <views:AttributesTabView DataContext="{Binding AttributesTab}"/>
            </TabItem>
            <TabItem Header="HISTORIQUE">
                <views:HistoryTabView DataContext="{Binding HistoryTab}"/>
            </TabItem>
            <TabItem Header="CONTRAT">
                <views:ContractTabView DataContext="{Binding ContractTab}"/>
            </TabItem>
        </TabControl>
    </Grid>
</UserControl>
```

**Tâche 3.2 : Créer ProfileTabView.axaml**
```
Affichage des infos générales :
- Photo (placeholder si absente)
- Nom complet
- Surnom / Ring Name
- Âge, Date de naissance
- Nationalité (avec drapeau)
- Taille, Poids
- Gimmick actuel
- Statut (Actif, Blessé, Suspendu)
```

**Livrables :**
```
- /src/RingGeneral.UI/Views/Profile/ProfileView.axaml
- /src/RingGeneral.UI/Views/Profile/ProfileTabView.axaml
```

---

##### **Jour 4 : Vue de Profil - Partie 2 (6-8h)**

**Tâche 4.1 : Créer AttributesTabView.axaml**
```xml
<ScrollViewer>
    <StackPanel>
        <!-- Section : ATTRIBUTS UNIVERSELS -->
        <Expander Header="ATTRIBUTS UNIVERSELS" IsExpanded="True">
            <StackPanel>
                <components:AttributeBar
                    AttributeName="Condition Physique"
                    Value="{Binding ConditionPhysique}"
                    Description="{Binding ConditionDescription}"/>
                <components:AttributeBar
                    AttributeName="Moral"
                    Value="{Binding Moral}"/>
            </StackPanel>
        </Expander>

        <!-- Section : IN-RING (si worker) -->
        <Expander Header="IN-RING" IsExpanded="True"
                  IsVisible="{Binding IsWorker}">
            <StackPanel>
                <components:AttributeBar AttributeName="Timing" Value="{Binding Timing}"/>
                <components:AttributeBar AttributeName="Psychology" Value="{Binding Psychology}"/>
                <components:AttributeBar AttributeName="Selling" Value="{Binding Selling}"/>
                <components:AttributeBar AttributeName="Stamina" Value="{Binding Stamina}"/>
                <components:AttributeBar AttributeName="Safety" Value="{Binding Safety}"/>
                <components:AttributeBar AttributeName="Technique" Value="{Binding Technique}"/>
            </StackPanel>
        </Expander>

        <!-- Idem pour ENTERTAINMENT, STORY, etc. -->
    </StackPanel>
</ScrollViewer>
```

**Tâche 4.2 : Créer HistoryTabView.axaml**
```
Affichage de l'historique :
- Derniers matchs (5) avec notes et opponents
- Storylines actives et passées (3 dernières)
- Titres détenus (actuels + historique)
- Timeline visuelle optionnelle
```

**Tâche 4.3 : Créer ContractTabView.axaml**
```
Affichage du contrat :
- Salaire annuel
- Date de début, Date de fin
- Durée restante (avec ProgressBar)
- Clauses (Exclusivité, Rôle, etc.)
- Boutons : "Renégocier" | "Résilier"
```

**Livrables :**
```
- /src/RingGeneral.UI/Views/Profile/AttributesTabView.axaml
- /src/RingGeneral.UI/Views/Profile/HistoryTabView.axaml
- /src/RingGeneral.UI/Views/Profile/ContractTabView.axaml
```

---

##### **Jour 5 : Intégration et Tests (6-8h)**

**Tâche 5.1 : Intégrer ProfileView dans la navigation**
```csharp
// Dans ShellViewModel, ajouter la navigation vers ProfileView
NavigateToProfileCommand = ReactiveCommand.Create<int>(workerId =>
{
    var profileVM = new ProfileViewModel(_repository, workerId);
    NavigationService.NavigateTo(profileVM);
});
```

**Tâche 5.2 : Créer des données de test**
```csharp
// Dans ProfileViewModel.cs (ou un fichier séparé)
public static ProfileViewModel CreateTestData()
{
    return new ProfileViewModel
    {
        FullName = "John Cena",
        Role = "Main Event Star",
        Age = 47,
        Nationality = "USA",
        ConditionPhysique = 85,
        Moral = 90,
        Timing = 95,
        Psychology = 88,
        // ... etc.
    };
}
```

**Tâche 5.3 : Tester l'affichage**
```
Checklist de tests :
- [ ] ProfileView s'affiche correctement
- [ ] Les 4 onglets sont accessibles
- [ ] Les AttributeBar affichent les bonnes valeurs
- [ ] Les tooltips s'affichent au survol
- [ ] Les couleurs sont correctes (rouge/orange/vert)
- [ ] La navigation depuis RosterView fonctionne
- [ ] Les données de test s'affichent
```

**Tâche 5.4 : Connecter à la vraie DB (si seed disponible)**
```csharp
// Dans ProfileViewModel.cs
public ProfileViewModel(GameRepository repository, int workerId)
{
    var worker = repository.ChargerWorker(workerId);

    FullName = worker.Name;
    ConditionPhysique = worker.Attributes.Condition;
    Moral = worker.Attributes.Morale;
    // ... mapping complet
}
```

**Livrables :**
```
- Navigation fonctionnelle vers ProfileView
- Données de test ou vraies données affichées
- Tous les tests passés
- Documentation du composant (README.md)
```

---

#### Critères de Succès du Sprint 1

**À la fin du Sprint 1, on doit pouvoir :**

1. ✅ Cliquer sur un worker dans RosterView (ou une liste)
2. ✅ Naviguer vers sa fiche de profil complète
3. ✅ Voir tous ses attributs affichés avec des barres visuelles
4. ✅ Survoler un attribut et voir sa description détaillée
5. ✅ Naviguer entre les 4 onglets (Profil, Attributs, Historique, Contrat)
6. ✅ Voir un affichage cohérent et professionnel (style FM26)

**Résultat Visuel Attendu :**

```
┌──────────────────────────────────────────────────────────────┐
│  ← Retour          JOHN CENA - Main Event Star              │
├──────────────────────────────────────────────────────────────┤
│  [Photo]          John Cena                                  │
│                   "The Champ"                                │
│                   🇺🇸 USA • 47 ans • 113 kg • 185 cm         │
├──────────────────────────────────────────────────────────────┤
│  📋 PROFIL  |  📊 ATTRIBUTS  |  📜 HISTORIQUE  |  📄 CONTRAT│
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ▼ ATTRIBUTS UNIVERSELS                                     │
│     Condition Physique  ████████████████░░  85/100          │
│     Moral              █████████████████░░  90/100          │
│                                                              │
│  ▼ IN-RING                                                   │
│     Timing             ███████████████████  95/100  ↑       │
│     Psychology         █████████████████░░  88/100          │
│     Selling            ██████████████████░  92/100          │
│     Stamina            ████████████████░░░  82/100  ↓       │
│     Safety             ███████████████████  96/100          │
│     Technique          █████████████████░░  87/100          │
│                                                              │
│  ▼ ENTERTAINMENT                                             │
│     Charisma           ████████████████████ 100/100         │
│     Promo              ███████████████████░  98/100         │
│     Crowd Connection   ████████████████████ 100/100         │
│     Star Power         ███████████████████░  99/100         │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

#### Après le Sprint 1 : Prochaines Étapes

**Sprint 2** : Seed de la DB + RosterView complète
**Sprint 3** : Inbox & Actualités v1
**Sprint 4** : Calendrier & Création de Shows
**Sprint 5** : Amélioration Booking + Résultats
**Sprint 6** : Contrats v1

**Objectif Phase 1 complète** : 6-8 semaines

---

## 📋 TODO List Mise à Jour

### Immédiat (Sprint 1 - Cette Semaine)

- [ ] **Jour 1** : Créer AttributeBar + AttributeDescriptions.fr.resx + DetailPanel
- [ ] **Jour 2** : Créer ProfileViewModel + ViewModels d'onglets
- [ ] **Jour 3** : Créer ProfileView + ProfileTabView
- [ ] **Jour 4** : Créer AttributesTabView + HistoryTabView + ContractTabView
- [ ] **Jour 5** : Intégrer, tester, connecter à la DB

### Court Terme (Sprints 2-3 - Semaines 2-3)

- [ ] Implémenter DbSeeder avec import BAKI1.1.db
- [ ] Créer RosterViewModel + RosterView complète
- [ ] Créer DashboardViewModel + DashboardView
- [ ] Créer CalendarViewModel + CalendarView
- [ ] Implémenter InboxService + InboxView

### Moyen Terme (Sprints 4-6 - Semaines 4-6)

- [ ] Améliorer BookingView avec validation complète
- [ ] Créer ShowResultsView pour afficher les résultats
- [ ] Implémenter ContractNegotiationDialog
- [ ] Créer le système de création de shows
- [ ] Connecter la boucle de jeu complète

---

## 🎯 Conclusion

**Recommandation : DÉMARRER LE SPRINT 1 IMMÉDIATEMENT**

La Fiche de Profil Universelle est le **meilleur point de départ** car :

1. ✅ Elle est **indépendante** (pas de blocage)
2. ✅ Elle pose les **standards UI** pour tout le projet
3. ✅ Elle crée les **composants réutilisables** essentiels
4. ✅ Elle est **visible et impressionnante** (bon moral d'équipe !)
5. ✅ Elle **débloque** tous les autres écrans (contrats, booking, youth)

**Estimation réaliste : 3-5 jours de travail concentré**

Une fois terminée, on aura une base solide pour attaquer les autres fonctionnalités de manière beaucoup plus rapide grâce aux composants créés.

---

**Prêt à démarrer ? 🚀**
