using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class TemplatePlaceholder : AuditableEntity
{
    // Unique key for the placeholder (e.g., "CANDIDATE_NAME", "BASIC_SALARY")
    public string PlaceholderKey { get; set; } = string.Empty;
    
    // User-friendly display name
    public string DisplayName { get; set; } = string.Empty;
    
    // Category for grouping placeholders
    public PlaceholderCategory Category { get; set; }
    
    // Data type hint (e.g., "string", "number", "date", "currency")
    public string DataType { get; set; } = "string";
    
    // Description or help text
    public string? Description { get; set; }
    
    // Sample value for preview
    public string? SampleValue { get; set; }
    
    // Whether this placeholder is required
    public bool IsRequired { get; set; } = false;
    
    // Format string for dates/numbers (e.g., "dd/MM/yyyy", "N2")
    public string? FormatString { get; set; }
    
    // Active status
    public bool IsActive { get; set; } = true;
}
