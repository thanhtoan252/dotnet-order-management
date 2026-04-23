using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;
using Order.Domain.Entities;
using Order.Infrastructure.Data;

namespace Order.UnitTests.Architecture;

public class CleanArchitectureTests
{
    private const string DomainNamespace = "Order.Domain";
    private const string ApplicationNamespace = "Order.Application";
    private const string InfrastructureNamespace = "Order.Infrastructure";
    private const string ApiNamespace = "Order.Api";

    [Test]
    public void Domain_Should_Not_Have_Dependency_On_Other_Projects()
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
    public void Application_Should_Not_Have_Dependency_On_Infrastructure()
    {
        var assembly = typeof(Order.Application.DependencyInjection).Assembly;

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
        var assembly = typeof(OrderDbContext).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
