using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    );
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
    Task<PagedResponse<EnrollmentResponseDto>> GetAllAsync(PagedRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id);
}

public class EnrollmentService(TmsDbContext _context, ILogger<EnrollmentService> _logger)
    : IEnrollmentService
{
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

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    )
    {
        // TODO 2: Insert, Save, Re-read
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct
    )
    {
        return await _context
            .Enrollments.AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct
    )
    {
        return await _context
            .Enrollments.AsNoTracking()
            .Where(e => e.CourseId == courseId) // Filter by course
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);
    }

    public async Task<PagedResponse<EnrollmentResponseDto>> GetAllAsync(PagedRequest request, CancellationToken ct)
    {
        var query = _context.Enrollments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = $
        }

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
