using ReactiveUI;
using System.Linq;
using System.Reactive;
using RingGeneral.Core.Models;
using System.Collections.ObjectModel;

namespace RingGeneral.UI.ViewModels.Common.Profile
{
    public class WorkerNotesViewModel : ViewModelBase
    {
        private readonly Worker _worker;
        private string _userNotes = string.Empty; // Backing field for UserNotes

        public ObservableCollection<string> AutoNotes { get; } = new();
        public string UserNotes
        {
            get => _userNotes;
            set => this.RaiseAndSetIfChanged(ref _userNotes, value); // Should persist to Worker.Notes ideally
        }

        public string StaffSuggestions => "• Consider feuding with 'The Giant' to improve popularity.\n• Needs to work on 'Mic Skills'."; // Placeholder

        public ReactiveCommand<Unit, Unit> SaveNotesCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearNotesCommand { get; }

        public WorkerNotesViewModel(Worker worker)
        {
            _worker = worker;
            // Load existing notes logic if applicable
            // Assuming Worker.Notes is a collection of Note objects with CreatedDate and Text properties
            _userNotes = string.Join("\n", _worker.Notes.Select(n => $"{n.CreatedDate:d}: {n.Text}"));

            SaveNotesCommand = ReactiveCommand.Create(() =>
            {
                // Logic to save notes back to Worker object would go here
                System.Diagnostics.Debug.WriteLine($"Saving notes: {_userNotes}");
            });

            ClearNotesCommand = ReactiveCommand.Create(() =>
           {
               UserNotes = "";
           });

            // Original AutoNotes population from LoadNotes()
            AutoNotes.Clear();
            AutoNotes.Add("Creative Suggestion: Push to Upper Midcard due to high momentum.");
            AutoNotes.Add("Chemistry: Great chemistry with [Random Worker].");
        }
    }
}
