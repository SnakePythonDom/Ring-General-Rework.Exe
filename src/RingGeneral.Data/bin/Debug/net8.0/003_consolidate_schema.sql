-- Migration 003: Consolide le schéma DB en supprimant les tables snake_case dupliquées
-- Cette migration résout la dette technique de duplication entre les tables snake_case (legacy)
-- et les tables PascalCase (nouvelles) en ne gardant que les tables PascalCase.

-- Note: Les migrations 001 et 002 ont déjà créé toutes les tables nécessaires en PascalCase.
-- Cette migration nettoie les anciennes tables snake_case qui étaient créées par GameRepository.Initialiser()

-- Étape 1: Vérifier que les données sont dans les tables PascalCase
-- Si des données existent dans les tables snake_case mais pas dans PascalCase, les migrer d'abord

-- Migrer les workers si nécessaire (snake_case -> PascalCase)
INSERT OR IGNORE INTO Workers (WorkerId, Name, FirstName, LastName, CompanyId, Nationality, InRing, Entertainment, Story, Popularity, Fatigue, InjuryStatus, Momentum, RoleTv)
SELECT
    worker_id,
    COALESCE(nom || ' ' || prenom, nom, prenom, 'Unknown') as Name,
    prenom as FirstName,
    nom as LastName,
    company_id,
    COALESCE(Nationality, 'US') as Nationality,
    in_ring,
    entertainment,
    story,
    popularite,
    fatigue,
    blessure,
    momentum,
    role_tv
FROM workers
WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='workers');

-- Migrer les companies si nécessaire
INSERT OR IGNORE INTO Companies (CompanyId, Name, RegionId, Prestige, Treasury, AverageAudience, Reach)
SELECT
    company_id,
    nom,
    region as RegionId,
    prestige,
    tresorerie,
    audience_moyenne,
    reach
FROM companies
WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='companies');

-- Migrer les titles si nécessaire
INSERT OR IGNORE INTO Titles (TitleId, Name, Prestige, ChampionWorkerId, CompanyId)
SELECT
    title_id,
    nom,
    prestige,
    detenteur_id,
    company_id
FROM titles
WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='titles');

-- Étape 2: Supprimer les tables snake_case legacy
DROP TABLE IF EXISTS workers;
DROP TABLE IF EXISTS companies;
DROP TABLE IF EXISTS titles;
DROP TABLE IF EXISTS storylines;
DROP TABLE IF EXISTS storyline_participants;
DROP TABLE IF EXISTS shows;
DROP TABLE IF EXISTS tv_deals;
DROP TABLE IF EXISTS segments;
DROP TABLE IF EXISTS chimies;
DROP TABLE IF EXISTS finances;
DROP TABLE IF EXISTS show_history;
DROP TABLE IF EXISTS audience_history;
DROP TABLE IF EXISTS segment_history;
DROP TABLE IF EXISTS backstage_incidents;
DROP TABLE IF EXISTS disciplinary_actions;
DROP TABLE IF EXISTS morale_history;
DROP TABLE IF EXISTS contracts;
DROP TABLE IF EXISTS contract_offers;
DROP TABLE IF EXISTS contract_clauses;
DROP TABLE IF EXISTS negotiation_state;
DROP TABLE IF EXISTS youth_structures;
DROP TABLE IF EXISTS youth_trainees;
DROP TABLE IF EXISTS youth_programs;
DROP TABLE IF EXISTS youth_staff_assignments;
DROP TABLE IF EXISTS youth_generation_state;
DROP TABLE IF EXISTS worker_attributes;
DROP TABLE IF EXISTS worker_generation_counters;
DROP TABLE IF EXISTS worker_generation_events;
DROP TABLE IF EXISTS scout_reports;
DROP TABLE IF EXISTS scout_missions;
DROP TABLE IF EXISTS game_settings;
DROP TABLE IF EXISTS ui_table_settings;
DROP TABLE IF EXISTS popularity_regionale;
DROP TABLE IF EXISTS match_types;
DROP TABLE IF EXISTS segment_templates;

-- Note: Les tables PascalCase existent déjà via les migrations 001 et 002
-- Cette migration nettoie simplement le schéma legacy
