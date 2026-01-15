using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class MatchTypeOptionViewModel
{
    public MatchTypeOptionViewModel(MatchTypeDefinition definition)
    {
        Definition = definition;
        MatchTypeId = definition.MatchTypeId;
        Libelle = definition.Libelle;
        Description = definition.Description ?? string.Empty;
        Participants = definition.Participants;
        DureeParDefaut = definition.DureeParDefaut;
    }

    public MatchTypeDefinition Definition { get; }
    public string MatchTypeId { get; }
    public string Libelle { get; }
    public string Description { get; }
    public int? Participants { get; }
    public int? DureeParDefaut { get; }
}
