using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.OfferLetter;

public class CreateOfferLetterDto
{
    // Candidate Information
    [Required(ErrorMessage = "Candidate name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string CandidateName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string CandidateEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string CandidatePhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string CandidateAddress { get; set; } = string.Empty;

    // Position Details
    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Designation is required")]
    public int DesignationId { get; set; }

    // Salary Details
    [Required(ErrorMessage = "Basic salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Basic salary must be positive")]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "HRA must be positive")]
    public decimal HRA { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Conveyance allowance must be positive")]
    public decimal ConveyanceAllowance { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Medical allowance must be positive")]
    public decimal MedicalAllowance { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Special allowance must be positive")]
    public decimal SpecialAllowance { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Other allowances must be positive")]
    public decimal OtherAllowances { get; set; }
    public decimal CTC { get; set; }

    // Deduction Details
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal TDS { get; set; }
    public decimal OtherDeductions { get; set; }

    // Offer Details
    [Required(ErrorMessage = "Joining date is required")]
    public DateTime JoiningDate { get; set; }

    [Required(ErrorMessage = "Location is required")]
    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string Location { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Additional terms cannot exceed 1000 characters")]
    public string? AdditionalTerms { get; set; }

    // Linkage (WP3)
    public int? EmployeeId { get; set; }
    public int? TemplateId { get; set; }
}
