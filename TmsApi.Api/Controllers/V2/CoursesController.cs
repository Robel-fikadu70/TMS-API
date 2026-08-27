using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Application.DTOs;

namespace TmsApi.Api.Controllers.V2;

[Authorize(Roles = "Instructor, Admin")]
[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(TmsDbContext context, ICachedCourseService cachedCourseService, IAuthorizationService authorizationService) : ControllerBase
{
    private readonly TmsDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var baseQuery = _context.Courses.AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);
        var rows = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code,
                c.Capacity,
                EnrollmentCount = c.Enrollments.Count,
            })
            .ToListAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasNext = page < totalPages;
        var hasPrevious = page > 1;
        return Ok(
            new
            {
                data = rows,
                meta = new
                {
                    totalCount,
                    page,
                    pageSize,
                    totalPages,
                    hasNext,
                    hasPrevious,
                },
                links = new
                {
                    self = $"/api/v2/courses?page={page}&pageSize={pageSize}",
                    next = hasNext
                        ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                        : (string?)null,
                    prev = hasPrevious
                        ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                        : (string?)null,
                    enroll = "/api/v2/enrollments",
                },
            }
        );
    }

    // 2. Add this NEW endpoint specifically for the Lab's "Popular Courses" scenario
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCourses(CancellationToken ct)
    {
        // This calls our Cache -> which calls the DB only if empty
        var courses = await cachedCourseService.GetAllCoursesAsync(ct);
        return Ok(courses);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);
        if(course == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, course, "CanEditCourse");
        if (!authResult.Succeeded)
        {
            return Forbid(); // 403 frobidden when caller doesn't own the resourse
        }
        course.Title = dto.Title;
        await _context.SaveChangesAsync();
        return NoContent();
    }

}
