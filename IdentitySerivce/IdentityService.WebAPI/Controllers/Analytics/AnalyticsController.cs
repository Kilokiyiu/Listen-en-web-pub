using System.Security.Claims;
using IdentitySerivce.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.WebAPI.Controllers;

[ApiController]
[Route("/api/identity/[controller]/[action]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        this.analyticsService = analyticsService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Track([FromBody] TrackPageViewRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Path) || string.IsNullOrWhiteSpace(request.VisitorId))
        {
            return BadRequest("invalid request");
        }

        var path = request.Path.Trim();
        if (path.Length > 200 || !path.StartsWith('/'))
        {
            return BadRequest("invalid path");
        }

        var visitorId = request.VisitorId.Trim();
        if (visitorId.Length > 64)
        {
            return BadRequest("invalid visitor id");
        }

        Guid? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }
        else if (Guid.TryParse(request.UserId, out var bodyUserId))
        {
            userId = bodyUserId;
        }

        await analyticsService.TrackPageViewAsync(path, visitorId, userId, cancellationToken);
        return Ok(new { code = 200 });
    }
}

public class TrackPageViewRequest
{
    public string Path { get; set; } = string.Empty;
    public string VisitorId { get; set; } = string.Empty;
    public string? UserId { get; set; }
}
