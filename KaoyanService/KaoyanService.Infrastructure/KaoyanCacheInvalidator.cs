using MyCache;

namespace KaoyanService.Infrastructure;

public class KaoyanCacheInvalidator
{
    private readonly ICacheService cache;

    public KaoyanCacheInvalidator(ICacheService cache)
    {
        this.cache = cache;
    }

    public Task InvalidateAllAsync() =>
        cache.RemoveByPrefixAsync(KaoyanCacheKeys.Prefix);

    public Task InvalidatePaperAsync(Guid paperId) =>
        Task.WhenAll(
            cache.RemoveAsync(KaoyanCacheKeys.Paper(paperId)),
            cache.RemoveAsync(KaoyanCacheKeys.Sections(paperId)),
            cache.RemoveAsync(KaoyanCacheKeys.Questions(paperId)),
            cache.RemoveByPrefixAsync("kaoyan:papers:"));
}
