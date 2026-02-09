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
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.CandidateAddress)
            .IsRequired()
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

        // Indexes
        builder.HasIndex(o => o.CandidateEmail);
        builder.HasIndex(o => o.GeneratedOn);
        builder.HasIndex(o => o.Status);
    }
}
