using KaoyanService.Domain;
using KaoyanService.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace KaoyanService.Infrastructure;

public class KaoyanRepo : IKaoyanRepo
{
    private readonly KaoyanDbContext db;

    public KaoyanRepo(KaoyanDbContext db)
    {
        this.db = db;
    }

    public Task<ExamPaper[]> GetVisiblePapersAsync(string? series = null)
    {
        var q = db.ExamPapers.AsNoTracking().Where(p => p.IsVisible);
        if (!string.IsNullOrWhiteSpace(series))
        {
            var s = ExamPaper.NormalizeSeries(series);
            q = q.Where(p => p.Series == s);
        }

        return q.OrderByDescending(p => p.Year)
            .ThenBy(p => p.Series)
            .ToArrayAsync();
    }

    public Task<ExamPaper?> GetPaperByIdAsync(Guid paperId) =>
        db.ExamPapers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == paperId);

    public Task<ExamSection[]> GetSectionsByPaperIdAsync(Guid paperId) =>
        db.ExamSections.AsNoTracking()
            .Where(s => s.PaperId == paperId)
            .OrderBy(s => s.SequenceNumber)
            .ToArrayAsync();

    public Task<ExamQuestion[]> GetQuestionsBySectionIdsAsync(IEnumerable<Guid> sectionIds)
    {
        var ids = sectionIds.ToArray();
        return db.ExamQuestions.AsNoTracking()
            .Where(q => ids.Contains(q.SectionId))
            .OrderBy(q => q.SequenceNumber)
            .ThenBy(q => q.Number)
            .ToArrayAsync();
    }

    public Task<ExamPaper[]> GetAllPapersAsync() =>
        db.ExamPapers.AsNoTracking()
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.Series)
            .ToArrayAsync();

    public Task<ExamQuestion?> GetQuestionByIdAsync(Guid questionId) =>
        db.ExamQuestions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == questionId);
}
