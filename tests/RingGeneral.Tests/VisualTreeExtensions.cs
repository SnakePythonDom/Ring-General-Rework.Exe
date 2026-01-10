using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace RingGeneral.Tests;

/// <summary>
/// Extensions pour parcourir l'arbre visuel Avalonia dans les tests
/// </summary>
public static class VisualTreeExtensions
{
    /// <summary>
    /// Récupère tous les descendants visuels d'un élément
    /// </summary>
    public static IEnumerable<Visual> GetVisualDescendants(this Visual visual)
    {
        if (visual == null)
            yield break;

        foreach (var child in visual.GetVisualChildren())
        {
            yield return child;
            foreach (var descendant in child.GetVisualDescendants())
            {
                yield return descendant;
            }
        }
    }
}
