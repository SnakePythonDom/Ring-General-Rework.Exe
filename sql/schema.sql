-- =========================================================================
-- Ring General schema loader
-- Uses sqlite3 dot-commands to apply migrations in deterministic order.
-- =========================================================================

PRAGMA foreign_keys = ON;

.read data/migrations/001_init.sql
.read data/migrations/002_backstage.sql
.read data/migrations/002_booking_segments.sql
.read data/migrations/002_broadcast.sql
.read data/migrations/002_broadcast_v1.sql
.read data/migrations/002_contracts_v1.sql
.read data/migrations/002_finances.sql
.read data/migrations/002_game_state.sql
.read data/migrations/002_library.sql
.read data/migrations/002_medical.sql
.read data/migrations/002_save_games.sql
.read data/migrations/002_scouting.sql
.read data/migrations/002_show_results.sql
.read data/migrations/002_shows_calendar.sql
.read data/migrations/002_storylines.sql
.read data/migrations/002_titles.sql
.read data/migrations/002_youth.sql
.read data/migrations/002_youth_v1.sql
.read data/migrations/003_consolidate_schema.sql
.read data/migrations/004_owner_booker_governance.sql
.read data/migrations/005_company_identity.sql
.read data/migrations/006_catch_styles.sql
