using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript([FromBody] object? _)
    {
        // Simulate a long-running task (e.g., generating a PDF)
        await Task.Delay(5000); 
        return Ok(new { Message = "Transcript generated." });
    }
}