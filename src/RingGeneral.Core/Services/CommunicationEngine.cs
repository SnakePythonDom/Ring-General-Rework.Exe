using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Crisis;
using RingGeneral.Data.Repositories;
using System;

namespace RingGeneral.Core.Services;

/// <summary>
/// Moteur de communications backstage pour résoudre les crises.
/// Gère les 4 types de communications avec prédiction de succès.
/// </summary>
public sealed class CommunicationEngine : ICommunicationEngine
{
    private readonly ICrisisRepository? _crisisRepository;
    private readonly Random _random = new();

    public CommunicationEngine(ICrisisRepository? crisisRepository = null)
    {
        _crisisRepository = crisisRepository;
    }

    public int CalculateSuccessChance(
        string communicationType,
        string tone,
        Crisis? crisis,
        int initiatorInfluence)
    {
        // Formule de base: Influence de l'initiateur (40%) + Type approprié (30%) + Ton approprié (30%)

        var baseChance = (int)(initiatorInfluence * 0.4);

        // Bonus de type selon la crise
        var typeBonus = CalculateTypeBonus(communicationType, crisis);
        baseChance += (int)(typeBonus * 0.3);

        // Bonus de ton
        var toneBonus = CalculateToneBonus(tone, crisis);
        baseChance += (int)(toneBonus * 0.3);

        // Pénalité si crise très sévère
        if (crisis != null && crisis.Severity >= 4)
        {
            baseChance -= 15;
        }

        // Pénalité si crise très escaladée
        if (crisis != null && crisis.EscalationScore >= 80)
        {
            baseChance -= 10;
        }

        // Bonus si intervention précoce (WeakSignals ou Rumors)
        if (crisis != null && crisis.Stage is "WeakSignals" or "Rumors")
        {
            baseChance += 10;
        }

        return Math.Clamp(baseChance, 10, 95);
    }

    public Communication CreateCommunication(
        string companyId,
        int? crisisId,
        string communicationType,
        string initiatorId,
        string? targetId,
        string message,
        string tone)
    {
        Crisis? crisis = null;
        if (crisisId.HasValue && _crisisRepository != null)
        {
            crisis = _crisisRepository.GetCrisisByIdAsync(crisisId.Value).Result;
        }

        // Calculer la chance de succès (influence de 70 par défaut pour initiateur)
        var successChance = CalculateSuccessChance(communicationType, tone, crisis, 70);

        var communication = new Communication
        {
            CompanyId = companyId,
            CrisisId = crisisId,
            CommunicationType = communicationType,
            InitiatorId = initiatorId,
            TargetId = targetId,
            Message = message,
            Tone = tone,
            SuccessChance = successChance,
            CreatedAt = DateTime.Now
        };

        // Sauvegarder
        if (_crisisRepository != null)
        {
            _crisisRepository.SaveCommunicationAsync(communication).Wait();
        }

        return communication;
    }

    public CommunicationOutcome ExecuteCommunication(int communicationId)
    {
        if (_crisisRepository == null)
            return CommunicationOutcome.CreateFailed(communicationId);

        var communication = _crisisRepository.GetCommunicationByIdAsync(communicationId).Result;
        if (communication == null)
            return CommunicationOutcome.CreateFailed(communicationId);

        // Déterminer le succès basé sur SuccessChance
        var success = _random.Next(100) < communication.SuccessChance;

        CommunicationOutcome outcome;

        if (success)
        {
            // Communication réussie
            var moraleBonus = CalculateMoraleBonus(communication);
            var relationshipBonus = CalculateRelationshipBonus(communication);
            var escalationReduction = CalculateEscalationReduction(communication);

            outcome = new CommunicationOutcome
            {
                CommunicationId = communicationId,
                WasSuccessful = true,
                MoraleImpact = moraleBonus,
                RelationshipImpact = relationshipBonus,
                CrisisEscalationChange = -escalationReduction,
                Feedback = GenerateSuccessFeedback(communication),
                CreatedAt = DateTime.Now
            };
        }
        else
        {
            // Communication échouée
            var moralePenalty = CalculateMoralePenalty(communication);
            var relationshipPenalty = CalculateRelationshipPenalty(communication);
            var escalationIncrease = CalculateEscalationIncrease(communication);

            outcome = new CommunicationOutcome
            {
                CommunicationId = communicationId,
                WasSuccessful = false,
                MoraleImpact = -moralePenalty,
                RelationshipImpact = -relationshipPenalty,
                CrisisEscalationChange = escalationIncrease,
                Feedback = GenerateFailureFeedback(communication),
                CreatedAt = DateTime.Now
            };
        }

        // Sauvegarder le résultat
        _crisisRepository.SaveCommunicationOutcomeAsync(outcome).Wait();

        // Appliquer les effets
        ApplyOutcomeEffects(outcome, communication.CrisisId);

        return outcome;
    }

    public void ApplyOutcomeEffects(CommunicationOutcome outcome, int? crisisId)
    {
        if (_crisisRepository == null || !crisisId.HasValue)
            return;

        var crisis = _crisisRepository.GetCrisisByIdAsync(crisisId.Value).Result;
        if (crisis == null || !crisis.IsActive())
            return;

        // Appliquer le changement d'escalade
        var updatedCrisis = outcome.CrisisEscalationChange < 0
            ? crisis.DecreaseEscalation(Math.Abs(outcome.CrisisEscalationChange))
            : crisis.IncreaseEscalation(outcome.CrisisEscalationChange);

        // Si l'escalade est tombée à 0 ou proche, résoudre
        if (updatedCrisis.EscalationScore <= 10 && outcome.WasSuccessful)
        {
            updatedCrisis = updatedCrisis.Resolve();
        }

        _crisisRepository.UpdateCrisisAsync(updatedCrisis).Wait();

        // TODO: Appliquer MoraleImpact et RelationshipImpact via MoraleEngine
        // (nécessiterait injection de IMoraleEngine)
    }

