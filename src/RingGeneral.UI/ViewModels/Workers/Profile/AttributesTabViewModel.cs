using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models.Attributes;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Workers.Profile;

/// <summary>
/// ViewModel for Attributes tab (30 attributes display and edit).
/// Manages InRing, Entertainment, and Story attributes.
/// </summary>
public sealed class AttributesTabViewModel : ViewModelBase
{
    private readonly IWorkerAttributesRepository _repository;

    private int _workerId;
    private WorkerInRingAttributes? _inRingAttributes;
    private WorkerEntertainmentAttributes? _entertainmentAttributes;
    private WorkerStoryAttributes? _storyAttributes;
    private bool _isEditMode;

    public AttributesTabViewModel(IWorkerAttributesRepository repository)
    {
        _repository = repository;

        // Commands
        ToggleEditModeCommand = ReactiveCommand.Create(ToggleEditMode);
        SaveAttributesCommand = ReactiveCommand.Create(SaveAttributes);
        CancelEditCommand = ReactiveCommand.Create(CancelEdit);
    }

    // ====================================================================
    // PROPERTIES
    // ====================================================================

    /// <summary>
    /// Worker ID
    /// </summary>
    public int WorkerId
    {
        get => _workerId;
        private set => this.RaiseAndSetIfChanged(ref _workerId, value);
    }

    /// <summary>
    /// In-Ring attributes (10 attributes)
    /// </summary>
    public WorkerInRingAttributes? InRingAttributes
    {
        get => _inRingAttributes;
        private set => this.RaiseAndSetIfChanged(ref _inRingAttributes, value);
    }

    /// <summary>
    /// Entertainment attributes (10 attributes)
    /// </summary>
    public WorkerEntertainmentAttributes? EntertainmentAttributes
    {
        get => _entertainmentAttributes;
        private set => this.RaiseAndSetIfChanged(ref _entertainmentAttributes, value);
    }

    /// <summary>
    /// Story attributes (10 attributes)
    /// </summary>
    public WorkerStoryAttributes? StoryAttributes
    {
        get => _storyAttributes;
        private set => this.RaiseAndSetIfChanged(ref _storyAttributes, value);
    }

    /// <summary>
    /// Overall average (InRing + Entertainment + Story) / 3
    /// </summary>
    public int OverallAverage
    {
        get
        {
            if (InRingAttributes == null || EntertainmentAttributes == null || StoryAttributes == null)
                return 0;

            return (InRingAttributes.InRingAvg + EntertainmentAttributes.EntertainmentAvg + StoryAttributes.StoryAvg) / 3;
        }
    }

    /// <summary>
    /// Is edit mode enabled?
    /// </summary>
    public bool IsEditMode
    {
        get => _isEditMode;
        private set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
    }

    // ====================================================================
    // COMMANDS
    // ====================================================================

    public ReactiveCommand<Unit, Unit> ToggleEditModeCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveAttributesCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }

    // ====================================================================
    // PUBLIC METHODS
    // ====================================================================

    /// <summary>
    /// Load attributes for a worker
    /// </summary>
    public void LoadWorker(int workerId)
    {
        WorkerId = workerId;

        var (inRing, entertainment, story) = _repository.GetAllAttributes(workerId);

        InRingAttributes = inRing;
        EntertainmentAttributes = entertainment;
        StoryAttributes = story;

        this.RaisePropertyChanged(nameof(OverallAverage));
    }

    /// <summary>
    /// Update a specific attribute value
    /// </summary>
    public void UpdateAttribute(string category, string attributeName, int value)
    {
        if (WorkerId == 0) return;

        switch (category.ToLower())
        {
            case "inring":
                _repository.UpdateInRingAttribute(WorkerId, attributeName, value);
                break;
            case "entertainment":
                _repository.UpdateEntertainmentAttribute(WorkerId, attributeName, value);
                break;
            case "story":
                _repository.UpdateStoryAttribute(WorkerId, attributeName, value);
                break;
        }

        // Reload to reflect changes
        LoadWorker(WorkerId);
    }

    // ====================================================================
    // PRIVATE METHODS
    // ====================================================================

    private void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
    }

    private void SaveAttributes()
    {
        if (WorkerId == 0) return;

        // Save all attributes
        if (InRingAttributes != null)
            _repository.SaveInRingAttributes(InRingAttributes);

        if (EntertainmentAttributes != null)
            _repository.SaveEntertainmentAttributes(EntertainmentAttributes);

        if (StoryAttributes != null)
            _repository.SaveStoryAttributes(StoryAttributes);

        IsEditMode = false;
        LoadWorker(WorkerId); // Reload to show saved values
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        LoadWorker(WorkerId); // Reload to discard changes
    }
}
