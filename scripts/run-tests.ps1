# Script PowerShell pour exécuter les tests du projet Ring General
# Utilisation: .\scripts\run-tests.ps1 [options]

param(
    [switch]$Unit,
    [switch]$Integration,
    [switch]$UI,
    [switch]$Coverage,
    [string]$Filter = "",
    [switch]$Help
)

# Fonction d'affichage coloré
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Afficher l'aide
if ($Help) {
    Write-Host "Usage: .\scripts\run-tests.ps1 [options]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Unit          Exécuter seulement les tests unitaires"
    Write-Host "  -Integration   Exécuter seulement les tests d'intégration"
    Write-Host "  -UI            Exécuter seulement les tests UI"
    Write-Host "  -Coverage      Générer un rapport de couverture de code"
    Write-Host "  -Filter FILTER Filtrer les tests (ex: 'RingGeneral.Tests.FinanceEngineTests')"
    Write-Host "  -Help          Afficher cette aide"
    Write-Host ""
    Write-Host "Sans options, exécute tous les tests."
    exit 0
}

# Vérifier que dotnet est installé
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet CLI n'est pas installé ou n'est pas dans le PATH"
    exit 1
}

Write-Info "Vérification de la version de .NET..."
dotnet --version

# Options par défaut
$runAll = -not ($Unit -or $Integration -or $UI)
$runUnit = $Unit -or $runAll
$runIntegration = $Integration -or $runAll
$runUI = $UI -or $runAll

# Fonction pour exécuter les tests
function Run-Tests {
    param(
        [string]$TestType,
        [string]$Filter
    )

    Write-Info "Exécution des tests $TestType..."

    $projectPath = "tests/RingGeneral.Tests/RingGeneral.Tests.csproj"
    $cmd = "dotnet test $projectPath --logger `"console;verbosity=detailed`""

    if ($Filter) {
        $cmd += " --filter `"$Filter`""
    }

    if ($Coverage) {
        $cmd += " --collect:`"XPlat Code Coverage`""
    }

    Write-Info "Commande: $cmd"

    try {
        Invoke-Expression $cmd
        Write-Success "Tests $TestType terminés avec succès"
        return $true
    }
    catch {
        Write-Error "Échec des tests $TestType"
        return $false
    }
}

# Exécuter les tests selon les options choisies
$allSuccess = $true

if ($runUnit) {
    if (!(Run-Tests -TestType "unitaires" -Filter $Filter)) {
        $allSuccess = $false
    }
}

if ($runIntegration) {
    if (!(Run-Tests -TestType "d'intégration" -Filter $Filter)) {
        $allSuccess = $false
    }
}

if ($runUI) {
    if (!(Run-Tests -TestType "UI" -Filter $Filter)) {
        $allSuccess = $false
    }
}

if ($allSuccess) {
    Write-Success "Tous les tests sélectionnés ont été exécutés avec succès!"
}
else {
    Write-Error "Certains tests ont échoué."
    exit 1
}

if ($Coverage) {
    Write-Info "Rapports de couverture générés dans le répertoire TestResults/"
}