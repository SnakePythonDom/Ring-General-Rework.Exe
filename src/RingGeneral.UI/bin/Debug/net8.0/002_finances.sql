PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS CompanyBalanceSnapshots (
    CompanyBalanceSnapshotId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    Week INTEGER NOT NULL,
    Balance REAL NOT NULL,
    Revenues REAL NOT NULL,
    Expenses REAL NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

ALTER TABLE Contracts ADD COLUMN PayFrequency TEXT NOT NULL DEFAULT 'Hebdomadaire';

CREATE INDEX IF NOT EXISTS idx_finance_transactions_company_week
    ON FinanceTransactions(CompanyId, Week);

CREATE INDEX IF NOT EXISTS idx_balance_snapshots_company_week
    ON CompanyBalanceSnapshots(CompanyId, Week);
