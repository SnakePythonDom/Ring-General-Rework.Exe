using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.UI.ViewModels.Common.Profile;

using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Common.Profile;

using RingGeneral.UI.Services.Navigation;
using RingGeneral.UI.ViewModels.Contracts;
using RingGeneral.Core.Services;
using System.Text;
using System.Linq;

namespace RingGeneral.UI.ViewModels.Common
{
    public class WorkerProfileViewModel : ViewModelBase
    {
        private Worker _worker;
        private readonly IGameRepository _repository;
        private readonly INavigationService? _navigationService;
        private int _selectedTabIndex;

        public Worker Worker
        {
            get => _worker;
            set => this.RaiseAndSetIfChanged(ref _worker, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
        }

        // Sub-ViewModels
        public WorkerAttributesViewModel AttributesVM { get; }
        public WorkerContractViewModel ContractVM { get; }
        public WorkerGimmickViewModel GimmickVM { get; }
        public WorkerRelationsViewModel RelationsVM { get; }
        public WorkerHistoryViewModel HistoryVM { get; }
        public WorkerNotesViewModel NotesVM { get; }

        // Calculated Display Properties
        public string FullName => Worker.Name;
        public string Nickname => !string.IsNullOrEmpty(Worker.CurrentGimmick) ? $"\"{Worker.CurrentGimmick}\"" : "\"The Prototype\"";
        public string RoleDisplay => Worker.Type.ToString();
        public string AlignmentIcon => Worker.AlignmentIcon;
        public string AlignmentDisplay => Worker.AlignmentDisplayName;
        public string PushIcon => Worker.PushLevelIcon;
        public string PushDisplay => Worker.PushLevelDisplayName;

        // Mockup: Personality & Description
        public string PersonalityTrait => GetPersonalityDisplayName(Worker.PersonalityProfile ?? PersonalityProfile.NonDéterminé);
        public string PersonalityDescription => GetPersonalityDescription(Worker.PersonalityProfile ?? PersonalityProfile.NonDéterminé);
        public string PersonalityBulleted => GetPersonalityBullets(Worker.PersonalityProfile ?? PersonalityProfile.NonDéterminé);

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> NegotiateCommand { get; }
        public ReactiveCommand<Unit, Unit> FireCommand { get; }

        public WorkerProfileViewModel(Worker worker, IGameRepository repository, IReadOnlyDictionary<string, string> workerNames, INavigationService? navigationService = null)
        {
            _worker = worker;
            _repository = repository;
            _navigationService = navigationService;
            _selectedTabIndex = 0;

            // Calculate Personality if MentalAttributes exist
            if (_worker.MentalAttributes != null)
            {
                _worker.PersonalityProfile = PersonalityGenerator.DeterminePersonality(_worker.MentalAttributes);
            }

            // Initialize Sub-ViewModels
            AttributesVM = new WorkerAttributesViewModel(worker);
            ContractVM = new WorkerContractViewModel(worker, _repository, _navigationService);
            GimmickVM = new WorkerGimmickViewModel(worker, _repository);
            RelationsVM = new WorkerRelationsViewModel(worker, workerNames, _repository);
            HistoryVM = new WorkerHistoryViewModel(worker);
            NotesVM = new WorkerNotesViewModel(worker);

            CloseCommand = ReactiveCommand.Create(() => { /* Close logic handled by View/DialogHost */ });

            SaveCommand = ReactiveCommand.Create(() =>
            {
                try
                {
                    _repository.UpdateWorker(_worker);
                    // Optionally notify user of success
                }
                catch (Exception ex)
                {
                    // Handle error
                    System.Diagnostics.Debug.WriteLine($"Error saving worker: {ex.Message}");
                }
            });

            NegotiateCommand = ReactiveCommand.Create(() =>
            {
                if (_navigationService != null)
                {
                    var currentCompanyId = _worker.CurrentContract?.CompanyId ?? "C001";
                    var context = new NegotiationContext
                    {
                        WorkerId = !string.IsNullOrEmpty(_worker.WorkerId) ? _worker.WorkerId : _worker.Id.ToString(),
                        CompanyId = currentCompanyId,
                        CurrentWeek = 1 // Placeholder
                    };
                    _navigationService.NavigateTo<ContractNegotiationViewModel>(context);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("NavigationService is null");
                }
            });

            FireCommand = ReactiveCommand.Create(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Fire worker {_worker.Name} requested.");
                var workerId = !string.IsNullOrEmpty(_worker.WorkerId) ? _worker.WorkerId : _worker.Id.ToString();
                _repository.TerminateCurrentContract(workerId, DateTime.Now);
            });
        }

