PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS MatchTypes (
    MatchTypeId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS SegmentTemplates (
    SegmentTemplateId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    SegmentType TEXT NOT NULL,
    DurationMinutes INTEGER NOT NULL,
    IsMainEvent INTEGER NOT NULL DEFAULT 0,
    Intensity INTEGER NOT NULL DEFAULT 50,
    MatchTypeId TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (MatchTypeId) REFERENCES MatchTypes(MatchTypeId)
);

CREATE INDEX IF NOT EXISTS idx_match_types_active ON MatchTypes(IsActive);
CREATE INDEX IF NOT EXISTS idx_segment_templates_type ON SegmentTemplates(SegmentType);
