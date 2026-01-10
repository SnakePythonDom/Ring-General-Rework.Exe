using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Interfaces;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public sealed class CompanyRepository : RepositoryBase, ICompanyRepository, ITvDealRepository
{
    public CompanyRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public IReadOnlyList<CompanyState> ChargerCompagnies()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT CompanyId, Name, RegionId, Prestige, Treasury, AverageAudience, Reach
            FROM Companies;
            """;
        using var reader = command.ExecuteReader();
        var compagnies = new List<CompanyState>();
        while (reader.Read())
        {
            compagnies.Add(new CompanyState(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetDouble(4),
                reader.GetInt32(5),
                reader.GetInt32(6)));
        }

        return compagnies;
    }

    public CompanyState? ChargerEtatCompagnie(string companyId)
    {
        using var connexion = OpenConnection();
        if (TableExiste(connexion, "Companies"))
        {
            return SharedQueries.ChargerCompagnie(connexion, companyId);
        }

        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT company_id, nom, region, prestige, tresorerie, audience_moyenne, reach
            FROM companies
            WHERE company_id = $companyId;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new CompanyState(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetDouble(4),
            reader.GetInt32(5),
            reader.GetInt32(6));
    }

    public void AppliquerImpactCompagnie(string compagnieId, int deltaPrestige, double deltaTresorerie)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE Companies
            SET Prestige = MAX(0, MIN(100, Prestige + $deltaPrestige)),
                Treasury = Treasury + $deltaTresorerie
            WHERE CompanyId = $companyId;
            """;
        command.Parameters.AddWithValue("$deltaPrestige", deltaPrestige);
        command.Parameters.AddWithValue("$deltaTresorerie", deltaTresorerie);
        command.Parameters.AddWithValue("$companyId", compagnieId);
        command.ExecuteNonQuery();
    }

    public string ChargerCompagnieIdPourShow(string showId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT CompanyId FROM Shows WHERE ShowId = $showId;";
        command.Parameters.AddWithValue("$showId", showId);
        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    public IReadOnlyList<ContractPayroll> ChargerPaieContrats(string companyId)
    {
        using var connexion = OpenConnection();
        if (TableExiste(connexion, "Contracts"))
        {
            return ChargerPaieContratsUpper(connexion, companyId);
        }

        return ChargerPaieContratsLower(connexion, companyId);
    }

    public double AppliquerTransactionsFinancieres(
        string companyId,
        int semaine,
        IReadOnlyList<FinanceTransaction> transactions)
    {
        if (transactions.Count == 0)
        {
            var compagnie = ChargerEtatCompagnie(companyId);
            return compagnie?.Tresorerie ?? 0;
        }

        using var connexion = OpenConnection();
        using var dbTransaction = connexion.BeginTransaction();

        foreach (var transactionFin in transactions)
        {
            using var command = connexion.CreateCommand();
            command.Transaction = dbTransaction;
            command.CommandText = """
                INSERT INTO FinanceTransactions (CompanyId, ShowId, Date, Week, Category, Amount, Description)
                VALUES ($companyId, NULL, $date, $week, $category, $amount, $description);
                """;
            command.Parameters.AddWithValue("$companyId", companyId);
            command.Parameters.AddWithValue("$date", DateTimeOffset.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$week", semaine);
            command.Parameters.AddWithValue("$category", transactionFin.Type);
            command.Parameters.AddWithValue("$amount", transactionFin.Montant);
            command.Parameters.AddWithValue("$description", transactionFin.Libelle);
            command.ExecuteNonQuery();

            using var treasuryCommand = connexion.CreateCommand();
            treasuryCommand.Transaction = dbTransaction;
            treasuryCommand.CommandText = "UPDATE Companies SET Treasury = Treasury + $amount WHERE CompanyId = $companyId;";
            treasuryCommand.Parameters.AddWithValue("$amount", transactionFin.Montant);
            treasuryCommand.Parameters.AddWithValue("$companyId", companyId);
            treasuryCommand.ExecuteNonQuery();
        }

        using var balanceCommand = connexion.CreateCommand();
        balanceCommand.Transaction = dbTransaction;
        balanceCommand.CommandText = "SELECT Treasury FROM Companies WHERE CompanyId = $companyId;";
        balanceCommand.Parameters.AddWithValue("$companyId", companyId);
        var tresorerie = Convert.ToDouble(balanceCommand.ExecuteScalar());

        dbTransaction.Commit();
        return tresorerie;
    }

    public void EnregistrerSnapshotFinance(string companyId, int semaine)
    {
        using var connexion = OpenConnection();
        if (!TableExiste(connexion, "CompanyBalanceSnapshots") || !TableExiste(connexion, "FinanceTransactions"))
        {
            return;
        }

        using var transaction = connexion.BeginTransaction();

        using var totalsCommand = connexion.CreateCommand();
        totalsCommand.Transaction = transaction;
        totalsCommand.CommandText = """
            SELECT
                COALESCE(SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN Amount < 0 THEN -Amount ELSE 0 END), 0)
            FROM FinanceTransactions
            WHERE CompanyId = $companyId AND Week = $week;
            """;
        totalsCommand.Parameters.AddWithValue("$companyId", companyId);
        totalsCommand.Parameters.AddWithValue("$week", semaine);
        using var reader = totalsCommand.ExecuteReader();
        var revenus = 0.0;
        var depenses = 0.0;
        if (reader.Read())
        {
            revenus = reader.GetDouble(0);
            depenses = reader.GetDouble(1);
        }

        using var balanceCommand = connexion.CreateCommand();
        balanceCommand.Transaction = transaction;
        balanceCommand.CommandText = "SELECT Treasury FROM Companies WHERE CompanyId = $companyId;";
        balanceCommand.Parameters.AddWithValue("$companyId", companyId);
        var balance = Convert.ToDouble(balanceCommand.ExecuteScalar());

        using var insertCommand = connexion.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = """
            INSERT INTO CompanyBalanceSnapshots (CompanyId, Week, Balance, Revenues, Expenses)
            VALUES ($companyId, $week, $balance, $revenus, $depenses);
            """;
        insertCommand.Parameters.AddWithValue("$companyId", companyId);
        insertCommand.Parameters.AddWithValue("$week", semaine);
        insertCommand.Parameters.AddWithValue("$balance", balance);
        insertCommand.Parameters.AddWithValue("$revenus", revenus);
        insertCommand.Parameters.AddWithValue("$depenses", depenses);
        insertCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    public IReadOnlyList<TvDeal> ChargerTvDeals(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT tv_deal_id, company_id, network_name, reach_bonus, audience_cap, audience_min,
                   base_revenue, revenue_per_point, penalty, constraints
            FROM tv_deals
            WHERE company_id = $companyId
            ORDER BY network_name;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var deals = new List<TvDeal>();
        while (reader.Read())
        {
            deals.Add(new TvDeal
            {
                TvDealId = reader.GetString(0),
                CompanyId = reader.GetString(1),
                NetworkName = reader.GetString(2),
                ReachBonus = reader.GetInt32(3),
                AudienceCap = reader.GetInt32(4),
                MinimumAudience = reader.GetInt32(5),
                BaseRevenue = reader.GetDouble(6),
                RevenuePerPoint = reader.GetDouble(7),
                Penalty = reader.GetDouble(8),
                Constraints = reader.GetString(9)
            });
        }

        return deals;
    }

    /// <summary>
    /// Phase 2.1 - Enregistre un nouveau TV Deal
    /// </summary>
    public void EnregistrerTvDeal(TvDeal deal, int startWeek, int endWeek)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        // Utiliser StartDate et EndDate (en semaines) au lieu de start_week/end_week
        command.CommandText = """
            INSERT INTO tv_deals (
                tv_deal_id, company_id, network_name, reach_bonus, audience_cap, audience_min,
                base_revenue, revenue_per_point, penalty, constraints, start_date, end_date
            ) VALUES (
                $dealId, $companyId, $networkName, $reachBonus, $audienceCap, $audienceMin,
                $baseRevenue, $revenuePerPoint, $penalty, $constraints, $startWeek, $endWeek
            );
            """;
        command.Parameters.AddWithValue("$dealId", deal.TvDealId);
        command.Parameters.AddWithValue("$companyId", deal.CompanyId);
        command.Parameters.AddWithValue("$networkName", deal.NetworkName);
        command.Parameters.AddWithValue("$reachBonus", deal.ReachBonus);
        command.Parameters.AddWithValue("$audienceCap", deal.AudienceCap);
        command.Parameters.AddWithValue("$audienceMin", deal.MinimumAudience);
        command.Parameters.AddWithValue("$baseRevenue", deal.BaseRevenue);
        command.Parameters.AddWithValue("$revenuePerPoint", deal.RevenuePerPoint);
        command.Parameters.AddWithValue("$penalty", deal.Penalty);
        command.Parameters.AddWithValue("$constraints", deal.Constraints);
        command.Parameters.AddWithValue("$startWeek", startWeek);
        command.Parameters.AddWithValue("$endWeek", endWeek);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<InboxItem> ChargerInbox()
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "SELECT Type, Title, Content, Week FROM InboxItems ORDER BY Week DESC, InboxItemId DESC;";
        using var reader = command.ExecuteReader();
        var items = new List<InboxItem>();
        while (reader.Read())
        {
            items.Add(new InboxItem(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }

        return items;
    }

    public void AjouterInboxItem(InboxItem item)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "INSERT INTO InboxItems (Type, Title, Content, Week) VALUES ($type, $titre, $contenu, $semaine);";
        command.Parameters.AddWithValue("$type", item.Type);
        command.Parameters.AddWithValue("$titre", item.Titre);
        command.Parameters.AddWithValue("$contenu", item.Contenu);
        command.Parameters.AddWithValue("$semaine", item.Semaine);
        command.ExecuteNonQuery();
    }

    // === Helpers privés (Catégorie B - Company domain) ===

    private static IReadOnlyList<ContractPayroll> ChargerPaieContratsUpper(SqliteConnection connexion, string companyId)
    {
        var hasSalary = ColonneExiste(connexion, "contracts", "salaire");
        var salaryColumn = hasSalary ? "contracts.salaire" : "0";

        using var command = connexion.CreateCommand();
        command.CommandText = $"""
            SELECT contracts.worker_id,
                   COALESCE(workers.nom || ' ' || workers.prenom, contracts.worker_id),
                   {salaryColumn},
                   'Hebdomadaire'
            FROM contracts
            LEFT JOIN workers ON contracts.worker_id = workers.worker_id
            WHERE contracts.company_id = $companyId AND contracts.actif = 1;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var paies = new List<ContractPayroll>();
        while (reader.Read())
        {
            paies.Add(new ContractPayroll
            {
                Frequence = ConvertFrequence(reader.GetString(3)),
                Salaire = LireDecimal(reader, 2)
            });
        }

        return paies;
    }

    private static IReadOnlyList<ContractPayroll> ChargerPaieContratsLower(SqliteConnection connexion, string companyId)
    {
        var hasSalary = ColonneExiste(connexion, "contracts", "salaire");
        var salaryColumn = hasSalary ? "contracts.salaire" : "0";

        using var command = connexion.CreateCommand();
        command.CommandText = $"""
            SELECT contracts.worker_id,
                   COALESCE(workers.nom || ' ' || workers.prenom, contracts.worker_id),
                   {salaryColumn},
                   'Hebdomadaire'
            FROM contracts
            LEFT JOIN workers ON contracts.worker_id = workers.worker_id
            WHERE contracts.company_id = $companyId AND contracts.actif = 1;
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var paies = new List<ContractPayroll>();
        while (reader.Read())
        {
            paies.Add(new ContractPayroll
            {
                Frequence = ConvertFrequence(reader.GetString(3)),
                Salaire = LireDecimal(reader, 2)
            });
        }

        return paies;
    }

    // ============================================================================
    // SYSTÈME QUOTIDIEN ET CONTRATS HYBRIDES
    // ============================================================================

    /// <summary>
    /// Surcharge de AppliquerTransactionsFinancieres avec DateTime (pour système quotidien)
    /// </summary>
    public double AppliquerTransactionsFinancieres(
        string companyId,
        DateTime date,
        IReadOnlyList<FinanceTransaction> transactions)
    {
        // Calculer la semaine approximative pour compatibilité (jour / 7)
        var semaine = (int)Math.Ceiling(date.Subtract(new DateTime(2024, 1, 1)).TotalDays / 7.0);
        return AppliquerTransactionsFinancieres(companyId, semaine, transactions);
    }

    /// <summary>
    /// Charge les contrats hybrides actifs pour une compagnie
    /// </summary>
    public IReadOnlyList<HybridContract> ChargerContratsHybrides(string companyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT 
                contract_id,
                worker_id,
                company_id,
                COALESCE(MonthlyWage, 0) AS MonthlyWage,
                COALESCE(AppearanceFee, 0) AS AppearanceFee,
                COALESCE(exclusif, 1) AS IsExclusive,
                COALESCE(debut_semaine, 1) AS StartWeek,
                fin_semaine AS EndWeek,
                LastPaymentDate,
                LastAppearanceDate
            FROM contracts
            WHERE company_id = $companyId 
              AND statut = 'actif'
              AND (COALESCE(MonthlyWage, 0) > 0 OR COALESCE(AppearanceFee, 0) > 0);
            """;
        command.Parameters.AddWithValue("$companyId", companyId);
        using var reader = command.ExecuteReader();
        var contracts = new List<HybridContract>();
        
        while (reader.Read())
        {
            var startWeek = reader.GetInt32(6);
            var endWeek = reader.GetInt32(7);
            // Convertir semaines en dates approximatives (1 semaine = 7 jours depuis 2024-01-01)
            var startDate = new DateTime(2024, 1, 1).AddDays((startWeek - 1) * 7);
            var endDate = new DateTime(2024, 1, 1).AddDays((endWeek - 1) * 7);
            
            contracts.Add(new HybridContract(
                reader.GetString(0), // ContractId
                reader.GetString(1), // WorkerId
                reader.GetString(2), // CompanyId
                reader.GetDouble(3), // MonthlyWage
                reader.GetDouble(4), // AppearanceFee
                reader.GetInt32(5) == 1, // IsExclusive
                startDate,
                endDate,
                reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)), // LastPaymentDate
                reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9))  // LastAppearanceDate
            ));
        }
        
        return contracts;
    }

    /// <summary>
    /// Met à jour la date de dernier paiement mensuel pour un contrat
    /// </summary>
    public void MettreAJourDatePaiement(string contractId, DateTime paymentDate)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE contracts 
            SET LastPaymentDate = $paymentDate
            WHERE contract_id = $contractId;
            """;
        command.Parameters.AddWithValue("$contractId", contractId);
        command.Parameters.AddWithValue("$paymentDate", paymentDate.ToString("yyyy-MM-dd"));
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Met à jour la date de dernière apparition payée pour un contrat
    /// </summary>
    public void MettreAJourDateApparition(string contractId, DateTime appearanceDate)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            UPDATE contracts 
            SET LastAppearanceDate = $appearanceDate
            WHERE contract_id = $contractId;
            """;
        command.Parameters.AddWithValue("$contractId", contractId);
        command.Parameters.AddWithValue("$appearanceDate", appearanceDate.ToString("yyyy-MM-dd"));
        command.ExecuteNonQuery();
    }
}
