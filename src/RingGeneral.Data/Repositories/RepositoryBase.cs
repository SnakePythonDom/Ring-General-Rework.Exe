using Microsoft.Data.Sqlite;
using RingGeneral.Data.Database;

namespace RingGeneral.Data.Repositories;

public abstract class RepositoryBase
{
    protected readonly SqliteConnectionFactory _factory;

    protected RepositoryBase(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    protected SqliteConnection OpenConnection()
    {
        return _factory.OuvrirConnexion();
    }

    protected void WithTransaction(Action<SqliteConnection, SqliteTransaction> action)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        try
        {
            action(connexion, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    protected T WithTransaction<T>(Func<SqliteConnection, SqliteTransaction, T> func)
    {
        using var connexion = OpenConnection();
        using var transaction = connexion.BeginTransaction();
        try
        {
            var result = func(connexion, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    protected static void AjouterParametre(SqliteCommand commande, string nom, object valeur)
    {
        commande.Parameters.AddWithValue(nom, valeur ?? DBNull.Value);
    }
}
