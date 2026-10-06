using KaoyanService.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KaoyanService.Infrastructure;

/// <summary>
/// 播种历年试卷壳（英语一/二 + 完形/阅读分区）。
/// 正文与题目需经管理端导入；此处不含受版权保护的真题原文。
/// </summary>
public static class PaperCatalogSeeder
{
    public const int StartYear = 2010;
    public const int EndYear = 2025;

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KaoyanDbContext>();

        var existingKeys = await db.ExamPapers
            .AsNoTracking()
            .Select(p => new { p.Year, p.Series })
            .ToListAsync();
        var keySet = existingKeys.Select(x => $"{x.Year}:{x.Series}").ToHashSet();

        var added = 0;
        foreach (var year in Enumerable.Range(StartYear, EndYear - StartYear + 1))
        {
            foreach (var series in new[] { ExamPaper.SeriesEnglish1, ExamPaper.SeriesEnglish2 })
            {
                var key = $"{year}:{series}";
                if (keySet.Contains(key))
                    continue;

                var title = $"{year}年考研{ExamPaper.SeriesDisplayName(series)}";
                var paper = new ExamPaper(year, series, title, (EndYear - year) * 2 + (series == ExamPaper.SeriesEnglish1 ? 0 : 1));
                db.ExamPapers.Add(paper);

                db.ExamSections.Add(new ExamSection(
                    paper.Id,
                    ExamSection.TypeCloze,
                    "完形填空",
                    "",
                    1));
                db.ExamSections.Add(new ExamSection(
                    paper.Id,
                    ExamSection.TypeReading,
                    "阅读理解",
                    "",
                    2));
                added++;
            }
        }

        if (added > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Kaoyan paper catalog seeded: {Count} papers", added);
        }

        await EnsureDemoContentAsync(db, logger);
    }

    /// <summary>
    /// 为最新一年英语一补一份原创演示题，便于联调在线做题。
    /// </summary>
    private static async Task EnsureDemoContentAsync(KaoyanDbContext db, ILogger logger)
    {
        var demoPaper = await db.ExamPapers
            .FirstOrDefaultAsync(p => p.Year == EndYear && p.Series == ExamPaper.SeriesEnglish1);
        if (demoPaper == null)
            return;

        var cloze = await db.ExamSections
            .FirstOrDefaultAsync(s => s.PaperId == demoPaper.Id && s.SectionType == ExamSection.TypeCloze);
        if (cloze == null)
            return;

        var hasQuestions = await db.ExamQuestions.AnyAsync(q => q.SectionId == cloze.Id);
        if (hasQuestions)
            return;

        cloze.Update(
            "完形填空（演示）",
            "Learning a language takes time. Practice every day and you will __1__ progress. " +
            "Reading widely can __2__ your vocabulary.",
            cloze.SequenceNumber);

        db.ExamQuestions.Add(new ExamQuestion(
            cloze.Id, 1, "",
            new[] { "A) make", "B) do", "C) take", "D) give" },
            0, 1, "固定搭配 make progress。"));
        db.ExamQuestions.Add(new ExamQuestion(
            cloze.Id, 2, "",
            new[] { "A) reduce", "B) expand", "C) ignore", "D) hide" },
            1, 2, "广泛阅读有助于扩大词汇量。"));

        var reading = await db.ExamSections
            .FirstOrDefaultAsync(s => s.PaperId == demoPaper.Id && s.SectionType == ExamSection.TypeReading);
        if (reading != null)
        {
            reading.Update(
                "阅读理解（演示）",
                "Many students find that short daily reading sessions are more effective than long weekly ones. " +
                "Consistency helps the brain form lasting memory traces.",
                reading.SequenceNumber);

            db.ExamQuestions.Add(new ExamQuestion(
                reading.Id, 1,
                "According to the passage, what is more effective?",
                new[]
                {
                    "A) One long session each week",
                    "B) Short daily reading sessions",
                    "C) Avoiding reading entirely",
                    "D) Only listening practice"
                },
                1, 1, "文中明确提到 short daily reading sessions 更有效。"));
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Kaoyan demo quiz content ensured for {Year} eng1", EndYear);
    }
}
