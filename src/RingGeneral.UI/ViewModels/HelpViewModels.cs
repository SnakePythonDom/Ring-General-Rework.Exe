using System.Collections.ObjectModel;
using ReactiveUI;

namespace RingGeneral.UI.ViewModels;

public sealed class HelpPanelViewModel : ViewModelBase
{
    public string Titre
    {
        get => _titre;
        set => this.RaiseAndSetIfChanged(ref _titre, value);
    }
    private string _titre = "Aide";

    public string Resume
    {
        get => _resume;
        set => this.RaiseAndSetIfChanged(ref _resume, value);
    }
    private string _resume = string.Empty;

    public ObservableCollection<HelpSectionViewModel> Sections { get; } = new();

    public ObservableCollection<string> ErreursFrequentes { get; } = new();
}

public sealed class HelpSectionViewModel
{
    public HelpSectionViewModel(string titre, string contenu)
    {
        Titre = titre;
        Contenu = contenu;
    }

    public string Titre { get; }
    public string Contenu { get; }
}

public sealed class AttributeViewModel
{
    public AttributeViewModel(string libelle, int valeur, string tooltip)
    {
        Libelle = libelle;
        Valeur = valeur;
        Tooltip = tooltip;
    }

    public string Libelle { get; }
    public int Valeur { get; }
    public string Tooltip { get; }
}

public sealed class CodexViewModel : ViewModelBase
{
    private readonly List<CodexArticleViewModel> _tousLesArticles;

    public CodexViewModel(IEnumerable<CodexArticleViewModel> articles)
    {
        _tousLesArticles = articles.ToList();
        Articles = new ObservableCollection<CodexArticleViewModel>(_tousLesArticles);
        ArticleSelectionne = Articles.FirstOrDefault();
    }

    public ObservableCollection<CodexArticleViewModel> Articles { get; }

    public CodexArticleViewModel? ArticleSelectionne
    {
        get => _articleSelectionne;
        set => this.RaiseAndSetIfChanged(ref _articleSelectionne, value);
    }
    private CodexArticleViewModel? _articleSelectionne;

    public string? Recherche
    {
        get => _recherche;
        set
        {
            this.RaiseAndSetIfChanged(ref _recherche, value);
            FiltrerArticles();
        }
    }
    private string? _recherche;

    public void OuvrirArticle(string id)
    {
        var article = _tousLesArticles.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (article is not null)
        {
            ArticleSelectionne = article;
        }
    }

    private void FiltrerArticles()
    {
        var filtre = Recherche?.Trim();
        Articles.Clear();

        IEnumerable<CodexArticleViewModel> selection = _tousLesArticles;
        if (!string.IsNullOrWhiteSpace(filtre))
        {
            selection = selection.Where(article =>
                article.Titre.Contains(filtre, StringComparison.OrdinalIgnoreCase) ||
                article.Contenu.Contains(filtre, StringComparison.OrdinalIgnoreCase) ||
                article.Categorie.Contains(filtre, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var article in selection)
        {
            Articles.Add(article);
        }
    }
}

public sealed class CodexArticleViewModel
{
    public CodexArticleViewModel(string id, string titre, string categorie, string contenu, IEnumerable<string> liens)
    {
        Id = id;
        Titre = titre;
        Categorie = categorie;
        Contenu = contenu;
        Liens = new ReadOnlyCollection<string>(liens.ToList());
    }

    public string Id { get; }
    public string Titre { get; }
    public string Categorie { get; }
    public string Contenu { get; }
    public IReadOnlyList<string> Liens { get; }
}

public sealed class ImpactPageViewModel : ViewModelBase
{
    public ImpactPageViewModel(string id, string titre, string pourquoi, string commentAmeliorer)
    {
        Id = id;
        Titre = titre;
        Pourquoi = pourquoi;
        CommentAmeliorer = commentAmeliorer;
    }

    public string Id { get; }
    public string Titre { get; }

    public string Pourquoi
    {
        get => _pourquoi;
        set => this.RaiseAndSetIfChanged(ref _pourquoi, value);
    }
    private string _pourquoi = string.Empty;

    public string CommentAmeliorer
    {
        get => _commentAmeliorer;
        set => this.RaiseAndSetIfChanged(ref _commentAmeliorer, value);
    }
    private string _commentAmeliorer = string.Empty;

    public ObservableCollection<string> Deltas { get; } = new();
}
