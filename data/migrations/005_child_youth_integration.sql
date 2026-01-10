-- Phase 2.3 - Migration pour intégrer filiales avec Youth System

-- Ajouter colonne YouthStructureId dans ChildCompaniesExtended pour lier avec YouthStructure
ALTER TABLE ChildCompaniesExtended ADD COLUMN YouthStructureId TEXT NULL;

-- Créer index pour la relation
CREATE INDEX IF NOT EXISTS idx_child_youth_structure ON ChildCompaniesExtended(YouthStructureId);

-- Foreign key vers YouthStructures
-- Note: SQLite ne supporte pas ALTER TABLE ADD CONSTRAINT FOREIGN KEY
-- La contrainte sera gérée au niveau applicatif
