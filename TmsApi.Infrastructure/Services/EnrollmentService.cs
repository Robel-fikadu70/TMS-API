using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

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
            .Where(e => e.CourseId == courseId)
            .ToListAsync(ct);

        return new PagedResponse<EnrollmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct)
    {
        // AnyAsync is faster and returns a bool.
        // We don't need .Include() because we are accessing e.Course.Code directly
        return await _context.Enrollments.AnyAsync(
            e => e.StudentId == studentId && e.Course.Code == courseCode,
            ct
        );
    }

    public async Task<List<EnrollmentWithDetailsResponseDto>?> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct
    )
    {
        return await _context
            .Enrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => new EnrollmentWithDetailsResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.Course.Code,
                e.StudentId,
                e.EnrolledAt
            ))
            .ToListAsync(ct);
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

    public async Task<EnrollmentDetailDto> GetEnrollmentById(int id, CancellationToken ct)
    {
        return await _context
                .Enrollments.AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new EnrollmentDetailDto(
                    e.Id,
                    e.StudentId,
                    e.Student.Name,
                    e.CourseId,
                    e.Course.Title,
                    e.Status,
                    e.EnrolledAt
                ))
                .FirstOrDefaultAsync(ct)
            ?? throw new Exception($"Enrollment with id {id} was not found.");
    }

    public async Task<PagedResponse<EnrollmentDetailDto>> GetDetailedEnrollments(
        PagedRequest request,
        CancellationToken ct
    )
    {
        var query = _context.Enrollments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(e =>
                e.Student.Name.Contains(request.Search)
                || e.Course.Title.Contains(request.Search)
                || e.Status.ToString().Contains(request.Search)
            );
        }

        var totalCount = await query.CountAsync(ct);

        var OrderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "EnrolledAt" : request.OrderBy;

        query = OrderBy switch
        {
            "Grade" => request.Descending
                ? query.OrderByDescending(e => e.Grade)
                : query.OrderBy(e => e.Grade),
            "StudentName" => request.Descending
                ? query.OrderByDescending(e => e.Student.Name)
                : query.OrderBy(e => e.Student.Name),
            "CourseTitle" => request.Descending
                ? query.OrderByDescending(e => e.Course.Title)
                : query.OrderBy(e => e.Course.Title),
            "Status" => request.Descending
                ? query.OrderByDescending(e => e.Status)
                : query.OrderBy(e => e.Status),
            "EnrolledAt" or _ => request.Descending
                ? query.OrderByDescending(e => e.EnrolledAt)
                : query.OrderBy(e => e.EnrolledAt),
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EnrollmentDetailDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status,
                e.EnrolledAt
            ))
            .ToListAsync(ct);

        return new PagedResponse<EnrollmentDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<EnrollmentDetailDto> UpdateEnrollmentStatus(
        int id,
        EnrollmentStatus status,
        CancellationToken ct
    )
    {
        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (enrollment is null)
            throw new("Enrollment not found.");
        enrollment.Status = status;
        await _context.SaveChangesAsync(ct);
        return await GetEnrollmentById(id, ct);
    }
}

public class TmsDatabaseException(string message) : Exception(message);
