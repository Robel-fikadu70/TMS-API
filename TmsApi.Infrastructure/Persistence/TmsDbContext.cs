using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence.Configurations;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This line tells EF Core to find and apply all configurations
        // that implement IEntityTypeConfiguration in the same assembly as TmsDbContext.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);

        // Call the base method
        base.OnModelCreating(modelBuilder);
    }
}
