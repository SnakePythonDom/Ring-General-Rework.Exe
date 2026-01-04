# Guide de modding (FR)

## Source de vérité
Toutes les données de gameplay et d'UI proviennent des fichiers JSON dans `/specs`.
Ne modifiez pas les IDs existants pour éviter de casser les références.

## Ajouter du contenu via les specs
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

## Bonnes pratiques
- Texte court, clair et 100% français.
- Garder les IDs stables pour la rétro-compatibilité.
- Vérifier le JSON avec les tests avant de partager.
