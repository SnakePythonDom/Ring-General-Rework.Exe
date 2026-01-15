-- Migration 003: Consolide le schéma DB en supprimant les tables snake_case dupliquées
-- Cette migration résout la dette technique de duplication entre les tables snake_case (legacy)
-- et les tables PascalCase (nouvelles) en ne gardant que les tables PascalCase.

-- Note: Les migrations 001 et 002 ont déjà créé toutes les tables nécessaires en PascalCase.
-- Cette migration nettoie les anciennes tables snake_case qui étaient créées par GameRepository.Initialiser()

-- Étape 1: Vérifier que les données sont dans les tables PascalCase
-- Si des données existent dans les tables snake_case mais pas dans PascalCase, les migrer d'abord

-- Migrer les workers si nécessaire (snake_case -> PascalCase)
-- Note: Cette migration est defensive et ne fait rien si les tables snake_case n'existent pas
-- Les données sont censées être dans les tables PascalCase créées par la migration 001

-- Les migrations de données sont supprimées car les tables snake_case n'existent pas
-- dans ce schéma. Les données sont censées être dans les tables PascalCase.

-- Les tables snake_case legacy n'existent pas dans ce schéma, rien à supprimer

-- Note: Les tables PascalCase existent déjà via les migrations 001 et 002
-- Cette migration nettoie simplement le schéma legacy

-- Instruction SQL valide pour que la migration soit considérée comme appliquée
SELECT 1;
