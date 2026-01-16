-- ============================================================================
-- Migration 035: Fix Database Integrity (FK Mismatches)
-- ============================================================================

BEGIN TRANSACTION;

-- 1. FIX GIMMICK HISTORY FK
DROP TABLE IF EXISTS GimmickHistory;
CREATE TABLE GimmickHistory (
    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId TEXT NOT NULL,
    GimmickId TEXT,
    GimmickName TEXT NOT NULL,
    StartDate TEXT NOT NULL DEFAULT (datetime('now')),
    EndDate TEXT,
    AdoptionReason TEXT,
    SuccessRating INTEGER DEFAULT 50,
    Notes TEXT,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId) ON DELETE CASCADE,
    FOREIGN KEY (GimmickId) REFERENCES Gimmicks(GimmickId)
);
CREATE INDEX idx_gimmick_history_worker ON GimmickHistory(WorkerId);
CREATE INDEX idx_gimmick_history_gimmick ON GimmickHistory(GimmickId);

-- 2. FIX APPEARANCEFEE LOG FK (Referencing 'shows' instead of 'Shows')
DROP TABLE IF EXISTS AppearanceFeeLog;
CREATE TABLE AppearanceFeeLog (
    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    ContractId TEXT NOT NULL,
    ShowId TEXT NOT NULL,
    PaymentDate TEXT NOT NULL,
    Amount REAL NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId) ON DELETE CASCADE,
    FOREIGN KEY (ContractId) REFERENCES Contracts(ContractId) ON DELETE CASCADE,
    FOREIGN KEY (ShowId) REFERENCES shows(show_id) ON DELETE CASCADE
);
CREATE INDEX idx_appearance_fee_worker_date ON AppearanceFeeLog(WorkerId, PaymentDate);
CREATE INDEX idx_appearance_fee_show ON AppearanceFeeLog(ShowId);

COMMIT;

