using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.TotalDays)
            .HasPrecision(5, 2);

        builder.Property(l => l.UsedDays)
            .HasPrecision(5, 2);

        builder.Property(l => l.AvailableDays)
            .HasPrecision(5, 2);

        builder.Property(l => l.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(l => l.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(l => l.Employee)
            .WithMany(e => e.LeaveBalances)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.LeaveType)
            .WithMany(lt => lt.LeaveBalances)
            .HasForeignKey(l => l.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => new { l.EmployeeId, l.LeaveTypeId, l.Year })
            .IsUnique();
    }
}
