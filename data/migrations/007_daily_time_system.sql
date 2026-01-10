-- Migration 007: Daily Time System & Hybrid Contracts
-- Transition du système hebdomadaire vers un système quotidien
-- Implémentation du système financier hybride (mensuel garanti + frais d'apparition)

PRAGMA foreign_keys = ON;

-- ============================================================================
-- SYSTÈME TEMPOREL QUOTIDIEN
-- Ajout des colonnes pour tracker le jour actuel
-- ============================================================================

-- Ajouter colonnes pour système quotidien dans SaveGames
ALTER TABLE SaveGames ADD COLUMN CurrentDay INTEGER NOT NULL DEFAULT 1;
ALTER TABLE SaveGames ADD COLUMN StartDate TEXT NOT NULL DEFAULT '2024-01-01';

-- Mettre à jour CurrentDay pour les sauvegardes existantes (semaine × 7 jours)
UPDATE SaveGames 
SET CurrentDay = CurrentWeek * 7,
    StartDate = COALESCE(CurrentDate, '2024-01-01')
WHERE CurrentDay = 1 AND CurrentWeek > 1;

-- ============================================================================
-- SYSTÈME FINANCIER HYBRIDE
-- Ajout des colonnes pour contrats hybrides (mensuel garanti + frais d'apparition)
-- ============================================================================

-- Ajouter colonnes pour contrats hybrides dans contracts
ALTER TABLE contracts ADD COLUMN MonthlyWage REAL NOT NULL DEFAULT 0;
ALTER TABLE contracts ADD COLUMN AppearanceFee REAL NOT NULL DEFAULT 0;
ALTER TABLE contracts ADD COLUMN LastPaymentDate TEXT;
ALTER TABLE contracts ADD COLUMN LastAppearanceDate TEXT;

-- Migration des données existantes : convertir salaire hebdomadaire en mensuel
-- Hypothèse : 4 semaines par mois, donc salaire mensuel = salaire hebdomadaire × 4
UPDATE contracts
SET MonthlyWage = CASE 
    WHEN salaire > 0 THEN salaire * 4.0  -- Convertir salaire hebdomadaire en mensuel
    ELSE 0
END,
AppearanceFee = CASE
    WHEN bonus_show > 0 THEN bonus_show  -- bonus_show devient AppearanceFee
    ELSE 0
END
WHERE MonthlyWage = 0;

-- ============================================================================
-- TABLE: MonthlyPayrollLog
-- Tracking des paiements mensuels pour éviter les doubles prélèvements
-- ============================================================================

CREATE TABLE IF NOT EXISTS MonthlyPayrollLog (
    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
    CompanyId TEXT NOT NULL,
    WorkerId TEXT NOT NULL,
    ContractId TEXT NOT NULL,
    PaymentDate TEXT NOT NULL,
    Amount REAL NOT NULL,
    Month INTEGER NOT NULL,
    Year INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId) ON DELETE CASCADE,
    FOREIGN KEY (WorkerId) REFERENCES Workers(WorkerId) ON DELETE CASCADE,
    FOREIGN KEY (ContractId) REFERENCES contracts(contract_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_monthly_payroll_date 
    ON MonthlyPayrollLog(CompanyId, WorkerId, Year, Month);
CREATE INDEX IF NOT EXISTS idx_monthly_payroll_contract 
    ON MonthlyPayrollLog(ContractId);
CREATE INDEX IF NOT EXISTS idx_monthly_payroll_company_date 
    ON MonthlyPayrollLog(CompanyId, Year, Month);

-- ============================================================================
-- TABLE: AppearanceFeeLog
-- Tracking des frais d'apparition pour éviter les doubles paiements
-- ============================================================================

CREATE TABLE IF NOT EXISTS AppearanceFeeLog (
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
    FOREIGN KEY (ContractId) REFERENCES contracts(contract_id) ON DELETE CASCADE,
    FOREIGN KEY (ShowId) REFERENCES Shows(ShowId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_appearance_fee_worker_date 
    ON AppearanceFeeLog(WorkerId, PaymentDate);
CREATE INDEX IF NOT EXISTS idx_appearance_fee_show 
    ON AppearanceFeeLog(ShowId);
CREATE INDEX IF NOT EXISTS idx_appearance_fee_contract 
    ON AppearanceFeeLog(ContractId);

-- ============================================================================
-- VUES UTILITAIRES
-- ============================================================================

-- Vue pour les contrats avec informations hybrides
CREATE VIEW IF NOT EXISTS vw_HybridContracts AS
SELECT
    c.contract_id AS ContractId,
    c.worker_id AS WorkerId,
    c.company_id AS CompanyId,
    c.MonthlyWage,
    c.AppearanceFee,
    c.IsExclusive AS IsExclusive,
    c.StartDate AS StartDate,
    c.EndDate AS EndDate,
    c.LastPaymentDate,
    c.LastAppearanceDate,
    -- Type de contrat déterminé par les valeurs
    CASE 
        WHEN c.MonthlyWage > 0 AND c.AppearanceFee > 0 THEN 'Hybrid'
        WHEN c.MonthlyWage > 0 THEN 'Fixed'
        WHEN c.AppearanceFee > 0 THEN 'PerAppearance'
        ELSE 'Unknown'
    END AS ContractType
FROM contracts c
WHERE c.statut = 'actif';

-- ============================================================================
-- CONTRAINTES ET VALIDATIONS
-- ============================================================================

-- Vérifier qu'un contrat a au moins un type de paiement
-- Note: SQLite ne supporte pas les CHECK complexes, donc validation applicative
-- Un contrat doit avoir MonthlyWage > 0 OU AppearanceFee > 0 (ou les deux)

-- ============================================================================
-- INDEX POUR PERFORMANCE
-- ============================================================================

-- Index sur contracts pour requêtes fréquentes
CREATE INDEX IF NOT EXISTS idx_contracts_monthly_wage 
    ON contracts(company_id, MonthlyWage) WHERE MonthlyWage > 0;
CREATE INDEX IF NOT EXISTS idx_contracts_appearance_fee 
    ON contracts(company_id, AppearanceFee) WHERE AppearanceFee > 0;
CREATE INDEX IF NOT EXISTS idx_contracts_last_payment 
    ON contracts(company_id, LastPaymentDate);
CREATE INDEX IF NOT EXISTS idx_contracts_last_appearance 
    ON contracts(company_id, LastAppearanceDate);
