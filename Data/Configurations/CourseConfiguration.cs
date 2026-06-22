using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Code).IsRequired().HasMaxLength(10);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);

        builder.Property(c => c.Capacity).IsRequired();

        // Relationships
        // Course has many Enrollments
        builder
            .HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict deletion to prevent orphaned enrollments

        // Course has many Assessments
        // builder.HasMany(c => c.Assessments)
        //        .WithOne(a => a.Course)
        //        .HasForeignKey(a => a.CourseId)
        //        .OnDelete(DeleteBehavior.Cascade); Assessments delete with Course

        // Course has many Certificates
        // builder.HasMany(c => c.Certificates)
        //        .WithOne(c => c.Course)
        //        .HasForeignKey(c => c.CourseId)
        //        .OnDelete(DeleteBehavior.Restrict); Restrict deletion
    }
}
