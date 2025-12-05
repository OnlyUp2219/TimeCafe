

namespace Auth.TimeCafe.Domain.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTimeOffset Expires { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? ReplacedByToken { get; set; }
}
