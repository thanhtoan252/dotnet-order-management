using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;

namespace Inventory.UnitTests.Architecture;

public class CleanArchitectureTests
{
    private const string DomainNamespace = "Inventory.Domain";
    private const string ApplicationNamespace = "Inventory.Application";
    private const string InfrastructureNamespace = "Inventory.Infrastructure";
    private const string ApiNamespace = "Inventory.Api";

    [Test]
    public void Domain_Should_Not_Have_Dependency_On_Other_Projects()
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
    public void Application_Should_Not_Have_Dependency_On_Infrastructure()
    {
        var assembly = typeof(Inventory.Application.DependencyInjection).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Test]
    public void Infrastructure_Should_Not_Have_Dependency_On_Api()
    {
        var assembly = typeof(InventoryDbContext).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
