using KaoyanService.Domain;
using KaoyanService.Domain.Entity;
using Microsoft.Extensions.Options;
using MyCache;

namespace KaoyanService.Infrastructure;

public class CachedKaoyanRepo : IKaoyanRepo
{
    private readonly KaoyanRepo inner;
    private readonly ICacheService cache;
    private readonly CacheOptions options;

    public CachedKaoyanRepo(KaoyanRepo inner, ICacheService cache, IOptions<CacheOptions> options)
    {
        this.inner = inner;
        this.cache = cache;
        this.options = options.Value;
    }

    public Task<ExamPaper[]> GetVisiblePapersAsync(string? series = null) =>
        cache.GetOrSetAsync(
            KaoyanCacheKeys.VisiblePapers(series),
            () => inner.GetVisiblePapersAsync(series),
            DefaultExpiry());

    public Task<ExamPaper?> GetPaperByIdAsync(Guid paperId) =>
        cache.GetOrSetAsync(
            KaoyanCacheKeys.Paper(paperId),
            () => inner.GetPaperByIdAsync(paperId),
            DefaultExpiry());

    public Task<ExamSection[]> GetSectionsByPaperIdAsync(Guid paperId) =>
        cache.GetOrSetAsync(
            KaoyanCacheKeys.Sections(paperId),
            () => inner.GetSectionsByPaperIdAsync(paperId),
            DefaultExpiry());

    public Task<ExamQuestion[]> GetQuestionsBySectionIdsAsync(IEnumerable<Guid> sectionIds) =>
        inner.GetQuestionsBySectionIdsAsync(sectionIds);

    public Task<ExamQuestion[]> GetQuestionsByPaperIdAsync(Guid paperId) =>
        cache.GetOrSetAsync(
            KaoyanCacheKeys.Questions(paperId),
            () => inner.GetQuestionsByPaperIdAsync(paperId),
            DefaultExpiry());

    public Task<ExamPaper[]> GetAllPapersAsync() =>
        // 管理端列表不走长缓存，避免后台改完看不到
        inner.GetAllPapersAsync();

    public Task<ExamQuestion?> GetQuestionByIdAsync(Guid questionId) =>
        inner.GetQuestionByIdAsync(questionId);

    private TimeSpan DefaultExpiry() => TimeSpan.FromMinutes(options.DefaultExpirationMinutes);
}
