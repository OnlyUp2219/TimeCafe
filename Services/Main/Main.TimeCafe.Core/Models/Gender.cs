namespace TimeCafe.Core.Models;

public partial class Gender
{
    public int GenderId { get; set; }

    public string GenderName { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
