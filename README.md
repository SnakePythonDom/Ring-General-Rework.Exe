# Ring General Rework — Vision Produit (FR)

## Source de vérité
Toutes les spécifications officielles (UI, modèles, boucle hebdo, booking, impacts, services) sont désormais la source de vérité dans le dossier `/specs`.
Merci de vous référer aux fichiers JSON en français de ce dossier pour toute mise à jour.

## Documentation
- [Guide du jeu (FR)](docs/JEU_GUIDE_FR.md)
- [Guide du modding (FR)](docs/MODDING_GUIDE_FR.md)
- [Guide de développement (FR)](docs/DEV_GUIDE_FR.md)

## 1) Vision produit (TEW/PWS + FM26)

> Les spécifications détaillées (navigation, onglets par page, attributs, Youth, boucle hebdo)
> sont disponibles dans `specs/` en format JSON pour servir de base de données source.

### Boucle de jeu hebdomadaire
1. **Boîte de réception** (emails, incidents, demandes, news, offers)
2. **Scouting / Observations** (rapports, talents inconnus, shortlist)
3. **Négociations** (contrats catcheurs + staff + partenariats + diffusion)
4. **Préparation show** (planning, booking, scripts, consignes, agents)
5. **Show** (exécution segments)
6. **Résultats** (notes, storyline/heat, momentum, blessures, finances, popularité régionale)
7. **Gestion** (staff, training/youth, médical, discipline, marketing)
8. **Répétition**

### “Le joueur vit dans” (hub pages)
- Tableau de bord
- Boîte de réception / Actualités
- Effectif
- Booking
- Storylines
- Titres
- Finances
- Diffusion (TV/Streaming)
- Youth (Wrestling School / Dojo / PC / Academy / Developmental)
- Scouting

## 2) Navigation complète (Sidebar) + nouvelles pages

**A. Accueil**
1. Tableau de bord
2. Boîte de réception
3. Actualités (flux global)

**B. Compagnie**
4. Ma Compagnie
5. Règlement & Discipline
6. Marketing & Merch
7. Relations & Partenariats

**C. Effectif**
8. Catcheurs
9. Staff
10. Équipes & Stables
11. Médical
12. Contrats

**D. Créatif**
13. Booking
14. Storylines
15. Titres
16. Gimmicks & Personnages (alignement, gimmick, rôle TV)
17. Segments / Bibliothèque (angles, promos, interviews, cinématiques)
18. Match Types (stipulations + règles)
19. Plans créatifs (objectifs, push, roadmaps internes)

**E. Événements**
20. Calendrier
21. Shows
22. Tournées & Lieux

**F. Scout & Développement**
23. Scouting
24. Youth (Dojo/School/PC/Academy/Dev Territory)
25. Tryouts / Sélections
26. Prêts / Excursions (NOAH-like, etc.)

**G. Business**
27. Finances
28. Diffusion (TV/Streaming)
29. Sponsors
30. Billetterie

**H. Base de données & Options**
31. Éditeur Base de données
32. Import / Export (packs)
33. Paramètres
34. Aide / Encyclopédie (Codex)

## 3) Onglets par page (format FM26 : liste + panneau détails)

### 3.1 Tableau de bord (hub)
**Onglets**
- Aperçu
- Prochain show
- Alertes
- Objectifs
- Performances

**Widgets clés**
- Résumé semaine (news + blessures + contrats + finances)
- Storylines chaudes (Heat) / Momentum top & bottom
- Prochain show (statut booking : % complet)
- Alertes (fatigue, moral bas, contrat expire, injure)

**Boutons**
- Passer à la semaine suivante
- Aller au booking
- Lire inbox
- Action rapide (menu : signer / tryout / créer storyline / book main event)

### 3.2 Boîte de réception / Actualités
**Onglets**
- Messages
- Contrats
- Médical
- Scouting
- Diffusion
- Incidents

**Boutons**
- Répondre (choix : accepter / refuser / négocier)
- Marquer comme lu
- Filtrer
- Ouvrir la fiche liée (worker/show/deal)

### 3.3 Catcheurs (Effectif)
**Onglets**
- Liste
- Disponibilités
- Forme & fatigue
- Popularité
- Rôles TV
- Shortlist

**Boutons**
- Signer
- Proposer contrat
- Libérer
- Assigner rôle (Main Event, Upper Midcard…)
- Ajouter à storyline
- Comparer (2–3 profils)

