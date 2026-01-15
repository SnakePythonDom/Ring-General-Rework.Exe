-- Script de migration des données existantes vers le système quotidien
-- À exécuter après la migration 007_daily_time_system.sql
-- Ce script convertit les données existantes pour être compatibles avec le nouveau système

PRAGMA foreign_keys = ON;

-- ============================================================================
-- ÉTAPE 1 : Conversion CurrentWeek → CurrentDay dans SaveGames
-- ============================================================================

-- Mettre à jour CurrentDay pour les sauvegardes existantes
-- Hypothèse : 1 semaine = 7 jours
UPDATE SaveGames
SET CurrentDay = CASE 
    WHEN CurrentDay = 1 AND CurrentWeek > 1 THEN CurrentWeek * 7
    WHEN CurrentDay = 1 THEN 1
    ELSE CurrentDay  -- Garder la valeur si déjà définie
END,
StartDate = CASE
    WHEN StartDate = '2024-01-01' AND CurrentWeek > 1 THEN 
        date('2024-01-01', '+' || ((CurrentWeek - 1) * 7) || ' days')
    WHEN StartDate = '2024-01-01' THEN '2024-01-01'
    ELSE StartDate  -- Garder la valeur si déjà définie
END,
CurrentDate = CASE
    WHEN CurrentDate IS NULL OR CurrentDate = '' THEN 
        date('2024-01-01', '+' || ((CurrentWeek - 1) * 7) || ' days')
    ELSE CurrentDate  -- Garder la valeur si déjà définie
END
WHERE CurrentDay = 1 OR CurrentDate IS NULL OR CurrentDate = '';

-- ============================================================================
-- ÉTAPE 2 : Conversion des contrats existants vers le système hybride
-- ============================================================================

-- Mettre à jour MonthlyWage pour les contrats existants
-- Hypothèse : Si PayrollFrequency = 'Mensuelle', convertir salaire hebdomadaire × 4
-- Si PayrollFrequency = 'Hebdomadaire', garder salaire × 4 pour MonthlyWage (approximation)
UPDATE contracts
SET MonthlyWage = CASE
    -- Si MonthlyWage n'est pas encore défini et qu'on a un salaire
    WHEN MonthlyWage = 0 AND salaire > 0 THEN salaire * 4.0  -- Convertir hebdomadaire → mensuel
    ELSE MonthlyWage  -- Garder la valeur si déjà définie
END,
AppearanceFee = CASE
    -- Si AppearanceFee n'est pas encore défini et qu'on a un bonus_show
    WHEN AppearanceFee = 0 AND bonus_show > 0 THEN bonus_show
    ELSE AppearanceFee  -- Garder la valeur si déjà définie
END
WHERE MonthlyWage = 0 OR AppearanceFee = 0;

-- ============================================================================
-- ÉTAPE 3 : Initialisation des dates de paiement
-- ============================================================================

-- Initialiser LastPaymentDate pour les contrats avec MonthlyWage > 0
-- On met une date dans le passé pour éviter un paiement immédiat au premier tick
UPDATE contracts
SET LastPaymentDate = date('now', '-1 month')
WHERE MonthlyWage > 0 
  AND (LastPaymentDate IS NULL OR LastPaymentDate = '');

-- Initialiser LastAppearanceDate à NULL (jamais payé pour apparition)
-- (Déjà NULL par défaut, donc pas besoin de mise à jour)

-- ============================================================================
-- ÉTAPE 4 : Validation et nettoyage
-- ============================================================================

-- Vérifier qu'il n'y a pas de contrats sans aucun type de paiement
-- (Un contrat doit avoir au moins MonthlyWage > 0 OU AppearanceFee > 0)
-- Note: Ceci est une validation, pas une correction automatique
-- Les contrats sans paiement devront être corrigés manuellement

-- Log des contrats problématiques (pour information)
-- SELECT contract_id, worker_id, MonthlyWage, AppearanceFee 
-- FROM contracts 
-- WHERE statut = 'actif' 
--   AND MonthlyWage = 0 
--   AND AppearanceFee = 0;

-- ============================================================================
-- ÉTAPE 5 : Migration des transactions financières existantes
-- ============================================================================

-- Les transactions existantes dans FinanceTransactions utilisent encore "Week"
-- On peut les laisser telles quelles pour compatibilité historique
-- Les nouvelles transactions utiliseront DateTime directement

-- ============================================================================
-- VALIDATION FINALE
-- ============================================================================

-- Vérifier que toutes les sauvegardes actives ont CurrentDay et CurrentDate définis
SELECT 
    SaveGameId,
    PlayerCompanyId,
    CurrentDay,
    CurrentDate,
    CurrentWeek
FROM SaveGames
WHERE IsActive = 1
  AND (CurrentDay IS NULL OR CurrentDate IS NULL OR CurrentDate = '');

-- Vérifier que tous les contrats actifs ont au moins un type de paiement
SELECT 
    contract_id,
    worker_id,
    MonthlyWage,
    AppearanceFee,
    CASE 
        WHEN MonthlyWage > 0 AND AppearanceFee > 0 THEN 'Hybrid'
        WHEN MonthlyWage > 0 THEN 'Fixed'
        WHEN AppearanceFee > 0 THEN 'PerAppearance'
        ELSE 'ERROR: No payment type'
    END AS ContractType
FROM contracts
WHERE statut = 'actif'
ORDER BY ContractType;
