namespace TimeCafe.Core.Models;

public partial class ClientAdditionalInfo
{
    public int InfoId { get; set; }

    public int? ClientId { get; set; }

    public string InfoText { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; }
}
