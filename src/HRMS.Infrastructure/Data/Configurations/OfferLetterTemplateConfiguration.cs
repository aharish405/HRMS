using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class OfferLetterTemplateConfiguration : IEntityTypeConfiguration<OfferLetterTemplate>
{
    public void Configure(EntityTypeBuilder<OfferLetterTemplate> builder)
    {
        builder.ToTable("OfferLetterTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Content)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasMaxLength(100);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(t => t.OfferLetters)
            .WithOne(o => o.Template)
            .HasForeignKey(o => o.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.IsDefault);
    }
}
