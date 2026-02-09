namespace HRMS.Application.DTOs.Leave;

public class LeaveBalanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;

    public int Year { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int AvailableDays { get; set; }
}
