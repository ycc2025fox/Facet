namespace Facet.Tests.UnitTests.Core.Facet;

public class ConfiguredInheritanceSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public abstract class ConfiguredInheritanceBase
{
    public int Id { get; set; }
}

public interface IConfiguredNameFacet
{
    string Name { get; set; }
}

public interface IConfiguredIdFacet
{
    int Id { get; set; }
}

public interface IConfiguredDescriptionFacet
{
    string Description { get; set; }
}

[Facet(
    typeof(ConfiguredInheritanceSource),
    BaseType = typeof(ConfiguredInheritanceBase),
    Interfaces = [typeof(IConfiguredNameFacet), typeof(IConfiguredIdFacet)])]
public partial class ConfiguredInheritanceFacet
{
}

[Facet(
    typeof(ConfiguredInheritanceSource),
    BaseType = typeof(ConfiguredInheritanceBase),
    Interfaces = [typeof(IConfiguredNameFacet)],
    GenerateEquality = true)]
public partial class ConfiguredInheritanceEqualityFacet
{
}

[Facet(
    typeof(ConfiguredInheritanceSource),
    Include = [nameof(ConfiguredInheritanceSource.Id), nameof(ConfiguredInheritanceSource.Name)],
    BaseType = typeof(ConfiguredInheritanceBase),
    Interfaces = [typeof(IConfiguredDescriptionFacet)])]
public partial class ConfiguredInheritanceUserDeclaredFacet
{
    public string Description { get; set; } = string.Empty;
}

public class ConfiguredInheritanceTests
{
    [Fact]
    public void Constructor_ShouldMapMembers_WithConfiguredBaseTypeAndInterfaces()
    {
        var source = new ConfiguredInheritanceSource
        {
            Id = 7,
            Name = "Facet",
            Description = "Configured"
        };

        var facet = new ConfiguredInheritanceFacet(source);

        facet.Id.Should().Be(7);
        facet.Name.Should().Be("Facet");
        facet.Description.Should().Be("Configured");
        facet.Should().BeAssignableTo<ConfiguredInheritanceBase>();
        facet.Should().BeAssignableTo<IConfiguredNameFacet>();
        facet.Should().BeAssignableTo<IConfiguredIdFacet>();
    }

    [Fact]
    public void GeneratedType_ShouldNotRedeclareConfiguredBaseMembers()
    {
        var declaredProperties = typeof(ConfiguredInheritanceFacet)
            .GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Select(p => p.Name)
            .ToList();

        declaredProperties.Should().NotContain("Id");
        declaredProperties.Should().Contain("Name");
        declaredProperties.Should().Contain("Description");
    }

    [Fact]
    public void Projection_ShouldPopulateConfiguredBaseMembers()
    {
        var facets = new[]
        {
            new ConfiguredInheritanceSource { Id = 1, Name = "A", Description = "One" },
            new ConfiguredInheritanceSource { Id = 2, Name = "B", Description = "Two" }
        }
        .AsQueryable()
        .Select(ConfiguredInheritanceFacet.Projection)
        .ToList();

        facets.Should().HaveCount(2);
        facets[0].Id.Should().Be(1);
        facets[0].Name.Should().Be("A");
        facets[1].Id.Should().Be(2);
        facets[1].Description.Should().Be("Two");
    }

    [Fact]
    public void EqualityFacet_ShouldStillImplementConfiguredInterfaces()
    {
        var facet = new ConfiguredInheritanceEqualityFacet(new ConfiguredInheritanceSource
        {
            Id = 3,
            Name = "Equal",
            Description = "Value"
        });

        facet.Should().BeAssignableTo<ConfiguredInheritanceBase>();
        facet.Should().BeAssignableTo<IConfiguredNameFacet>();
        typeof(IEquatable<ConfiguredInheritanceEqualityFacet>).IsAssignableFrom(typeof(ConfiguredInheritanceEqualityFacet)).Should().BeTrue();
    }

    [Fact]
    public void UserDeclaredProperty_ShouldSatisfyConfiguredInterface()
    {
        var facet = new ConfiguredInheritanceUserDeclaredFacet(new ConfiguredInheritanceSource
        {
            Id = 9,
            Name = "Manual",
            Description = "Declared"
        });

        facet.Id.Should().Be(9);
        facet.Name.Should().Be("Manual");
        facet.Description.Should().Be(string.Empty);
        facet.Should().BeAssignableTo<IConfiguredDescriptionFacet>();
    }
}