### 3.4 Staff (agents, bookers, road agents, producteurs, coachs…)
**Onglets**
- Liste
- Rôles
- Compétences
- Paie
- Affectations (qui produit quel match, qui coach qui)
- Shortlist

**Boutons**
- Recruter staff
- Assigner à Youth
- Changer rôle
- Renouveler

### 3.5 Profil unique (Catcheur / Staff / Trainee)
**Onglets**
- Aperçu
- Attributs
- Contrat
- Santé
- Historique
- Relations
- Créatif (gimmick, alignement, catchphrase, notes de booking)
- Développement (plans d’entraînement / évolution)

**Boutons**
- Booker (ajoute au show en cours)
- Proposer un contrat
- Définir push
- Envoyer en Youth / excursion
- Repos / médical
- Notes internes (privées)

---

## 4) Monde vivant (LOD) & performances (Phase 7)

### Activer / régler le LOD mondial
Le niveau de détail mondial est configurable via `specs/models/world-sim.fr.json` :
- `nbCompagniesLod0` : nombre de compagnies simulées en **détaillé** (hors joueur).
- `budgetMsParTick` : budget de temps par tick hebdomadaire.
- `frequenceLod1Semaines` : fréquence des simulations **résumées**.
- `frequenceLod2Semaines` : fréquence des simulations **statistiques**.
- `seed` : graine pour le déterminisme (mêmes réglages = mêmes résultats).

### Générer un monde de test (seed)
Pour un monde reproductible :
1. Choisir un `seed` dans `specs/models/world-sim.fr.json`.
2. Lancer une nouvelle partie et avancer plusieurs semaines : les résultats mondiaux doivent rester cohérents.

### Conseils perfs (200k workers)
- Éviter tout chargement global de workers (requêtes ciblées uniquement).
- Utiliser les index SQL sur `workers`, `contracts` et `titles`.
- Surveiller les logs `[WorldSim]` pour le temps de simulation hebdo.

### Publication .exe
Commande recommandée :
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

### 3.6 Booking (cœur de jeu)
**Onglets**
- Carte du show
- Segments
- Matchs
- Angles
- Timing
- Notes & scripts
- Validation

**Boutons**
- Ajouter match
- Ajouter angle
- Auto-remplir
- Optimiser rythme (répartition intensité/promo)
- Vérifier cohérence (warnings)
- Lancer le show
- Sauver modèle

### 3.7 Storylines
**Onglets**
- Toutes
- Actives
- En montée
- En baisse
- Archivées
- Plans (feuds à préparer)

---

## Lancer le projet en local

### Prérequis
- .NET SDK 8.0

### Démarrage (UI Avalonia)
```bash
dotnet run --project src/RingGeneral.UI/RingGeneral.UI.csproj
```

Une base SQLite `ringgeneral.db` est créée dans le dossier courant lors du premier lancement.

