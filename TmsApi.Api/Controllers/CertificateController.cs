using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api")]
[Tags("Certificates")]
public class CertificatesController(ICertificateService certificateService) : ControllerBase
{
    // POST /api/certificates
    [HttpPost("certificates")]
    public async Task<IActionResult> Issue(IssueCertificateRequest request, CancellationToken ct)
    {
        try
        {
            var result = await certificateService.IssueAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    // GET /api/certificates/{id}
    [HttpGet("certificates/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var cert = await certificateService.GetByIdAsync(id, ct);
        return cert != null ? Ok(cert) : NotFound();
    }

    // GET /api/certificates/verify/{serial}
    // Pro Tip: Useful for QR codes on printed certificates
    [HttpGet("certificates/verify/{serial}")]
    public async Task<IActionResult> Verify(string serial, CancellationToken ct)
    {
        var cert = await certificateService.GetBySerialAsync(serial, ct);
        return cert != null
            ? Ok(cert)
            : NotFound(new { Message = "Invalid Certificate Serial Number" });
    }

    // GET /api/students/{studentId}/certificates
    [HttpGet("students/{studentId:int}/certificates")]
    public async Task<IActionResult> GetByStudent(int studentId, CancellationToken ct)
    {
        var certificates = await certificateService.GetByStudentIdAsync(studentId, ct);
        return Ok(certificates);
    }

    // DELETE /api/certificates/{id}
    [HttpDelete("certificates/{id:int}")]
    public async Task<IActionResult> Revoke(int id, CancellationToken ct)
    {
        var revoked = await certificateService.RevokeAsync(id, ct);
        return revoked ? NoContent() : NotFound();
    }
}
