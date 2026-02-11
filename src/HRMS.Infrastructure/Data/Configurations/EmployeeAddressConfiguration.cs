using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeAddressConfiguration : IEntityTypeConfiguration<EmployeeAddress>
{
    public void Configure(EntityTypeBuilder<EmployeeAddress> builder)
    {
        builder.ToTable("EmployeeAddresses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AddressLine1)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.AddressLine2)
            .HasMaxLength(200);

        builder.Property(e => e.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Type)
            .IsRequired();

        builder.Property(e => e.IsPrimary)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Employee)
            .WithMany(e => e.Addresses)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.EmployeeId, e.IsPrimary });
    }
}