        private string GetPersonalityDisplayName(PersonalityProfile p)
        {
            return p switch
            {
                PersonalityProfile.ProfessionnelExemplaire => "PROFESSIONNEL EXEMPLAIRE",
                PersonalityProfile.CitoyenModele => "CITOYEN MODÈLE",
                PersonalityProfile.Déterminé => "DÉTERMINÉ",
                PersonalityProfile.Ambitieux => "AMBITIEUX",
                PersonalityProfile.LeaderDeVestiaire => "LEADER DE VESTIAIRE",
                PersonalityProfile.Mercenaire => "MERCENAIRE",
                PersonalityProfile.TempéramentDeFeu => "TEMPÉRAMENT DE FEU",
                PersonalityProfile.FrancTireur => "FRANC-TIREUR",
                PersonalityProfile.Inconstant => "INCONSTANT",
                PersonalityProfile.Égoïste => "ÉGOÏSTE",
                PersonalityProfile.Diva => "DIVA",
                PersonalityProfile.Paresseux => "PARESSEUX",
                PersonalityProfile.VétéranRusé => "VÉTÉRAN RUSÉ",
                PersonalityProfile.MaîtreDuStorytelling => "MAÎTRE DU STORYTELLING",
                PersonalityProfile.Politicien => "POLITICIEN",
                PersonalityProfile.AccroAuRing => "ACCRO AU RING",
                PersonalityProfile.PilierFiable => "PILIER FIABLE",
                PersonalityProfile.MachineDeGuerre => "MACHINE DE GUERRE",
                PersonalityProfile.ObsédéParLImage => "OBSÉDÉ PAR L'IMAGE",
                PersonalityProfile.CharismatiqueImprévisible => "CHARISMATIQUE IMPRÉVISIBLE",
                PersonalityProfile.AimantÀPublic => "AIMANT À PUBLIC",
                PersonalityProfile.SaboteurPassif => "SABOTEUR PASSIF",
                PersonalityProfile.InstableChronique => "INSTABLE CHRONIQUE",
                PersonalityProfile.PoidsMort => "POIDS MORT",
                PersonalityProfile.Équilibré => "ÉQUILIBRÉ",
                _ => "NON DÉTERMINÉ"
            };
        }

        private string GetPersonalityDescription(PersonalityProfile p)
        {
            return p switch
            {
                PersonalityProfile.ProfessionnelExemplaire => "Un modèle de professionnalisme. Fiable, respectueux et calme sous la pression.",
                PersonalityProfile.CitoyenModele => "Un pilier du vestiaire. Loyal, altruiste, fait passer l'entreprise avant tout.",
                PersonalityProfile.Déterminé => "N'abandonne jamais. Prospère dans l'adversité et les grands matchs.",
                PersonalityProfile.Ambitieux => "Poussé à atteindre le Main Event. A faim de succès et de titres.",
                PersonalityProfile.LeaderDeVestiaire => "Général de vestiaire. Inspire le respect, guide les jeunes talents.",
                PersonalityProfile.Mercenaire => "Suit l'argent. Aucune loyauté envers l'entreprise, partira pour une meilleure offre.",
                PersonalityProfile.TempéramentDeFeu => "Explosif mais talentueux. Risque d'incident en coulisses, mais livre sur le ring.",
                PersonalityProfile.FrancTireur => "Imprévisible. Créatif mais chaotique.",
                PersonalityProfile.Inconstant => "Performances erratiques. On ne peut pas compter sur lui dans les grands moments.",
                PersonalityProfile.Égoïste => "Refuse de mettre les autres en valeur. Tout pour la gloire personnelle.",
                PersonalityProfile.Diva => "Drame constant en coulisses. Ego massif, mauvais tempérament.",
                PersonalityProfile.Paresseux => "Paresseux, effort minimum. Ne se soucie pas de l'amélioration.",
                PersonalityProfile.VétéranRusé => "Opérateur politique en coulisses. Sait comment utiliser le système.",
                PersonalityProfile.MaîtreDuStorytelling => "Maître de la psychologie sur le ring. Crée des récits captivants.",
                PersonalityProfile.Politicien => "Joueur de pouvoir en coulisses. Tire les ficelles dans l'ombre.",
                PersonalityProfile.AccroAuRing => "Vit pour lutter. Travaillerait tous les soirs si possible.",
                PersonalityProfile.PilierFiable => "Pierre angulaire de l'entreprise. Toujours là quand on a besoin de lui.",
                PersonalityProfile.MachineDeGuerre => "Bourreau de travail indestructible. Ne casse jamais, n'abandonne jamais.",
                PersonalityProfile.ObsédéParLImage => "Veut le statut de célébrité plus que l'excellence en lutte.",
                PersonalityProfile.CharismatiqueImprévisible => "Joker avec un charisme naturel. Brillant ou désastreux.",
                PersonalityProfile.AimantÀPublic => "Connexion naturelle avec les foules. Énergie favorite des fans.",
                PersonalityProfile.SaboteurPassif => "Traître. Utilise son influence pour saboter les autres.",
                PersonalityProfile.InstableChronique => "Risque constant. Peu fiable et volatile.",
                PersonalityProfile.PoidsMort => "Aucun intérêt pour l'amélioration. Poids mort.",
                PersonalityProfile.Équilibré => "Professionnel standard. Pas de traits marquants positifs ou négatifs.",
                _ => "Profil non encore analysé ou ne correspondant à aucune catégorie spécifique."
            };
        }

