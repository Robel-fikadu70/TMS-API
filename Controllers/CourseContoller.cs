using Microsoft.AspNetCore.Mvc;
using TmsApi.Data;
using TmsApi.DTOs; // For CourseRecord DTO
using TmsApi.Services; // For ICourseService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService; // Correct variable name for consistency

    // Constructor injection
    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET /api/courses
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses); // Returns 200 OK with list of CourseRecord DTOs
    }

    // GET /api/courses/{code}
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var record = await _courseService.GetByCodeAsync(code);
        return record is not null ? Ok(record) : NotFound(); // Returns 200 OK or 404 Not Found
    }

    // POST /api/courses
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        try
        {
            var record = await _courseService.CreateAsync(
                request.Code,
                request.Title,
                request.Capacity
            );
            // Returns 201 Created with Location header and the created CourseRecord DTO
            return CreatedAtAction(nameof(GetByCode), new { code = record.Code }, record);
        }
        catch (ArgumentException ex)
        {
            // Catch specific validation exceptions for better error messages
            return BadRequest(new { Message = ex.Message });
        }
    }

    // DELETE /api/courses/{code}
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await _courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound(); // Returns 204 No Content or 404 Not Found
    }
}

// Request Model
public record CreateCourseRequest(string Code, string Title, int Capacity);
