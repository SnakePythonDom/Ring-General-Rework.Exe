---
name: Correction Ownership Filtrage DataBinding Recrutement
overview: Correction de l'ownership (Player n'a pas d'owner, seuls les Rivals en ont), ajout de PlayerCompanies filtré, réveil du DataBinding UI, modification de HireStaffCommand pour accepter un paramètre ChildCompany avec validation budget/Manager, et fix du threading pour les ObservableCollection.
todos:
  - id: "1"
    content: Ajouter propriété PlayerCompanies dans CompanyHubViewModel avec RaiseAndSetIfChanged
    status: completed
  - id: "2"
    content: Injecter IChildCompanyExtendedRepository et IChildCompanyStaffService dans CompanyHubViewModel
    status: completed
  - id: "3"
    content: Créer méthode LoadPlayerCompaniesAsync() avec filtrage par ParentCompanyId et threading UI
    status: completed
  - id: "4"
    content: Vérifier et corriger le DataContext de CompanyHubView
    status: completed
  - id: "5"
    content: Ajouter contrôle XAML avec ItemsSource lié à PlayerCompanies
    status: completed
  - id: "6"
    content: Modifier HireStaffCommand pour accepter string childCompanyId comme paramètre
    status: completed
  - id: "7"
    content: Créer prédicat canExecute pour HireStaffCommand (budget + absence Manager)
    status: completed
  - id: "8"
    content: Implémenter assignation Manager via ChildCompanyStaffService dans HireStaffAsync
    status: completed
  - id: "9"
    content: Appliquer RxApp.MainThreadScheduler pour toutes les mises à jour ObservableCollection
    status: completed
---

# Plan de Correction : Ownership, Filtrage, DataBinding et Recrutement

## 1. Correction Ownership & Filtrage

### 1.1 Ajout de la propriété PlayerCompanies dans CompanyHubViewModel

**Fichier** : `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs`

- Ajouter une propriété `PlayerCompanies` de type `ObservableCollection<ChildCompanyExtended>`
- Injecter `IChildCompanyExtendedRepository` dans le constructeur
- Créer une méthode `LoadPlayerCompaniesAsync()` qui :
  - Récupère le `PlayerCompanyId` depuis `SaveGames` (IsActive = 1)
  - Appelle `_childCompanyRepository.GetChildCompaniesByParentIdAsync(playerCompanyId)`
  - Met à jour `PlayerCompanies` sur le thread UI via `RxApp.MainThreadScheduler`
- Appeler `LoadPlayerCompaniesAsync()` dans `OnNavigatedTo()` et après `LoadPlayerCompany()`

**Code à ajouter** :

```csharp
private ObservableCollection<ChildCompanyExtended> _playerCompanies;
public ObservableCollection<ChildCompanyExtended> PlayerCompanies
{
    get => _playerCompanies;
    private set => this.RaiseAndSetIfChanged(ref _playerCompanies, value);
}
```

### 1.2 Vérification de l'absence d'OwnerId pour Player

**Confirmation** : Le Player n'a PAS d'owner. La compagnie du joueur est identifiée via `SaveGames.PlayerCompanyId` et `Companies.IsPlayerControlled = 1`. Seuls les Rivals (compagnies avec `IsPlayerControlled = 0`) peuvent avoir un Owner dans la table `Owners`.

## 2. Réveil de l'UI (DataBinding)

### 2.1 Vérification du DataContext

**Fichier** : `src/RingGeneral.UI/Views/CompanyHub/CompanyHubView.axaml`

- Vérifier que le DataContext est bien défini (via le système de navigation ou dans le code-behind)
- Si nécessaire, ajouter `DataContext="{Binding}"` dans le XAML ou vérifier l'injection dans `App.axaml.cs`

### 2.2 Liaison ItemsSource dans le XAML

**Fichier** : `src/RingGeneral.UI/Views/CompanyHub/CompanyHubView.axaml`

- Identifier où afficher la liste des filiales du joueur (probablement dans un onglet ou section dédiée)
- Ajouter un contrôle (ListBox, DataGrid, ItemsControl) avec `ItemsSource="{Binding PlayerCompanies}"`
- Créer un DataTemplate pour afficher les informations de chaque filiale (nom, objectif, statut Manager, etc.)

