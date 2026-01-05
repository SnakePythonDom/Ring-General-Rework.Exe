PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS MatchTypes (
    MatchTypeId TEXT PRIMARY KEY,
    Libelle TEXT NOT NULL,
    Description TEXT,
    Participants INTEGER,
    DureeParDefaut INTEGER
);

CREATE TABLE IF NOT EXISTS SegmentTemplates (
    TemplateId TEXT PRIMARY KEY,
    Libelle TEXT NOT NULL,
    Description TEXT,
    SegmentsJson TEXT NOT NULL
);
