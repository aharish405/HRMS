using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Designation;

public class CreateDesignationDto
{
    [Required(ErrorMessage = "Designation title is required")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Designation code is required")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Level is required")]
    [Range(1, 10)]
    public int Level { get; set; }

    public bool IsActive { get; set; } = true;
}
