using Microsoft.AspNetCore.Mvc;

namespace SmartStickyReviewer.Api.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public ActionResult<object> Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "Smart Sticky Reviewer",
            Timestamp = DateTime.UtcNow
        });
    }
}
