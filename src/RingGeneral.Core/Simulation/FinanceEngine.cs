using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class FinanceEngine
{
    private readonly FinanceSettings _settings;

    public FinanceEngine(FinanceSettings settings)
    {
        _settings = settings;
    }

    public FinanceShowResult CalculerFinancesShow(ShowFinanceContext context)
    {
        var capacite = CalculerCapacite(context.Compagnie);
        var tauxRemplissage = CalculerTauxRemplissage(context.Audience);
        var attendance = (int)Math.Round(capacite * tauxRemplissage);
        attendance = Math.Clamp(attendance, 0, capacite);

        var prixBillet = CalculerPrixBillet(context.Compagnie, context.Audience);
        var billetterie = Math.Round(attendance * prixBillet, 2);

        var merch = Math.Round(CalculerMerch(attendance, context.PopularitesWorkers), 2);

        var tv = context.DiffuseTv
            ? Math.Round(_settings.Tv.RevenuBase + context.Audience * _settings.Tv.RevenuParAudience, 2)
            : 0;

        var coutProduction = Math.Round(
            _settings.Production.CoutBase + (context.DureeMinutes * _settings.Production.CoutParMinute) + (attendance * _settings.Production.CoutParSpectateur),
            2);

        var transactions = new List<FinanceTransactionModel>
        {
            new("billetterie", (decimal)billetterie, "Billetterie"),
            new("merch", (decimal)merch, "Merchandising"),
            new("production", (decimal)-coutProduction, "Coûts de production")
        };

        if (tv > 0)
        {
            transactions.Add(new FinanceTransactionModel("tv", (decimal)tv, "Droits TV"));
        }

        return new FinanceShowResult((decimal)billetterie, (decimal)merch, (decimal)tv, (decimal)coutProduction, transactions);
    }

    public FinanceTickResult CalculerFinancesHebdo(WeeklyFinanceContext context)
    {
        var transactions = new List<FinanceTransactionModel>();
        var totalDepenses = 0.0;

        var totalPaie = CalculerPaie(context.Semaine, context.Contrats);
        if (totalPaie > 0)
        {
            transactions.Add(new FinanceTransactionModel("paie", (decimal)-totalPaie, "Paie des contrats"));
            totalDepenses += totalPaie;
        }

        return new FinanceTickResult(0m, (decimal)totalDepenses, transactions);
    }

    private int CalculerCapacite(CompanyState compagnie)
    {
        var capacite = _settings.Venue.CapaciteBase
                       + (compagnie.Reach * _settings.Venue.CapaciteParReach)
                       + (compagnie.Prestige * _settings.Venue.CapaciteParPrestige);

        return Math.Clamp(capacite, _settings.Venue.CapaciteMin, _settings.Venue.CapaciteMax);
    }

    private double CalculerTauxRemplissage(int audience)
    {
        var taux = _settings.Billetterie.TauxRemplissageBase
                   + ((audience - 50) * _settings.Billetterie.TauxRemplissageParPoint);

        return Math.Clamp(taux, _settings.Billetterie.TauxRemplissageMin, _settings.Billetterie.TauxRemplissageMax);
    }

    private double CalculerPrixBillet(CompanyState compagnie, int audience)
    {
        var prix = _settings.Billetterie.PrixBase
                   + (audience * _settings.Billetterie.PrixParAudience)
                   + (compagnie.Prestige * _settings.Billetterie.PrixParPrestige);

        return Math.Clamp(prix, _settings.Billetterie.PrixMin, _settings.Billetterie.PrixMax);
    }

    private double CalculerMerch(int attendance, IReadOnlyList<int> popularites)
    {
        if (attendance <= 0)
        {
            return 0;
        }

        var topStars = popularites
            .OrderByDescending(pop => pop)
            .Take(_settings.Merch.StarsPrisesEnCompte)
            .ToList();

        var starPower = topStars.Count == 0 ? 0 : topStars.Average() / 100.0;

        var multiplicateur = 1 + (starPower * _settings.Merch.MultiplicateurStars);
        return attendance * _settings.Merch.DepenseParFan * multiplicateur;
    }

    private double CalculerPaie(int semaine, IReadOnlyList<ContractPayroll> contrats)
    {
        if (contrats.Count == 0)
        {
            return 0;
        }

        var total = 0.0;
        foreach (var contrat in contrats)
        {
            var payer = contrat.Frequence switch
            {
                PayrollFrequency.Hebdomadaire => true,
                PayrollFrequency.Mensuelle => semaine % _settings.Paie.SemainesParMois == 0,
                _ => true
            };

            if (payer)
            {
                total += (double)contrat.Salaire;
            }
        }

        return Math.Round(total, 2);
    }
}
