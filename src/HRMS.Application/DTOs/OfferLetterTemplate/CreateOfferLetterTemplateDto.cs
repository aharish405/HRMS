using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.OfferLetterTemplate;

public class CreateOfferLetterTemplateDto
{
    [Required(ErrorMessage = "Template name is required")]
    [StringLength(200, ErrorMessage = "Template name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Template content is required")]
    public string Content { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}
