using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Employee;

public class UpdateEmployeeDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public int Gender { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    public DateTime? RelievingDate { get; set; }

    [Required]
    public int Status { get; set; }

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
