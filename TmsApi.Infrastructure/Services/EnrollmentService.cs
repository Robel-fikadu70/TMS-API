using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    );
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<PagedResponse<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        PagedRequest request,
        CancellationToken ct
    );
    Task<bool> DeleteAsync(int id);
}

public class EnrollmentService(TmsDbContext _context, ILogger<EnrollmentService> _logger)
    : IEnrollmentService
{
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

    public async Task<PagedResponse<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        PagedRequest request,
        CancellationToken ct
    )
    {
        //start with no tracking
        var query = _context.Enrollments.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var sortBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "EnrolledAt" : request.OrderBy;

        query = sortBy switch
        {
            "Grade" => request.Descending
                ? query.OrderByDescending(e => e.Grade)
                : query.OrderBy(e => e.Grade),
            "EnrolledAt" or _ => request.Descending
                ? query.OrderByDescending(e => e.EnrolledAt)
                : query.OrderBy(e => e.EnrolledAt),
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);

        return new PagedResponse<EnrollmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
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
