namespace TimeCafe.Core.Models;

public partial class ClientStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
