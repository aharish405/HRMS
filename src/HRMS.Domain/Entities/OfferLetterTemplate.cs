using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class OfferLetterTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Template content stored as HTML with placeholders like {{CANDIDATE_NAME}}
    public string Content { get; set; } = string.Empty;
    
    // Template metadata
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int Version { get; set; } = 1;
    
    // Optional: Category or type of template (e.g., "Senior", "Junior", "Executive")
    public string? Category { get; set; }
    
    // Navigation Properties
    public ICollection<OfferLetter> OfferLetters { get; set; } = new List<OfferLetter>();
}
