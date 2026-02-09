using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Department : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
