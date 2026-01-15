# Suite de Tests - Ring General

Ce document décrit la suite complète de tests automatisés créée pour le projet Ring General.

## Vue d'ensemble

La suite de tests comprend plusieurs catégories :

- **Tests Unitaires** : Tests des composants individuels (services, ViewModels)
- **Tests d'Intégration** : Tests des interactions entre composants (repositories, base de données)
- **Tests UI** : Tests automatisés de l'interface utilisateur avec Avalonia Headless
- **Tests de Validation** : Tests des règles métier et contraintes des modèles

## Structure des Tests

```
tests/
├── RingGeneral.Tests/
│   ├── RingGeneral.Tests.csproj          # Configuration du projet de tests
│   ├── BookerArchetypeTests.cs           # Tests existants pour BookerAIEngine
│   ├── FinanceEngineTests.cs             # Tests unitaires pour FinanceEngine
│   ├── ShowSimulationEngineTests.cs      # Tests unitaires pour ShowSimulationEngine
│   ├── WorkerRepositoryTests.cs          # Tests d'intégration pour WorkerRepository
│   ├── DashboardViewModelTests.cs        # Tests unitaires pour DashboardViewModel
│   ├── ModelValidationTests.cs           # Tests de validation des modèles
│   ├── DashboardViewUITests.cs           # Tests UI pour DashboardView
│   ├── RepositoryTestBase.cs             # Classe de base pour les tests de repository
│   └── ...
└── README.md                             # Ce fichier
```

## Technologies Utilisées

### Frameworks de Test
- **xUnit** : Framework de test principal
- **FluentAssertions** : Assertions expressives et lisibles
- **Moq** : Framework de mocking pour les dépendances
- **AutoFixture** : Génération automatique de données de test

### Tests UI
- **Avalonia.Headless** : Tests UI sans interface graphique
- **Avalonia.Headless.XUnit** : Intégration xUnit pour Avalonia

### Couverture de Code
- **coverlet** : Collecteur de couverture de code

## Exécution des Tests

### Tous les tests
```powershell
# Depuis la racine du projet
.\scripts\run-tests.ps1
```

### Tests spécifiques
```powershell
# Tests unitaires seulement
.\scripts\run-tests.ps1 -Unit

# Tests d'intégration seulement
.\scripts\run-tests.ps1 -Integration

# Tests UI seulement
.\scripts\run-tests.ps1 -UI

# Avec couverture de code
.\scripts\run-tests.ps1 -Coverage

# Filtrer les tests
.\scripts\run-tests.ps1 -Filter "RingGeneral.Tests.FinanceEngineTests"
```

### Avec dotnet CLI directement
```bash
# Tous les tests
dotnet test tests/RingGeneral.Tests/

# Tests spécifiques
dotnet test tests/RingGeneral.Tests/ --filter "RingGeneral.Tests.FinanceEngineTests"

# Avec couverture
dotnet test tests/RingGeneral.Tests/ --collect:"XPlat Code Coverage"
```

## Description des Tests

### FinanceEngineTests

Tests unitaires complets pour le moteur de calcul financier :

- **Calcul des finances de show** : Vérification des calculs de billetterie, merchandising, TV
- **Calcul des capacités** : Test des formules de calcul de capacité des venues
- **Calcul des taux de remplissage** : Validation des taux selon l'audience
- **Calcul des prix de billets** : Test des prix selon prestige et audience
- **Calcul du merchandising** : Vérification des revenus merchandising
- **Calcul des paies** : Test des fréquences de paiement (hebdomadaire/mensuel)

### ShowSimulationEngineTests

Tests unitaires pour le moteur de simulation de shows :

- **Initialisation** : Test des constructeurs et dépendances
- **Simulation complète** : Vérification du résultat global d'une simulation
- **Calcul de la chaleur du public** : Test des formules de crowd heat
- **Gestion des blessures** : Test de la logique de génération de blessures
- **Bénéfices de niche** : Test des multiplicateurs pour fédérations de niche
- **Intégration des repositories** : Test des interactions avec les repositories externes

### WorkerRepositoryTests

Tests d'intégration pour le repository des workers :

- **Base de données en mémoire** : Utilise SQLite en mémoire pour l'isolation
- **Récupération du roster** : Test de `ChargerBackstageRoster`
- **Gestion de la morale** : Test de `ChargerMorale` et `ModifierMorale`
- **Morales multiples** : Test de `ChargerMorales` et `ModifierMorales`

