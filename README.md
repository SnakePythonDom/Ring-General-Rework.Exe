# 🎭 Ring General — Wrestling Promotion Manager

**Un jeu de gestion de compagnie de catch professionnel** (style Football Manager × Total Extreme Wrestling)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.0.6-8B44AC)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Architecture](https://img.shields.io/badge/architecture-8.5%2F10-success)](docs/ARCHITECTURE_REVIEW_FR.md)

---

## 📊 État Actuel du Projet

**Version :** Phase 1.9+ — ~50-55% complété
**Dernière mise à jour :** 10 janvier 2026

### ✅ Ce Qui Est Fait

- **Architecture exemplaire** : 23+ repositories spécialisés créés et enregistrés en DI
- **Refactoring majeur réussi** : GameRepository réduit de 75% (3,874 → 977 lignes)
- **Systèmes backstage sophistiqués** : Moral, Rumeurs, Népotisme, Crises, IA Booker/Propriétaire
- **40 attributs de performance** détaillés (In-Ring, Entertainment, Story, Mental)
- **25+ profils de personnalité** automatiques (style Football Manager)
- **🆕 Système d'Auto-Booking IA** : Le Booker génère automatiquement des cartes complètes 🎯
- **🆕 Flux Show Day complet** : Simulation de bout en bout avec impacts automatiques
- **48+ ViewModels** créés avec navigation complète
- **Base de données SQLite** avec import automatique BAKI
- **Compilation réussie** : Solution complète avec 0 erreurs, 1 avertissement mineur

### ⏳ En Cours

- Interface utilisateur (13+ vues créées, autres en développement)
- Boucle de jeu hebdomadaire (éléments séparés, orchestration en cours)
- Composants UI réutilisables
- Documentation des nouveaux systèmes backstage

---

## 🚀 Démarrage Rapide

### Prérequis

- **.NET 8.0 SDK** ou ultérieur
- **Windows/Linux/macOS** (Avalonia cross-platform)
- **Visual Studio 2022+** / **Rider** / **VS Code** recommandé

### Installation

```bash
# Cloner le repository
git clone https://github.com/SnakePythonDom/Ring-General-Rework.Exe.git
cd Ring-General-Rework.Exe

# Restaurer les dépendances
dotnet restore RingGeneral.sln

# Lancer l'application
dotnet run --project src/RingGeneral.UI/RingGeneral.UI.csproj
```

**Pour plus de détails :** Consultez le [Guide de démarrage rapide](docs/QUICK_START_GUIDE.md)

---

## 📚 Documentation

### 📌 Documents de Référence

| Document | Description |
|----------|-------------|
| **[docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md)** | ⭐ État consolidé du projet (source de vérité unique) |
| **[docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md)** | Analyse architecture (v2.3, Note: 8.5/10) |
| **[docs/ROADMAP_MISE_A_JOUR.md](docs/ROADMAP_MISE_A_JOUR.md)** | Plan de développement (Phases 1-5, Release Avril 2026) |
| **[docs/INDEX.md](docs/INDEX.md)** | Index complet de toute la documentation |

### 📖 Guides Utilisateur

- **[docs/QUICK_START_GUIDE.md](docs/QUICK_START_GUIDE.md)** — Guide de démarrage rapide
- **[docs/DEV_GUIDE_FR.md](docs/DEV_GUIDE_FR.md)** — Guide de développement & modding
- **[docs/DATABASE_GUIDE_FR.md](docs/DATABASE_GUIDE_FR.md)** — Guide de la base de données SQLite
- **[docs/IMPORT_GUIDE_FR.md](docs/IMPORT_GUIDE_FR.md)** — Import de bases de données

---

## 🏗️ Architecture & Technologies

### Stack Technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Framework** | .NET | 8.0 LTS |
| **Langage** | C# | 12 |
| **UI Framework** | Avalonia | 11.0.6 |
| **Reactive UI** | ReactiveUI | (via Avalonia) |
| **Base de données** | SQLite | 8.0.0 |

### Architecture

```
┌─────────────────────────────────────┐
│  UI (Avalonia MVVM)                 │ RingGeneral.UI
├─────────────────────────────────────┤
│  Business Logic (Domain Services)   │ RingGeneral.Core
├─────────────────────────────────────┤
│  Data Access (23+ Repositories)     │ RingGeneral.Data
├─────────────────────────────────────┤
│  Configuration (JSON Specs)         │ RingGeneral.Specs
└─────────────────────────────────────┘
```

**Points forts :**
- ✅ 23+ repositories spécialisés (modulaire et maintenable)
- ✅ Immutable records (C# 12)
- ✅ Dependency Injection
- ✅ Clean architecture (pas de dépendances circulaires)
- ✅ Configuration data-driven (JSON specs)

**Pour plus de détails :** Consultez l'[Analyse d'architecture](docs/ARCHITECTURE_REVIEW_FR.md)

---

## 📁 Structure du Projet

```
Ring-General-Rework.Exe/
├── src/                    # Code source C# (.NET 8.0)
│   ├── RingGeneral.UI/     # Interface Avalonia (95 fichiers)
│   ├── RingGeneral.Core/   # Logique métier (124 fichiers)
│   ├── RingGeneral.Data/   # Accès données (45 fichiers)
│   ├── RingGeneral.Specs/  # Configuration JSON
│   └── RingGeneral.Tools.* # Outils CLI
├── specs/                  # 3 fichiers JSON de configuration
├── docs/                   # Documentation complète (10 docs actifs)
├── data/                   # Assets & base de test (BAKI1.1.db)
├── tests/                  # Projet vide
└── _archived_files/        # Archives (30+ docs obsolètes)
```

---

## 🎯 Vision Produit

**Ring General** est un jeu de gestion de compagnie de catch professionnel combinant :
- La profondeur de **Football Manager** (attributs détaillés, personnalité, moral)
- La complexité de **Total Extreme Wrestling** (booking, storylines, heat)
- Une interface moderne inspirée de **Football Manager 2026**

### Boucle de Jeu Hebdomadaire

1. **Inbox** — Emails, incidents, demandes, offres
2. **Scouting** — Rapports, découverte de talents
3. **Négociations** — Contrats, partenariats, diffusion
4. **Préparation Show** — Booking, scripts, consignes
5. **Show** — Exécution en direct
6. **Résultats** — Ratings, heat, blessures, finances
7. **Gestion** — Staff, formation, médical, discipline

### Systèmes Clés

- **Booking** : Construction de cartes, validation, templates
- **🆕 Auto-Booking IA** : Génération automatique de cartes complètes par le Booker
  - 5 styles de produit : Hardcore, Puroresu, Technical, Entertainment, Balanced
  - Respect des préférences du Booker (Underdog, Veteran, Fast Rise, Slow Burn)
  - Utilisation du système de mémoire pour décisions cohérentes
  - Contraintes Owner personnalisables (budget, workers interdits, fatigue)
- **Storylines** : Feuds, heat progression, phases (BUILD/PEAK/BLOWOFF)
- **Attributs** : 40 attributs de performance (4 dimensions)
- **Personnalité** : 25+ profils automatiques (FM-like)
- **Backstage** : Moral, rumeurs, népotisme, crises
- **Simulation** : Engine sophistiqué de calcul de ratings
- **🆕 Show Day** : Flux complet de simulation avec impacts automatiques (finances, titres, blessures, moral)
- **IA** : Booker et Propriétaire avec décisions automatiques

---

## 🗺️ Roadmap

| Phase | Description | Status | Cible |
|-------|-------------|--------|-------|
| **Phase 0** | Infrastructure & Architecture | ✅ **Complet** | - |
| **Phase 1** | Fondations UI/UX & Gameplay de base | ✅ **Complet** | - |
| **Phase 1.5** | Systèmes Personnalité & Attributs | ✅ **Complet** | - |
| **Phase 1.9** | 🆕 Flux Show Day & Auto-Booking | ✅ **Complet** | - |
| **Phase 2** | Intégration Données & Features avancées | ⚠️ **En cours** | Jan 2026 |
| **Phase 3** | Fonctionnalités Métier complètes | ⚠️ **En cours** (15%) | Jan 2026 |
| **Phase 4** | Performance & Optimisation | ❌ **À démarrer** | Mar 2026 |
| **Phase 5** | QA & Polish | ❌ **À démarrer** | Avr 2026 |

**Roadmap complète :** [docs/ROADMAP_MISE_A_JOUR.md](docs/ROADMAP_MISE_A_JOUR.md)

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Consultez :
- **[docs/DEV_GUIDE_FR.md](docs/DEV_GUIDE_FR.md)** pour le guide de développement
- **[docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md)** pour comprendre l'architecture

### Standards de Code

- C# 12 avec nullable reference types
- Immutable records pour les modèles du domaine
- MVVM avec ReactiveUI
- Naming conventions en français (cohérent avec le projet)

---

## 📄 License

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

---

## 🔗 Liens Utiles

- **Documentation complète :** [docs/INDEX.md](docs/INDEX.md)
- **État du projet :** [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md)
- **Architecture :** [docs/ARCHITECTURE_REVIEW_FR.md](docs/ARCHITECTURE_REVIEW_FR.md)
- **Rapport de vérification (8 jan 2026) :** [docs/RAPPORT_VERIFICATION_ARCHITECTURE_2026-01-08.md](docs/RAPPORT_VERIFICATION_ARCHITECTURE_2026-01-08.md)

---

**Développé avec ❤️ en C# et Avalonia**

*Ring General est un projet personnel de simulation de gestion de catch. Il n'est pas affilié à WWE, AEW, NJPW ou toute autre organisation de catch professionnel.*
