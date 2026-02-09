using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Department;

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Department name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Department code is required")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
