PRAGMA foreign_keys = ON;

-- Table pour lier les deals aux régions (Support International/Multi-région)
CREATE TABLE IF NOT EXISTS TvDealRegions (
    TvDealId TEXT NOT NULL,
    RegionId TEXT NOT NULL,
    PRIMARY KEY (TvDealId, RegionId),
    FOREIGN KEY (TvDealId) REFERENCES TVDeals(TvDealId),
    FOREIGN KEY (RegionId) REFERENCES Regions(RegionId)
);

-- Enrichir la table TVDeals
ALTER TABLE TVDeals ADD COLUMN SlotType TEXT NOT NULL DEFAULT 'PrimeTime';
ALTER TABLE TVDeals ADD COLUMN ProductionRequirement INTEGER NOT NULL DEFAULT 50;
ALTER TABLE TVDeals ADD COLUMN DemographicFocus TEXT NOT NULL DEFAULT 'General';
ALTER TABLE TVDeals ADD COLUMN NegotiationPatience INTEGER NOT NULL DEFAULT 100;
ALTER TABLE TVDeals ADD COLUMN Distribution TEXT NOT NULL DEFAULT 'Regional';

-- Index pour les régions
CREATE INDEX IF NOT EXISTS idx_tv_deal_regions_deal ON TvDealRegions(TvDealId);
CREATE INDEX IF NOT EXISTS idx_tv_deal_regions_region ON TvDealRegions(RegionId);
