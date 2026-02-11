using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class SalaryConfiguration : IEntityTypeConfiguration<Salary>
{
    public void Configure(EntityTypeBuilder<Salary> builder)
    {
        builder.ToTable("Salaries");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BasicSalary)
            .HasPrecision(18, 2);

        builder.Property(s => s.HRA)
            .HasPrecision(18, 2);

        builder.Property(s => s.MedicalAllowance)
            .HasPrecision(18, 2);

        builder.Property(s => s.ConveyanceAllowance)
            .HasPrecision(18, 2);

        builder.Property(s => s.SpecialAllowance)
            .HasPrecision(18, 2);

        builder.Property(s => s.OtherAllowances)
            .HasPrecision(18, 2);

        builder.Property(s => s.PF)
            .HasPrecision(18, 2);

        builder.Property(s => s.ESI)
            .HasPrecision(18, 2);

        builder.Property(s => s.ProfessionalTax)
            .HasPrecision(18, 2);

        builder.Property(s => s.TDS)
            .HasPrecision(18, 2);

        builder.Property(s => s.OtherDeductions)
            .HasPrecision(18, 2);

        builder.Property(s => s.GrossSalary)
            .HasPrecision(18, 2);

        builder.Property(s => s.TotalDeductions)
            .HasPrecision(18, 2);

        builder.Property(s => s.NetSalary)
            .HasPrecision(18, 2);

        builder.Property(s => s.CTC)
            .HasPrecision(18, 2);

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(s => s.Employee)
            .WithMany(e => e.Salaries)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.OfferLetter)
            .WithMany(o => o.Salaries)
            .HasForeignKey(s => s.OfferLetterId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(s => s.EmployeeId);
        builder.HasIndex(s => s.OfferLetterId);
        builder.HasIndex(s => s.EffectiveFrom);
        builder.HasIndex(s => new { s.EmployeeId, s.EffectiveFrom });
    }
}
