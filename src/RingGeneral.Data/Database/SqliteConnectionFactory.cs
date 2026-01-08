using Microsoft.Data.Sqlite;

namespace RingGeneral.Data.Database;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
        DatabasePath = new SqliteConnectionStringBuilder(connectionString).DataSource;
    }

    public string DatabasePath { get; }

    public string GetConnectionString() => _connectionString;

    public SqliteConnection OuvrirConnexion()
    {
        var connexion = new SqliteConnection(_connectionString);
        connexion.Open();
        return connexion;
    }
}
