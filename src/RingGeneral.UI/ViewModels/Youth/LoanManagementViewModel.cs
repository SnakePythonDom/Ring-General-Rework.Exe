using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Models;
using RingGeneral.Data.Repositories;

namespace RingGeneral.UI.ViewModels.Youth;

public sealed class LoanManagementViewModel : ViewModelBase
{
    private readonly GameRepository _gameRepository;
    private readonly YouthRepository _youthRepository;

    public ObservableCollection<LoanedWorkerItemViewModel> LoanedWorkers { get; } = new();

    public LoanManagementViewModel(GameRepository gameRepo, YouthRepository youthRepo)
    {
        _gameRepository = gameRepo;
        _youthRepository = youthRepo;
    }

    public void LoadLoanedWorkers()
    {
        LoanedWorkers.Clear();
        var structures = _youthRepository.ChargerYouthStructures();

        foreach (var structure in structures)
        {
            // Get all workers in the development company
            var roster = _gameRepository.ChargerBackstageRoster(structure.CompagnieId);

            // Get trainees and staff to exclude them
            var trainees = _youthRepository.ChargerYouthTrainees(structure.YouthId)
                                           .Select(t => t.WorkerId)
                                           .ToHashSet();

            var staff = _youthRepository.ChargerYouthStaffAssignments(structure.YouthId)
                                        .Select(s => s.WorkerId)
                                        .ToHashSet();

            foreach (var worker in roster)
            {
                if (!trainees.Contains(worker.WorkerId) && !staff.Contains(worker.WorkerId))
                {
                    // This worker is in the development company but is not a trainee or staff.
                    // Effectively a "Loaned" worker or regular roster member of the child company.
                    LoanedWorkers.Add(new LoanedWorkerItemViewModel
                    {
                        WorkerId = worker.WorkerId,
                        Name = worker.Nom, // WorkerBackstageProfile has Nom (which is full name from query)
                        CurrentCompany = structure.Nom, // Using Structure Name as location
                        MonthsAway = 0 // Placeholder
                    });
                }
            }
        }
    }
}

public sealed class LoanedWorkerItemViewModel : ViewModelBase
{
    public required string WorkerId { get; init; }
    public required string Name { get; init; }
    public required string CurrentCompany { get; init; }
    public int MonthsAway { get; set; } // Placeholder

    public ReactiveCommand<Unit, Unit> RecallCommand { get; }

    public LoanedWorkerItemViewModel()
    {
        RecallCommand = ReactiveCommand.Create(Recall);
    }

    private void Recall()
    {
        // Implement recall logic
    }
}
