using FluentValidation;

using System.Reflection;
using System.Text.RegularExpressions;

namespace Auth.TimeCafe.Test.Architecture;

public class ArchitectureRulesTests
{
    private readonly Assembly _application = typeof(LoginUserCommand).Assembly;
    private readonly Assembly _infrastructure = typeof(JwtService).Assembly;

    [Fact]
    public void Api_Endpoints_Should_Have_WithDescription_Summary_Name_Tags_And_BeAsync()
    {
        var repoRoot = FindRepoRoot();
        repoRoot.Should().NotBeNull("Не удалось найти корень репозитория (TimeCafe.sln)");

        var endpointsPath = Path.Combine(repoRoot!, "Services", "Auth", "Auth.TimeCafe.API", "Endpoints");
        Directory.Exists(endpointsPath).Should().BeTrue($"Папка эндпоинтов должна существовать: {endpointsPath}");

        var files = Directory.GetFiles(endpointsPath, "*.cs", SearchOption.AllDirectories);
        files.Should().NotBeEmpty();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            text.Should().Contain(".WithTags(", file + " missing WithTags");
            text.Should().Contain(".WithName(", file + " missing WithName");
            text.Should().Contain(".WithSummary(", file + " missing WithSummary");
            text.Should().Contain(".WithDescription(", file + " missing WithDescription");
            text.Should().Contain(".RequireAuthorization(", file + " missing RequireAuthorization");

            (text.Contains("async (") || text.Contains("await ")).Should().BeTrue(file + " should use async/await in handler lambda");

            var hasBindingAttributes = Regex.IsMatch(text, "\\[(FromServices|FromRoute|FromBody|FromQuery|FromHeader|FromForm)\\b", RegexOptions.IgnoreCase);
            hasBindingAttributes.Should().BeTrue(file + " should use parameter binding attributes like [FromServices], [FromRoute], [FromBody], [FromQuery], [FromHeader], or [FromForm]");
        }
    }

    private static string? FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        var cur = new DirectoryInfo(dir);
        while (cur != null)
        {
            var sln = Path.Combine(cur.FullName, "TimeCafe.sln");
            if (File.Exists(sln)) return cur.FullName;
            cur = cur.Parent;
        }
        return null;
    }

    [Fact]
    public void Application_IRequest_Types_Should_EndWith_Command_Or_Query_And_Have_Validators_And_Handler_Names()
    {
        var types = _application.GetTypes().ToList();


        var requestTypes = types.Where(t => t.IsValueType || t.IsClass)
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequest<>))
                        || (t.IsValueType && (t.Name.EndsWith("Query") || t.Name.EndsWith("Command"))))
            .ToList();

        requestTypes.Should().NotBeEmpty("Application layer должен содержать команды и запросы");

        var validators = types.Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>)).ToList();

        var errors = new List<string>();

        var handlers = types.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>))).ToList();

        foreach (var req in requestTypes)
        {
            if (!(req.Name.EndsWith("Command") || req.Name.EndsWith("Query")))
            {
                errors.Add($"{req.Name} должен заканчиваться на Command или Query");
            }

            var hasValidator = validators.Any(v => v.BaseType!.GetGenericArguments()[0] == req);
            if (!hasValidator)
            {
                errors.Add($"Должен существовать валидатор для {req.Name}");
            }

            var hasHandler = handlers.Any(h => h.Name.EndsWith(req.Name + "Handler") || h.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>) && i.GetGenericArguments()[0] == req));
            if (!hasHandler)
            {
                errors.Add($"Должен существовать handler для {req.Name} (название должно заканчиваться на {req.Name}Handler или реализовывать IRequestHandler<,>)");
            }
        }

        errors.Should().BeEmpty("Архитектурные проблемы:\n" + string.Join("\n", errors));
    }

    [Fact]
    public void Infrastructure_Repositories_Should_EndWith_Repository()
    {
        var repoTypes = _infrastructure.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Repository"))
            .ToList();

        repoTypes.Should().NotBeEmpty("Infrastructure должен содержать реализации репозиториев, оканчивающиеся на Repository");
    }
}
