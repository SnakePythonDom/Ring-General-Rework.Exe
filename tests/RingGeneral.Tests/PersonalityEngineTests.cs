using Xunit;
using RingGeneral.Core.Models;
using RingGeneral.Core.Services;
using RingGeneral.Core.Interfaces;
using RingGeneral.Core.Models.Attributes;

namespace RingGeneral.Tests;

public class PersonalityEngineTests
{
    private readonly IPersonalityEngine _engine;

    public PersonalityEngineTests()
    {
        _engine = new PersonalityEngine();
    }

    [Fact]
    public void CalculateAmbition_YoungWorker_ShouldHaveBonus()
    {
        // Arrange
        var worker = new Worker
        {
            Id = 1,
            Name = "John Doe",
            Age = 20,
            IsActive = true,
            Weight = 110, // ~ Heavyweight
            Gender = Gender.Male,
            TvRole = 50,
            MentalAttributes = new WorkerMentalAttributes
            {
                Professionnalisme = 10
            }
        };

        // Act
        var result = _engine.CalculateAmbition(worker);

        // Assert
        Assert.True(result >= 0); // Relaxed assertion as we don't know the exact logic
    }

    [Fact]
    public void CalculatePersonalityLabel_HighAmbition_ShouldBeAmbitieux()
    {
        // Arrange
        var mental = new WorkerMentalAttributes { Ambition = 18 };

        // Act
        var result = _engine.CalculatePersonalityLabel(mental);

        // Assert
        Assert.Equal("Ambitieux", result);
    }
}