### Publier un exécutable .exe
```bash
dotnet publish src/RingGeneral.UI/RingGeneral.UI.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

L'exécutable se trouve ensuite dans :
`src/RingGeneral.UI/bin/Release/net8.0/win-x64/publish/`

## Ajouter du contenu via /specs
- **Pages UI** : `specs/ui/pages/*.fr.json`
- **Segments** : `specs/booking/segment-types.fr.json`
- **Aide/Codex** : `specs/help/*.fr.json`

## Scénario de test manuel (boucle jouable)
1. Lancer l'application.
2. Vérifier que la section **Booking** affiche une carte de show avec segments.
3. Cliquer sur **Simuler le show**.
4. Vérifier que la section **Résultats** affiche une note globale, des notes de segments et des impacts (fatigue/momentum/popularité).
5. Vérifier que des entrées sont enregistrées dans l'historique (table `show_history` et `segment_history` dans `ringgeneral.db`).
6. Cliquer sur **Semaine suivante**.
7. Vérifier que l'**Inbox** se remplit avec 1–3 news, éventuellement un rapport scouting, et des alertes de contrats.
8. Relancer une simulation pour vérifier que la fatigue est réduite et que les impacts continuent de s'appliquer.

**Boutons**
- Créer storyline
- Ajouter participants
- Lier segment
- Définir objectif (titre, turn, blow-off)
- Clôturer

### 3.8 Titres
**Onglets**
- Titres
- Champions
- Historique règnes
- Classement / Contenders
- Règles (défenses, divisions)
- Statistiques

**Boutons**
- Créer titre
- Couronner
- Vacant
- Ajouter match pour le titre
- Définir #1 contender

### 3.9 Shows
**Onglets**
- Calendrier
- À venir
- Passés
- Modèles
- Lieux
- Production

**Boutons**
- Créer show
- Dupliquer
- Assigner diffusion
- Fixer lieu
- Aller au booking

### 3.10 Finances (format FM)
**Onglets**
- Résumé
- Revenus
- Dépenses
- Contrats
- Prévisions
- Transactions

**Boutons**
- Ajuster billetterie
- Ajuster merch
- Exporter rapport
- Fixer budget Youth

### 3.11 Diffusion (TV/Streaming)
**Onglets**
- Partenaires
- Contrats actuels
- Négociations
- Audience
- Production & contraintes

**Boutons**
- Négocier un deal
- Renouveler
- Résilier
- Assigner un show
- Paramétrer runtime

## 4) Youth (Dojo → Système “Jeunes & Développement”)

### Concept
Youth devient un système unique qui peut représenter :
- École de catch
- Dojo
- Performance Center
- Academy
- Territoire de développement (developmental territory)

Chaque structure Youth a :
- un type
- une philosophie (lucha, puro, entertainment, hybride)
- un niveau d’infrastructure
- des coachs / staff
- des programmes
- une cohorte d’élèves (trainees)
- des shows internes (optionnel)
- un pipeline vers le roster principal

### Page Youth
**Onglets**
- Structures
- Élèves
- Programmes
- Staff
- Évaluations
- Graduations
- Excursions / Prêts
- Budget

**Boutons**
- Créer une structure Youth
- Recruter un élève
- Organiser tryout
- Assigner coach
- Changer programme
- Promouvoir (graduation)
- Envoyer en excursion
- Libérer

### Logique TEW + FM
- Les élèves ont une **Progression** (potentiel + courbe)
- Les coachs influencent la vitesse et le type de progression
- Les élèves peuvent être “prêts” au roster via un seuil (ex : In-Ring + Psychologie + Charisme minimum)

## 5) Système d’attributs “mix FM + TEW”

### 5.1 Format d’affichage FM
- Attributs en valeurs 1–20 (lisible FM)
- Derrière, un “score” TEW-like 0–100 si besoin (conversion automatique)
- Affichage : barres + chiffre + tooltip
- Couleurs : faible / moyen / fort

### 5.2 Attributs universels (pour tout le monde)
**Profil & mental**
- Professionnalisme
- Ambition
- Loyauté
- Discipline
- Adaptabilité
- Résilience
- Esprit d’équipe
- Leadership
- Fiabilité (respect des consignes)

**Condition**
- Athlétisme
- Endurance
- Récupération
- Tolérance aux blessures

### 5.3 Module “Catcheur” (In-Ring TEW + FM)
**A. Performance catch**
- Technique
- Psychologie de match (ring psychology)
- Timing
- Vente (selling)
- Sécurité (évite blessures botch)
- Intensité
- Rythme
- Polyvalence (s’adapte aux styles)

**B. Style (axes)**
- Aérien
- Brawler
- Puro / Strong style
- Lucha
- Hardcore
- Catch au sol (grappling)

**C. Divertissement**
- Charisme
- Micro
- Jeu d’acteur
- Présence TV
- Connexion public (babyface/heel adaptable)
- Créativité (impro angles)

**D. Star power (FM-like)**
- Notoriété (globale)
- Popularité régionale (table régions)
- Momentum
- Aura (effet main event)

### 5.4 Module “Staff” (agents, bookers, coachs…)
**A. Backstage/production**
- Agenting (qualité de construction match)
- Production TV (rythme, transitions)
- Créatif (angles, storylines)
- Organisation
- Gestion des talents (conflicts, morale)

**B. Coaching / formation**
- Coaching technique
- Coaching psychologie
- Coaching promo
- Développement physique
- Détection potentiel (talent ID)

**C. Business**
- Négociation
- Gestion budget
- Marketing
- Relations diffuseurs

### 5.5 Module “Trainee” (élève Youth)
- Potentiel (1–20) : plafond probable
- Vitesse de progression (1–20)
- Discipline d’apprentissage (1–20)
- Spécialité naturelle (ex : aérien / promo / technique)
- Risque de stagnation (faible/moyen/élevé)

## 6) Roadmap détaillée (version FR)

### Étape 1 — Socle “Jeu en français” + Shell FM
**Objectif**
- Tout le jeu en FR : libellés, formats date/€, tooltips, messages.

**UI**
- Sidebar + Topbar + recherche globale.
- Glossaire (Codex) : définitions (Heat, Momentum, Psychologie…)

**Boutons**
- Passer à la semaine suivante
- Recherche
- Actions rapides

**Règles**
- Aucune mécanique profonde, juste structure + navigation.

### Étape 2 — UI Kit FM26 (composants + tableaux)
**Objectif**
- Tables triables, filtres, colonne custom, panneaux détails.

**Inclure**
- Empty state FR (“Aucun catcheur…”) + boutons d’action.

### Étape 3 — Sauvegarde/chargement + Base SQLite
**Objectif**
- Slots de sauvegarde + import/export pack.

**Boutons**
- Nouvelle partie
- Charger
- Exporter base
- Importer base

### Étape 4 — Base de données jeu (compagnie, workers, shows, titres)
**Objectif**
- Structures de données complètes, même si gameplay encore simple.

### Étape 5 — Fiches universelles (Catcheur/Staff/Trainee) + attributs
**Objectif**
- Fiche profil avec onglets + attributs 1–20 + tooltips.

**UI**
- Profil “universel” (même écran, rôles différents)

**Boutons**
- Modifier
- Ajouter notes
- Shortlist

### Étape 6 — Contrats v1 (catcheur + staff)
**Objectif**
- Négociation style FM : offre, contre-offre, clauses.

**Boutons**
- Proposer
- Contre-proposer
- Accepter / Refuser
- Libérer

### Étape 7 — Inbox & News (squelette boucle hebdo)
**Objectif**
- Semaine génère des messages : contrats, blessures, scouting, diffusion.

### Étape 8 — Scouting v1 (rapports & shortlist)
**Objectif**
- Découverte progressive des talents (inconnu → partiel → complet)

**Onglets Scouting**
- Régions / Rapports / Shortlist / Missions

### Étape 9 — Youth v1 (structures + trainees)
**Objectif**
- Créer une école/dojo/PC/academy/dev territory.
- Générer des élèves, progression basique.

### Étape 10 — Shows + Calendrier
**Objectif**
- Créer show, assigner runtime, lieu, diffusion.

### Étape 11 — Booking v1 (match/angle + validation)
**Objectif**
- Construire carte + segments + scripts + warnings.

### Étape 12 — Simulation show + ratings
**Objectif**
- Notes segment + note show + recap FM.

### Étape 13 — Storylines + Heat + Momentum
**Objectif**
- Storyline progresse si segments liés, impact sur notes.

### Étape 14 — Titres + historique + contenders
**Objectif**
- Gestion titre complète “TEW-like”.

### Étape 15 — Finances + billetterie + merch + paie
**Objectif**
- Boucle économique stable.

### Étape 16 — Diffusion (TV/Streaming) + audience
**Objectif**
- Contrats, reach, contraintes, revenus.

### Étape 17 — Blessures/Fatigue + médical
**Objectif**
- Gestion condition, repos, risques.

### Étape 18 — Profondeur backstage (discipline, morale, conflits)
**Objectif**
- Incidents inbox, pénalités, sanctions.

### Étape 19 — Bibliothèque segments + match types + templates
**Objectif**
- Modèles de shows, patterns de booking, auto-remplissage.

### Étape 20 — Modding + import/export (packs)
**Objectif**
- Éditeur DB solide, validation, réparations liens.

### Étape 21 — QA & équilibrage (TEW handbook vibe)
**Objectif**
- Stabiliser : Youth, contrats, persistance, scoring.

### Étape 22 — Packaging .exe + options + performance
**Objectif**
- Installateur, logs, crash reports, perfs UI.

## 7) Indispensables FM-like
- Recherche globale (catcheur/staff/titre/storyline/show)
- Favoris / Épingler
- Raccourcis clavier (Esc ferme modal, Ctrl+F recherche)
- Comparateur (2 profils côte à côte)
- Notes internes (privées, datées)
- Tooltips riches (expliquent l’effet d’un attribut)
- Alertes (fatigue, fin de contrat, blessure)
