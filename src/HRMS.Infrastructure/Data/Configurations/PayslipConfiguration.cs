using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BasicSalary).HasPrecision(18, 2);
        builder.Property(p => p.HRA).HasPrecision(18, 2);
        builder.Property(p => p.MedicalAllowance).HasPrecision(18, 2);
        builder.Property(p => p.ConveyanceAllowance).HasPrecision(18, 2);
        builder.Property(p => p.SpecialAllowance).HasPrecision(18, 2);
        builder.Property(p => p.OtherAllowances).HasPrecision(18, 2);
        builder.Property(p => p.PF).HasPrecision(18, 2);
        builder.Property(p => p.ESI).HasPrecision(18, 2);
        builder.Property(p => p.ProfessionalTax).HasPrecision(18, 2);
        builder.Property(p => p.TDS).HasPrecision(18, 2);
        builder.Property(p => p.OtherDeductions).HasPrecision(18, 2);
        builder.Property(p => p.LossOfPayDays).HasPrecision(5, 2);
        builder.Property(p => p.LossOfPayAmount).HasPrecision(18, 2);
        builder.Property(p => p.GrossSalary).HasPrecision(18, 2);
        builder.Property(p => p.TotalDeductions).HasPrecision(18, 2);
        builder.Property(p => p.NetSalary).HasPrecision(18, 2);

        builder.Property(p => p.FilePath)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(p => p.Employee)
            .WithMany(e => e.Payslips)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PayrollRun)
            .WithMany(pr => pr.Payslips)
            .HasForeignKey(p => p.PayrollRunId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.EmployeeId);
        builder.HasIndex(p => p.PayrollRunId);
        builder.HasIndex(p => new { p.EmployeeId, p.Month, p.Year })
            .IsUnique();
    }
}
