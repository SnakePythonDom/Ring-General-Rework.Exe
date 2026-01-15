using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace RingGeneral.UI.ViewModels.Booking;

public class WorkerSelectionViewModel : ViewModelBase
{
    private readonly IEnumerable<ParticipantViewModel> _allWorkers;
    private string _searchText = string.Empty;

    public WorkerSelectionViewModel(IEnumerable<ParticipantViewModel> workers)
    {
        _allWorkers = workers;
        FilteredWorkers = new ObservableCollection<ParticipantViewModel>(_allWorkers);

        SelectWorkerCommand = ReactiveCommand.Create<ParticipantViewModel>(SelectWorker);
        CancelCommand = ReactiveCommand.Create(CancelSelection);

        // Setup filtering
        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => FilterWorkers());
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public ObservableCollection<ParticipantViewModel> FilteredWorkers { get; }

    public ParticipantViewModel? SelectedWorker { get; private set; }

    public ReactiveCommand<ParticipantViewModel, Unit> SelectWorkerCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    // Action to callback when selection is confirmed
    public Action<ParticipantViewModel?>? OnSelectionConfirmed { get; set; }

    private void FilterWorkers()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            UpdateList(_allWorkers);
            return;
        }

        var filtered = _allWorkers.Where(w =>
            w.Nom.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        UpdateList(filtered);
    }

    private void UpdateList(IEnumerable<ParticipantViewModel> workers)
    {
        FilteredWorkers.Clear();
        foreach (var worker in workers)
        {
            FilteredWorkers.Add(worker);
        }
    }

    private void SelectWorker(ParticipantViewModel worker)
    {
        SelectedWorker = worker;
        OnSelectionConfirmed?.Invoke(worker);
    }

    private void CancelSelection()
    {
        SelectedWorker = null;
        OnSelectionConfirmed?.Invoke(null);
    }
}
