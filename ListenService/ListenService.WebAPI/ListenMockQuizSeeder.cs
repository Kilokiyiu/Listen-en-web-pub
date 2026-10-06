using DomainCommons;
using ListenService.Domain.Entity;
using ListenService.Infrastrucure;
using Microsoft.EntityFrameworkCore;

namespace ListenService.WebAPI;

/// <summary>
/// 开发/联调专用：为 CET-4 / CET-6 各播种一套原创听力在线题。
/// 仅应在 Development 环境写入；正式环境会隐藏已有联调卷，避免对用户可见。
/// 不含受版权保护的真题原文或音频。
/// </summary>
public static class ListenMockQuizSeeder
{
    /// <summary>当前联调卷名称</summary>
    public const string Cet4MockNameCn = "[Dev] 四级听力在线题联调卷";
    public const string Cet6MockNameCn = "[Dev] 六级听力在线题联调卷";

    /// <summary>历史命名（若库中仍有，生产环境一并隐藏）</summary>
    private static readonly string[] LegacyMockNameCns =
    [
        "大学英语四级听力模拟卷（第1套）",
        "大学英语六级听力模拟卷（第1套）",
        Cet4MockNameCn,
        Cet6MockNameCn,
    ];

    public static async Task SeedAsync(IServiceProvider services, ILogger logger, bool isDevelopment)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ListenDbContext>();

        if (!isDevelopment)
        {
            await HideDevMockAlbumsAsync(db, logger);
            return;
        }

        var cet4 = await EnsureCategoryAsync(db, "cet4", "大学英语四级", "CET-4", 2, "/images/cet4.png");
        var cet6 = await EnsureCategoryAsync(db, "cet6", "大学英语六级", "CET-6", 1, "/images/cet6.png");

