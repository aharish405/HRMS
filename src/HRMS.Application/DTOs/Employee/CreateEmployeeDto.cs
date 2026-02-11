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

    // Removed Status - Will default to Draft
    
    // KYC Details
    [Required(ErrorMessage = "PAN Number is required")]
    [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN Number format")]
    public string PanNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Aadhar Number is required")]
    [StringLength(12, MinimumLength = 12, ErrorMessage = "Aadhar Number must be exactly 12 digits")]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhar Number must contain only digits")]
    public string AadharNumber { get; set; } = string.Empty;

    public string? BloodGroup { get; set; }

    public string? EmergencyContactName { get; set; }
    
    [Phone]
    public string? EmergencyContactPhone { get; set; }
}
