using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class OfferLetter : AuditableEntity
{
    // Candidate Information
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidatePhone { get; set; } = string.Empty;
    public string CandidateAddress { get; set; } = string.Empty;

    // Position Details
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }

    // Salary Details (Allowances)
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal CTC { get; set; }

    // Deduction Details
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal TDS { get; set; }
    public decimal OtherDeductions { get; set; }

    // Computed Fields
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    // Offer Details
    public DateTime JoiningDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? AdditionalTerms { get; set; }

    // Employee Link (set after acceptance)
    public int? EmployeeId { get; set; }

    // Status
    public OfferLetterStatus Status { get; set; }

    // Template Support
    public int? TemplateId { get; set; }
    public string? GeneratedContent { get; set; } // Stores the final rendered HTML

    // Acceptance Tracking
    public string? AcceptanceToken { get; set; }
    public DateTime? AcceptedOn { get; set; }

    // Metadata
    public DateTime GeneratedOn { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;

    // Navigation Properties
    public Department Department { get; set; } = null!;
    public Designation Designation { get; set; } = null!;
    public OfferLetterTemplate? Template { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
}