        await EnsureCet4MockAsync(db, cet4.Id, logger);
        await EnsureCet6MockAsync(db, cet6.Id, logger);
    }

    private static async Task HideDevMockAlbumsAsync(ListenDbContext db, ILogger logger)
    {
        var albums = await db.Albums
            .Where(a => a.IsVisible && LegacyMockNameCns.Contains(a.Name.Chinese))
            .ToListAsync();
        if (albums.Count == 0)
            return;

        foreach (var album in albums)
            album.Hide();

        await db.SaveChangesAsync();
        logger.LogInformation("Hidden {Count} ListenService dev mock album(s) outside Development.", albums.Count);
    }

    private static async Task<Category> EnsureCategoryAsync(
        ListenDbContext db,
        string code,
        string nameCn,
        string nameEn,
        int sequenceNumber,
        string coverUrl)
    {
        var existing = await db.Categories.FirstOrDefaultAsync(c => c.Code == code);
        if (existing != null)
            return existing;

        var category = new Category(new MultilingualString(nameCn, nameEn), code, sequenceNumber, coverUrl);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    private static async Task EnsureCet4MockAsync(ListenDbContext db, Guid categoryId, ILogger logger)
    {
        if (await AlbumHasQuizAsync(db, categoryId, Cet4MockNameCn))
            return;

        var album = await EnsureAlbumAsync(
            db,
            categoryId,
            Cet4MockNameCn,
            "[Dev] CET-4 Listening Quiz Fixture",
            sequenceNumber: 0);

        await EnsureEpisodeShellAsync(db, album.Id);

        var news = new QuizSection(
            album.Id,
            "News Report 1",
            "A new study from a city university finds that students who study in groups for at least " +
            "two hours a week score higher on final exams. The researchers surveyed more than one " +
            "thousand undergraduates. They say group work helps students stay focused and share " +
            "useful notes. The library will open extra quiet rooms next month for team study.",
            null,
            1,
            "Section A");
        db.QuizSections.Add(news);

        db.QuizQuestions.Add(new QuizQuestion(
            news.Id, 1,
            "What does the study find about group study?",
            new[]
            {
                "A) It reduces exam scores",
                "B) It is linked to higher final exam scores",
                "C) It is only useful for graduate students",
                "D) It replaces all individual study"
            },
            1, 1,
            "新闻指出每周至少两小时小组学习的学生期末成绩更高。"));

        db.QuizQuestions.Add(new QuizQuestion(
            news.Id, 2,
            "What will the library do next month?",
            new[]
            {
                "A) Close on weekends",
                "B) Raise membership fees",
                "C) Open extra quiet rooms for team study",
                "D) Cancel group study programs"
            },
            2, 2,
            "末句提到图书馆下月将增设安静的小组学习房间。"));

        var conversation = new QuizSection(
            album.Id,
            "Long Conversation 1",
            "M: Hi, Lisa. Have you decided on a summer internship yet?\n" +
            "W: Not completely. I got an offer from a local media company, but the pay is quite low.\n" +
            "M: Still, the experience might help your résumé. What would you do there?\n" +
            "W: Mostly writing short news posts and helping with interviews. I like writing, so that part sounds good.\n" +
            "M: Then maybe take it if you can still work part-time at the café on weekends.\n" +
            "W: That is what I am thinking. I will reply to them tomorrow.",
            null,
            2,
            "Section B");
        db.QuizSections.Add(conversation);

        db.QuizQuestions.Add(new QuizQuestion(
            conversation.Id, 3,
            "What is Lisa mainly worried about?",
            new[]
            {
                "A) The company is too far away",
                "B) The internship pay is quite low",
                "C) She dislikes writing news posts",
                "D) She must work only on weekends"
            },
            1, 1,
            "Lisa 提到 offer 来自本地媒体公司，但薪水很低。"));

        db.QuizQuestions.Add(new QuizQuestion(
            conversation.Id, 4,
            "What will Lisa most likely do?",
            new[]
            {
                "A) Reject the offer immediately",
                "B) Ask for a full-time café job",
                "C) Reply to the company tomorrow",
                "D) Change her major"
            },
            2, 2,
            "对话结尾 Lisa 说明天会回复对方。"));

        await db.SaveChangesAsync();
        logger.LogInformation("Dev CET-4 listening quiz fixture seeded: {Name}", Cet4MockNameCn);
    }

    private static async Task EnsureCet6MockAsync(ListenDbContext db, Guid categoryId, ILogger logger)
    {
        if (await AlbumHasQuizAsync(db, categoryId, Cet6MockNameCn))
            return;

        var album = await EnsureAlbumAsync(
            db,
            categoryId,
            Cet6MockNameCn,
            "[Dev] CET-6 Listening Quiz Fixture",
            sequenceNumber: 0);

        await EnsureEpisodeShellAsync(db, album.Id);

        var conversation = new QuizSection(
            album.Id,
            "Long Conversation 1",
            "W: Professor Chen, I am struggling to choose a topic for my research paper.\n" +
            "M: What areas interest you most?\n" +
            "W: Urban transport and how cities reduce traffic congestion.\n" +
            "M: That is timely. You could compare bike-sharing programs in two cities and look at ridership data.\n" +
            "W: Would interviews with local planners be acceptable as sources?\n" +
            "M: Yes, if you also cite peer-reviewed studies. Aim for a clear thesis by next Friday.\n" +
            "W: Understood. I will draft an outline this weekend.",
            null,
            1,
            "Section A");
        db.QuizSections.Add(conversation);

        db.QuizQuestions.Add(new QuizQuestion(
            conversation.Id, 1,
            "What topic is the woman considering?",
            new[]
            {
                "A) Campus dining services",
                "B) Urban transport and congestion",
                "C) Online language learning",
                "D) International trade policy"
            },
            1, 1,
            "女生明确表示兴趣在城市交通与缓解拥堵。"));

        db.QuizQuestions.Add(new QuizQuestion(
            conversation.Id, 2,
            "What does the professor suggest she do?",
            new[]
            {
                "A) Drop the research paper course",
                "B) Focus only on interviews without studies",
                "C) Compare bike-sharing programs in two cities",
                "D) Write about rural farming instead"
            },
            2, 2,
            "教授建议比较两座城市的共享单车项目并查看 ridership data。"));

        var passage = new QuizSection(
            album.Id,
            "Passage 1",
            "Remote work has changed how many companies hire talent. Firms can recruit specialists " +
            "across regions, but they also face challenges in building team culture. According to a " +
            "recent survey, employees value flexible schedules most, followed by clear communication " +
            "from managers. Experts recommend short daily check-ins rather than long weekly meetings. " +
            "They also warn that without shared goals, remote teams may lose motivation over time.",
            null,
            2,
            "Section B");
        db.QuizSections.Add(passage);

        db.QuizQuestions.Add(new QuizQuestion(
            passage.Id, 3,
            "According to the survey, what do employees value most?",
            new[]
            {
                "A) Free office snacks",
                "B) Flexible schedules",
                "C) Longer weekly meetings",
                "D) Mandatory office attendance"
            },
            1, 1,
            "文中写 employees value flexible schedules most。"));

        db.QuizQuestions.Add(new QuizQuestion(
            passage.Id, 4,
            "What do experts recommend for remote teams?",
            new[]
            {
                "A) Cancel all meetings",
                "B) Hold only annual reviews",
                "C) Prefer short daily check-ins to long weekly meetings",
                "D) Avoid setting shared goals"
            },
            2, 2,
            "专家建议短时每日 check-in，而非冗长周会。"));

        await db.SaveChangesAsync();
        logger.LogInformation("Dev CET-6 listening quiz fixture seeded: {Name}", Cet6MockNameCn);
    }

    private static async Task<bool> AlbumHasQuizAsync(ListenDbContext db, Guid categoryId, string nameCn)
    {
        var album = await db.Albums
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.Name.Chinese == nameCn);
        if (album == null)
            return false;

        return await db.QuizSections.AsNoTracking().AnyAsync(s => s.AlbumId == album.Id);
    }

    private static async Task<Album> EnsureAlbumAsync(
        ListenDbContext db,
        Guid categoryId,
        string nameCn,
        string nameEn,
        int sequenceNumber)
    {
        var existing = await db.Albums
            .FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.Name.Chinese == nameCn);
        if (existing != null)
        {
            if (!existing.IsVisible)
                existing.Show();
            await db.SaveChangesAsync();
            return existing;
        }

        var album = new Album(new MultilingualString(nameCn, nameEn), categoryId, sequenceNumber);
        db.Albums.Add(album);
        await db.SaveChangesAsync();
        return album;
    }

    private static async Task EnsureEpisodeShellAsync(ListenDbContext db, Guid albumId)
    {
        if (await db.Episodes.AnyAsync(e => e.AlbumId == albumId))
            return;

        db.Episodes.Add(new Episode(
            new MultilingualString("完整听力（Dev 联调·无音频）", "Full Listening (Dev fixture · no audio)"),
            albumId,
            "",
            0,
            "开发测试用联调卷。请用下方分段原文与在线题目验证做题流程；勿作为正式内容发布。",
            1,
            "text"));
        await db.SaveChangesAsync();
    }
}
