-- Migration: 016_migrate_shows_week_to_date.sql
-- Description: Script de migration pour convertir les shows existants de Week vers Date
-- Date: 2026-01-08
-- Auteur: Claude (Lead Software Architect)

PRAGMA foreign_keys = ON;

-- =============================================================================
-- MIGRATION DES SHOWS EXISTANTS
-- =============================================================================

-- Convertir les shows qui ont Week mais pas Date
-- Hypothèse: Week 1 = 2024-01-01, chaque semaine = +7 jours
UPDATE Shows
SET Date = date('2024-01-01', '+' || ((COALESCE(Week, 1) - 1) * 7) || ' days')
WHERE Date IS NULL OR Date = ''
  AND Week IS NOT NULL;

-- =============================================================================
-- VÉRIFICATIONS POST-MIGRATION
-- =============================================================================

-- Compter les shows avec Date
SELECT COUNT(*) as ShowsAvecDate FROM Shows WHERE Date IS NOT NULL AND Date != '';

-- Compter les shows sans Date (nécessitent migration manuelle)
SELECT COUNT(*) as ShowsSansDate FROM Shows WHERE Date IS NULL OR Date = '';

-- Afficher quelques exemples de shows migrés
SELECT ShowId, Name, Week, Date, ShowType, Status
FROM Shows
WHERE Date IS NOT NULL AND Date != ''
ORDER BY Date ASC
LIMIT 10;
