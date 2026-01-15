using System.Collections.ObjectModel;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Youth;

public class YouthAlumniViewModel : ViewModelBase
{
    private readonly YouthRepository _youthRepository;
    private readonly string _structureId;

    public ObservableCollection<YouthAlumniItemViewModel> Alumni { get; } = new();

    private string _avgTrainingTime = "N/A";
    public string AvgTrainingTime
    {
        get => _avgTrainingTime;
        set => this.RaiseAndSetIfChanged(ref _avgTrainingTime, value);
    }

    private string _successRate = "N/A";
    public string SuccessRate
    {
        get => _successRate;
        set => this.RaiseAndSetIfChanged(ref _successRate, value);
    }

    private string _bestStyle = "N/A";
    public string BestStyle
    {
        get => _bestStyle;
        set => this.RaiseAndSetIfChanged(ref _bestStyle, value);
    }

    public YouthAlumniViewModel(YouthRepository youthRepository, string structureId)
    {
        _youthRepository = youthRepository;
        _structureId = structureId;

        // Dummy stats for now as we don't have full history tracking
        AvgTrainingTime = "14 Months";
        SuccessRate = "12% (Graduated vs Dropouts)";
        BestStyle = "Brawler (+10% Bonus)";

        LoadAlumni();
    }

    public void LoadAlumni()
    {
        Alumni.Clear();
        var graduates = _youthRepository.ChargerAlumni(_structureId);
        foreach (var grad in graduates)
        {
            Alumni.Add(new YouthAlumniItemViewModel(grad));
        }
    }
}

public class YouthAlumniItemViewModel : ViewModelBase
{
    private readonly YouthAlumniInfo _info;

    public string Name => _info.Nom;
    // Assuming GraduationDate is an integer week number, converting to a dummy date string or strictly week
    public string GraduationDate => $"Week {_info.GraduationDate}";
    public string StyleArchetype => _info.CurrentStyle;
    public string RatingAtGrad => "N/A"; // Not tracked yet
    public string CurrentRating => CalculateOverall(_info.InRing, _info.Entertainment, _info.Story).ToString();

    public YouthAlumniItemViewModel(YouthAlumniInfo info)
    {
        _info = info;
    }

    private int CalculateOverall(int inRing, int ent, int story)
    {
        return (inRing + ent + story) / 3;
    }
}
