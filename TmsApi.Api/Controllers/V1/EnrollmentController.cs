using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService = enrollmentService;

    // GET api/v1/enrollments
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EnrollmentDetailDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get paged enrollments")]
    [EndpointDescription(
        "Returns a paginated list of enrollments with student and course information."
    )]
    public async Task<ActionResult<PagedResponse<EnrollmentDetailDto>>> GetEnrollments(
        [FromQuery] PagedRequest request,
        CancellationToken ct
    )
    {
        var result = await _enrollmentService.GetDetailedEnrollments(request, ct);

        return Ok(result);
    }

    // PATCH api/v1/enrollments/5/status
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(EnrollmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Update enrollment status")]
    [EndpointDescription("Updates the status of an enrollment.")]
    public async Task<ActionResult<EnrollmentDetailDto>> UpdateEnrollmentStatus(
        int id,
        UpdateEnrollmentStatusRequest request,
        CancellationToken ct
    )
    {
        var result = await _enrollmentService.UpdateEnrollmentStatus(id, request.Status, ct);

        return Ok(result);
    }
}
