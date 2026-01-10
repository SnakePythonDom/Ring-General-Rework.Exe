#!/bin/bash

# Script pour exécuter les tests du projet Ring General
# Utilisation: ./scripts/run-tests.sh [options]

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction d'affichage coloré
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier que dotnet est installé
if ! command -v dotnet &> /dev/null; then
    log_error "dotnet CLI n'est pas installé ou n'est pas dans le PATH"
    exit 1
fi

# Aller dans le répertoire du projet
cd "$(dirname "$0")/.."

log_info "Vérification de la version de .NET..."
dotnet --version

# Options par défaut
RUN_ALL=true
RUN_UNIT=false
RUN_INTEGRATION=false
RUN_UI=false
RUN_COVERAGE=false
FILTER=""

# Parser les arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --unit)
            RUN_ALL=false
            RUN_UNIT=true
            shift
            ;;
        --integration)
            RUN_ALL=false
            RUN_INTEGRATION=true
            shift
            ;;
        --ui)
            RUN_ALL=false
            RUN_UI=true
            shift
            ;;
        --coverage)
            RUN_COVERAGE=true
            shift
            ;;
        --filter)
            FILTER="$2"
            shift
            shift
            ;;
        --help)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  --unit          Exécuter seulement les tests unitaires"
            echo "  --integration   Exécuter seulement les tests d'intégration"
            echo "  --ui            Exécuter seulement les tests UI"
            echo "  --coverage      Générer un rapport de couverture de code"
            echo "  --filter FILTER Filtrer les tests (ex: 'RingGeneral.Tests.FinanceEngineTests')"
            echo "  --help          Afficher cette aide"
            echo ""
            echo "Sans options, exécute tous les tests."
            exit 0
            ;;
        *)
            log_error "Option inconnue: $1"
            echo "Utilisez --help pour voir les options disponibles."
            exit 1
            ;;
    esac
done

# Fonction pour exécuter les tests avec les options appropriées
run_tests() {
    local test_type=$1
    local filter=$2
    local project_path="tests/RingGeneral.Tests/RingGeneral.Tests.csproj"

    log_info "Exécution des tests $test_type..."

    local cmd="dotnet test $project_path --logger \"console;verbosity=detailed\""

    if [ -n "$filter" ]; then
        cmd="$cmd --filter \"$filter\""
    fi

    if [ "$RUN_COVERAGE" = true ]; then
        cmd="$cmd --collect:\"XPlat Code Coverage\""
    fi

    log_info "Commande: $cmd"

    if eval "$cmd"; then
        log_success "Tests $test_type terminés avec succès"
    else
        log_error "Échec des tests $test_type"
        return 1
    fi
}

# Exécuter les tests selon les options choisies
if [ "$RUN_ALL" = true ] || [ "$RUN_UNIT" = true ]; then
    if ! run_tests "unitaires" "$FILTER"; then
        exit 1
    fi
fi

if [ "$RUN_ALL" = true ] || [ "$RUN_INTEGRATION" = true ]; then
    if ! run_tests "d'intégration" "$FILTER"; then
        exit 1
    fi
fi

if [ "$RUN_ALL" = true ] || [ "$RUN_UI" = true ]; then
    if ! run_tests "UI" "$FILTER"; then
        exit 1
    fi
fi

log_success "Tous les tests sélectionnés ont été exécutés avec succès!"

if [ "$RUN_COVERAGE" = true ]; then
    log_info "Rapports de couverture générés dans le répertoire TestResults/"
fi