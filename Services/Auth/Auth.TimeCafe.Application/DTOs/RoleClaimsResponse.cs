namespace Auth.TimeCafe.Application.DTOs;

public class RoleClaimsResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; }
    public List<string> Claims { get; set; }
}
