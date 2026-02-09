using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; }
    public string? IPAddress { get; set; }
}
