using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentRegistrationNumber, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(int id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly ILogger<EnrollmentService> _logger;
    private readonly TmsDbContext _context; // Inject DbContext
    private readonly IStudentService _studentService; // Inject Student Service
    private readonly ICourseService _courseService; // Inject Course Service

    public EnrollmentService(
        ILogger<EnrollmentService> logger,
        TmsDbContext context,
        IStudentService studentService,
        ICourseService courseService
    )
    {
        _logger = logger;
        _context = context;
        _studentService = studentService;
        _courseService = courseService;
    }

    // Helper method to map an Enrollment entity to an EnrollmentRecord DTO
    private EnrollmentRecord MapToEnrollmentRecord(Enrollment enrollment)
    {
        string studentRegNumber = enrollment.Student?.RegistrationNumber ?? "UNKNOWN";
        string courseCode = enrollment.Course?.Code ?? "UNKNOWN";

        return new EnrollmentRecord(
            Id: enrollment.Id,
            StudentId: studentRegNumber,
            CourseCode: courseCode,
            Grade: enrollment.Grade,
            EnrolledAt: enrollment.EnrolledAt
        );
    }

    public async Task<EnrollmentRecord> EnrollAsync(
        string studentRegistrationNumber,
        string courseCode
    )
    {
        // 1. Validate Student existence
        var studentEntity = await _context.Students.FirstOrDefaultAsync(s =>
            s.RegistrationNumber == studentRegistrationNumber
        );
        if (studentEntity == null)
        {
            throw new ArgumentException(
                $"Student with RegistrationNumber '{studentRegistrationNumber}' does not exist."
            );
        }

        // 2. Validate Course existence
        var courseEntity = await _context
            .Courses.Include(c => c.Enrollments) // Include enrollments to check capacity
            .FirstOrDefaultAsync(c => c.Code == courseCode);
        if (courseEntity == null)
        {
            throw new ArgumentException($"Course with Code '{courseCode}' does not exist.");
        }

        // 3. Check for duplicate enrollment
        var existingEnrollment = await _context.Enrollments.AnyAsync(e =>
            e.StudentId == studentEntity.Id && e.CourseId == courseEntity.Id
        );

        if (existingEnrollment)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt: Student '{StudentRegNum}' already in Course '{CourseCode}'",
                studentRegistrationNumber,
                courseCode
            );
            throw new ArgumentException(
                $"Student '{studentRegistrationNumber}' is already enrolled in Course '{courseCode}'."
            );
        }

        // 4. Capacity check
        if (courseEntity.Enrollments.Count >= courseEntity.Capacity)
        {
            throw new ArgumentException(
                $"Course '{courseCode}' is full. Current enrollment: {courseEntity.Enrollments.Count}/{courseEntity.Capacity}."
            );
        }

        // 5. Create new Enrollment entity
        var newEnrollment = new Enrollment
        {
            StudentId = studentEntity.Id,
            CourseId = courseEntity.Id,
            EnrolledAt = DateTime.UtcNow,
            Grade = null, // Initially no grade
        };

        _context.Enrollments.Add(newEnrollment); // Stage for insertion
        await _context.SaveChangesAsync(); // Commit to the database (Id is now populated)

        _logger.LogInformation(
            "Enrolled Student '{StudentRegNum}' in Course '{CourseCode}' (Enrollment ID: {EnrollmentId})",
            studentRegistrationNumber,
            courseCode,
            newEnrollment.Id
        );

        // Populate navigation properties for mapping
        newEnrollment.Student = studentEntity;
        newEnrollment.Course = courseEntity;

        return MapToEnrollmentRecord(newEnrollment);
    }

    public async Task<EnrollmentRecord?> GetByIdAsync(int id)
    {
        var enrollmentEntity = await _context
            .Enrollments.Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollmentEntity == null)
        {
            _logger.LogWarning("Enrollment with ID: {EnrollmentId} not found.", id);
            return null;
        }

        return MapToEnrollmentRecord(enrollmentEntity);
    }

    public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        var enrollmentEntities = await _context
            .Enrollments.Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync();

        var enrollmentRecords = enrollmentEntities.Select(MapToEnrollmentRecord).ToList();

        return enrollmentRecords.AsReadOnly();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollmentToDelete = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id);

        if (enrollmentToDelete == null)
        {
            _logger.LogWarning("Delete failed: Enrollment with ID '{EnrollmentId}' not found.", id);
            return false;
        }

        _context.Enrollments.Remove(enrollmentToDelete); // Stage for deletion
        await _context.SaveChangesAsync(); // Commit deletion to the database

        _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);

        return true;
    }
}

public class TmsDatabaseException(string message) : Exception(message);
