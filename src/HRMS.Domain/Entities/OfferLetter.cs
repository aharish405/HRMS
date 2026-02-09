using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class OfferLetter : AuditableEntity
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    public decimal CTC { get; set; }
    public DateTime JoiningDate { get; set; }
    public string? Location { get; set; }

    // Template and Content
    public string TemplateContent { get; set; } = string.Empty;
    public string GeneratedContent { get; set; } = string.Empty;

    // File Storage
    public string? FilePath { get; set; }

    public DateTime GeneratedOn { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;

    // Navigation Properties
    public Employee Employee { get; set; } = null!;
}
