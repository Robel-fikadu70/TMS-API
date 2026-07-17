// TmsApi/Data/Configurations/EnrollmentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(150);
        // for MaxScore e.g. 100.00
        builder.Property(a => a.MaxScore).HasColumnType("decimal(5,2)");
        // for Weight e.g. 0.35 for 35%
        builder.Property(a => a.Weight).HasColumnType("decimal(3, 2)");
        builder
            .HasOne(a => a.Course)
            .WithMany(c => c.Assessments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
