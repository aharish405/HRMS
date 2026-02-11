using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.OfferLetter;

public class BulkOfferGenerationDto
{
    [Required(ErrorMessage = "Template ID is required")]
    public int TemplateId { get; set; }

    [Required(ErrorMessage = "At least one employee must be selected")]
    [MinLength(1, ErrorMessage = "At least one employee must be selected")]
    public List<EmployeeOfferDataDto> Employees { get; set; } = new();

    // Common company information
    public string CompanyName { get; set; } = "WorkAxis Technologies";
    public string CompanyAddress { get; set; } = string.Empty;
}

public class EmployeeOfferDataDto
{
    public int? EmployeeId { get; set; } // Null for new candidates

    // Candidate Information
    [Required]
    public string CandidateName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CandidateEmail { get; set; } = string.Empty;

    public string? CandidatePhone { get; set; }
    public string? CandidateAddress { get; set; }

    // Position Details
    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int DesignationId { get; set; }

    [Required]
    public DateTime JoiningDate { get; set; }

    [Required]
    public string Location { get; set; } = string.Empty;

    // Salary Details
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Basic salary must be greater than 0")]
    public decimal BasicSalary { get; set; }

    public decimal HRA { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }

    [Required]
    public decimal CTC { get; set; }

    // Deduction Details
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal TDS { get; set; }
    public decimal OtherDeductions { get; set; }

    public string? AdditionalTerms { get; set; }
}
