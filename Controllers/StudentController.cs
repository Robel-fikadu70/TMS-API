using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.DTOs;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    // Constructor injection
    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // GET /api/students
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _studentService.GetAllAsync();
        return Ok(students); // Returns 200 OK with list of StudentRecord DTOs
    }

    //GET /api/students/paged
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedStudents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var pagedResult = await _studentService.GetPagedStudentsAsync(pageNumber, pageSize);
        return Ok(pagedResult);
    }

    // GET /api/students/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, [FromQuery] bool adminMode = false)
    {
        var student = await _studentService.GetByIdAsync(id, adminMode);
        return student == null ? NotFound() : Ok(student);
    }

    // POST /api/students
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var record = await _studentService.RegisterAsync(request.name, request.GPA);

        // Returns 201 Created with Location header and the created StudentRecord DTO
        return CreatedAtAction(nameof(Get), new { id = record.Id }, record);
    }

    [HttpPut("{id}")]
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

    [HttpDelete("soft/{id}")]
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
    public async Task<IActionResult> Delete(int id) // Changed id type to int
    {
        var deleted = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound(); // Returns 204 No Content or 404 Not Found
    }
}

//request model
public record CreateStudentRequest(string name, decimal GPA);
