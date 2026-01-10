-- Phase 2.3 - Migration pour intégrer filiales avec Youth System

-- Créer la table ChildCompaniesExtended si elle n'existe pas encore
-- (Cette table sera créée complètement dans la migration 014, mais on en a besoin ici)
CREATE TABLE IF NOT EXISTS ChildCompaniesExtended (
    ChildCompanyId TEXT PRIMARY KEY,
    ParentCompanyId TEXT NOT NULL,
    Objective TEXT NOT NULL DEFAULT 'Development' CHECK(Objective IN ('Entertainment', 'Niche', 'Independence', 'Development')),
    HasFullAutonomy INTEGER NOT NULL DEFAULT 0 CHECK(HasFullAutonomy IN (0, 1)),
    AssignedBookerId TEXT NULL,
    IsLaboratory INTEGER NOT NULL DEFAULT 0 CHECK(IsLaboratory IN (0, 1)),
    TestStyle TEXT NULL,
    NicheType TEXT CHECK(NicheType IN ('Hardcore', 'Technical', 'Lucha', 'StrongStyle', 'Entertainment')),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK(IsActive IN (0, 1)),
    YouthStructureId TEXT NULL,
    FOREIGN KEY (ParentCompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedBookerId) REFERENCES Bookers(BookerId) ON DELETE SET NULL,
    FOREIGN KEY (ChildCompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE
);

-- Ajouter colonne YouthStructureId si elle n'existe pas déjà (pour les bases existantes)
-- Note: SQLite ne supporte pas IF NOT EXISTS pour ALTER TABLE ADD COLUMN
-- On utilise une approche avec gestion d'erreur dans le code applicatif
-- La colonne est déjà incluse dans le CREATE TABLE ci-dessus pour les nouvelles tables

-- Créer index pour la relation
CREATE INDEX IF NOT EXISTS idx_child_youth_structure ON ChildCompaniesExtended(YouthStructureId);

-- Foreign key vers YouthStructures
-- Note: SQLite ne supporte pas ALTER TABLE ADD CONSTRAINT FOREIGN KEY
-- La contrainte sera gérée au niveau applicatif
