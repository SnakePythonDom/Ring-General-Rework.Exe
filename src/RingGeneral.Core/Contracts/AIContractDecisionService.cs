using RingGeneral.Core.Models;

namespace RingGeneral.Core.Contracts;

public sealed class AIContractDecisionService
{
    public ContractAiDecision EvaluerOffre(ContractOfferDraft draft, ContractAiContext contexte)
    {
        if (contexte.Morale < 20)
        {
            return new ContractAiDecision("refuser", null, "Le talent n'est pas motivé à négocier.");
        }

        if (draft.SalaireHebdo <= contexte.BudgetHebdo && contexte.BesoinRoster >= 0)
        {
            return new ContractAiDecision("accepter", null, "L'offre est dans le budget et répond au besoin.");
        }

        var seuilRefus = contexte.BudgetHebdo * 1.4m;
        if (draft.SalaireHebdo > seuilRefus)
        {
            return new ContractAiDecision("refuser", null, "L'offre dépasse largement le budget disponible.");
        }

        var salaireAjuste = Math.Min(draft.SalaireHebdo, contexte.BudgetHebdo);
        var duree = Math.Max(4, draft.EndWeek - draft.StartWeek - 4);
        var nouvelleFin = draft.StartWeek + duree;

        var contre = draft with
        {
            SalaireHebdo = salaireAjuste,
            EndWeek = nouvelleFin,
            ExpirationDelaiSemaines = Math.Max(1, draft.ExpirationDelaiSemaines - 1)
        };

        return new ContractAiDecision("contre", contre, "Contre-proposition ajustée au budget et à la durée.");
    }
}
