using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
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

        builder.HasIndex(d => d.Name);
    }
}
