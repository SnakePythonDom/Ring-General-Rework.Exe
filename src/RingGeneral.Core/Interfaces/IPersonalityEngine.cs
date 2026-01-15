using RingGeneral.Core.Models.Attributes;
using RingGeneral.Core.Models;
using System.Collections.Generic;

namespace RingGeneral.Core.Interfaces;

public interface IPersonalityEngine
{
    int CalculateAmbition(Worker worker);
    int CalculateLoyalty(Worker worker);
    int CalculateVolatility(Worker worker);
    string CalculatePersonalityLabel(WorkerMentalAttributes mental);
    void UpdateMentalAttributes(WorkerMentalAttributes mental, string eventType, int intensity);
    bool ShouldPersonalityChange(WorkerMentalAttributes mental, PersonalityProfile currentProfile);
    List<string> GenerateSecondaryTraits(WorkerMentalAttributes mental);
    WorkerMentalAttributes GenerateRandomMentalAttributes();
    InteractionResult ProcessInteraction(Worker worker, InteractionType interactionType);
}
