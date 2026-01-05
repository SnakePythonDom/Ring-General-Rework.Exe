namespace RingGeneral.Specs.Models;

public sealed class RoadmapSpec
{
    public MetaSpec Meta { get; set; } = new();
    public List<RoadmapStepSpec> Etapes { get; set; } = [];
}

public sealed class RoadmapStepSpec
{
    public string Id { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Objectif { get; set; } = string.Empty;
    public List<string> Fonctionnalites { get; set; } = [];
    public List<string> Boutons { get; set; } = [];
    public List<string> Onglets { get; set; } = [];
    public string Statut { get; set; } = "a_faire";
}
