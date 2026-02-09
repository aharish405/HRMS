using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Designation : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
