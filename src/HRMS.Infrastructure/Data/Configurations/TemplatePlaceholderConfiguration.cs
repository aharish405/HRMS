using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class TemplatePlaceholderConfiguration : IEntityTypeConfiguration<TemplatePlaceholder>
{
    public void Configure(EntityTypeBuilder<TemplatePlaceholder> builder)
    {
        builder.ToTable("TemplatePlaceholders");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PlaceholderKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.DataType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("string");

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.SampleValue)
            .HasMaxLength(500);

        builder.Property(p => p.FormatString)
            .HasMaxLength(100);

        builder.Property(p => p.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(p => p.PlaceholderKey)
            .IsUnique();

        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.IsActive);
    }
}
