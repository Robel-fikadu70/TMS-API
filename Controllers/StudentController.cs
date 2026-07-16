using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.DTOs;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService _studentService, LinkGenerator linkGenerator)
    : ControllerBase
{
    // POST /api/students
    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Create a new Student")]
    [EndpointDescription("Creates a Student with a unique Registration number.")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterStudentRequest request,
        CancellationToken ct
    )
    {
        var record = await _studentService.RegisterAsync(request, ct);

        if (record == null)
            return BadRequest("Student registraion failed. something went wrong.");
        // Returns 201 Created with Location header and the created Student record
        return CreatedAtAction(nameof(GetStudentById), new { id = record.Id }, record);
    }

    //GET /api/students
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDTO>), StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered list of students. pageSize is capped at 50. with optional admin mode to include soft deleted students."
    )]
    public async Task<IActionResult> GetPagedStudents(
        [FromQuery] PagedRequest request,
        CancellationToken ct,
        [FromQuery] bool adminMode = false
    )
    {
        var pagedResult = await _studentService.GetPagedStudentsAsync(request, adminMode, ct);
        return Ok(pagedResult);
    }

    //GET /api/students/{id}
    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get Student by ID")]
    [EndpointDescription(
        "Returns Student details with HATEOAS links. Return 404 if the Student does not exist."
    )]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var student = await _studentService.GetByIdAsync(id, ct);
        if (student == null)
            return NotFound();

        var links = new List<LinkDto>
        {
            //self
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetStudentById), new { id }),
                "self",
                "GET"
            ),
            //link for actions
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetStudentById), new { id }),
                "delete",
                "DELETE"
            ),
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetStudentById), new { id }),
                "update",
                "PATCH"
            ),
        };

        var detaildDto = new StudentDetailDTO
        {
            Id = student.Id,
            RegistrationNumber = student.RegistrationNumber,
            Name = student.Name,
            GPA = student.GPA,
            IsActive = student.IsActive,
            Links = links,
        };
        return Ok(detaildDto);
    }

    //PATCH api/students/{id}
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Update Student by ID")]
    [EndpointDescription(
        "Returns student details after change. Returns conflict if race condition detected."
    )]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        try
        {
            var result = await _studentService.UpdateAsync(id, request);
            return result == null ? NotFound() : Ok(result);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Data was modified by another user. Please refresh." });
        }
    }

    //DELETE api/students/soft/{id}
    [HttpDelete("soft/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete Student by ID")]
    [EndpointDescription("soft deletes student record.")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var success = await _studentService.SoftDeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    // Professional Bulk Endpoint
    [HttpPatch("archive-old-enrollments/{year}")]
    public async Task<IActionResult> Archive(int year)
    {
        var count = await _studentService.BulkArchiveEnrollmentsAsync(year);
        return Ok(new { archivedCount = count });
    }

    // DELETE /api/students/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete Student by ID")]
    [EndpointDescription("Hard deletes student record.")]
    public async Task<IActionResult> Delete(int id) // Changed id type to int
    {
        var deleted = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound(); // Returns 204 No Content or 404 Not Found
    }
}

//request model
public record CreateStudentRequest(string name, decimal GPA);
