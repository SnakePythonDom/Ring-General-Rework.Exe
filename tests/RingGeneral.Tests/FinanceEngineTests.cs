using System.Linq;
using RingGeneral.Core.Models;
using RingGeneral.Core.Simulation;
using Xunit;

namespace RingGeneral.Tests;

public sealed class FinanceEngineTests
{
    [Fact]
    public void Show_genere_recettes_et_depenses()
    {
        var engine = new FinanceEngine(FinanceSettings.V1());
        var compagnie = new CompanyState("COMP-1", "Compagnie", "FR", 55, 10000, 50, 5);
        var context = new ShowFinanceContext(
            compagnie,
            Audience: 62,
            DureeMinutes: 120,
            PopularitesWorkers: new[] { 75, 68, 60 },
            DiffuseTv: true);

        var result = engine.CalculerFinancesShow(context);

        Assert.Contains(result.Transactions, tx => tx.Montant > 0);
        Assert.Contains(result.Transactions, tx => tx.Montant < 0);
    }

    [Fact]
    public void Paie_hebdo_et_mensuelle_sont_appliquees_selon_semaine()
    {
        var engine = new FinanceEngine(FinanceSettings.V1());
        var contrats = new[]
        {
            new ContractPayroll("W-1", "Alpha", 1000, PayrollFrequency.Hebdomadaire),
            new ContractPayroll("W-2", "Beta", 2400, PayrollFrequency.Mensuelle)
        };

        var semaine3 = new WeeklyFinanceContext("COMP-1", 3, 5000, contrats);
        var resultat3 = engine.CalculerFinancesHebdo(semaine3);
        var montant3 = resultat3.Transactions.Single().Montant;

        var semaine4 = new WeeklyFinanceContext("COMP-1", 4, 5000, contrats);
        var resultat4 = engine.CalculerFinancesHebdo(semaine4);
        var montant4 = resultat4.Transactions.Single().Montant;

        Assert.Equal(-1000, montant3);
        Assert.Equal(-3400, montant4);
    }
}
