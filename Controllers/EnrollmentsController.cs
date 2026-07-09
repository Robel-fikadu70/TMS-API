using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs; // For EnrollmentRecord DTO
using TmsApi.Services; // For IEnrollmentService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService _courseService,
    IEnrollmentService _enrollmentService
) : ControllerBase
{
    // GET /api/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get enrolments for a course")]
    [EndpointDescription("Returns enrollment for a course, Returns 404 if course does not exist.")]
    public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
    {
        // Rule: Always check if the parent (Course) exists first (404 check)
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
            return NotFound();

        var result = await _enrollmentService.GetByCourseAsync(courseId, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription(
        "Returns 404 if the course does not exist, 409if the course has reached MaxCapacity."
    )]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    )
    {
        // Rule 1: 404 before 409. Does the course exist?
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
            return NotFound();

        // Rule 2: Check capacity
        if (course.EnrollmentCount >= course.Capacity)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Course is full",
                    Detail =
                        $"Course '{course.Title}' has reached its maximum capacity of {course.Capacity}.",
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }

        var result = await _enrollmentService.CreateAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetEnrollment), new { courseId, id = result.Id }, result);
    }

    // DELETE /api/enrollments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Changed id type to int
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// Request Model
public record CreateEnrollmentRequest(string StudentId, string CourseCode);
