using System.Collections.Generic;
using Xunit;

namespace Tomlet.Tests;

public class PlanetsSerializationTest
{
    public class Pest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Pet
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Pest> Pests { get; set; }
    }

    public class Inhabitant
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Pet> Pets { get; set; }
    }

    public class Planet
    {
        public string Name { get; set; }
        public List<Inhabitant> Inhabitants { get; set; }
    }

    public class Galaxy
    {
        public List<Planet> Planets { get; set; }
    }

    [Fact]
    public void Test()
    {
        var milkyWay = new Galaxy
        {
            Planets = new()
        };

        var earth = new Planet
        {
            Name = "Earth",
            Inhabitants = new()
        };

        var mammals = new Inhabitant
        {
            Name = "Humans",
            Description = "xyz",
            Pets = new()
        };

        var flea = new Pest
        {
            Name = "Fleas",
            Description = "abc"
        };

        var dogs = new Pet
        {
            Name = "Dogs",
            Description = "xyzzy",
            Pests = new() { flea }
        };

        var cats = new Pet
        {
            Name = "Cats",
            Description = "xyzzy",
            Pests = new() { flea }
        };

        mammals.Pets.Add(dogs);
        mammals.Pets.Add(cats);

        earth.Inhabitants.Add(mammals);

        milkyWay.Planets.Add(earth);

        var mars = new Planet
        {
            Name = "Mars",
            Inhabitants = new()
        };

        var grayOnes = new Inhabitant
        {
            Name = "Little Gray Men",
            Description = "xyz",
            Pets = new()
        };

        var martianBrainSlug = new Pet
        {
            Name = "Martian Brain Slug",
            Description = "xyzzy"
        };

        grayOnes.Pets.Add(martianBrainSlug);

        mars.Inhabitants.Add(grayOnes);

        milkyWay.Planets.Add(mars);

        var tomlString = TomletMain.TomlStringFrom(milkyWay);
        
        //Convert back to object. This should a) not throw due to broken keys, and b) have the same data.
        var galaxy = TomletMain.To<Galaxy>(tomlString);
        
        Assert.Equal(milkyWay.Planets.Count, galaxy.Planets.Count);
        Assert.Equal(milkyWay.Planets[0].Inhabitants.Count, galaxy.Planets[0].Inhabitants.Count);
        Assert.Equal(milkyWay.Planets[0].Inhabitants[0].Pets.Count, galaxy.Planets[0].Inhabitants[0].Pets.Count);
    } 
}