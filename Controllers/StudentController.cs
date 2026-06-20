using Microsoft.AspNetCore.Mvc;
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

    // GET /api/students/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _studentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound(); // Returns 200 OK or 404 Not Found
    }

    // POST /api/students
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var record = await _studentService.RegisterAsync(request.name, request.GPA);

        // Returns 201 Created with Location header and the created StudentRecord DTO
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
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
