using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProcessedBy)
            .HasMaxLength(256);

        builder.Property(p => p.ApprovedBy)
            .HasMaxLength(256);

        builder.Property(p => p.LockedBy)
            .HasMaxLength(256);

        builder.Property(p => p.TotalGrossPay)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalDeductions)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalNetPay)
            .HasPrecision(18, 2);

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(p => new { p.Month, p.Year })
            .IsUnique();

        builder.HasIndex(p => p.Status);
    }
}
