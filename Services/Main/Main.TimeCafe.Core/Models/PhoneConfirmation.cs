namespace TimeCafe.Core.Models;

public partial class PhoneConfirmation
{
    public int ConfirmationId { get; set; }

    public int? ClientId { get; set; }

    public string PhoneNumber { get; set; }

    public string ConfirmationCode { get; set; }

    public DateTime GeneratedTime { get; set; }

    public bool IsConfirmed { get; set; }

    public virtual Client Client { get; set; }
}
