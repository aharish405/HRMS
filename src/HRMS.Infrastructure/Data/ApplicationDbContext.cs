using HRMS.Domain.Entities;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Salary> Salaries => Set<Salary>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<OfferLetter> OfferLetters => Set<OfferLetter>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OfferLetterTemplate> OfferLetterTemplates => Set<OfferLetterTemplate>();
    public DbSet<TemplatePlaceholder> TemplatePlaceholders => Set<TemplatePlaceholder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Designation>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Salary>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Payroll>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<PayrollRun>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Payslip>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<LeaveType>().HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<LeaveBalance>().HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<OfferLetter>().HasQueryFilter(o => !o.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set audit fields
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.AuditableEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedOn = DateTime.UtcNow;
                // CreatedBy will be set by the service layer
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.ModifiedOn = DateTime.UtcNow;
                // ModifiedBy will be set by the service layer
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
