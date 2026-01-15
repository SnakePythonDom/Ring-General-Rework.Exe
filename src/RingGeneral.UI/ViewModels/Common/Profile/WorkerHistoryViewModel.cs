using ReactiveUI;
using RingGeneral.Core.Models;
using System.Collections.ObjectModel;

namespace RingGeneral.UI.ViewModels.Common.Profile
{
    public class WorkerHistoryViewModel : ViewModelBase
    {
        private readonly Worker _worker;

        public ObservableCollection<MatchHistoryItem> RecentMatches { get; } = new();
        public ObservableCollection<string> Chronology { get; } = new(); // Mock string list for now

        public WorkerHistoryViewModel(Worker worker)
        {
            _worker = worker;
            LoadHistory();
        }

        private void LoadHistory()
        {
            RecentMatches.Clear();
            if (_worker.MatchHistory != null)
            {
                foreach (var match in _worker.MatchHistory.OrderByDescending(m => m.MatchDate).Take(10))
                {
                    RecentMatches.Add(match);
                }
            }

            // Mock Chronology
            Chronology.Add("2025 - Present: Ring General (Main Roster)");
            Chronology.Add("2023 - 2025: Youth Development Center");
        }

        public int WorldTitleCount => 0; // Placeholder
        public int SecondaryTitleCount => 0; // Placeholder
        public string WinLossRecord => $"{_worker.TotalWins} - {_worker.TotalLosses}";
        public int MainEventCount => 0; // Placeholder
        public int FiveStarMatches => 0; // Placeholder
        public string AverageRating => "75%"; // Placeholder
    }
}
