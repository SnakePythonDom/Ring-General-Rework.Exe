using Microsoft.Data.Sqlite;

namespace RingGeneral.Data.Repositories;

public abstract class RepositoryBase
{
    protected static void AjouterParametre(SqliteCommand commande, string nom, object valeur)
    {
        commande.Parameters.AddWithValue(nom, valeur ?? DBNull.Value);
    }
}
