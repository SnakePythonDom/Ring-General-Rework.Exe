using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class ParticipantViewModel
{
    public ParticipantViewModel(string workerId, string nom)
    {
        WorkerId = workerId;
        Nom = nom;
        InRing = 0;
        Popularite = 0;
        RoleTv = string.Empty;
        Blessure = string.Empty;
    }

    /// <summary>
    /// Constructeur prenant un WorkerSnapshot complet.
    /// </summary>
    public ParticipantViewModel(WorkerSnapshot worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        WorkerId = worker.WorkerId;
        Nom = worker.NomComplet;
        InRing = worker.InRing;
        Popularite = worker.Popularite;
        RoleTv = worker.RoleTv;
        Blessure = worker.Blessure;
    }

    public string WorkerId { get; }
    public string Nom { get; }
    public int InRing { get; }
    public int Popularite { get; }
    public string RoleTv { get; }
    public string Blessure { get; }
}
