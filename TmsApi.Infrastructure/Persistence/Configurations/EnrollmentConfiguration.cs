// TmsApi/Data/Configurations/EnrollmentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Grade).HasColumnType("decimal(3, 2)");

        builder.Property(e => e.EnrolledAt).IsRequired();

        // Foreign Keys
        builder.Property(e => e.StudentId).IsRequired();
        builder.Property(e => e.CourseId).IsRequired();

        // Composite Unique Index: A student can only enroll in a specific course once.
        builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
    }
}
