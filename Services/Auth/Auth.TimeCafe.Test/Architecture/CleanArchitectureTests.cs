namespace Auth.TimeCafe.Test.Architecture;

/// <summary>
/// Тесты архитектурных ограничений Clean Architecture.
/// Проверяют правильность зависимостей между слоями проекта.
/// 
/// Правила Clean Architecture:
/// - Domain: не зависит ни от кого (только .NET базовые типы)
/// - Application: зависит только от Domain
/// - Infrastructure: зависит от Domain и Application
/// - API: зависит от Domain, Application, Infrastructure
/// </summary>
public class CleanArchitectureTests
{
    private const string DomainProject = "Auth.TimeCafe.Domain";
    private const string ApplicationProject = "Auth.TimeCafe.Application";
    private const string InfrastructureProject = "Auth.TimeCafe.Infrastructure";
    private const string ApiProject = "Auth.TimeCafe.API";

    [Fact]
    public void Domain_Should_NotDependOnAnyOtherLayer()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Domain.Permissions.Permission).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name != null && name.StartsWith("Auth.TimeCafe"))
            .ToList();

        // Assert
        referencedAssemblies.Should().BeEmpty(
            "Domain layer должен быть независимым и не иметь ссылок на другие слои проекта");
    }

    [Fact]
    public void Application_Should_DependOnlyOnDomain()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Application.CQRS.Auth.Commands.LoginUserCommand).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name != null && name.StartsWith("Auth.TimeCafe"))
            .ToList();

        // Assert
        referencedAssemblies.Should().Contain(DomainProject,
            "Application layer должен зависеть от Domain");

        referencedAssemblies.Should().NotContain(InfrastructureProject,
            "Application layer не должен зависеть от Infrastructure");

        referencedAssemblies.Should().NotContain(ApiProject,
            "Application layer не должен зависеть от API");
    }

    [Fact]
    public void Infrastructure_Should_DependOnDomainAndApplication()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Infrastructure.Data.ApplicationDbContext).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name != null && name.StartsWith("Auth.TimeCafe"))
            .ToList();

        // Assert
        referencedAssemblies.Should().Contain(name => name == DomainProject,
            "Infrastructure layer должен зависеть от Domain");

        referencedAssemblies.Should().Contain(name => name == ApplicationProject,
            "Infrastructure layer должен зависеть от Application");

        referencedAssemblies.Should().NotContain(name => name == ApiProject,
            "Infrastructure layer не должен зависеть от API");
    }

    [Fact]
    public void Api_Should_DependOnAllLayers()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name != null && name.StartsWith("Auth.TimeCafe"))
            .ToList();

        // Assert
        referencedAssemblies.Should().Contain(name => name == DomainProject,
            "API layer должен зависеть от Domain");

        referencedAssemblies.Should().Contain(name => name == ApplicationProject,
            "API layer должен зависеть от Application");

        referencedAssemblies.Should().Contain(name => name == InfrastructureProject,
            "API layer должен зависеть от Infrastructure");
    }

    [Fact]
    public void Domain_Should_NotReferenceEntityFramework()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Domain.Permissions.Permission).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        // Assert
        referencedAssemblies.Should().NotContain(name => name!.Contains("EntityFramework"),
            "Domain layer не должен иметь прямую зависимость от EntityFramework");
    }

    [Fact]
    public void Application_Should_NotReferenceEntityFramework()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Application.CQRS.Auth.Commands.LoginUserCommand).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        // Assert
        referencedAssemblies.Should().NotContain(name => name!.Contains("EntityFramework"),
            "Application layer не должен иметь прямую зависимость от EntityFramework (только через интерфейсы)");
    }

    [Fact]
    public void Domain_Should_NotReferenceAspNetCore()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Domain.Permissions.Permission).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        // Assert
        referencedAssemblies.Should().NotContain(name => name!.Contains("AspNetCore"),
            "Domain layer не должен иметь зависимость от ASP.NET Core");
    }

    [Fact]
    public void Application_Should_NotReferenceAspNetCore()
    {
        // Arrange
        var assembly = typeof(Auth.TimeCafe.Application.CQRS.Auth.Commands.LoginUserCommand).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        // Разрешённые AspNetCore зависимости для Auth сервиса
        var allowedAspNetCoreDependencies = new[]
        {
            "Microsoft.AspNetCore.Identity",           // Identity framework (не web-зависимость)
            "Microsoft.AspNetCore.Authorization",      // Атрибуты авторизации (Permissions)
            "Microsoft.AspNetCore.WebUtilities"        // WebEncoders для токенов
        };

        var forbiddenDependencies = referencedAssemblies
            .Where(name => name!.Contains("AspNetCore") && !allowedAspNetCoreDependencies.Contains(name))
            .ToList();

        // Assert
        forbiddenDependencies.Should().BeEmpty(
            "Application layer не должен иметь зависимость от ASP.NET Core Web-компонентов (Mvc, Http, Routing и т.д.)");
    }
}
