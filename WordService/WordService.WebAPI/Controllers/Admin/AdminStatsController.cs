using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordService.Infrastructure;

namespace WordService.WebAPI.Controllers.Admin;

[ApiController]
[Route("Admin/[action]")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly WordDbContext dbContext;

    public AdminStatsController(WordDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetLearningStats(CancellationToken cancellationToken)
    {
        var last7Days = DateTime.Now.Date.AddDays(-7);
        var activeUsers = await dbContext.UserWords.Select(x => x.UserId).Distinct().CountAsync(cancellationToken);
        var totalWords = await dbContext.UserWords.CountAsync(cancellationToken);
        var reviewsLast7Days = await dbContext.WordReviewLogs.CountAsync(x => x.CreationTime >= last7Days, cancellationToken);
        var totalReviews = await dbContext.WordReviewLogs.CountAsync(cancellationToken);

        return Ok(new
        {
            code = 200,
            data = new
            {
                activeUsers,
                totalWords,
                reviewsLast7Days,
                totalReviews,
            },
        });
    }
}
