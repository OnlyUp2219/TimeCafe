namespace TimeCafe.UI.Models;

public class ClientBalanceInfo
{
    public int ClientId { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public decimal Balance { get; set; }
    public decimal Debt { get; set; }
    public DateTime LastTransactionDate { get; set; }
    public bool IsActive { get; set; }
}