-- Phase 3.1 - Migration pour ajouter la table ContractTemplates

CREATE TABLE IF NOT EXISTS ContractTemplates (
    TemplateId TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    MonthlyWage REAL NOT NULL DEFAULT 0,
    AppearanceFee REAL NOT NULL DEFAULT 0,
    DurationMonths INTEGER NOT NULL DEFAULT 12,
    IsExclusive INTEGER NOT NULL DEFAULT 1,
    HasRenewalOption INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insérer les templates par défaut
INSERT OR IGNORE INTO ContractTemplates (TemplateId, Name, Description, MonthlyWage, AppearanceFee, DurationMonths, IsExclusive, HasRenewalOption)
VALUES 
    ('TPL_MAIN_EVENT', 'Main Event Star', 'Contrat pour une star principale (salaire mensuel élevé + frais d''apparition)', 50000, 5000, 24, 1, 1),
    ('TPL_MID_CARD', 'Mid-Card Regular', 'Contrat pour un talent régulier de la midcard (salaire mensuel modéré)', 15000, 1500, 12, 1, 0),
    ('TPL_UNDERCARD', 'Undercard', 'Contrat pour un talent de l''undercard (salaire mensuel basique)', 5000, 500, 6, 0, 0),
    ('TPL_TRAINEE', 'Trainee', 'Contrat de développement pour un trainee (salaire mensuel minimal)', 2000, 0, 12, 1, 1);
