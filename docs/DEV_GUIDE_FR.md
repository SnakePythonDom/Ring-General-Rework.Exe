# Guide de développement (FR)

## Prérequis
- .NET SDK 8.0

## Lancer l'UI
```bash
dotnet run --project src/RingGeneral.UI/RingGeneral.UI.csproj
```

## Tests
```bash
dotnet test
```

## Publier un exécutable
```bash
dotnet publish src/RingGeneral.UI/RingGeneral.UI.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Source de vérité
Le dossier `/specs` est la source de vérité pour :
- navigation
- pages UI
- aide/Codex
- boucles et services

## Ajouter une nouvelle page d'aide
1. Déclarer la page dans `specs/help/pages.fr.json`.
2. Ajouter ou mettre à jour les tooltips dans `specs/help/tooltips.fr.json`.
3. (Optionnel) ajouter un article dans le Codex via `glossaire.fr.json` ou `systems.fr.json`.

## Architecture rapide
- `RingGeneral.Core` : logique de simulation
- `RingGeneral.Data` : accès SQLite
- `RingGeneral.Specs` : modèles et lecteur de specs
- `RingGeneral.UI` : Avalonia + ViewModels

---

## 🎨 Guide de Modding

### Source de vérité
Toutes les données de gameplay et d'UI proviennent des fichiers JSON dans `/specs`.
Ne modifiez pas les IDs existants pour éviter de casser les références.

### Ajouter du contenu via les specs
- **Navigation** : `specs/navigation.fr.json`
- **Pages UI** : `specs/ui/pages/*.fr.json`
- **Aide/Codex** : `specs/help/*.fr.json`

### Ajouter un terme au Codex
1. Ouvrir `specs/help/glossaire.fr.json`.
2. Ajouter une entrée avec `id`, `terme`, `definition`.
3. Optionnel : ajouter `liens` vers d'autres articles.

### Ajouter un système documenté
1. Ouvrir `specs/help/systems.fr.json`.
2. Ajouter un système avec `id`, `titre`, `resume`, `points`.

### Ajouter un tooltip
1. Ouvrir `specs/help/tooltips.fr.json`.
2. Ajouter un `id` clair et un texte court.

### Bonnes pratiques modding
- Texte court, clair et 100% français.
- Garder les IDs stables pour la rétro-compatibilité.
- Vérifier le JSON avec les tests avant de partager.
