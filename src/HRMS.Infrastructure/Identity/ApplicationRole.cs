using Microsoft.AspNetCore.Identity;

namespace HRMS.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
