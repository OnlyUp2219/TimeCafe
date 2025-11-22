

namespace Auth.TimeCafe.Domain.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;
    public DateTimeOffset Expires { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? ReplacedByToken { get; set; }
}
