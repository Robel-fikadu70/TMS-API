using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs; // For EnrollmentRecord DTO
using TmsApi.Services; // For IEnrollmentService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    // GET /api/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    // GET /api/enrollments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) // Changed id type to int
    {
        var record = await _enrollmentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    // POST /api/enrollments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        try
        {
            var record = await _enrollmentService.EnrollAsync(
                request.StudentId,
                request.CourseCode
            );
            // Returns 201 Created with Location header pointing to GetById
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }
        catch (ArgumentException ex)
        {
            // Catch specific validation exceptions for better error messages
            return BadRequest(new { Message = ex.Message });
        }
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
