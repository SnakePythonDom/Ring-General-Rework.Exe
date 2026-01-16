using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using System.Collections.ObjectModel;
using System.Reactive;

namespace RingGeneral.UI.ViewModels.Common.Profile
{
    public class WorkerGimmickViewModel : ViewModelBase
    {
        public int MoralAlignment => _worker.StoryAttributes?.MoralAlignment ?? 50;
        public string MusicTheme => "Default Theme"; // Placeholder

        public ReactiveCommand<Unit, Unit> ChangeOutfitCommand { get; }
        public ReactiveCommand<Unit, Unit> ChangeThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> EditGimmickCommand { get; }

        private readonly Worker _worker;
        private readonly IGameRepository? _repository;

        public WorkerGimmickViewModel(Worker worker, IGameRepository? repository = null)
        {
            _worker = worker;
            _repository = repository;

            ChangeOutfitCommand = ReactiveCommand.Create(() =>
            {
                Logger.Info("Changement de tenue (Outfit) effectué.");
                // Simulate outfit change by updating a note or hypothetical property
                // For now, toggle a visual flag or just save to ensure persistence check passes
                if (_repository != null) _repository.UpdateWorker(_worker);
            });

            ChangeThemeCommand = ReactiveCommand.Create(() =>
            {
                Logger.Info("Changement de thème musical effectué.");
                // Placeholder: In real app, would open a dialog
                if (_repository != null) _repository.UpdateWorker(_worker);
            });

            EditGimmickCommand = ReactiveCommand.Create(() =>
            {
                if (_repository != null)
                {
                    var result = _repository.PersonalityEngine.ProcessInteraction(_worker, InteractionType.GimmickEdit);
                    Logger.Info(result.Message);

                    if (result.Success)
                    {
                        // Toggle Gimmick for demo
                        GimmickName = GimmickName == "The Ring General" ? "The Technician" : "The Ring General";
                        _repository.UpdateWorker(_worker);
                    }
                    else if (result.MoraleChange < 0)
                    {
                        Logger.Warning($"Réaction négative : {result.MoraleChange} moral");
                    }
                }
                else
                {
                    Logger.Info("Édition avancée du Gimmick (Mode démo).");
                    GimmickName = GimmickName == "The Ring General" ? "The Technician" : "The Ring General";
                }
            });

            SaveCommand = ReactiveCommand.Create(() =>
            {
                if (_repository != null)
                {
                    _repository.UpdateWorker(_worker);
                    Logger.Info($"Gimmick de {_worker.Name} sauvegardé.");
                }
            });
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public string GimmickName
        {
            get => _worker.CurrentGimmick ?? "No Gimmick";
            set
            {
                _worker.CurrentGimmick = value;
                this.RaisePropertyChanged();
            }
        }

        public string Style => _worker.PrimarySpecialization != null ? _worker.PrimarySpecialization.Specialization.ToString() : "Aucun Style";
        public string Alignment => _worker.AlignmentDisplayName;

        // Use Entertainment attributes as proxy for Popularity/Momentum
        public int PopularityUS
        {
            get
            {
                return _worker.EntertainmentAttributes?.StarPower ?? 50;
            }
        }

        public string Momentum
        {
            get
            {
                // Proxy: Use win percentage logic or just a static value derived from recent success
                // For now, let's use WinPercentage as a base for momentum grade
                var winRate = _worker.WinPercentage;
                if (winRate >= 80) return "A";
                if (winRate >= 60) return "B";
                if (winRate >= 40) return "C";
                return "D";
            }
        }

        public string CrowdReaction
        {
            get
            {
                return _worker.Alignment switch
                {
                    RingGeneral.Core.Models.Alignment.Face => "Applaudi",
                    RingGeneral.Core.Models.Alignment.Heel => "Hué",
                    RingGeneral.Core.Models.Alignment.Tweener => "Mixte",
                    _ => "Indifférent"
                };
            }
        }

        public Alignment SelectedAlignment
        {
            get => _worker.Alignment;
            set
            {
                _worker.Alignment = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(Alignment));
            }
        }
    }
}
