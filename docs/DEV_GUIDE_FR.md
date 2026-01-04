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
