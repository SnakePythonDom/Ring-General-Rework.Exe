PRAGMA foreign_keys = ON;

ALTER TABLE Contracts ADD COLUMN ContractType TEXT NOT NULL DEFAULT 'exclusif';
ALTER TABLE Contracts ADD COLUMN StartWeek INTEGER NOT NULL DEFAULT 1;
ALTER TABLE Contracts ADD COLUMN BonusShow REAL NOT NULL DEFAULT 0;
ALTER TABLE Contracts ADD COLUMN Buyout REAL NOT NULL DEFAULT 0;
ALTER TABLE Contracts ADD COLUMN NonCompeteWeeks INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Contracts ADD COLUMN AutoRenew INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Contracts ADD COLUMN Status TEXT NOT NULL DEFAULT 'actif';

CREATE TABLE IF NOT EXISTS ContractOffers (
    ContractOfferId INTEGER PRIMARY KEY AUTOINCREMENT,
    OfferId TEXT NOT NULL UNIQUE,
    NegotiationId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    ContractType TEXT NOT NULL,
    StartWeek INTEGER NOT NULL,
    EndWeek INTEGER NOT NULL,
    Salary REAL NOT NULL DEFAULT 0,
    BonusShow REAL NOT NULL DEFAULT 0,
    Buyout REAL NOT NULL DEFAULT 0,
    NonCompeteWeeks INTEGER NOT NULL DEFAULT 0,
    AutoRenew INTEGER NOT NULL DEFAULT 0,
    IsExclusive INTEGER NOT NULL DEFAULT 0,
    Status TEXT NOT NULL,
    CreatedWeek INTEGER NOT NULL,
    ExpirationWeek INTEGER NOT NULL,
    ParentOfferId TEXT,
    IsAi INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE IF NOT EXISTS ContractClauses (
    ContractClauseId INTEGER PRIMARY KEY AUTOINCREMENT,
    ContractId INTEGER,
    OfferId TEXT,
    ClauseType TEXT NOT NULL,
    ClauseValue TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ContractId) REFERENCES Contracts(ContractId)
);

CREATE TABLE IF NOT EXISTS NegotiationState (
    NegotiationId TEXT PRIMARY KEY,
    WorkerId TEXT NOT NULL,
    CompanyId TEXT NOT NULL,
    Status TEXT NOT NULL,
    LastOfferId TEXT,
    UpdatedWeek INTEGER NOT NULL,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE INDEX IF NOT EXISTS idx_contracts_company_end ON Contracts(CompanyId, EndDate);
CREATE INDEX IF NOT EXISTS idx_contracts_worker ON Contracts(WorkerId);
CREATE INDEX IF NOT EXISTS idx_contract_offers_company ON ContractOffers(CompanyId);
CREATE INDEX IF NOT EXISTS idx_contract_offers_worker ON ContractOffers(WorkerId);
CREATE INDEX IF NOT EXISTS idx_contract_offers_expiration ON ContractOffers(ExpirationWeek);
