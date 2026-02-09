namespace HRMS.Application.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Address { get; set; }

    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public int DesignationId { get; set; }
    public string DesignationTitle { get; set; } = string.Empty;

    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }

    public DateTime JoiningDate { get; set; }
    public DateTime? RelievingDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
