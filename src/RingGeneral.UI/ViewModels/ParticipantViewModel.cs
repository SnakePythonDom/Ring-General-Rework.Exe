using System;
using RingGeneral.Core.Models;

namespace RingGeneral.UI.ViewModels;

public sealed class ParticipantViewModel
{
    public ParticipantViewModel(string workerId, string nom)
    {
        WorkerId = workerId;
        Nom = nom;
    }

    /// <summary>
    /// Constructeur prenant un Worker complet.
    /// </summary>
    public ParticipantViewModel(Worker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        WorkerId = worker.WorkerId;
        Nom = worker.NomComplet;
    }

    public string WorkerId { get; }
    public string Nom { get; }
}
