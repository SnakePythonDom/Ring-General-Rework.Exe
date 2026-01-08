-- ============================================================================
-- Import promotions from legacy BAKI database into Ring General Companies
-- ============================================================================

PRAGMA foreign_keys = ON;

INSERT INTO Companies (
    CompanyId,
    LegacyPromotionId,
    Name,
    CountryId,
    RegionId,
    Prestige
)
SELECT
    'COMP_' || p.promotionID,
    p.promotionID,
    p.fullName,
    c.CountryId,
    COALESCE(
        (
            SELECT r.RegionId
            FROM Regions r
            WHERE r.CountryId = c.CountryId
            ORDER BY r.Name
            LIMIT 1
        ),
        'REGION_DEFAULT'
    ),
    p.prestige
FROM legacy.promotions p
LEFT JOIN Countries c ON c.Name = p.basedInCountry
ON CONFLICT(LegacyPromotionId) DO NOTHING;

INSERT OR IGNORE INTO Companies (CompanyId, Name, CountryId, RegionId, Prestige)
VALUES ('0', 'Free Agent', 'COUNTRY_DEFAULT', 'REGION_DEFAULT', 0);
