using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.NumberOfDays)
            .HasPrecision(5, 2);

        builder.Property(l => l.Reason)
            .HasMaxLength(1000);

        builder.Property(l => l.ApprovedBy)
            .HasMaxLength(256);

        builder.Property(l => l.ApprovalComments)
            .HasMaxLength(1000);

        builder.Property(l => l.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(l => l.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(l => l.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.LeaveType)
            .WithMany(lt => lt.LeaveRequests)
            .HasForeignKey(l => l.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => l.EmployeeId);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.StartDate);
    }
}
