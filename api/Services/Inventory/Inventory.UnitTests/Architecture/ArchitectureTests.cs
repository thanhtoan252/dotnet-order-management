using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;
using Inventory.Domain.Entities;
using Inventory.Application.Abstractions;

namespace Inventory.UnitTests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "Inventory.Domain";
    private const string ApplicationNamespace = "Inventory.Application";
    private const string InfrastructureNamespace = "Inventory.Infrastructure";
    private const string ApiNamespace = "Inventory.Api";

    [Test]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        var assembly = typeof(InventoryItem).Assembly;

        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            ApiNamespace
        };

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Test]
    public void Application_Should_Not_HaveDependencyOnInfrastructure()
    {
        var assembly = typeof(IInventoryDbContext).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
