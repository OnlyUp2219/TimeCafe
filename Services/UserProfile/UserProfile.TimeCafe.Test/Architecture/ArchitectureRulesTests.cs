using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;

namespace UserProfile.TimeCafe.Test.Architecture;

public partial class ArchitectureRulesTests
{
    private readonly Assembly _application = typeof(CreateProfileCommand).Assembly;
    private readonly Assembly _infrastructure = typeof(UserRepositories).Assembly;

    [Fact]
    public void Api_Endpoints_Should_Have_WithDescription_Summary_Name_Tags_And_UseAsyncHandlers()
    {
        var repoRoot = FindRepoRoot();
        repoRoot.Should().NotBeNull("Не удалось найти корень репозитория (TimeCafe.sln)");

        var endpointsPath = Path.Combine(repoRoot!, "Services", "UserProfile", "UserProfile.TimeCafe.API", "Endpoints");
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

            var hasBindingAttributes = MyRegex().IsMatch(text);
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
            var slnx = Path.Combine(cur.FullName, "TimeCafe.slnx");
            if (File.Exists(sln) || File.Exists(slnx))
                return cur.FullName;
            cur = cur.Parent;
        }
        return null;
    }

    [Fact]
    public void Application_IRequest_Types_Should_EndWith_Command_Or_Query_And_Have_Validators_And_Handler_Names()
    {
        var types = _application.GetTypes().ToList();


        var requestTypes = types.Where(t => (t.IsValueType || t.IsClass) && (t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequest<>))
                        || t.IsValueType && (t.Name.EndsWith("Query") || t.Name.EndsWith("Command"))))
            .ToList();

        requestTypes.Should().NotBeEmpty("Application layer должен содержать команды и запросы");

        var validators = types.Where(t => t.BaseType?.IsGenericType == true && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>)).ToList();

        var errors = new List<string>();

        var handlers = types.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>))).ToList();

        foreach (var req in requestTypes)
        {
            ValidateRequestTypeName(req, errors);
            ValidateRequestHasValidator(req, validators, errors);
            ValidateRequestHasHandler(req, handlers, errors);
        }

        errors.Should().BeEmpty("Архитектурные проблемы:\n" + string.Join("\n", errors));
    }

    private static void ValidateRequestTypeName(Type req, List<string> errors)
    {
        if (!(req.Name.EndsWith("Command") || req.Name.EndsWith("Query")))
            errors.Add($"{req.Name} должен заканчиваться на Command или Query");
    }

    private static void ValidateRequestHasValidator(Type req, List<Type> validators, List<string> errors)
    {
        var hasValidator = validators.Any(v => v.BaseType!.GetGenericArguments()[0] == req);
        if (!hasValidator)
            errors.Add($"Должен существовать валидатор для {req.Name}");
    }

    private static void ValidateRequestHasHandler(Type req, List<Type> handlers, List<string> errors)
    {
        var hasHandler = handlers.Any(h => h.Name.EndsWith(req.Name + "Handler") || h.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>) && i.GetGenericArguments()[0] == req));

        if (!hasHandler)
            errors.Add($"Должен существовать handler для {req.Name} (название должно заканчиваться на {req.Name}Handler или реализовывать IRequestHandler<,>)");
    }

    [Fact]
    public void Infrastructure_Repositories_Should_EndWith_Repository()
    {
        var repoTypes = _infrastructure.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Repository"))
            .ToList();

        repoTypes.Should().NotBeEmpty("Infrastructure должен содержать реализации репозиториев, оканчивающиеся на Repository");
    }

    [GeneratedRegex("\\[(FromServices|FromRoute|FromBody|FromQuery|FromHeader|FromForm)\\b", RegexOptions.IgnoreCase, "ru-RU")]
    private static partial Regex MyRegex();
}
