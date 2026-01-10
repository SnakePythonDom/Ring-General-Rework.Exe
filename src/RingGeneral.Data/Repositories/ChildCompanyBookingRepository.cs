using Microsoft.Data.Sqlite;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

/// <summary>
/// Repository pour gérer le contrôle de booking des child companies
/// </summary>
public sealed class ChildCompanyBookingRepository : RepositoryBase
{
    public ChildCompanyBookingRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Crée ou met à jour le contrôle de booking pour une child company
    /// </summary>
    public void SauvegarderControle(string childCompanyId, ChildCompanyBookingControl control)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            INSERT INTO ChildCompanyBookingControl (ControlId, ChildCompanyId, ControlLevel, OwnerCanOverride, AutoScheduleShows, UpdatedAt)
            VALUES ($controlId, $childCompanyId, $controlLevel, $ownerCanOverride, $autoScheduleShows, $updatedAt)
            ON CONFLICT(ControlId) DO UPDATE SET
                ControlLevel = $controlLevel,
                OwnerCanOverride = $ownerCanOverride,
                AutoScheduleShows = $autoScheduleShows,
                UpdatedAt = $updatedAt;
            """;
        command.Parameters.AddWithValue("$controlId", control.ControlId);
        command.Parameters.AddWithValue("$childCompanyId", control.ChildCompanyId);
        command.Parameters.AddWithValue("$controlLevel", control.ControlLevel.ToString());
        command.Parameters.AddWithValue("$ownerCanOverride", control.OwnerCanOverride ? 1 : 0);
        command.Parameters.AddWithValue("$autoScheduleShows", control.AutoScheduleShows ? 1 : 0);
        command.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Charge le contrôle de booking pour une child company
    /// </summary>
    public ChildCompanyBookingControl? ChargerControle(string childCompanyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = """
            SELECT ControlId, ChildCompanyId, ControlLevel, OwnerCanOverride, AutoScheduleShows
            FROM ChildCompanyBookingControl
            WHERE ChildCompanyId = $childCompanyId;
            """;
        command.Parameters.AddWithValue("$childCompanyId", childCompanyId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        var controlLevelStr = reader.GetString(2);
        var controlLevel = Enum.TryParse<BookingControlLevel>(controlLevelStr, true, out var level) 
            ? level 
            : BookingControlLevel.CoBooker;

        return new ChildCompanyBookingControl(
            reader.GetString(0),
            reader.GetString(1),
            controlLevel,
            reader.GetInt32(3) == 1,
            reader.GetInt32(4) == 1);
    }

    /// <summary>
    /// Met à jour le niveau de contrôle
    /// </summary>
    public void MettreAJourNiveauControle(string childCompanyId, BookingControlLevel level)
    {
        var controle = ChargerControle(childCompanyId);
        if (controle == null)
        {
            // Créer un nouveau contrôle avec valeurs par défaut
            controle = new ChildCompanyBookingControl(
                Guid.NewGuid().ToString("N"),
                childCompanyId,
                level,
                OwnerCanOverride: true,
                AutoScheduleShows: false);
        }
        else
        {
            controle = controle with { ControlLevel = level };
        }

        SauvegarderControle(childCompanyId, controle);
    }

    /// <summary>
    /// Active ou désactive la planification automatique
    /// </summary>
    public void MettreAJourPlanificationAuto(string childCompanyId, bool enabled)
    {
        var controle = ChargerControle(childCompanyId);
        if (controle == null)
        {
            // Créer un nouveau contrôle avec valeurs par défaut
            controle = new ChildCompanyBookingControl(
                Guid.NewGuid().ToString("N"),
                childCompanyId,
                BookingControlLevel.Spectator,
                OwnerCanOverride: true,
                AutoScheduleShows: enabled);
        }
        else
        {
            controle = controle with { AutoScheduleShows = enabled };
        }

        SauvegarderControle(childCompanyId, controle);
    }

    /// <summary>
    /// Supprime le contrôle de booking (retour aux valeurs par défaut)
    /// </summary>
    public void SupprimerControle(string childCompanyId)
    {
        using var connexion = OpenConnection();
        using var command = connexion.CreateCommand();
        command.CommandText = "DELETE FROM ChildCompanyBookingControl WHERE ChildCompanyId = $childCompanyId;";
        command.Parameters.AddWithValue("$childCompanyId", childCompanyId);
        command.ExecuteNonQuery();
    }
}
