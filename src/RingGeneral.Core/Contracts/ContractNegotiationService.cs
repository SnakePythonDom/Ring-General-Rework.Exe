using System.Globalization;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Contracts;

public sealed class ContractNegotiationService
{
    private readonly IContractRepository _repository;

    public ContractNegotiationService(IContractRepository repository)
    {
        _repository = repository;
    }

    public ContractOffer CreerOffre(ContractOfferDraft draft, int semaineCourante, bool estIa)
    {
        ValiderDraft(draft);

        var negociation = _repository.ChargerNegociationPourWorker(draft.WorkerId, draft.CompanyId)
            ?? new ContractNegotiationState(
                Guid.NewGuid().ToString("N"),
                draft.WorkerId,
                draft.CompanyId,
                "en_cours",
                null,
                semaineCourante);

        var offerId = Guid.NewGuid().ToString("N");
        var expirationWeek = semaineCourante + Math.Max(1, draft.ExpirationDelaiSemaines);

        var offre = new ContractOffer(
            offerId,
            negociation.NegociationId,
            draft.WorkerId,
            draft.CompanyId,
            draft.TypeContrat,
            draft.StartWeek,
            draft.EndWeek,
            draft.SalaireHebdo,
            draft.BonusShow,
            draft.Buyout,
            draft.NonCompeteWeeks,
            draft.RenouvellementAuto,
            draft.EstExclusif,
            "proposee",
            semaineCourante,
            expirationWeek,
            null,
            estIa);

        var clauses = ConstruireClauses(offre);
        _repository.AjouterOffre(offre, clauses);

        var updatedNegociation = negociation with
        {
            Statut = "en_cours",
            DerniereOffreId = offerId,
            DerniereMiseAJourSemaine = semaineCourante
        };
        _repository.EnregistrerNegociation(updatedNegociation);

        return offre;
    }

    public ContractOffer ContreProposer(string offerId, ContractOfferDraft draft, int semaineCourante)
    {
        ValiderDraft(draft);

        var offreInitiale = _repository.ChargerOffre(offerId)
            ?? throw new InvalidOperationException("Offre introuvable pour la contre-proposition.");

        _repository.MettreAJourStatutOffre(offerId, "contree");

        var nouvelleOffre = new ContractOffer(
            Guid.NewGuid().ToString("N"),
            offreInitiale.NegociationId,
            draft.WorkerId,
            draft.CompanyId,
            draft.TypeContrat,
            draft.StartWeek,
            draft.EndWeek,
            draft.SalaireHebdo,
            draft.BonusShow,
            draft.Buyout,
            draft.NonCompeteWeeks,
            draft.RenouvellementAuto,
            draft.EstExclusif,
            "contre",
            semaineCourante,
            semaineCourante + Math.Max(1, draft.ExpirationDelaiSemaines),
            offerId,
            false);

        var clauses = ConstruireClauses(nouvelleOffre);
        _repository.AjouterOffre(nouvelleOffre, clauses);

        var negociation = _repository.ChargerNegociation(offreInitiale.NegociationId)
            ?? new ContractNegotiationState(offreInitiale.NegociationId, draft.WorkerId, draft.CompanyId, "en_cours", null, semaineCourante);

        _repository.EnregistrerNegociation(negociation with
        {
            Statut = "en_cours",
            DerniereOffreId = nouvelleOffre.OfferId,
            DerniereMiseAJourSemaine = semaineCourante
        });

        return nouvelleOffre;
    }

    public ActiveContract AccepterOffre(string offerId, int semaineCourante)
    {
        var offre = _repository.ChargerOffre(offerId)
            ?? throw new InvalidOperationException("Offre introuvable pour acceptation.");

        _repository.MettreAJourStatutOffre(offerId, "acceptee");

        var contrat = new ActiveContract(
            Guid.NewGuid().ToString("N"),
            offre.WorkerId,
            offre.CompanyId,
            offre.TypeContrat,
            offre.StartWeek,
            offre.EndWeek,
            offre.SalaireHebdo,
            offre.BonusShow,
            offre.Buyout,
            offre.NonCompeteWeeks,
            offre.RenouvellementAuto,
            offre.EstExclusif,
            "actif",
            semaineCourante);

        var clauses = _repository.ChargerClausesPourOffre(offerId);
        _repository.AjouterContratActif(contrat, clauses);

        var negociation = _repository.ChargerNegociation(offre.NegociationId);
        if (negociation is not null)
        {
            _repository.EnregistrerNegociation(negociation with
            {
                Statut = "acceptee",
                DerniereOffreId = offerId,
                DerniereMiseAJourSemaine = semaineCourante
            });
        }

        return contrat;
    }

    public void RefuserOffre(string offerId, int semaineCourante)
    {
        var offre = _repository.ChargerOffre(offerId)
            ?? throw new InvalidOperationException("Offre introuvable pour refus.");

        _repository.MettreAJourStatutOffre(offerId, "refusee");

        var negociation = _repository.ChargerNegociation(offre.NegociationId);
        if (negociation is not null)
        {
            _repository.EnregistrerNegociation(negociation with
            {
                Statut = "refusee",
                DerniereOffreId = offerId,
                DerniereMiseAJourSemaine = semaineCourante
            });
        }
    }

    public void LibererContrat(string contractId, int semaineCourante)
    {
        _repository.ResilierContrat(contractId, semaineCourante);
    }

    private static void ValiderDraft(ContractOfferDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.WorkerId))
        {
            throw new InvalidOperationException("WorkerId requis pour une offre.");
        }

        if (string.IsNullOrWhiteSpace(draft.CompanyId))
        {
            throw new InvalidOperationException("CompanyId requis pour une offre.");
        }

        if (draft.EndWeek <= draft.StartWeek)
        {
            throw new InvalidOperationException("La durée du contrat est invalide.");
        }

        if (draft.SalaireHebdo < 0)
        {
            throw new InvalidOperationException("Le salaire hebdo doit être positif.");
        }

        if (draft.ExpirationDelaiSemaines <= 0)
        {
            throw new InvalidOperationException("Le délai d'expiration doit être supérieur à 0.");
        }
    }

    private static IReadOnlyList<ContractClause> ConstruireClauses(ContractOffer offre)
    {
        return new List<ContractClause>
        {
            new("duree", (offre.EndWeek - offre.StartWeek).ToString(CultureInfo.InvariantCulture)),
            new("salaire", offre.SalaireHebdo.ToString(CultureInfo.InvariantCulture)),
            new("bonus_show", offre.BonusShow.ToString(CultureInfo.InvariantCulture)),
            new("buyout", offre.Buyout.ToString(CultureInfo.InvariantCulture)),
            new("non_compete", offre.NonCompeteWeeks.ToString(CultureInfo.InvariantCulture)),
            new("renouvellement_auto", offre.RenouvellementAuto ? "1" : "0")
        };
    }
}
