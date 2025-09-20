using Bogus;

namespace Main.TimeCafe.Application.Services;

public class BogusDataGeneratorServices
{
    private readonly Faker<Client> _clientFaker;
    private readonly Faker<Tariff> _tariffFaker;

    public BogusDataGeneratorServices()
    {
        _clientFaker = new Faker<Client>("ru")
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.MiddleName, f => f.Name.FirstName())
            .RuleFor(c => c.GenderId, f => f.Random.Number(1, 2)) // 1 - мужской, 2 - женский
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(80, DateTime.Now.AddYears(-18))))
            .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("+375 (##) ### ####"))
            .RuleFor(c => c.AccessCardNumber, f => null)
            .RuleFor(c => c.CreatedAt, f => DateTime.Now);

        _tariffFaker = new Faker<Tariff>("ru")
            .RuleFor(t => t.TariffName, f => f.Commerce.ProductName())
            .RuleFor(t => t.DescriptionTitle, f => f.Commerce.ProductAdjective())
            .RuleFor(t => t.Description, f => f.Commerce.ProductDescription())
            .RuleFor(t => t.Price, f => f.Random.Decimal(100, 1000))
            .RuleFor(t => t.CreatedAt, f => DateTime.Now)
            .RuleFor(t => t.LastModified, f => DateTime.Now);
    }

    public Client GenerateClient()
    {
        return _clientFaker.Generate();
    }

    public Tariff GenerateTariff()
    {
        return _tariffFaker.Generate();
    }
}