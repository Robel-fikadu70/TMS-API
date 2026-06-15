using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentsService) : ControllerBase
{
    //GET /api/students
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentsService.GetAllAsync();
        return Ok(students);
    }

    //GET /api/studets/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await studentsService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    //POST /api/students
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var record = await studentsService.RegisterAsync(request.name, request.age, request.GPA);

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    //DELETE /api/studets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentsService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

//request model
public record CreateStudentRequest(string name, int age, decimal GPA);
