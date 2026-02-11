using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.OfferLetterTemplate;

public class TemplatePlaceholderDto
{
    public int Id { get; set; }
    public string PlaceholderKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public PlaceholderCategory Category { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SampleValue { get; set; }
    public bool IsRequired { get; set; }
    public string? FormatString { get; set; }
    public bool IsActive { get; set; }
}
