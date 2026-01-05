using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IContractRepository
{
    void AjouterOffre(ContractOffer offre, IReadOnlyList<ContractClause> clauses);
    ContractOffer? ChargerOffre(string offerId);
    IReadOnlyList<ContractOffer> ChargerOffres(string companyId, int offset, int limit);
    IReadOnlyList<ContractOffer> ChargerOffresExpirant(int semaine);
    IReadOnlyList<ContractClause> ChargerClausesPourOffre(string offerId);
    void MettreAJourStatutOffre(string offerId, string statut);

    void AjouterContratActif(ActiveContract contrat, IReadOnlyList<ContractClause> clauses);
    ActiveContract? ChargerContratActif(string contractId);
    ActiveContract? ChargerContratActif(string workerId, string companyId);
    IReadOnlyList<ContractClause> ChargerClausesPourContrat(string contractId);
    void ResilierContrat(string contractId, int finSemaine);

    void EnregistrerNegociation(ContractNegotiationState negociation);
    ContractNegotiationState? ChargerNegociation(string negociationId);
    ContractNegotiationState? ChargerNegociationPourWorker(string workerId, string companyId);
}
