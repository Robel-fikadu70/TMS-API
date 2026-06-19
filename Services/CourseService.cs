using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.
using TmsApi.Data;
using TmsApi.DTOs; // Use the DTO for the interface
using TmsApi.Entities; // For the actual database entities

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, int capacity);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private readonly ILogger<CourseService> _logger;
    private readonly TmsDbContext _context;

    public CourseService(ILogger<CourseService> logger, TmsDbContext context) // Inject TmsDbContext
    {
        _logger = logger;
        _context = context;
    }

    // Helper method to map a Course entity to a CourseRecord DTO
    // includes calculating EnrolledCount from the database
    private CourseRecord MapToCourseRecord(Course course)
    {
        int enrolledCount = course.Enrollments?.Count ?? 0; // Safely get count if loaded

        return new CourseRecord(
            Code: course.Code,
            Title: course.Title,
            Capacity: course.Capacity,
            EnrolledCount: enrolledCount
        );
    }

    public async Task<CourseRecord> CreateAsync(string code, string title, int capacity)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Course code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title is required.", nameof(title));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));

        // Check if a course with this code already exists in the database
        var existingCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Code == code);
        if (existingCourse != null)
        {
            throw new ArgumentException($"Course {code} already exists.");
        }

        // Create a new Course entity
        var courseEntity = new Course
        {
            Code = code.ToUpper(),
            Title = title,
            Capacity = capacity,
            // Enrollments, Assessments, Certificates collections are initialized by default
        };

        _context.Courses.Add(courseEntity); // Stage for insertion
        await _context.SaveChangesAsync(); // Commit to the database (Id is now populated)

        _logger.LogInformation(
            "Created course {CourseCode} with title {CourseTitle}",
            courseEntity.Code,
            courseEntity.Title
        );

        // Map the created entity to the DTO before returning (EnrolledCount is 0 for a new course)
        return MapToCourseRecord(courseEntity);
    }

    public async Task<CourseRecord?> GetByCodeAsync(string code)
    {
        // Query the database, including Enrollments to calculate EnrolledCount
        var courseEntity = await _context
            .Courses.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code);

        if (courseEntity == null)
        {
            _logger.LogWarning("Course {CourseCode} not found.", code);
            return null;
        }

        // Map the found entity to the DTO
        return MapToCourseRecord(courseEntity);
    }

    public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        // Query the database, including Enrollments for each course
        var courseEntities = await _context
            .Courses.Include(c => c.Enrollments) // Eagerly load enrollments
            .ToListAsync();

        // Map the list of entities to a list of DTOs
        var courseRecords = courseEntities.Select(MapToCourseRecord).ToList();

        return courseRecords.AsReadOnly(); // Return as IReadOnlyList for immutability
    }

    public async Task<bool> DeleteAsync(string code)
    {
        // Find the course entity first
        var courseToDelete = await _context.Courses.FirstOrDefaultAsync(c => c.Code == code);

        if (courseToDelete == null)
        {
            _logger.LogWarning("Delete failed: course {CourseCode} not found.", code);
            return false;
        }

        // Check for existing enrollments for this course using the DbContext
        var hasEnrollment = await _context.Enrollments.AnyAsync(e =>
            e.CourseId == courseToDelete.Id
        );

        if (hasEnrollment)
        {
            _logger.LogWarning(
                "Cannot delete course {CourseCode}: active enrollments exist.",
                code
            );
            return false;
        }

        // Check for existing assessments for this course using the DbContext
        var hasAssessments = await _context.Assessments.AnyAsync(a =>
            a.CourseId == courseToDelete.Id
        );

        if (hasAssessments)
        {
            _logger.LogWarning(
                "Cannot delete course {CourseCode}: existing assessments are tied to it.",
                code
            );
            return false;
        }

        // Check for existing certificates for this course using the DbContext
        var hasCertificates = await _context.Certificates.AnyAsync(cert =>
            cert.CourseId == courseToDelete.Id
        );

        if (hasCertificates)
        {
            _logger.LogWarning(
                "Cannot delete course {CourseCode}: existing certificates are tied to it.",
                code
            );
            return false;
        }

        _context.Courses.Remove(courseToDelete); // Stage for deletion
        await _context.SaveChangesAsync(); // Commit deletion to the database

        _logger.LogInformation("Deleted course {CourseCode}", code);
        return true;
    }
}