    public string RecommendCommunicationType(Crisis crisis)
    {
        // Recommandations basées sur le stage et la sévérité
        return crisis.Stage switch
        {
            "WeakSignals" => "One-on-One",           // Discussion privée précoce
            "Rumors" => "LockerRoomMeeting",         // Adresser le groupe
            "Declared" => crisis.Severity >= 4
                ? "Mediation"                        // Médiation pour crises graves
                : "LockerRoomMeeting",               // Réunion pour crises modérées
            "InResolution" => "PublicStatement",      // Déclaration publique
            _ => "One-on-One"
        };
    }

    public string RecommendTone(Crisis crisis, string communicationType)
    {
        // Recommandations de ton basées sur contexte
        if (crisis.Severity >= 4)
        {
            // Crise grave: ton apologétique ou diplomatique
            return communicationType == "PublicStatement" ? "Apologetic" : "Diplomatic";
        }

        if (crisis.EscalationScore >= 70)
        {
            // Crise très escaladée: ton ferme ou diplomatique
            return communicationType == "Mediation" ? "Diplomatic" : "Firm";
        }

        // Crise modérée: ton diplomatique par défaut
        return "Diplomatic";
    }

    public double GetCommunicationSuccessRate(string companyId)
    {
        if (_crisisRepository == null)
            return 0.0;

        return _crisisRepository.CalculateCommunicationSuccessRateAsync(companyId).Result;
    }

    // ====================================================================
    // HELPER METHODS
    // ====================================================================

    private int CalculateTypeBonus(string communicationType, Crisis? crisis)
    {
        if (crisis == null)
            return 50; // Neutre si préventif

        // Bonus si type approprié au stage
        var isAppropriate = crisis.Stage switch
        {
            "WeakSignals" => communicationType == "One-on-One",
            "Rumors" => communicationType is "One-on-One" or "LockerRoomMeeting",
            "Declared" => communicationType is "LockerRoomMeeting" or "Mediation",
            "InResolution" => communicationType is "Mediation" or "PublicStatement",
            _ => false
        };

        return isAppropriate ? 80 : 40;
    }

    private int CalculateToneBonus(string tone, Crisis? crisis)
    {
        if (crisis == null)
            return 50;

        // Diplomatic et Apologetic généralement plus sûrs
        return tone switch
        {
            "Diplomatic" => 70,
            "Apologetic" => crisis.Severity >= 4 ? 75 : 60,
            "Firm" => crisis.Severity <= 2 ? 65 : 40,
            "Confrontational" => 30, // Risqué
            _ => 50
        };
    }

    private int CalculateMoraleBonus(Communication communication)
    {
        return communication.CommunicationType switch
        {
            "One-on-One" => 15,
            "LockerRoomMeeting" => 25,
            "Mediation" => 20,
            "PublicStatement" => 30,
            _ => 10
        };
    }

    private int CalculateRelationshipBonus(Communication communication)
    {
        return communication.Tone switch
        {
            "Diplomatic" => 15,
            "Apologetic" => 20,
            "Firm" => 5,
            "Confrontational" => 0,
            _ => 10
        };
    }

    private int CalculateEscalationReduction(Communication communication)
    {
        var base_reduction = communication.CommunicationType switch
        {
            "One-on-One" => 15,
            "LockerRoomMeeting" => 25,
            "Mediation" => 35,
            "PublicStatement" => 30,
            _ => 10
        };

        // Bonus si ton approprié
        if (communication.Tone is "Diplomatic" or "Apologetic")
        {
            base_reduction += 10;
        }

        return base_reduction;
    }

    private int CalculateMoralePenalty(Communication communication)
    {
        // Pénalité moindre pour tons moins agressifs
        return communication.Tone switch
        {
            "Confrontational" => 20,
            "Firm" => 10,
            "Diplomatic" => 5,
            "Apologetic" => 3,
            _ => 8
        };
    }

    private int CalculateRelationshipPenalty(Communication communication)
    {
        return communication.Tone switch
        {
            "Confrontational" => 15,
            "Firm" => 8,
            "Diplomatic" => 3,
            "Apologetic" => 2,
            _ => 5
        };
    }

    private int CalculateEscalationIncrease(Communication communication)
    {
        // Échec augmente légèrement l'escalade
        return communication.Tone == "Confrontational" ? 20 : 10;
    }

    private string GenerateSuccessFeedback(Communication communication)
    {
        return communication.CommunicationType switch
        {
            "One-on-One" => "La discussion privée a porté ses fruits et la situation s'améliore.",
            "LockerRoomMeeting" => "La réunion de vestiaire a permis de clarifier la situation.",
            "Mediation" => "La médiation a réussi à apaiser les tensions.",
            "PublicStatement" => "La déclaration publique a été bien reçue.",
            _ => "La communication a été efficace."
        };
    }

    private string GenerateFailureFeedback(Communication communication)
    {
        return communication.Tone switch
        {
            "Confrontational" => "L'approche confrontationnelle a aggravé les tensions.",
            "Firm" => "Le ton ferme n'a pas été bien reçu.",
            "Diplomatic" => "Malgré l'approche diplomatique, la communication n'a pas convaincu.",
            "Apologetic" => "Les excuses n'ont pas suffi à apaiser la situation.",
            _ => "La communication n'a pas eu l'effet escompté."
        };
    }
}
