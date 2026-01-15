namespace RingGeneral.Data.Database;

public interface IDbValidator
{
    DbValidationResult Valider(string cheminDb);
}
