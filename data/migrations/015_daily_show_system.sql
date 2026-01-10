-- Migration: 015_daily_show_system.sql
-- Description: Système jour par jour avec shows quotidiens, brands, templates et contrôle child companies
-- Date: 2026-01-08
-- Auteur: Claude (Lead Software Architect)

PRAGMA foreign_keys = ON;

-- =============================================================================
-- TABLE BRANDS (entités séparées pour une compagnie)
-- =============================================================================

CREATE TABLE IF NOT EXISTS Brands (
    BrandId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- =============================================================================
-- EXTENSIONS TABLE SHOWS
-- =============================================================================

-- Ajouter colonne Date à Shows
-- Note: SQLite lance une erreur "duplicate column name" si la colonne existe déjà.
-- Cette erreur est gérée gracieusement par DbInitializer.cs qui marque la migration
-- comme appliquée même si certaines colonnes existent déjà.
ALTER TABLE Shows ADD COLUMN Date TEXT;

-- Ajouter colonne BrandId à Shows
-- Note: SQLite lance une erreur "duplicate column name" si la colonne existe déjà.
-- Cette erreur est gérée gracieusement par DbInitializer.cs qui marque la migration
-- comme appliquée même si certaines colonnes existent déjà.
ALTER TABLE Shows ADD COLUMN BrandId TEXT;

-- =============================================================================
-- TABLE SHOW TEMPLATES (shows récurrents)
-- =============================================================================

CREATE TABLE IF NOT EXISTS ShowTemplates (
    TemplateId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Name TEXT NOT NULL,
    ShowType TEXT NOT NULL,
    RecurrencePattern TEXT NOT NULL CHECK(RecurrencePattern IN ('Weekly', 'BiWeekly', 'Monthly', 'Custom')),
    DayOfWeek INTEGER CHECK(DayOfWeek BETWEEN 0 AND 6), -- 0=Dimanche (Sunday), 1=Lundi (Monday), ..., 6=Samedi (Saturday) - Convention .NET System.DayOfWeek
    DefaultDuration INTEGER NOT NULL,
    DefaultVenueId TEXT,
    DefaultBroadcast TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- =============================================================================
-- TABLE CHILD COMPANY BOOKING CONTROL
-- =============================================================================

CREATE TABLE IF NOT EXISTS ChildCompanyBookingControl (
    ControlId TEXT PRIMARY KEY,
    ChildCompanyId TEXT NOT NULL,
    ControlLevel TEXT NOT NULL CHECK(ControlLevel IN ('Spectator', 'Producer', 'CoBooker', 'Dictator')),
    OwnerCanOverride INTEGER NOT NULL DEFAULT 1,
    AutoScheduleShows INTEGER NOT NULL DEFAULT 0,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ChildCompanyId) REFERENCES ChildCompaniesExtended(ChildCompanyId) ON DELETE CASCADE
);

-- =============================================================================
-- INDEXES POUR PERFORMANCES
-- =============================================================================

-- Index Brands
CREATE INDEX IF NOT EXISTS idx_brands_company ON Brands(CompanyId);

-- Index Shows (Date et BrandId)
CREATE INDEX IF NOT EXISTS idx_shows_date ON Shows(Date);
CREATE INDEX IF NOT EXISTS idx_shows_brand_date ON Shows(BrandId, Date);
CREATE INDEX IF NOT EXISTS idx_shows_company_date ON Shows(CompanyId, Date);

-- Index ShowTemplates
CREATE INDEX IF NOT EXISTS idx_templates_company ON ShowTemplates(CompanyId);
CREATE INDEX IF NOT EXISTS idx_templates_active ON ShowTemplates(CompanyId, IsActive) WHERE IsActive = 1;

-- Index ChildCompanyBookingControl
CREATE INDEX IF NOT EXISTS idx_child_booking_control ON ChildCompanyBookingControl(ChildCompanyId);

-- =============================================================================
-- VÉRIFICATIONS POST-MIGRATION
-- =============================================================================

-- Vérifier que les tables ont été créées
SELECT name FROM sqlite_master WHERE type='table' AND name IN (
    'Brands',
    'ShowTemplates',
    'ChildCompanyBookingControl'
);

-- Vérifier que les colonnes ont été ajoutées à Shows
PRAGMA table_info(Shows);
