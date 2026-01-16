using Dapper;
using Microsoft.Data.Sqlite;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingGeneral.Data.Repositories;

public class TitleHistoryRepository : RepositoryBase, ITitleHistoryRepository
{
    public TitleHistoryRepository(SqliteConnectionFactory factory) : base(factory)
    {
    }

    public async Task<List<TitleReign>> GetTitleHistoryAsync(string titleId)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        // Using Dapper for easier mapping
        var reigns = await conn.QueryAsync<TitleReign>(
            @"SELECT * FROM TitleReigns 
              WHERE TitleId = @TitleId 
              ORDER BY WonDate DESC",
            new { TitleId = titleId });

        return reigns.ToList();
    }

    public async Task<List<TitleReign>> GetWorkerTitleHistoryAsync(string workerId)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        var reigns = await conn.QueryAsync<TitleReign>(
            @"SELECT * FROM TitleReigns 
              WHERE WorkerId = @WorkerId 
              ORDER BY WonDate DESC",
            new { WorkerId = workerId });

        return reigns.ToList();
    }

    public async Task<List<TitleReign>> GetCurrentChampionsAsync(string companyId)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        // Join with Titles/Workers? For now just fetch reigns and filter active
        // Ideally we filter by Title's CompanyId if needed, but TitleReign doesn't have it.
        // We need to JOIN with Titles table to filter by CompanyId.

        string sql = @"
            SELECT tr.* 
            FROM TitleReigns tr
            JOIN Titles t ON tr.TitleId = t.TitleId
            WHERE t.CompanyId = @CompanyId
            AND tr.LostDate IS NULL
            ORDER BY t.Prestige DESC";

        var reigns = await conn.QueryAsync<TitleReign>(sql, new { CompanyId = companyId });
        return reigns.ToList();
    }

    public async Task AddTitleReignAsync(TitleReign reign)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        string sql = @"
            INSERT INTO TitleReigns 
            (WorkerId, TitleId, WonDate, WonShowId, LostDate, LostShowId, DaysHeld, ReignNumber)
            VALUES 
            (@WorkerId, @TitleId, @WonDate, @WonShowId, @LostDate, @LostShowId, @DaysHeld, @ReignNumber);
            SELECT last_insert_rowid();";

        var id = await conn.ExecuteScalarAsync<int>(sql, reign);
        reign.Id = id;
    }

    public async Task UpdateTitleReignAsync(TitleReign reign)
    {
        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        string sql = @"
            UPDATE TitleReigns 
            SET LostDate = @LostDate, 
                LostShowId = @LostShowId, 
                DaysHeld = @DaysHeld 
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, reign);
    }

    public async Task AddTitleDefenseAsync(string reignId, string opponentId, string showId, string matchNote)
    {
        // For AddTitleDefense, we need data about the reign to record it properly in TitleMatches.
        // Also note: interface uses string reignId, but model uses int Id. Parsing needed.

        if (!int.TryParse(reignId, out int rId)) return;

        using var conn = _factory.CreateGeneralConnection();
        conn.Open();

        // 1. Get Reign details to know Champion and Title
        var reign = await conn.QueryFirstOrDefaultAsync<TitleReign>("SELECT * FROM TitleReigns WHERE Id = @Id", new { Id = rId });
        if (reign == null) return;

        // 2. Create TitleMatch record
        // Note: Week is required in schema, but not passed in arg. We might defaults to current week?
        // Or we should update interface. For now, defaulting to 0 or fetching from GameState ideally.
        // Assuming 0 for now as repository doesn't know GameState directly without Service.

        var titleMatch = new TitleMatch
        {
            TitleId = reign.TitleId,
            ShowId = showId,
            ChampionId = reign.WorkerId,
            ChallengerId = opponentId,
            WinnerId = reign.WorkerId, // Successful defense
            IsTitleChange = false,
            PrestigeDelta = 0, // Logic should define this
            Week = 0 // Placeholder
        };

        string insertSql = @"
            INSERT INTO TitleMatches 
            (TitleId, ShowId, Week, ChampionId, ChallengerId, WinnerId, IsTitleChange, PrestigeDelta, CreatedAt)
            VALUES 
            (@TitleId, @ShowId, @Week, @ChampionId, @ChallengerId, @WinnerId, @IsTitleChange, @PrestigeDelta, @CreatedAt)";

        await conn.ExecuteAsync(insertSql, titleMatch);
    }
}
