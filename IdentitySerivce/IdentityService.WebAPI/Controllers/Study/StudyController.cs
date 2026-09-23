using System.Security.Claims;
using IdentitySerivce.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.WebAPI.Controllers;

[ApiController]
[Route("/api/identity/[controller]/[action]")]
[Authorize]
public class StudyController : ControllerBase
{
    private readonly IStudyService studyService;

    public StudyController(IStudyService studyService)
    {
        this.studyService = studyService;
    }

    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordStudyBody body, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (body == null)
        {
            return BadRequest("invalid request");
        }

        try
        {
            var dto = await studyService.RecordAsync(
                userId,
                new RecordStudyRequest(
                    body.ActivityType,
                    body.ContentId,
                    body.Title ?? string.Empty,
                    body.Category ?? string.Empty,
                    body.DurationSeconds),
                cancellationToken);
            return Ok(new { code = 200, data = dto });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MySummary(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var data = await studyService.GetSummaryAsync(userId, cancellationToken);
        return Ok(new { code = 200, data });
    }

    [HttpGet]
    public async Task<IActionResult> MyList(
        [FromQuery] string? activityType,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var (items, total) = await studyService.GetListAsync(userId, activityType, category, page, pageSize, cancellationToken);
        return Ok(new { code = 200, data = new { items, total, page, pageSize } });
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out userId);
    }
}

public class RecordStudyBody
{
    public string ActivityType { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Category { get; set; }
    public int DurationSeconds { get; set; }
}