### 2.3 Utilisation de RaiseAndSetIfChanged

**Fichier** : `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs`

- Vérifier que toutes les propriétés utilisent `this.RaiseAndSetIfChanged(ref _field, value)`
- S'assurer que `PlayerCompanies` utilise bien cette méthode

## 3. Logique de Recrutement (Commands)

### 3.1 Modification de HireStaffCommand

**Fichier** : `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs` (ou le ViewModel approprié qui gère le recrutement)

- Modifier la signature de `HireStaffCommand` pour accepter un paramètre `string childCompanyId` :
  ```csharp
  public ReactiveCommand<string, Unit> HireStaffCommand { get; }
  ```

- Créer un prédicat `canExecute` qui vérifie :
  - Le joueur a le budget suffisant (vérifier `CurrentCompany.Tresorerie` >= coût du staff)
  - La filiale n'a pas encore de Manager (vérifier via `IChildCompanyStaffRepository` qu'il n'y a pas d'assignation active avec `AssignmentType = DedicatedRotation` et `TimePercentage = 1.0` pour cette ChildCompany)

**Code du prédicat** :

```csharp
var canHireStaff = this.WhenAnyValue(
    x => x.CurrentCompany,
    x => x.PlayerCompanies)
    .Select((tuple) => {
        var (company, companies) = tuple;
        if (company == null) return false;
        // Vérifier budget et absence de Manager pour chaque filiale
        return company.Tresorerie > 0; // + vérification Manager
    });
```

### 3.2 Implémentation de l'assignation Manager

**Fichier** : `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs`

- Injecter `IChildCompanyStaffService` dans le constructeur
- Dans le handler `HireStaffAsync(string childCompanyId)` :
  - Récupérer le StaffMember à recruter (depuis un paramètre ou une sélection)
  - Appeler `_childCompanyStaffService.AssignStaffToChildCompanyAsync()` avec :
    - `staffId` : ID du staff recruté
    - `childCompanyId` : paramètre de la commande
    - `assignmentType` : `StaffAssignmentType.DedicatedRotation`
    - `timePercentage` : `1.0` (100% dédié)
    - `missionObjective` : "Manager" ou similaire
  - Recharger `PlayerCompanies` après l'assignation

## 4. Fix Threading

### 4.1 Utilisation de RxApp.MainThreadScheduler

**Fichier** : `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs`

- Dans `LoadPlayerCompaniesAsync()`, utiliser `RxApp.MainThreadScheduler` pour les mises à jour de `PlayerCompanies` :
  ```csharp
  await Task.Run(async () => {
      var companies = await _childCompanyRepository.GetChildCompaniesByParentIdAsync(playerCompanyId);
      RxApp.MainThreadScheduler.Schedule(() => {
          PlayerCompanies.Clear();
          foreach (var company in companies) {
              PlayerCompanies.Add(company);
          }
      });
  });
  ```

- Appliquer le même pattern pour toutes les autres méthodes `LoadDataAsync` qui mettent à jour des `ObservableCollection`

## Fichiers à modifier

1. `src/RingGeneral.UI/ViewModels/CompanyHub/CompanyHubViewModel.cs`

   - Ajout propriété `PlayerCompanies`
   - Injection `IChildCompanyExtendedRepository` et `IChildCompanyStaffService`
   - Méthode `LoadPlayerCompaniesAsync()`
   - Modification `HireStaffCommand` avec paramètre et prédicat
   - Fix threading avec `RxApp.MainThreadScheduler`

2. `src/RingGeneral.UI/Views/CompanyHub/CompanyHubView.axaml`

   - Ajout contrôle pour afficher `PlayerCompanies`
   - Liaison `ItemsSource="{Binding PlayerCompanies}"`
   - DataTemplate pour afficher les filiales

3. `src/RingGeneral.UI/App.axaml.cs` (si nécessaire)

   - Vérifier l'injection de `IChildCompanyExtendedRepository` et `IChildCompanyStaffService` dans le conteneur DI