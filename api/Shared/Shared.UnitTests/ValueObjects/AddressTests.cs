using FluentAssertions;
using NUnit.Framework;
using Shared.Core.ValueObjects;

namespace Shared.UnitTests.ValueObjects;

public class AddressTests
{
    [Test]
    public void Create_ShouldReturnAddress_WhenAllFieldsAreValid()
    {
        // Act
        var address = Address.Create("123 Main St", "Seattle", "WA", "98101");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Seattle");
        address.Province.Should().Be("WA");
        address.ZipCode.Should().Be("98101");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_ShouldThrow_WhenStreetIsInvalid(string? street)
    {
        // Act
        Action act = () => Address.Create(street!, "City", "Province", "Zip");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_ShouldTrimValues()
    {
        // Act
        var address = Address.Create("  123 Main St  ", " Seattle ", " WA ", " 98101 ");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Seattle");
        address.Province.Should().Be("WA");
        address.ZipCode.Should().Be("98101");
    }
}
