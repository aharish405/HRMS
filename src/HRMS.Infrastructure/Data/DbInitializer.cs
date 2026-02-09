using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminUserAsync(userManager);

        // Seed Master Data
        await SeedMasterDataAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "HRAdmin", "Employee", "Manager" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role"
                };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@workaxis.com";

        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@12345");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }

    private static async Task SeedMasterDataAsync(ApplicationDbContext context)
    {
        // Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new Department { Name = "Information Technology", Code = "IT", Description = "IT Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Human Resources", Code = "HR", Description = "HR Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Finance", Code = "FIN", Description = "Finance Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Sales", Code = "SAL", Description = "Sales Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Department { Name = "Marketing", Code = "MKT", Description = "Marketing Department", IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        // Seed Designations
        if (!await context.Designations.AnyAsync())
        {
            var designations = new List<Designation>
            {
                new Designation { Title = "Software Engineer", Code = "SE", Level = 1, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Senior Software Engineer", Code = "SSE", Level = 2, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Team Lead", Code = "TL", Level = 3, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Project Manager", Code = "PM", Level = 4, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "HR Manager", Code = "HRM", Level = 4, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new Designation { Title = "Finance Manager", Code = "FM", Level = 4, IsActive = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.Designations.AddRangeAsync(designations);
            await context.SaveChangesAsync();
        }

        // Seed Leave Types
        if (!await context.LeaveTypes.AnyAsync())
        {
            var leaveTypes = new List<LeaveType>
            {
                new LeaveType { Name = "Casual Leave", Type = LeaveTypeEnum.CasualLeave, DefaultDaysPerYear = 12, IsActive = true, IsPaid = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new LeaveType { Name = "Sick Leave", Type = LeaveTypeEnum.SickLeave, DefaultDaysPerYear = 12, IsActive = true, IsPaid = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new LeaveType { Name = "Earned Leave", Type = LeaveTypeEnum.EarnedLeave, DefaultDaysPerYear = 15, IsActive = true, IsPaid = true, CreatedBy = "System", CreatedOn = DateTime.UtcNow },
                new LeaveType { Name = "Loss of Pay", Type = LeaveTypeEnum.LossOfPay, DefaultDaysPerYear = 0, IsActive = true, IsPaid = false, CreatedBy = "System", CreatedOn = DateTime.UtcNow }
            };

            await context.LeaveTypes.AddRangeAsync(leaveTypes);
            await context.SaveChangesAsync();
        }
    }
}
