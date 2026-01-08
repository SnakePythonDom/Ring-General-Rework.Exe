#!/bin/bash
# ============================================================================
# Ring General - Database Initialization Script
# Version: 2.0.0
# Description: Deterministic initialization from legacy BAKI1.1.db
# ============================================================================

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
DB_NAME="${RING_GENERAL_DB:-ring_general.db}"
BAKI_DB="${BAKI_DB:-data/BAKI1.1.db}"
SCHEMA_FILE="sql/schema.sql"
SEED_COUNTRIES_FILE="sql/seed_countries.sql"
SEED_REGIONS_FILE="sql/seed_regions.sql"
SEED_STYLES_FILE="sql/seed_styles.sql"
IMPORT_COMPANIES_FILE="sql/import_companies.sql"
IMPORT_WORKERS_FILE="sql/import_workers.sql"
MAP_COMPANIES_FILE="sql/map_companies.sql"
MAP_WORKERS_FILE="sql/map_workers.sql"
VALIDATE_FILE="sql/validate.sql"
REFERENCE_BUILDER="scripts/build_reference_data.py"

# ============================================================================
# FUNCTIONS
# ============================================================================

print_header() {
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
    echo ""
}

print_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

check_command() {
    if ! command -v "$1" &> /dev/null; then
        print_error "$1 is not installed"
        exit 1
    fi
    print_success "$1 is installed"
}

check_file() {
    if [ ! -f "$1" ]; then
        print_error "File not found: $1"
        exit 1
    fi
    print_success "File found: $1"
}

run_sql_file() {
    local db=$1
    local file=$2
    sqlite3 "$db" < "$file"
}

# ============================================================================
# MAIN SCRIPT
# ============================================================================

print_header "🗄️  RING GENERAL DATABASE INITIALIZATION"

# Step 1: Check prerequisites
print_step "1/7 Checking prerequisites..."
check_command sqlite3
check_command python3
check_file "$SCHEMA_FILE"
check_file "$SEED_STYLES_FILE"
check_file "$IMPORT_COMPANIES_FILE"
check_file "$IMPORT_WORKERS_FILE"
check_file "$MAP_COMPANIES_FILE"
check_file "$MAP_WORKERS_FILE"
check_file "$VALIDATE_FILE"
check_file "$REFERENCE_BUILDER"
check_file "$BAKI_DB"

# Step 2: Reset database
print_step "2/7 Resetting database..."
if [ -f "$DB_NAME" ]; then
    BACKUP_NAME="${DB_NAME}.backup.$(date +%Y%m%d_%H%M%S)"
    print_warning "Existing database found, creating backup: $BACKUP_NAME"
    cp "$DB_NAME" "$BACKUP_NAME"
    print_success "Backup created"
    rm -f "$DB_NAME"
    print_success "Old database removed"
else
    print_success "No existing database, proceeding with fresh installation"
fi

# Step 3: Create schema
print_step "3/7 Creating database schema..."
run_sql_file "$DB_NAME" "$SCHEMA_FILE"
TABLE_COUNT=$(sqlite3 "$DB_NAME" "SELECT COUNT(*) FROM sqlite_master WHERE type='table';")
print_success "Database schema created ($TABLE_COUNT tables)"

# Step 4: Build reference data (countries/regions)
print_step "4/7 Generating reference data from legacy DB..."
python3 "$REFERENCE_BUILDER" --legacy-db "$BAKI_DB" --output-dir "sql"
check_file "$SEED_COUNTRIES_FILE"
check_file "$SEED_REGIONS_FILE"
print_success "Reference data generated"

# Step 5: Seed structure tables (countries, regions, styles)
print_step "5/7 Seeding structure tables..."
run_sql_file "$DB_NAME" "$SEED_COUNTRIES_FILE"
run_sql_file "$DB_NAME" "$SEED_REGIONS_FILE"
run_sql_file "$DB_NAME" "$SEED_STYLES_FILE"
print_success "Structure tables seeded"

# Step 6: Import legacy data + mapping
print_step "6/7 Importing legacy data..."
sqlite3 "$DB_NAME" <<SQL
PRAGMA foreign_keys = ON;
ATTACH DATABASE '$BAKI_DB' AS baki;
.read $IMPORT_COMPANIES_FILE
.read $IMPORT_WORKERS_FILE
.read $MAP_COMPANIES_FILE
.read $MAP_WORKERS_FILE
DETACH DATABASE baki;
SQL
print_success "Legacy data imported and mapped"

# Step 7: Validation
print_step "7/7 Running validation..."
run_sql_file "$DB_NAME" "$VALIDATE_FILE"
print_success "Validation passed"

print_header "✅ DATABASE INITIALIZATION COMPLETE!"

echo ""
echo -e "${GREEN}Next steps:${NC}"
echo "  1. Run the application: ${CYAN}dotnet run --project src/RingGeneral.UI${NC}"
echo "  2. Create or select a company in the start menu"
echo "  3. Verify countries/regions/styles are populated"
echo ""
echo -e "${YELLOW}Database file:${NC} $DB_NAME"
echo ""

exit 0
