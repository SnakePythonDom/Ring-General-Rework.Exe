namespace RingGeneral.Core.Models;

/// <summary>
/// Représente une brand (entité séparée pour une compagnie)
/// Exemple : WWE Raw, WWE SmackDown sont deux brands de WWE
/// </summary>
public sealed record Brand(
    string BrandId,
    string CompanyId,
    string Name,
    DateTime CreatedAt);

/// <summary>
/// Requête pour créer une nouvelle brand
/// </summary>
public sealed record BrandCreateRequest(
    string CompanyId,
    string Name);

/// <summary>
/// Requête pour mettre à jour une brand existante
/// </summary>
public sealed record BrandUpdateRequest(
    string? Name = null);
