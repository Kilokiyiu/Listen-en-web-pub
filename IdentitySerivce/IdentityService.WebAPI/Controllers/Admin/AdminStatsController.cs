using IdentitySerivce.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.WebAPI.Controllers;

[ApiController]
[Route("/api/identity/Admin/[action]")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IAnalyticsService analyticsService;

    public AdminStatsController(IAnalyticsService analyticsService)
    {
        this.analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken)
    {
        var data = await analyticsService.GetOverviewAsync(cancellationToken);
        return Ok(new { code = 200, data });
    }

    [HttpGet]
    public async Task<IActionResult> Registrations([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var data = await analyticsService.GetRegistrationTrendAsync(days, cancellationToken);
        return Ok(new { code = 200, data });
    }

    [HttpGet]
    public async Task<IActionResult> Traffic([FromQuery] int days = 7, CancellationToken cancellationToken = default)
    {
        var data = await analyticsService.GetTrafficTrendAsync(days, cancellationToken);
        return Ok(new { code = 200, data });
    }

    [HttpGet]
    public async Task<IActionResult> TopPages([FromQuery] int days = 7, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var data = await analyticsService.GetTopPagesAsync(days, limit, cancellationToken);
        return Ok(new { code = 200, data });
    }
}
