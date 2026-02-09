namespace HRMS.Application.DTOs.Designation;

public class DesignationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
}
