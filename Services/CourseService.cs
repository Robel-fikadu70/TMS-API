using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities; // For the actual database entities

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest course, CancellationToken ct);
    Task<bool> DeleteAsync(string code);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct
    );

    Task<IReadOnlyList<TopCourseSummaryRecord>> GetTopCoursesByEnrollmentAsync(int topCount);
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

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct
    )
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            Capacity = request.Capacity,
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);

        // Re-query to get the full DTO shape
        return (await GetByIdAsync(course.Id, ct))!;
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context
            .Courses.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.Capacity,
                c.Enrollments.Count
            ))
            .FirstOrDefaultAsync(ct);
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

    public async Task<IReadOnlyList<TopCourseSummaryRecord>> GetTopCoursesByEnrollmentAsync(
        int topCount
    )
    {
        if (topCount < 1)
            topCount = 5;
        var topCourses = await _context
            .Courses.Include(c => c.Enrollments)
            .OrderByDescending(x => x.Enrollments.Count)
            .Select(c => new TopCourseSummaryRecord(
                CourseCode: c.Code,
                CourseTitle: c.Title,
                EnrollmentCount: c.Enrollments.Count
            ))
            //.OrderByDescending(x => x.EnrollmentCount)
            .Take(topCount)
            .ToListAsync();

        return topCourses.AsReadOnly();
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct
    )
    {
        // TODO 1: Start with NoTracking
        var query = _context.Courses.AsNoTracking();

        // TODO 2: Search (Case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = $"%{request.Search}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, search) || EF.Functions.ILike(c.Code, search)
            );
        }

        // TODO 3: Count BEFORE paging
        var totalCount = await query.CountAsync(ct);

        // TODO 4: Sorting (Whitelisted to prevent SQL injection)
        query = request.OrderBy switch
        {
            "Code" => request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code),
            "Capacity" => request.Descending
                ? query.OrderByDescending(c => c.Capacity)
                : query.OrderBy(c => c.Capacity),
            _ => request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title),
        };

        // TODO 5: Paging & Projection
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.Capacity,
                c.Enrollments.Count
            ))
            .ToListAsync(ct);

        // TODO 6: Return the paged envelope
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        await _context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);
}
