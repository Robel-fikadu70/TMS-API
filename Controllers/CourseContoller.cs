using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    // GET /api/courses
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await courseService.GetAllAsync();
        return Ok(courses);
    }

    // GET /api/courses/{code}
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var record = await courseService.GetByCodeAsync(code);
        return record is not null ? Ok(record) : NotFound();
    }

    // POST /api/courses
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var record = await courseService.CreateAsync(request.Code, request.Title, request.Capacity);

        return CreatedAtAction(nameof(GetByCode), new { code = record.Code }, record);
    }

    // DELETE /api/courses/{code}
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound();
    }
}

// Request Model
public record CreateCourseRequest(string Code, string Title, int Capacity);
