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

        builder.Property(o => o.EmployeeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Designation)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.CTC)
            .HasPrecision(18, 2);

        builder.Property(o => o.Location)
            .HasMaxLength(100);

        builder.Property(o => o.TemplateContent)
            .IsRequired();

        builder.Property(o => o.GeneratedContent)
            .IsRequired();

        builder.Property(o => o.FilePath)
            .HasMaxLength(500);

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
        builder.HasOne(o => o.Employee)
            .WithMany(e => e.OfferLetters)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.EmployeeId);
        builder.HasIndex(o => o.GeneratedOn);
    }
}
