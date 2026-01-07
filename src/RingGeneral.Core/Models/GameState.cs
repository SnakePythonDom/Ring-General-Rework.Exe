using System;

namespace RingGeneral.Core.Models;

/// <summary>
/// Représente l'état d'une partie sauvegardée
/// </summary>
public sealed class GameState
{
    public int GameStateId { get; set; }
    public string SaveName { get; set; } = string.Empty;
    public string PlayerCompanyId { get; set; } = string.Empty;
    public int CurrentWeek { get; set; } = 1;
    public string CurrentDate { get; set; } = "2024-01-01";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
