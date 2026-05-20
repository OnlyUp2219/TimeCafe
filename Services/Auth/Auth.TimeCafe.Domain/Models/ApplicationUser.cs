using Audit.EntityFramework;

namespace Auth.TimeCafe.Domain.Models;

[AuditIgnore]
public class ApplicationUser : IdentityUser<Guid>;
