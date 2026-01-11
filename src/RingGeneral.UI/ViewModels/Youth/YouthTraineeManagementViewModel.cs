using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class YouthTraineeManagementViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly GameRepository _gameRepository;

    public ObservableCollection<YouthTraineeItemViewModel> Trainees { get; } = new();

    public YouthTraineeManagementViewModel(YouthRepository youthRepository, GameRepository gameRepository)
    {
        _youthRepository = youthRepository;
        _gameRepository = gameRepository;
        LoadTrainees();
    }

    public void LoadTrainees()
    {
        Trainees.Clear();
        // Uses "ChargerYouthTraineesPourProgression" as a way to get all active trainees
        var trainees = _youthRepository.ChargerYouthTraineesPourProgression();

        // We might need to map Structure Name from YouthId. 
        // For efficiency, maybe load structures once.
        var structures = _youthRepository.ChargerYouthStructures().ToDictionary(s => s.YouthId, s => s.Nom);

        foreach (var t in trainees)
        {
            var structureName = structures.TryGetValue(t.YouthId, out var sName) ? sName : t.YouthId;
            Trainees.Add(new YouthTraineeItemViewModel(t, structureName));
        }
    }
}

public sealed class YouthTraineeItemViewModel : ViewModelBase
{
    public string WorkerId { get; }
    public string Name { get; }
    public string StructureName { get; }
    public int InRing { get; }
    public int Entertainment { get; }
    public int Story { get; }
    public string Statut { get; }
    public int SemaineInscription { get; }

    public string AttributesDisplay => $"🤼 {InRing} | 🎤 {Entertainment} | 📖 {Story}";

    // Calculated Progress (Placeholder logic, or derived from attributes)
    public int Average => (InRing + Entertainment + Story) / 3;

    public YouthTraineeItemViewModel(YouthTraineeProgressionState state, string structureName)
    {
        WorkerId = state.WorkerId;
        Name = state.Nom;
        StructureName = structureName;
        InRing = state.InRing;
        Entertainment = state.Entertainment;
        Story = state.Story;
        Statut = state.Statut;
        SemaineInscription = state.SemaineInscription;
    }
}
