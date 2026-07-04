using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs;
using TmsApi.Services; // For ICourseService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService _courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct
    )
    {
        var result = await _courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        // TODO 3: Call service and return Ok or NotFound
        var course = await _courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // Check business rule BEFORE trying to save
        if (await _courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Course code already exists",
                    Detail = $"A course with code '{request.Code}' is already registered.",
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }
        // TODO 4: Call CreateAsync and return CreatedAtAction
        var result = await _courseService.CreateAsync(request, ct);

        // This pattern is required for the 'Location' header in the response
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

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
