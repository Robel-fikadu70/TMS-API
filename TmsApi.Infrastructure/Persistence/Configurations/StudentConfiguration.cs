// TmsApi/Data/Configurations/StudentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(20);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

        builder.Property(s => s.GPA).HasColumnType("decimal(3, 2)");

        builder.Property(s => s.IsActive).IsRequired();

        builder.HasIndex(s => s.RegistrationNumber).IsUnique(); // Make RegistrationNumber unique.

        builder.Property(s => s.Version).IsRowVersion(); //  Concurrency (Ex 8) This tells EF Core to use this for concurrency checks

        builder.Property<DateTime>("LastUpdated"); //Shadow Property for Audit (Ex 8)

        // Soft Delete Filter (Ex 9)
        // This automatically hides deleted students from EVERY query
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Relationships:

        // Student (One) to Enrollment (Many)
        // If a Student is deleted, prevent deletion if there are associated Enrollments.
        builder
            .HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student (One) to Certificate (Many)
        // If a Student is deleted, prevent deletion if there are issued Certificates.
        // This ensures the validity of issued certificates.
        builder
            .HasMany(s => s.Certificates)
            .WithOne(c => c.Student)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
