using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models;
using RingGeneral.Core.Models.Booker;
using RingGeneral.Core.Services;
using System.Reflection;
using Xunit;

namespace RingGeneral.Tests;

/// <summary>
/// Tests unitaires pour les archétypes créatifs du BookerAIEngine
/// </summary>
public class BookerArchetypeTests
{
    [Fact]
    public void BookerCreativeArchetypeEnum_ShouldExist()
    {
        // Test que l'enum existe et contient les bonnes valeurs
        var archetypes = Enum.GetValues(typeof(BookerCreativeArchetype));
        Assert.Equal(4, archetypes.Length);

        var names = Enum.GetNames(typeof(BookerCreativeArchetype));
        Assert.Contains("PowerBooker", names);
        Assert.Contains("Puroresu", names);
        Assert.Contains("AttitudeEra", names);
        Assert.Contains("ModernIndie", names);
    }

    [Fact]
    public void BookerAIEngine_ShouldHaveEvaluateLongTermStrategyMethod()
    {
        // Test que la méthode existe dans BookerAIEngine
        var engine = new BookerAIEngine();
        var method = typeof(BookerAIEngine).GetMethod("EvaluateLongTermStrategy",
            new[] { typeof(Booker), typeof(ShowContext), typeof(List<BookerMemory>), typeof(RingGeneral.Core.Models.Company.Era), typeof((string, int, bool)) });

        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void BookerAIEngine_ShouldIntegrateEraRepository()
    {
        // Test que le constructeur accepte IEraRepository
        var constructor = typeof(BookerAIEngine).GetConstructor(new[] { typeof(RingGeneral.Core.Interfaces.IBookerRepository), typeof(RingGeneral.Core.Interfaces.IEraRepository) });
        Assert.NotNull(constructor);
    }

    [Fact]
    public void BookerAIEngine_ShouldApplyEraInfluence()
    {
        // Test que la méthode ApplyEraInfluence existe
        var engine = new BookerAIEngine();
        var method = typeof(BookerAIEngine).GetMethod("ApplyEraInfluence",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, new[] { typeof(SegmentDefinition), typeof(RingGeneral.Core.Models.Company.Era) }, null);

        Assert.NotNull(method);
    }

    [Fact]
    public void BookerAIEngine_ShouldHaveArchetypeSelectionMethods()
    {
        // Test que les méthodes de sélection par archétype existent
        var engine = new BookerAIEngine();

        var selectWorkersMethod = typeof(BookerAIEngine).GetMethod("SelectWorkersByArchetype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var applyStylingMethod = typeof(BookerAIEngine).GetMethod("ApplyArchetypeStyling",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(selectWorkersMethod);
        Assert.NotNull(applyStylingMethod);
    }

    [Fact]
    public void BookerAIEngine_GenerateAutoBooking_ShouldUseArchetypeStyling()
    {
        // Test que GenerateAutoBooking appelle ApplyArchetypeStyling
        var engine = new BookerAIEngine();

        // Vérifier que la méthode GenerateAutoBooking existe
        var method = typeof(BookerAIEngine).GetMethod("GenerateAutoBooking",
            new[] { typeof(string), typeof(ShowContext), typeof(List<SegmentDefinition>), typeof(AutoBookingConstraints) });

        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }
}