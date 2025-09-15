namespace TimeCafe.Common.Constants;

public static class CacheKeys
{
    public const string BillingTypes_All = "billingtypes:all";
    public static string BillingTypes_ById(int id) => $"billingtypes:id:{id}";


    public const string ClientAdditionalInfo_All = "clientadditionalinfo:all";
    public static string ClientAdditionalInfo_ById(int id) => $"clientadditionalinfo:id:{id}";


    public const string Client_All = "client:all";
    private const string ClientPagesPrefix = "client:page";
    private const string ClientPagesVersionKey = "client:page:version";
    public static string Client_ById(int id) => $"client:id:{id}";
    public static string Client_Page(int page, int version) => $"{ClientPagesPrefix}:v{version}:p{page}";
    public static string ClientPagesVersion() => ClientPagesVersionKey;


    public const string FinancialTransaction_All = "financialtransaction:all";
    public static string FinancialTransaction_ById(int financialId) => $"financialtransaction:id:{financialId}";
    public static string FinancialTransaction_ByClientId(int clientId) => $"financialtransaction:clientid:{clientId}";
    public static string FinancialTransaction_Limit_ByClientId(int clientId, int? limit) => $"financialtransaction:clientid:{clientId}:limit{limit}";


    public const string Tariff_All = "tariff:all";
    private const string TariffPagesPrefix = "tariff:page";
    private const string TariffPagesVersionKey = "tariff:page:version";
    public static string Tariff_ById(int id) => $"tariff:id:{id}";
    public static string Tariff_Page(int page, int version) => $"{TariffPagesPrefix}:v{version}:p{page}";
    public static string TariffPagesVersion() => TariffPagesVersionKey;


    public const string Themes_All = "themes:all";


    public const string Visit_All = "visit:all";
    public static string Visit_ById(int id) => $"visit:id:{id}";
    public static string Visit_ByCliendId(int clientid) => $"visit:clientid:{clientid}";


}