ALTER TABLE Storylines ADD COLUMN Phase TEXT NOT NULL DEFAULT 'Setup';
ALTER TABLE Storylines ADD COLUMN Status TEXT NOT NULL DEFAULT 'Active';

CREATE TABLE IF NOT EXISTS StorylineEvents (
    StorylineEventId INTEGER PRIMARY KEY AUTOINCREMENT,
    StorylineId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    SegmentId TEXT,
    Type TEXT NOT NULL,
    Note INTEGER,
    HeatDelta INTEGER NOT NULL DEFAULT 0,
    MomentumDelta INTEGER NOT NULL DEFAULT 0,
    Description TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (StorylineId) REFERENCES Storylines(StorylineId),
    FOREIGN KEY (SegmentId) REFERENCES ShowSegments(ShowSegmentId)
);

CREATE INDEX IF NOT EXISTS idx_storyline_events_storyline ON StorylineEvents(StorylineId);
CREATE INDEX IF NOT EXISTS idx_storyline_events_week ON StorylineEvents(Week);
