using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        //properties
        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(20);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

        builder.Property(s => s.GPA).HasColumnType("decimal(3, 2)");

        builder.Property(s => s.IsActive).IsRequired();

        // Relationships
        // Student has many Enrollments
        builder
            .HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict deletion to prevent orphaned enrollments

        // Student has many Certificates
        // builder.HasMany(s => s.Certificates)
        //        .WithOne(c => c.Student)
        //        .HasForeignKey(c => c.StudentId)
        //        .OnDelete(DeleteBehavior.Restrict); // Restrict deletion
    }
}
