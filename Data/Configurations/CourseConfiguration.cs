// TmsApi/Data/Configurations/CourseConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(10);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);

        builder.Property(c => c.Capacity).IsRequired();

        builder.HasIndex(c => c.Code).IsUnique(); // Make Course Code unique.

        // Relationships:

        // Course (One) to Enrollment (Many)
        // If a Course is deleted, prevent deletion if there are associated Enrollments.
        // This ensures the integrity of student enrollment history.
        builder
            .HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course (One) to Assessment (Many)
        // If a Course is deleted, cascade delete its associated Assessments.
        // Assessments are specific to a course and lose meaning without it.
        builder
            .HasMany(c => c.Assessments)
            .WithOne(a => a.Course)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course (One) to Certificate (Many)
        // If a Course is deleted, prevent deletion if there are issued Certificates.
        // This ensures the validity of issued certificates.
        builder
            .HasMany(c => c.Certificates)
            .WithOne(c => c.Course)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
