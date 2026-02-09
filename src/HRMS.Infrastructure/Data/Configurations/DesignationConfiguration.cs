using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.ToTable("Designations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Code)
            .HasMaxLength(20);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(d => d.RowVersion)
            .IsRowVersion();

        builder.HasIndex(d => d.Title);
    }
}
