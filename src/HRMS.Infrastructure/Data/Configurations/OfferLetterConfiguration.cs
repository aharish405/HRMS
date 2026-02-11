using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class OfferLetterConfiguration : IEntityTypeConfiguration<OfferLetter>
{
    public void Configure(EntityTypeBuilder<OfferLetter> builder)
    {
        builder.ToTable("OfferLetters");

        builder.HasKey(o => o.Id);

        // Candidate Information
        builder.Property(o => o.CandidateName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.CandidateEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.CandidatePhone)
            .HasMaxLength(20);

        builder.Property(o => o.CandidateAddress)
            .HasMaxLength(500);

        // Salary Details
        builder.Property(o => o.BasicSalary)
            .HasPrecision(18, 2);

        builder.Property(o => o.HRA)
            .HasPrecision(18, 2);

        builder.Property(o => o.ConveyanceAllowance)
            .HasPrecision(18, 2);

        builder.Property(o => o.MedicalAllowance)
            .HasPrecision(18, 2);

        builder.Property(o => o.SpecialAllowance)
            .HasPrecision(18, 2);

        builder.Property(o => o.OtherAllowances)
            .HasPrecision(18, 2);

        builder.Property(o => o.CTC)
            .HasPrecision(18, 2);

        // Deduction Details
        builder.Property(o => o.PF)
            .HasPrecision(18, 2);

        builder.Property(o => o.ESI)
            .HasPrecision(18, 2);

        builder.Property(o => o.ProfessionalTax)
            .HasPrecision(18, 2);

        builder.Property(o => o.TDS)
            .HasPrecision(18, 2);

        builder.Property(o => o.OtherDeductions)
            .HasPrecision(18, 2);

        // Computed Fields
        builder.Property(o => o.GrossSalary)
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalDeductions)
            .HasPrecision(18, 2);

        builder.Property(o => o.NetSalary)
            .HasPrecision(18, 2);

        // Offer Details
        builder.Property(o => o.Location)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.AdditionalTerms)
            .HasMaxLength(1000);

        builder.Property(o => o.GeneratedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(o => o.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(o => o.Department)
            .WithMany()
            .HasForeignKey(o => o.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Designation)
            .WithMany()
            .HasForeignKey(o => o.DesignationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Template)
            .WithMany()
            .HasForeignKey(o => o.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Employee)
            .WithMany(e => e.OfferLetters)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(o => o.CandidateEmail);
        builder.HasIndex(o => o.GeneratedOn);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.AcceptanceToken);
    }
}
