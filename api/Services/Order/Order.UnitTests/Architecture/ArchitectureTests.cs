using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;
using Order.Domain.Entities;
using Order.Application.Abstractions;
using Order.Infrastructure.Data;

namespace Order.UnitTests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "Order.Domain";
    private const string ApplicationNamespace = "Order.Application";
    private const string InfrastructureNamespace = "Order.Infrastructure";
    private const string ApiNamespace = "Order.Api";

    [Test]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        var assembly = typeof(OrderAggregate).Assembly;

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
        var assembly = typeof(IOrderDbContext).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
