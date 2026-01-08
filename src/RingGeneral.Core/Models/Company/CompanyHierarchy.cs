using System;
using System.ComponentModel.DataAnnotations;
using RingGeneral.Core.Enums;

namespace RingGeneral.Core.Models.Company;

/// <summary>
/// Représente la structure hiérarchique d'une compagnie.
/// Détermine si la compagnie est mono-brand ou multi-brand et gère les rôles de direction.
/// </summary>
public sealed record CompanyHierarchy
{
    /// <summary>
    /// Identifiant unique de la hiérarchie
    /// </summary>
    [Required]
    public required string HierarchyId { get; init; }

    /// <summary>
    /// Identifiant de la compagnie
    /// </summary>
    [Required]
    public required string CompanyId { get; init; }

    /// <summary>
    /// Type de hiérarchie (MonoBrand ou MultiBrand)
    /// </summary>
    [Required]
    public required CompanyHierarchyType Type { get; init; }

    /// <summary>
    /// Identifiant du propriétaire (Owner)
    /// Toujours présent, sommet de la hiérarchie
    /// </summary>
    [Required]
    public required string OwnerId { get; init; }

    /// <summary>
    /// Identifiant du Head Booker (uniquement si MultiBrand)
    /// Responsable de la vision créative globale et coordonne les bookers de brands
    /// </summary>
    public string? HeadBookerId { get; init; }

    /// <summary>
    /// Nombre de brands actifs
    /// 1 = MonoBrand, 2+ = MultiBrand
    /// </summary>
    [Range(1, 10)]
    public int ActiveBrandCount { get; init; } = 1;

    /// <summary>
    /// Indique si la structure permet l'autonomie créative des brands
    /// Si true, chaque booker de brand a plus de liberté
    /// Si false, le Head Booker (ou Owner) contrôle tout
    /// </summary>
    public bool AllowsBrandAutonomy { get; init; } = true;

    /// <summary>
    /// Niveau de centralisation des décisions (0-100)
    /// - 0: Très décentralisé, autonomie maximale des brands
    /// - 50: Équilibré
    /// - 100: Très centralisé, contrôle total du Head Booker
    /// </summary>
    [Range(0, 100)]
    public int CentralizationLevel { get; init; } = 50;

    /// <summary>
    /// Indique si la hiérarchie est active
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Date de création de la hiérarchie
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Date de dernière modification de la structure
    /// </summary>
    public DateTime LastModifiedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Valide que la CompanyHierarchy respecte les contraintes métier
    /// </summary>
    public bool IsValid(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(HierarchyId))
        {
            errorMessage = "HierarchyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompanyId))
        {
            errorMessage = "CompanyId ne peut pas être vide";
            return false;
        }

        if (string.IsNullOrWhiteSpace(OwnerId))
        {
            errorMessage = "OwnerId ne peut pas être vide";
            return false;
        }

        // Validation: MultiBrand DOIT avoir un HeadBooker
        if (Type == CompanyHierarchyType.MultiBrand && string.IsNullOrWhiteSpace(HeadBookerId))
        {
            errorMessage = "Un HeadBookerId est requis pour une structure MultiBrand";
            return false;
        }

        // Validation: MonoBrand NE DOIT PAS avoir de HeadBooker
        if (Type == CompanyHierarchyType.MonoBrand && !string.IsNullOrWhiteSpace(HeadBookerId))
        {
            errorMessage = "Un HeadBooker ne devrait pas exister dans une structure MonoBrand";
            return false;
        }

        // Validation: ActiveBrandCount doit correspondre au type
        if (Type == CompanyHierarchyType.MonoBrand && ActiveBrandCount != 1)
        {
            errorMessage = "Une structure MonoBrand doit avoir exactement 1 brand actif";
            return false;
        }

        if (Type == CompanyHierarchyType.MultiBrand && ActiveBrandCount < 2)
        {
            errorMessage = "Une structure MultiBrand doit avoir au moins 2 brands actifs";
            return false;
        }

        if (ActiveBrandCount is < 1 or > 10)
        {
            errorMessage = "ActiveBrandCount doit être entre 1 et 10";
            return false;
        }

        if (CentralizationLevel is < 0 or > 100)
        {
            errorMessage = "CentralizationLevel doit être entre 0 et 100";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Détermine si la hiérarchie est multi-brand
    /// </summary>
    public bool IsMultiBrand() => Type == CompanyHierarchyType.MultiBrand;

    /// <summary>
    /// Détermine si un Head Booker est requis
    /// </summary>
    public bool RequiresHeadBooker() => IsMultiBrand();

    /// <summary>
    /// Calcule le niveau de complexité de la structure (0-100)
    /// Plus il y a de brands, plus c'est complexe
    /// </summary>
    public int CalculateComplexityScore()
    {
        var baseComplexity = Type switch
        {
            CompanyHierarchyType.MonoBrand => 10,
            CompanyHierarchyType.MultiBrand => 40,
            _ => 20
        };

        // Ajouter complexité par brand additionnel
        var brandComplexity = (ActiveBrandCount - 1) * 15;

        // Ajouter complexité si centralisé (plus de contrôle = plus complexe)
        var centralizationComplexity = (int)(CentralizationLevel * 0.3);

        var totalComplexity = baseComplexity + brandComplexity + centralizationComplexity;

        return Math.Clamp(totalComplexity, 0, 100);
    }

    /// <summary>
    /// Retourne une description de la structure hiérarchique
    /// </summary>
    public string GetStructureDescription()
    {
        if (Type == CompanyHierarchyType.MonoBrand)
        {
            return "Structure simple: Owner → Booker → Staff";
        }

        var autonomy = AllowsBrandAutonomy ? "autonome" : "centralisée";
        return $"Structure multi-brand ({ActiveBrandCount} brands): Owner → Head Booker → Bookers de brands → Staff ({autonomy})";
    }

    /// <summary>
    /// Détermine le niveau d'autonomie créative
    /// </summary>
    public string GetAutonomyLevel()
    {
        if (!AllowsBrandAutonomy) return "Aucune autonomie";

        return CentralizationLevel switch
        {
            <= 20 => "Autonomie très élevée",
            <= 40 => "Autonomie élevée",
            <= 60 => "Autonomie modérée",
            <= 80 => "Autonomie limitée",
            _ => "Autonomie minimale"
        };
    }

    /// <summary>
    /// Calcule le potentiel de conflits hiérarchiques (0-100)
    /// Plus la structure est complexe et centralisée, plus le risque est élevé
    /// </summary>
    public int CalculateConflictPotential()
    {
        // Structure simple = peu de conflits
        if (Type == CompanyHierarchyType.MonoBrand) return 10;

        // Multi-brand avec autonomie = conflits modérés
        if (AllowsBrandAutonomy && CentralizationLevel < 50) return 30;

        // Multi-brand centralisé = conflits élevés
        if (CentralizationLevel >= 70) return 60;

        // Complexité par nombre de brands
        var brandConflict = (ActiveBrandCount - 1) * 10;

        return Math.Clamp(40 + brandConflict, 0, 100);
    }
}
