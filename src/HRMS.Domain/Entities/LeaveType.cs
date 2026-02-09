using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class LeaveType : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LeaveTypeEnum Type { get; set; }
    public int DefaultDaysPerYear { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPaid { get; set; } = true;

    // Navigation Properties
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
