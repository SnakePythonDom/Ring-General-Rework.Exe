using System;
using System.Collections.Generic;
using RingGeneral.Core.Models;

namespace RingGeneral.Core.Services;

public sealed class TemplateService
{
    public SegmentDefinition AppliquerTemplate(SegmentTemplate template)
    {
        var segmentId = $"SEG-{Guid.NewGuid():N}".ToUpperInvariant();
        return new SegmentDefinition(
            segmentId,
            template.TypeSegment,
            Array.Empty<string>(),
            Math.Max(1, template.DureeMinutes),
            template.EstMainEvent,
            null,
            null,
            template.Intensite,
            null,
            null);
    }

    /// <summary>
    /// Charge les templates de segments disponibles.
    /// </summary>
    public IReadOnlyList<SegmentTemplate> LoadTemplates()
    {
        // TODO: Charger depuis le repository ou fichier de configuration
        return Array.Empty<SegmentTemplate>();
    }

    /// <summary>
    /// Phase 3.1 - Charge les templates de contrats disponibles
    /// </summary>
    public IReadOnlyList<ContractTemplate> LoadContractTemplates()
    {
        // TODO: Charger depuis la DB si disponible
        // Pour l'instant, retourner les templates par défaut
        return new List<ContractTemplate>
        {
            new ContractTemplate(
                "TPL_MAIN_EVENT",
                "Main Event Star",
                "Contrat pour une star principale (salaire mensuel élevé + frais d'apparition)",
                50_000m,
                5_000m,
                24, // 2 ans
                true,
                true),
            new ContractTemplate(
                "TPL_MID_CARD",
                "Mid-Card Regular",
                "Contrat pour un talent régulier de la midcard (salaire mensuel modéré)",
                15_000m,
                1_500m,
                12, // 1 an
                true,
                false),
            new ContractTemplate(
                "TPL_UNDERCARD",
                "Undercard",
                "Contrat pour un talent de l'undercard (salaire mensuel basique)",
                5_000m,
                500m,
                6, // 6 mois
                false,
                false),
            new ContractTemplate(
                "TPL_TRAINEE",
                "Trainee",
                "Contrat de développement pour un trainee (salaire mensuel minimal)",
                2_000m,
                0m,
                12, // 1 an
                true,
                true)
        };
    }
}
