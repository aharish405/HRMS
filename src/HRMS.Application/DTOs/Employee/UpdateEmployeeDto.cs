using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Employee;

public class UpdateEmployeeDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public int Gender { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int DesignationId { get; set; }

    public int? ReportingManagerId { get; set; }

    public DateTime? RelievingDate { get; set; }

    [Required]
    public int Status { get; set; }
}
