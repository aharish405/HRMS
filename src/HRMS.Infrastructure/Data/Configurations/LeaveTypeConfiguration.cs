using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(l => l.RowVersion)
            .IsRowVersion();

        builder.HasIndex(l => l.Name);
    }
}
