using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Notes tab.
/// Manages booker notes and observations.
/// </summary>
public sealed class NotesTabViewModel : ViewModelBase
{
    private readonly INotesRepository _repository;

    private int _workerId;
    private ObservableCollection<WorkerNote> _notes = new();
    private WorkerNote? _selectedNote;
    private string _newNoteText = string.Empty;
    private NoteCategory _newNoteCategory = NoteCategory.Other;

    public NotesTabViewModel(INotesRepository repository)
    {
        _repository = repository;

        // Commands
        AddNoteCommand = ReactiveCommand.Create(AddNote);
        EditNoteCommand = ReactiveCommand.Create(EditNote);
        DeleteNoteCommand = ReactiveCommand.Create(DeleteNote);
        FilterByCategoryCommand = ReactiveCommand.Create<NoteCategory>(FilterByCategory);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    public int WorkerId
    {
        get => _workerId;
        private set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    /// <summary>
    /// All notes for this worker
    /// </summary>
    public ObservableCollection<WorkerNote> Notes
    {
        get => _notes;
        private set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    /// <summary>
    /// Selected note
    /// </summary>
    public WorkerNote? SelectedNote
    {
        get => _selectedNote;
        set => this.RaiseAndSetIfChanged(ref _selectedNote, value);
    }

    /// <summary>
    /// New note text (for adding)
    /// </summary>
    public string NewNoteText
    {
        get => _newNoteText;
        set => this.RaiseAndSetIfChanged(ref _newNoteText, value);
    }

    /// <summary>
    /// New note category (for adding)
    /// </summary>
    public NoteCategory NewNoteCategory
    {
        get => _newNoteCategory;
        set => this.RaiseAndSetIfChanged(ref _newNoteCategory, value);
    }

    /// <summary>
    /// Booking ideas notes
    /// </summary>
    public ObservableCollection<WorkerNote> BookingIdeasNotes =>
        new(Notes.Where(n => n.Category == NoteCategory.BookingIdeas));

    /// <summary>
    /// Personal notes
    /// </summary>
    public ObservableCollection<WorkerNote> PersonalNotes =>
        new(Notes.Where(n => n.Category == NoteCategory.Personal));

    /// <summary>
    /// Injury notes
    /// </summary>
    public ObservableCollection<WorkerNote> InjuryNotes =>
        new(Notes.Where(n => n.Category == NoteCategory.Injury));

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> AddNoteCommand { get; }
    public ReactiveCommand<Unit, Unit> EditNoteCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteNoteCommand { get; }
    public ReactiveCommand<NoteCategory, Unit> FilterByCategoryCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        // Load all notes
        var notes = _repository.GetNotesForWorker(workerId);
        Notes = new ObservableCollection<WorkerNote>(notes);

        RefreshCategoryCollections();
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void AddNote()
    {
        if (WorkerId == 0 || string.IsNullOrWhiteSpace(NewNoteText))
            return;

        var note = new WorkerNote
        {
            WorkerId = WorkerId,
            Text = NewNoteText,
            Category = NewNoteCategory,
            CreatedDate = DateTime.Now
        };

        _repository.CreateNote(note);

        // Clear input and reload
        NewNoteText = string.Empty;
        LoadWorker(WorkerId);
    }

    private void EditNote()
    {
        if (SelectedNote == null) return;
        System.Console.WriteLine($"[NotesTab] Edit note {SelectedNote.Id}");
        // TODO: Show edit note dialog
    }

    private void DeleteNote()
    {
        if (SelectedNote == null) return;
        _repository.DeleteNote(SelectedNote.Id);
        LoadWorker(WorkerId);
    }

    private void FilterByCategory(NoteCategory category)
    {
        var filtered = _repository.GetNotesByCategory(WorkerId, category);
        Notes = new ObservableCollection<WorkerNote>(filtered);
    }

    private void RefreshCategoryCollections()
    {
        this.RaisePropertyChanged(nameof(BookingIdeasNotes));
        this.RaisePropertyChanged(nameof(PersonalNotes));
        this.RaisePropertyChanged(nameof(InjuryNotes));
    }
}
