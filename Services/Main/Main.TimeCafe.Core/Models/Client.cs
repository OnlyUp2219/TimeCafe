namespace TimeCafe.Core.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public int? GenderId { get; set; }

    public string Email { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string PhoneNumber { get; set; }

    public string AccessCardNumber { get; set; }

    public int? StatusId { get; set; }

    public string RefusalReason { get; set; }

    public byte[] Photo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ClientAdditionalInfo> ClientAdditionalInfos { get; set; } = new List<ClientAdditionalInfo>();

    public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

    public virtual Gender Gender { get; set; }

    public virtual ICollection<PhoneConfirmation> PhoneConfirmations { get; set; } = new List<PhoneConfirmation>();

    public virtual ClientStatus Status { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
