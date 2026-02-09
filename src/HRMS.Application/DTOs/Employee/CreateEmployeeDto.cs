using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Employee;

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Employee code is required")]
    [StringLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public int Gender { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Designation is required")]
    public int DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    [Required(ErrorMessage = "Joining date is required")]
    public DateTime JoiningDate { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public int Status { get; set; }
}
