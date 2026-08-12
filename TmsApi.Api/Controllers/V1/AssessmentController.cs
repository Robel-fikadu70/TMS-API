using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[Tags("Assessments")]
public class AssessmentController(
    IAssessmentService _assessmentService,
    ICourseService _courseService,
    ILogger<AssessmentController> _logger,
    LinkGenerator linkGenerator
) : ControllerBase
{
    [HttpPost("courses/{courseId:int}/assessments")]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Create Assessment")]
    public async Task<IActionResult> Create(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct
    )
    {
        try
        {
            var course = await _courseService.GetByIdAsync(courseId, ct);
            if (course == null)
                return NotFound();

            var result = await _assessmentService.CreateAsync(courseId, request, ct);

            return CreatedAtAction(nameof(GetAssessmentById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new ProblemDetails { Title = "Weight Limit Exceed", Detail = ex.Message }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Creating Assessment");
            return StatusCode(500, "An internal error occured");
        }
    }

    [HttpGet("courses/{courseId:int}/assessments", Name = nameof(GetByCourse))]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get Course Assessments")]
    public async Task<IActionResult> GetByCourse(int courseId, CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
            return NotFound();

        var assessments = await _assessmentService.GetByCourseIdAsync(courseId, ct);
        return Ok(assessments);
    }

    [HttpGet("assessments/{id:int}", Name = nameof(GetAssessmentById))]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get Assessment by Id")]
    public async Task<IActionResult> GetAssessmentById(int id, CancellationToken ct)
    {
        var assessment = await _assessmentService.GetByIdAsync(id, ct);
        if (assessment == null)
            return NotFound();

        var links = new List<LinkDto>
        {
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(GetAssessmentById), new { id }),
                "self",
                "GET"
            ),
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(Update), new { id }),
                "Update",
                "PUT"
            ),
            new(
                linkGenerator.GetPathByName(HttpContext, nameof(Delete), new { id }),
                "Delete",
                "DELETE"
            ),
        };

        var detailedDTO = new AssessmentDetailDto
        {
            Id = assessment.Id,
            Title = assessment.Title,
            MaxScore = assessment.MaxScore,
            Weight = assessment.Weight,
            CourseId = assessment.CourseId,
            Links = links,
        };

        return Ok(detailedDTO);
    }

    [HttpPut("assessment/{id:int}", Name = nameof(Update))]
    public async Task<IActionResult> Update(
        int id,
        UpdateAssessmentRequest request,
        CancellationToken ct
    )
    {
        try
        {
            var result = await _assessmentService.UpdateAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    // DELETE /api/assessments/10
    [HttpDelete("assessments/{id:int}")]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete Assessment")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _assessmentService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
