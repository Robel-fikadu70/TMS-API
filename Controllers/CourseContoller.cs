using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs;
using TmsApi.Services; // For ICourseService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService _courseService, LinkGenerator linkGenerator)
    : ControllerBase
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
        if (course == null)
            return NotFound();

        var links = new List<LinkDto>
        {
            //'self' link that points back to this exact method
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id }),
                "self",
                "GET"
            ),
            //link for actions (updat/delete)
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id }),
                "delete",
                "DELETE"
            ),
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id }),
                "update",
                "PUT"
            ),
            //link to the list of enrollments using the name of the method in enrollment controller
            new(
                linkGenerator.GetPathByName(
                    HttpContext,
                    "ListCourseEnrollments",
                    new { courseId = id }
                ),
                "enrollments",
                "GET"
            ),
        };
        //only show enrollment link if the capacity is not full
        if (course.EnrollmentCount < course.Capacity)
        {
            links.Add(
                new(
                    linkGenerator.GetPathByName(
                        HttpContext,
                        "ListCourseEnrollments",
                        new { courseId = id }
                    ),
                    "enroll",
                    "POST"
                )
            );
        }

        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            Capacity = course.Capacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links,
        };

        return Ok(detailDto);
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
