using ListenService.Domain;
using ListenService.Domain.Entity;
using Microsoft.Extensions.Options;
using MyCache;

namespace ListenService.Infrastrucure;

public class CachedListenRepo : IListenRepo
{
    private readonly ListenRepo inner;
    private readonly ICacheService cache;
    private readonly CacheOptions options;

    public CachedListenRepo(ListenRepo inner, ICacheService cache, IOptions<CacheOptions> options)
    {
        this.inner = inner;
        this.cache = cache;
        this.options = options.Value;
    }

    public Task<Category[]> GetAllCategoriesAsync()
    {
        return cache.GetOrSetAsync(
            ListenCacheKeys.Categories,
            () => inner.GetAllCategoriesAsync(),
            DefaultExpiry());
    }

    public Task<Category> GetCategoryByIdAsync(Guid categoryId)
    {
        return inner.GetCategoryByIdAsync(categoryId);
    }

    public Task<Category?> FindCategoryByCodeAsync(string code)
    {
        return inner.FindCategoryByCodeAsync(code);
    }

    public Task<Album[]> GetAllAlbumAsync(Guid categoryId)
    {
        return cache.GetOrSetAsync(
            ListenCacheKeys.Albums(categoryId),
            () => inner.GetAllAlbumAsync(categoryId),
            DefaultExpiry());
    }

    public Task<(Guid Id, DateTime CreationTime)[]> GetVisibleAlbumSitemapItemsAsync()
    {
        return inner.GetVisibleAlbumSitemapItemsAsync();
    }

    public Task<Album> GetAlbumByIdAsync(Guid albumId)
    {
        return cache.GetOrSetAsync(
            ListenCacheKeys.Album(albumId),
            () => inner.GetAlbumByIdAsync(albumId),
            DefaultExpiry());
    }

    public Task<Album?> FindAlbumByCategoryAndNameAsync(Guid categoryId, string nameChinese)
    {
        return inner.FindAlbumByCategoryAndNameAsync(categoryId, nameChinese);
    }

    public Task<int> GetMaxAlbumSequenceAsync(Guid categoryId)
    {
        return inner.GetMaxAlbumSequenceAsync(categoryId);
    }

    public Task<Episode[]> GetAllEpisodesAsync(Guid albumId)
    {
        return cache.GetOrSetAsync(
            ListenCacheKeys.Episodes(albumId),
            () => inner.GetAllEpisodesAsync(albumId),
            DefaultExpiry());
    }

    public Task<Episode> GetEpisodeByIdAsync(Guid episodeId)
    {
        return cache.GetOrSetAsync(
            ListenCacheKeys.Episode(episodeId),
            () => inner.GetEpisodeByIdAsync(episodeId),
            DefaultExpiry());
    }

    public Task<QuizSection[]> GetQuizSectionsByAlbumIdAsync(Guid albumId) =>
        inner.GetQuizSectionsByAlbumIdAsync(albumId);

    public Task<QuizQuestion[]> GetQuizQuestionsBySectionIdsAsync(IEnumerable<Guid> sectionIds) =>
        inner.GetQuizQuestionsBySectionIdsAsync(sectionIds);

    private TimeSpan DefaultExpiry() => TimeSpan.FromMinutes(options.DefaultExpirationMinutes);
}
