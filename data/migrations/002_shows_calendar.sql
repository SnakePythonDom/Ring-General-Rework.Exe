PRAGMA foreign_keys = ON;

ALTER TABLE Shows ADD COLUMN ShowType TEXT NOT NULL DEFAULT 'TV';
ALTER TABLE Shows ADD COLUMN Broadcast TEXT;
ALTER TABLE Shows ADD COLUMN TicketPrice REAL NOT NULL DEFAULT 0;
ALTER TABLE Shows ADD COLUMN Status TEXT NOT NULL DEFAULT 'ABOOKER';

CREATE TABLE IF NOT EXISTS ShowSettings (
    ShowSettingsId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    ShowType TEXT NOT NULL,
    RuntimeMinMinutes INTEGER NOT NULL,
    RuntimeMaxMinutes INTEGER NOT NULL,
    TicketPriceMin REAL,
    TicketPriceMax REAL,
    BroadcastRequired INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS CalendarEntries (
    CalendarEntryId TEXT PRIMARY KEY,
    CompanyId TEXT NOT NULL,
    Date TEXT NOT NULL,
    EntryType TEXT NOT NULL,
    ReferenceId TEXT,
    Title TEXT,
    Notes TEXT,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE INDEX IF NOT EXISTS idx_show_settings_company_type ON ShowSettings(CompanyId, ShowType);
CREATE INDEX IF NOT EXISTS idx_calendar_entries_company_date ON CalendarEntries(CompanyId, Date);
