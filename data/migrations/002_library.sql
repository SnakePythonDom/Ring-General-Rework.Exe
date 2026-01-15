PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS MatchTypes (
    MatchTypeId TEXT PRIMARY KEY,
    Libelle TEXT NOT NULL,
    Description TEXT,
    Participants INTEGER,
    DureeParDefaut INTEGER,
    Actif INTEGER NOT NULL DEFAULT 1,
    Ordre INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS SegmentTemplates (
    TemplateId TEXT PRIMARY KEY,
    Libelle TEXT NOT NULL,
    Description TEXT,
    TypeSegment TEXT NOT NULL,
    DureeMinutes INTEGER NOT NULL,
    EstMainEvent INTEGER NOT NULL DEFAULT 0,
    Intensite INTEGER NOT NULL DEFAULT 50,
    MatchTypeId TEXT,
    SegmentsJson TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (MatchTypeId) REFERENCES MatchTypes(MatchTypeId)
);

CREATE INDEX IF NOT EXISTS idx_match_types_active ON MatchTypes(Actif);
CREATE INDEX IF NOT EXISTS idx_segment_templates_type ON SegmentTemplates(TypeSegment);
