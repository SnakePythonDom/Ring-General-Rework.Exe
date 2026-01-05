using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using Xunit;

namespace RingGeneral.Tests;

public sealed class TitleServiceTests
{
    [Fact]
    public void Changement_champion_cree_un_nouveau_regne()
    {
        var repository = new InMemoryTitleRepository();
        repository.CreerTitre(new TitleRecord("T-1", "COMP-1", "Titre Test", 50, "W-1"));
        repository.AjouterRegne(new TitleReignRecord(0, "T-1", "W-1", 1, null, true));
        var service = new TitleService(repository);

        var resultat = service.EnregistrerDefense(new TitleDefenseRequest(
            "T-1",
            "W-1",
            "W-2",
            "W-2",
            "W-1",
            10,
            "SHOW-1",
            "SEG-1"));

        Assert.True(resultat.ChampionChange);
        Assert.Equal(2, repository.Reigns.Count);
        Assert.Equal("W-2", repository.Titles["T-1"].HolderWorkerId);
        Assert.Equal(10, repository.Reigns[0].EndDate);
        Assert.True(repository.Reigns[1].IsCurrent);
    }

    [Fact]
    public void Prestige_varie_selon_les_defenses()
    {
        var repository = new InMemoryTitleRepository();
        repository.CreerTitre(new TitleRecord("T-1", "COMP-1", "Titre Test", 50, "W-1"));
        repository.AjouterRegne(new TitleReignRecord(0, "T-1", "W-1", 1, null, true));
        var service = new TitleService(repository);

        service.EnregistrerDefense(new TitleDefenseRequest(
            "T-1",
            "W-1",
            "W-2",
            "W-1",
            "W-2",
            2,
            null,
            null));

        service.EnregistrerDefense(new TitleDefenseRequest(
            "T-1",
            "W-1",
            "W-3",
            "W-3",
            "W-1",
            3,
            null,
            null));

        Assert.True(repository.PrestigeAdjustments[0].Delta > 0);
        Assert.True(repository.PrestigeAdjustments[1].Delta < 0);
    }

    private sealed class InMemoryTitleRepository : ITitleRepository
    {
        private int _reignId = 1;
        private int _matchId = 1;

        public Dictionary<string, TitleRecord> Titles { get; } = new();
        public List<TitleReignRecord> Reigns { get; } = new();
        public List<TitleMatchRecord> Matches { get; } = new();
        public List<(string TitleId, int Delta)> PrestigeAdjustments { get; } = new();

        public TitleRecord? ChargerTitre(string titleId)
        {
            return Titles.TryGetValue(titleId, out var title) ? title : null;
        }

        public IReadOnlyList<TitleRecord> ChargerTitresCompagnie(string companyId)
        {
            return Titles.Values.Where(title => title.CompanyId == companyId).ToList();
        }

        public void CreerTitre(TitleRecord title)
        {
            Titles[title.TitleId] = title;
        }

        public void MettreAJourTitre(TitleRecord title)
        {
            Titles[title.TitleId] = title;
        }

        public void SupprimerTitre(string titleId)
        {
            Titles.Remove(titleId);
            Reigns.RemoveAll(reign => reign.TitleId == titleId);
            Matches.RemoveAll(match => match.TitleId == titleId);
        }

        public TitleReignRecord? ChargerRegneActuel(string titleId)
        {
            return Reigns.LastOrDefault(reign => reign.TitleId == titleId && reign.IsCurrent);
        }

        public IReadOnlyList<TitleReignRecord> ChargerRegnes(string titleId)
        {
            return Reigns.Where(reign => reign.TitleId == titleId).ToList();
        }

        public int AjouterRegne(TitleReignRecord reign)
        {
            var created = reign with { TitleReignId = _reignId++ };
            Reigns.Add(created);
            return created.TitleReignId;
        }

        public void CloreRegne(int reignId, int endDate)
        {
            var index = Reigns.FindIndex(reign => reign.TitleReignId == reignId);
            if (index >= 0)
            {
                var reign = Reigns[index];
                Reigns[index] = reign with { EndDate = endDate, IsCurrent = false };
            }
        }

        public void MettreAJourDetenteur(string titleId, string? workerId)
        {
            if (Titles.TryGetValue(titleId, out var title))
            {
                Titles[titleId] = title with { HolderWorkerId = workerId };
            }
        }

        public void AjouterMatchTitre(TitleMatchRecord match)
        {
            Matches.Add(match with { TitleMatchId = _matchId++ });
        }

        public IReadOnlyList<TitleMatchRecord> ChargerMatchsTitrePourWorker(string workerId)
        {
            return Matches.Where(match =>
                match.ChampionWorkerId == workerId ||
                match.ChallengerWorkerId == workerId ||
                match.WinnerWorkerId == workerId ||
                match.LoserWorkerId == workerId).ToList();
        }

        public void AjusterPrestige(string titleId, int delta)
        {
            PrestigeAdjustments.Add((titleId, delta));
        }
    }
}
