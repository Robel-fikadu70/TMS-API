using Microsoft.AspNetCore.Mvc;
using TmsApi.Data;
using TmsApi.DTOs; // For CourseRecord DTO
using TmsApi.Entities;
using TmsApi.Services; // For ICourseService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService _courseService) : ControllerBase
{
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

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        // TODO 3: Call service and return Ok or NotFound
        var course = await _courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
    {
        // TODO 4: Call CreateAsync and return CreatedAtAction
        var result = await _courseService.CreateAsync(course, ct);

        // This pattern is required for the 'Location' header in the response
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

    // // POST /api/courses
    // [HttpPost]
    // public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    // {
    //     try
    //     {
    //         var record = await _courseService.CreateAsync(
    //             request.Code,
    //             request.Title,
    //             request.Capacity
    //         );
    //         // Returns 201 Created with Location header and the created CourseRecord DTO
    //         return CreatedAtAction(nameof(GetByCode), new { code = record.Code }, record);
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         // Catch specific validation exceptions for better error messages
    //         return BadRequest(new { Message = ex.Message });
    //     }
    // }

    // DELETE /api/courses/{code}
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await _courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound(); // Returns 204 No Content or 404 Not Found
    }

    // GET /api/courses/top-by-enrollment
    [HttpGet("top-by-enrollment")]
    public async Task<IActionResult> GetTopCoursesByEnrollment([FromQuery] int topCount = 5)
    {
        var topCourses = await _courseService.GetTopCoursesByEnrollmentAsync(topCount);
        return Ok(topCourses);
    }
}

// Request Model
public record CreateCourseRequest(string Code, string Title, int Capacity);