        private string GetPersonalityBullets(PersonalityProfile p)
        {
            var notes = new List<string>();

            // Logic to generate bullets based on requirements of the profile
            // This is a simplified static mapping for now
            switch (p)
            {
                case PersonalityProfile.ProfessionnelExemplaire:
                    notes.Add("• Mentor exceptionnel pour le Dojo");
                    notes.Add("• Performances très stables");
                    break;
                case PersonalityProfile.CitoyenModele:
                    notes.Add("• Impact positif sur le moral");
                    notes.Add("• Ne demandera jamais à partir");
                    break;
                case PersonalityProfile.Ambitieux:
                    notes.Add("• Demandera souvent des pushs");
                    notes.Add("• Progression rapide des skills");
                    break;
                case PersonalityProfile.LeaderDeVestiaire:
                    notes.Add("• Résout les conflits backstage");
                    notes.Add("• Bonus pour les partenaires Tag");
                    break;
                case PersonalityProfile.Mercenaire:
                    notes.Add("• Négociations contractuelles difficiles");
                    notes.Add("• Acceptera n'importe quel booking si payé");
                    break;
                case PersonalityProfile.TempéramentDeFeu:
                    notes.Add("• Peut refuser de perdre");
                    notes.Add("• Matchs intenses et spectaculaires");
                    break;
                case PersonalityProfile.Diva:
                    notes.Add("• Baisse le moral du vestiaire");
                    notes.Add("• Exige un traitement préférentiel");
                    break;
                case PersonalityProfile.Paresseux:
                    notes.Add("• Progression des stats lente");
                    notes.Add("• Mauvaise endurance");
                    break;
                case PersonalityProfile.MaîtreDuStorytelling:
                    notes.Add("• Bonus aux segments micro");
                    notes.Add("• Peut porter des matchs longs");
                    break;
                case PersonalityProfile.AccroAuRing:
                    notes.Add("• Ne se plaint jamais de la fatigue");
                    notes.Add("• Récupère vite des blessures");
                    break;
                case PersonalityProfile.CharismatiqueImprévisible:
                    notes.Add("• Ventes de merch élevées");
                    notes.Add("• Peut disparaître sans prévenir");
                    break;
                case PersonalityProfile.SaboteurPassif:
                    notes.Add("• Danger pour les jeunes talents");
                    notes.Add("• Crée des cliques toxiques");
                    break;
                default:
                    if (_worker.MentalAttributes != null)
                    {
                        if (_worker.MentalAttributes.Professionnalisme > 12) notes.Add("• Assez professionnel");
                        if (_worker.MentalAttributes.Ambition > 12) notes.Add("• Travailleur motivé");
                        if (_worker.MentalAttributes.Loyauté > 12) notes.Add("• Loyal envers le club");
                    }
                    else
                    {
                        notes.Add("• Aucune donnée mentale disponible");
                    }
                    break;
            }

            return string.Join("\n", notes);
        }
    }
}
