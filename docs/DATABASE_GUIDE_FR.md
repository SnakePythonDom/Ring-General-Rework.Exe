# Guide de remplissage de la base SQLite (FR)

## Philosophie
La base de données SQLite est **la source de vérité** du monde de jeu. Elle décrit l'intégralité de l'écosystème (pays, régions, compagnies, travailleurs, titres, contrats, etc.).
Aucun contenu n'est imposé : **l'utilisateur contrôle tout** et peut créer son univers sur mesure.

## Ordre recommandé de remplissage
1. **Countries / Regions**
2. **Companies**
3. **Titles**
4. **Workers**
5. **WorkerPopularityByRegion**
6. **Contracts**
7. **YouthStructures / YouthTrainees**

Cet ordre limite les références manquantes et évite de devoir corriger des clés étrangères plus tard.

## Règles de cohérence
- **CompanyId requis** pour les entités qui appartiennent à une compagnie (ex. `Companies`, `Contracts`, `Shows`, `Titles`).
- **Workers sans contrat = free agents** : un travailleur existe dans `Workers` sans entrée dans `Contracts`.
- **Popularité initiale** : recommandez une popularité de base par région (ex. 30–60) pour chaque worker.
- **ShowSegments & SegmentParticipants** : créez d'abord les segments, puis les participants.

## Bonnes pratiques pour 200k workers
- **Imports batch** : privilégier les transactions et les lots d'INSERT (`BEGIN; ... COMMIT;`).
- **Indexes activés** : gardez les index d'origine pour accélérer les recherches et la pagination.
- **Éviter les UPDATE massifs fréquents** : préférez des mises à jour par batch et regroupez les modifications par compagnie.
- **LOD (SimLevel / LastSimulatedAt)** : remplissez ces champs si vous gérez des niveaux de simulation.

## Rappels utiles
- Les champs `SimLevel` et `LastSimulatedAt` sont présents pour gérer les simulations à faible détail (LOD).
- Les tables `WorkerPopularityByRegion`, `Contracts`, `Titles`, `Shows` doivent être cohérentes avec vos identifiants.
- Aucune donnée n'est générée par défaut : **c'est à l'utilisateur de remplir** la base.

---

Pour l'import de base existante, consultez : **docs/IMPORT_GUIDE_FR.md**.
