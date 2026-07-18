using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);

        // Natural Key: Unique and restricted length
        builder.Property(c => c.SerialNumber).IsRequired().HasMaxLength(50);

        builder.HasIndex(c => c.SerialNumber).IsUnique();

        builder.Property(c => c.IssuedAt).IsRequired();

        // Relationship mapping
        builder
            .HasOne(c => c.Student)
            .WithMany(s => s.Certificates)
            .HasForeignKey(c => c.StudentId);

        builder
            .HasOne(c => c.Course)
            .WithMany(co => co.Certificates)
            .HasForeignKey(c => c.CourseId);
    }
}
