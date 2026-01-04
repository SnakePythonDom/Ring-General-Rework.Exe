using RingGeneral.Data.Database;

static int Main(string[] args)
{
    if (args.Length < 2 || !string.Equals(args[0], "db", StringComparison.OrdinalIgnoreCase))
    {
        return AfficherAide();
    }

    var commande = args[1].ToLowerInvariant();
    var options = args.Skip(2).ToArray();

    var initializer = new DbInitializer();
    var validator = new DbValidator();

    try
    {
        return commande switch
        {
            "create" => ExecuterCreate(options, initializer),
            "validate" => ExecuterValidate(options, validator),
            "upgrade" => ExecuterUpgrade(options, initializer),
            _ => AfficherAide()
        };
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Erreur : {ex.Message}");
        return 1;
    }
}

static int ExecuterCreate(string[] options, DbInitializer initializer)
{
    var output = ExtraireOption(options, "--output");
    if (string.IsNullOrWhiteSpace(output))
    {
        Console.Error.WriteLine("Chemin de sortie manquant. Utilisez --output <chemin>.");
        return 1;
    }

    if (File.Exists(output))
    {
        Console.Error.WriteLine("Le fichier existe déjà. Choisissez un autre chemin.");
        return 1;
    }

    initializer.CreateDatabaseIfMissing(output);
    Console.WriteLine($"Base créée : {output}");
    return 0;
}

static int ExecuterValidate(string[] options, DbValidator validator)
{
    var db = ExtraireOption(options, "--db");
    if (string.IsNullOrWhiteSpace(db))
    {
        Console.Error.WriteLine("Chemin de base manquant. Utilisez --db <chemin>.");
        return 1;
    }

    var resultat = validator.Valider(db);
    if (resultat.EstValide)
    {
        Console.WriteLine("Base valide.");
        return 0;
    }

    Console.Error.WriteLine("Base invalide :");
    foreach (var erreur in resultat.Erreurs)
    {
        Console.Error.WriteLine($"- {erreur}");
    }

    return 1;
}

static int ExecuterUpgrade(string[] options, DbInitializer initializer)
{
    var db = ExtraireOption(options, "--db");
    if (string.IsNullOrWhiteSpace(db))
    {
        Console.Error.WriteLine("Chemin de base manquant. Utilisez --db <chemin>.");
        return 1;
    }

    if (!File.Exists(db))
    {
        Console.Error.WriteLine("La base indiquée est introuvable.");
        return 1;
    }

    initializer.ApplyMigrations(db);
    Console.WriteLine("Migrations appliquées.");
    return 0;
}

static string? ExtraireOption(string[] options, string nom)
{
    for (var i = 0; i < options.Length - 1; i++)
    {
        if (string.Equals(options[i], nom, StringComparison.OrdinalIgnoreCase))
        {
            return options[i + 1];
        }
    }

    return null;
}

static int AfficherAide()
{
    Console.WriteLine("RingGeneral.Tools.DbManager");
    Console.WriteLine("Usage :");
    Console.WriteLine("  db create --output <chemin>");
    Console.WriteLine("  db validate --db <chemin>");
    Console.WriteLine("  db upgrade --db <chemin>");
    return 1;
}
