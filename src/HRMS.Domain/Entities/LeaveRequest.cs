using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class LeaveRequest : AuditableEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal NumberOfDays { get; set; }

    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; }

    public DateTime? ApprovedOn { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComments { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
