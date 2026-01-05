# Performance & Packaging (FR)

## Objectifs
- Publier un exécutable Windows (win-x64) via GitHub Actions.
- Documenter les logs de crash en FR (diagnostic simplifié).
- Définir des options de performance (LOD, fréquence des news, cap génération, cache).
- Auditer les perfs UI (virtualisation).

## Checklist — Packaging .exe (win-x64)
- [ ] Ajouter/valider le workflow GitHub Actions `build-windows.yml`.
- [ ] Produire un artefact `RingGeneral-win-x64` téléchargeable.
- [ ] Vérifier la commande de publish locale :
  ```bash
  dotnet publish src/RingGeneral.UI/RingGeneral.UI.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
  ```
- [ ] Documenter l’emplacement de l’artefact en release (notes de release ou README).

## Checklist — Logs de crash (FR)
Objectif : fournir un journal clair, daté, en français, exploitable par l’équipe support.
- [ ] Emplacement standardisé (ex: `%AppData%/RingGeneral/logs/`).
- [ ] Message d’erreur en FR avec contexte (version, scène, seed, paramètres).
- [ ] Rotations des logs (limite de taille / nombre de fichiers).
- [ ] Capture des exceptions non gérées UI + Core.
- [ ] Bouton « Ouvrir le dossier de logs » dans l’UI (optionnel).

## Checklist — Options de performance
Options exposées dans l’UI (avec valeurs par défaut + tooltips).
- [ ] **LOD** (niveau de détail)
  - [ ] Bas / Moyen / Élevé + description claire.
- [ ] **Fréquence des news**
  - [ ] Curseur : Off / Rare / Normal / Fréquent.
- [ ] **Cap génération**
  - [ ] Limite max de générations par frame/tick.
- [ ] **Cache**
  - [ ] Taille max du cache + bouton vider cache.

## Checklist — Audit perfs UI (virtualisation)
- [ ] Identifier les listes/collections longues (ex: tables de données, logs, timelines).
- [ ] Vérifier la virtualisation Avalonia (ItemsRepeater, DataGrid, etc.).
- [ ] Mesurer le temps de rendu initial et le scroll.
- [ ] Réduire la sur-rendu (templates, bindings coûteux, conversions).
- [ ] Documenter les résultats + actions dans un ticket.
