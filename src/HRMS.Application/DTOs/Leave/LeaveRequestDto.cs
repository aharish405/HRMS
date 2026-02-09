namespace HRMS.Application.DTOs.Leave;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public int? ApprovedBy { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedOn { get; set; }
    public string? ApprovalComments { get; set; }

    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
