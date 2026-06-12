using MyCache;

namespace ListenService.Infrastrucure;

public class ListenCacheInvalidator
{
    private readonly ICacheService cache;

    public ListenCacheInvalidator(ICacheService cache)
    {
        this.cache = cache;
    }

    public Task InvalidateCategoriesAsync()
    {
        return cache.RemoveAsync(ListenCacheKeys.Categories);
    }

    public Task InvalidateCategoryAsync(Guid categoryId)
    {
        return Task.WhenAll(
            cache.RemoveAsync(ListenCacheKeys.Categories),
            cache.RemoveAsync(ListenCacheKeys.Albums(categoryId)));
    }

    public Task InvalidateAlbumAsync(Guid albumId, Guid categoryId)
    {
        return Task.WhenAll(
            cache.RemoveAsync(ListenCacheKeys.Album(albumId)),
            cache.RemoveAsync(ListenCacheKeys.Albums(categoryId)),
            cache.RemoveAsync(ListenCacheKeys.Episodes(albumId)));
    }

    public Task InvalidateEpisodeAsync(Guid episodeId, Guid albumId, Guid categoryId)
    {
        return Task.WhenAll(
            cache.RemoveAsync(ListenCacheKeys.Episode(episodeId)),
            InvalidateAlbumAsync(albumId, categoryId));
    }

    public Task InvalidateAllListenCacheAsync()
    {
        return cache.RemoveByPrefixAsync(ListenCacheKeys.ListenPrefix);
    }
}
