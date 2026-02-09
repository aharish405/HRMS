using Microsoft.AspNetCore.Identity;

namespace HRMS.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public int? EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; }
    public DateTime? LastLoginOn { get; set; }
}
