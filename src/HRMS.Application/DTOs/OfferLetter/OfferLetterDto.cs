using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.OfferLetter;

public class OfferLetterDto
{
    public int Id { get; set; }

    // Candidate Information
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidatePhone { get; set; } = string.Empty;
    public string CandidateAddress { get; set; } = string.Empty;

    // Position Details
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int DesignationId { get; set; }
    public string DesignationTitle { get; set; } = string.Empty;

    // Salary Details
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal CTC { get; set; }

    // Offer Details
    public DateTime JoiningDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? AdditionalTerms { get; set; }

    // Status
    public OfferLetterStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();

    // Metadata
    public DateTime GeneratedOn { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
