#!/bin/bash
# ============================================================================
# Ring General - Database Initialization Script
# Version: 1.0.0
# Date: 2026-01-08
# Description: Automated database initialization from scratch
# ============================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
DB_NAME="ringgeneral.db"
BAKI_DB="BAKI1.1.db"
SCHEMA_FILE="src/RingGeneral.Data/Migrations/Base_Schema.sql"
IMPORT_FILE="src/RingGeneral.Data/Migrations/ImportWorkersFromBaki.sql"

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
    if ! command -v $1 &> /dev/null; then
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

run_sql() {
    local db=$1
    local sql=$2
    sqlite3 "$db" "$sql"
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
check_file "$SCHEMA_FILE"
check_file "$IMPORT_FILE"
check_file "$BAKI_DB"
echo ""

# Step 2: Backup existing database (if exists)
print_step "2/7 Checking for existing database..."
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
echo ""

# Step 3: Create database schema
print_step "3/7 Creating database schema..."
run_sql_file "$DB_NAME" "$SCHEMA_FILE"
TABLE_COUNT=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM sqlite_master WHERE type='table';")
print_success "Database schema created ($TABLE_COUNT tables)"
echo ""

# Step 4: Verify critical tables
print_step "4/7 Verifying critical tables..."
CRITICAL_TABLES=("Workers" "WorkerInRingAttributes" "WorkerEntertainmentAttributes" "WorkerStoryAttributes" "WorkerMentalAttributes")
for table in "${CRITICAL_TABLES[@]}"; do
    EXISTS=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='$table';")
    if [ "$EXISTS" -eq "1" ]; then
        print_success "Table '$table' exists"
    else
        print_error "Table '$table' not found!"
        exit 1
    fi
done
echo ""

# Step 5: Import from BAKI1.1.db
print_step "5/7 Importing workers from BAKI1.1.db..."
BAKI_WORKER_COUNT=$(run_sql "$BAKI_DB" "SELECT COUNT(*) FROM workers;")
print_success "Found $BAKI_WORKER_COUNT workers in BAKI1.1.db"

run_sql_file "$DB_NAME" "$IMPORT_FILE"
IMPORTED_COUNT=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM Workers;")
print_success "Imported $IMPORTED_COUNT workers"
echo ""

# Step 6: Verify attributes generation
print_step "6/7 Verifying attributes generation..."

INRING_COUNT=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM WorkerInRingAttributes;")
print_success "In-Ring attributes: $INRING_COUNT"

ENTERTAINMENT_COUNT=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM WorkerEntertainmentAttributes;")
print_success "Entertainment attributes: $ENTERTAINMENT_COUNT"

STORY_COUNT=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM WorkerStoryAttributes;")
print_success "Story attributes: $STORY_COUNT"

MENTAL_COUNT=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM WorkerMentalAttributes;")
print_success "Mental attributes: $MENTAL_COUNT"

# Check integrity
if [ "$IMPORTED_COUNT" -eq "$INRING_COUNT" ] && \
   [ "$IMPORTED_COUNT" -eq "$ENTERTAINMENT_COUNT" ] && \
   [ "$IMPORTED_COUNT" -eq "$STORY_COUNT" ] && \
   [ "$IMPORTED_COUNT" -eq "$MENTAL_COUNT" ]; then
    print_success "All workers have complete attributes"
else
    print_warning "Attribute counts don't match worker count"
    print_warning "Workers: $IMPORTED_COUNT, InRing: $INRING_COUNT, Entertainment: $ENTERTAINMENT_COUNT, Story: $STORY_COUNT, Mental: $MENTAL_COUNT"
fi
echo ""

# Step 7: Generate final report
print_step "7/7 Generating final report..."
echo ""

print_header "📊 DATABASE INITIALIZATION REPORT"

echo -e "${CYAN}TABLES${NC}"
echo "Total tables: $TABLE_COUNT"
echo ""

echo -e "${CYAN}WORKERS${NC}"
echo "Total workers: $IMPORTED_COUNT"
AVG_AGE=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(Age), 1) FROM Workers WHERE Age IS NOT NULL;")
echo "Average age: $AVG_AGE years"
AVG_POP=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(Popularity), 1) FROM Workers WHERE Popularity IS NOT NULL;")
echo "Average popularity: $AVG_POP"
echo ""

echo -e "${CYAN}PERFORMANCE ATTRIBUTES${NC}"
AVG_INRING=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(InRingAvg), 1) FROM WorkerInRingAttributes;")
echo "Average In-Ring: $AVG_INRING"
AVG_ENT=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(EntertainmentAvg), 1) FROM WorkerEntertainmentAttributes;")
echo "Average Entertainment: $AVG_ENT"
AVG_STORY=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(StoryAvg), 1) FROM WorkerStoryAttributes;")
echo "Average Story: $AVG_STORY"
echo ""

echo -e "${CYAN}MENTAL ATTRIBUTES (Phase 8)${NC}"
echo "Workers with mental attributes: $MENTAL_COUNT"
AVG_PRO=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(Professionnalisme), 1) FROM WorkerMentalAttributes;")
echo "Average Professionnalisme: $AVG_PRO / 20"
AVG_EGO=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(Égoïsme), 1) FROM WorkerMentalAttributes;")
echo "Average Égoïsme: $AVG_EGO / 20"
AVG_INF=$(run_sql "$DB_NAME" "SELECT ROUND(AVG(Influence), 1) FROM WorkerMentalAttributes;")
echo "Average Influence: $AVG_INF / 20"
echo ""

echo -e "${CYAN}INTEGRITY CHECKS${NC}"

# Check foreign keys
FK_VIOLATIONS=$(run_sql "$DB_NAME" "PRAGMA foreign_key_check;" | wc -l)
if [ "$FK_VIOLATIONS" -eq "0" ]; then
    print_success "No foreign key violations"
else
    print_error "$FK_VIOLATIONS foreign key violations found"
fi

# Check workers without attributes
MISSING_INRING=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM Workers w LEFT JOIN WorkerInRingAttributes a ON w.Id = a.WorkerId WHERE a.WorkerId IS NULL;")
MISSING_MENTAL=$(run_sql "$DB_NAME" "SELECT COUNT(*) FROM Workers w LEFT JOIN WorkerMentalAttributes a ON w.Id = a.WorkerId WHERE a.WorkerId IS NULL;")

if [ "$MISSING_INRING" -eq "0" ]; then
    print_success "All workers have In-Ring attributes"
else
    print_error "$MISSING_INRING workers missing In-Ring attributes"
fi

if [ "$MISSING_MENTAL" -eq "0" ]; then
    print_success "All workers have Mental attributes"
else
    print_error "$MISSING_MENTAL workers missing Mental attributes"
fi

echo ""
print_header "✅ DATABASE INITIALIZATION COMPLETE!"

echo ""
echo -e "${GREEN}Next steps:${NC}"
echo "  1. Run the application: ${CYAN}dotnet run --project src/RingGeneral.UI${NC}"
echo "  2. Open a worker profile and check the 🎭 PERSONNALITÉ tab"
echo "  3. Test the scouting system"
echo ""
echo -e "${YELLOW}Database file:${NC} $DB_NAME"
echo -e "${YELLOW}Documentation:${NC} INIT_DATABASE.md"
echo ""

exit 0
