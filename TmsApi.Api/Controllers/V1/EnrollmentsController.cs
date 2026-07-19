using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs; // For EnrollmentRecord DTO
using TmsApi.Infrastructure.Services; // For IEnrollmentService

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/courses/{courseId:int}/enrollments")]
[ApiVersion("1.0")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService _courseService,
    IStudentService _studentService,
    IEnrollmentService _enrollmentService
) : ControllerBase
{
    // POST api/courses/{courseId}/enrollments
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enroll a student in a course")]
    [EndpointDescription(
        "Returns 404 if the course or student does not exist, 409 if the course has reached MaxCapacity."
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

        // check if student exist
        var student = await _studentService.StudentExists(request.StudentId, ct);
        if (!student)
            return NotFound();

        var result = await _enrollmentService.CreateAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetEnrollment), new { courseId, id = result.Id }, result);
    }

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(PagedResponse<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get enrolments for a course")]
    [EndpointDescription(
        "Returns paged enrollment for a course, Returns 404 if course does not exist."
    )]
    public async Task<IActionResult> GetCourseEnrollments(
        int courseId,
        PagedRequest request,
        CancellationToken ct
    )
    {
        // Rule: Always check if the parent (Course) exists first (404 check)
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
            return NotFound();

        var result = await _enrollmentService.GetByCourseAsync(courseId, request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get specific enrolment for a course")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
            return NotFound();

        var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    // DELETE /api/course/{courseId}/enrollments/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete specific enrollment for a course")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// Request Model
public record CreateEnrollmentRequest(string StudentId, string CourseCode);
