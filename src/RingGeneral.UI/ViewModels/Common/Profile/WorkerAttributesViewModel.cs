using ReactiveUI;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels.Common.Profile
{
    public class WorkerAttributesViewModel : ViewModelBase
    {
        private readonly Worker _worker;

        public WorkerAttributesViewModel(Worker worker)
        {
            _worker = worker;
        }

        // In-Ring
        public int Striking => _worker.InRingAttributes?.Striking ?? 50;
        public int Grappling => _worker.InRingAttributes?.Grappling ?? 50;
        public int HighFlying => _worker.InRingAttributes?.HighFlying ?? 50;
        public int Powerhouse => _worker.InRingAttributes?.Powerhouse ?? 50;
        // Technical is covered by Grappling/Timing in this model
        public int Stamina => _worker.InRingAttributes?.Stamina ?? 50;
        public int Selling => _worker.InRingAttributes?.Selling ?? 50;
        public int Safety => _worker.InRingAttributes?.Safety ?? 50;
        public int Psychology => _worker.InRingAttributes?.Psychology ?? 50;

        // Entertainment
        public int Charisma => _worker.EntertainmentAttributes?.Charisma ?? 50;
        public int MicWork => _worker.EntertainmentAttributes?.MicWork ?? 50;
        public int Acting => _worker.EntertainmentAttributes?.Acting ?? 50;
        public int StarQuality => _worker.EntertainmentAttributes?.StarPower ?? 50;
        public int SexAppeal => _worker.EntertainmentAttributes?.SexAppeal ?? 50;

        // Story (Mock/Placeholder for now as these might not be in deep model yet)
        public int Storytelling => _worker.StoryAttributes?.StorytellingLongTerm ?? 50;
        public int CharacterWork => _worker.StoryAttributes?.CharacterDepth ?? 50;

        public int InRingAvg => _worker.InRingAttributes?.InRingAvg ?? 50;
        public int EntertainmentAvg => _worker.EntertainmentAttributes?.EntertainmentAvg ?? 50;
        public int StoryAvg => _worker.StoryAttributes?.StoryAvg ?? 50;
    }
}
