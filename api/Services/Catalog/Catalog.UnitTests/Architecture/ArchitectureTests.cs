using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;
using Catalog.Domain.Entities;
using Catalog.Application.Abstractions;

namespace Catalog.UnitTests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "Catalog.Domain";
    private const string ApplicationNamespace = "Catalog.Application";
    private const string InfrastructureNamespace = "Catalog.Infrastructure";
    private const string ApiNamespace = "Catalog.Api";

    [Test]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        var assembly = typeof(Product).Assembly;

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
        var assembly = typeof(ICatalogDbContext).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
