PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS SegmentSettings (
    ShowSegmentId TEXT NOT NULL,
    SettingKey TEXT NOT NULL,
    SettingValue TEXT NOT NULL,
    PRIMARY KEY (ShowSegmentId, SettingKey)
);

CREATE INDEX IF NOT EXISTS idx_show_segments_order ON ShowSegments(ShowId, OrderIndex);
