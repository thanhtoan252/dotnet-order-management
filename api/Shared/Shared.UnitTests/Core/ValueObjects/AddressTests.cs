using FluentAssertions;
using NUnit.Framework;
using Shared.Core.ValueObjects;

namespace Shared.UnitTests.Core.ValueObjects;

[TestFixture]
public class AddressTests
{
    [Test]
    public void Create_Should_ReturnAddress_WhenDataIsValid()
    {
        // Act
        var address = Address.Create("Street", "City", "Province", "12345");

        // Assert
        address.Street.Should().Be("Street");
        address.City.Should().Be("City");
        address.Province.Should().Be("Province");
        address.ZipCode.Should().Be("12345");
    }

    [Test]
    public void Equals_Should_ReturnTrue_WhenAddressesAreSame()
    {
        // Arrange
        var a1 = Address.Create("S", "C", "P", "1");
        var a2 = Address.Create("S", "C", "P", "1");

        // Assert
        a1.Should().Be(a2);
    }
}