### DashboardViewModelTests

Tests unitaires pour le ViewModel du tableau de bord :

- **Initialisation** : Valeurs par défaut et constructeurs
- **Notifications de propriété** : Test des événements PropertyChanged
- **Logique métier** : Calcul automatique des labels de morale
- **Validation des données** : Clamp des valeurs entre 0 et 100

### ModelValidationTests

Tests de validation pour les modèles de données :

- **Workers** : Validation de la santé, popularité, morale
- **Companies** : Validation du prestige, reach, audience
- **Shows** : Validation des contextes de simulation
- **Finances** : Validation des transactions et calculs

### DashboardViewUITests

Tests UI automatisés pour la vue tableau de bord :

- **Rendu de la vue** : Test que la vue se charge correctement
- **Affichage des données** : Vérification que les bindings fonctionnent
- **Mise à jour en temps réel** : Test des changements de ViewModel
- **Structure de l'UI** : Validation de la hiérarchie des contrôles

## Analyse des Logs d'Erreurs

Pour analyser les plantages de l'application :

1. **Récupérer la stack trace** : Copiez le message d'erreur du terminal
2. **Identifier le fichier source** : Cherchez le nom du fichier dans la stack trace
3. **Localiser la ligne** : Utilisez les numéros de ligne pour trouver le code fautif
4. **Comprendre le contexte** : Regardez les méthodes appelantes dans la stack trace
5. **Créer un test de reproduction** : Écrivez un test qui reproduit le bug

Exemple de stack trace :
```
System.NullReferenceException: Object reference not set to an instance of an object.
   at RingGeneral.Core.Simulation.FinanceEngine.CalculerFinancesShow(ShowFinanceContext context)
   at RingGeneral.UI.ViewModels.FinanceViewModel.ProcessShow(ShowData show)
   at RingGeneral.UI.Views.FinanceView.HandleShowClick(Object sender, RoutedEventArgs e)
```

## Bonnes Pratiques de Test

### Tests Unitaires
- **Isoler les dépendances** : Utiliser des mocks pour les externalités
- **Nommer clairement** : `MethodName_ShouldExpectedBehavior_WhenCondition`
- **Test des cas limites** : Valeurs nulles, vides, extrêmes
- **Un seul concept par test** : Chaque test vérifie une chose spécifique

### Tests d'Intégration
- **Base de données isolée** : SQLite en mémoire pour éviter les conflits
- **Nettoyage automatique** : Remise à zéro entre les tests
- **Données de test réalistes** : Utiliser des données représentatives

### Tests UI
- **Attendre le rendu** : Utiliser `Task.Delay()` pour laisser l'UI se dessiner
- **Trouver les contrôles** : Utiliser les noms ou la hiérarchie visuelle
- **Tester les bindings** : Vérifier que les changements de VM affectent l'UI
- **Structure de l'UI** : Valider la présence des contrôles attendus

## Maintenance des Tests

### Ajouter de nouveaux tests
1. **Identifier le composant** : Quel service/modèle/vue tester ?
2. **Créer la classe de test** : Suivre la convention `*Tests.cs`
3. **Ajouter au projet** : S'assurer que le fichier est inclus
4. **Écrire les tests** : Commencer par les cas d'usage principaux
5. **Vérifier la couverture** : Les nouveaux tests améliorent-ils la couverture ?

### Debugging des tests
- **Tests qui échouent** : Vérifier les assertions et les données de test
- **Tests lents** : Optimiser les setups et utiliser des mocks
- **Tests flaky** : Éviter les dépendances temporelles
- **Couverture insuffisante** : Identifier les chemins de code non testés

## Métriques de Qualité

- **Couverture de code** : Viser >80% pour le code critique
- **Temps d'exécution** : < 5 minutes pour la suite complète
- **Fiabilité** : Aucun test flaky (qui échoue aléatoirement)
- **Lisibilité** : Tests compréhensibles sans documentation supplémentaire

## Intégration Continue

Les tests peuvent être intégrés dans un pipeline CI/CD :

```yaml
# Exemple GitHub Actions
- name: Run tests
  run: .\scripts\run-tests.ps1 -Coverage

- name: Upload coverage
  uses: codecov/codecov-action@v3
  with:
    file: ./TestResults/*/coverage.cobertura.xml
```

---

Cette suite de tests fournit une couverture complète du projet Ring General, permettant de détecter les régressions et de valider les nouvelles fonctionnalités de manière automatisée.