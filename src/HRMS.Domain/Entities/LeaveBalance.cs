using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class LeaveBalance : AuditableEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int Year { get; set; }

    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal AvailableDays { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
