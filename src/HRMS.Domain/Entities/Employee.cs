using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class Employee : AuditableEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }

    // Employment Details
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int? ReportingManagerId { get; set; }
    public DateTime JoiningDate { get; set; }
    public DateTime? RelievingDate { get; set; }
    public EmployeeStatus Status { get; set; }

    // Identity Link
    public string? UserId { get; set; }

    // Navigation Properties
    public Department Department { get; set; } = null!;
    public Designation Designation { get; set; } = null!;
    public Employee? ReportingManager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<OfferLetter> OfferLetters { get; set; } = new List<OfferLetter>();

    // Computed Property
    public string FullName => $"{FirstName} {LastName}";
}
