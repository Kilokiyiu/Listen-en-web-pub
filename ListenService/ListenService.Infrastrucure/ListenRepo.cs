using ListenService.Domain;
using ListenService.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace ListenService.Infrastrucure;

public class ListenRepo : IListenRepo
{
    private readonly ListenDbContext dbContext;
    
    public ListenRepo(ListenDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Category[]> GetAllCategoriesAsync()
    {
        return dbContext.Categories.OrderBy(e => e.SequenceNumber).ToArrayAsync();
    }

    public Task<Category> GetCategoryByIdAsync(Guid categoryId)
    {
        return dbContext.Categories.FindAsync(categoryId).AsTask();
    }

    public Task<Category?> FindCategoryByCodeAsync(string code)
    {
        return dbContext.Categories.FirstOrDefaultAsync(c => c.Code == code);
    }

    public Task<Album[]> GetAllAlbumAsync(Guid categoryId)
    {
        // 只过滤 Album 本身是否可见，不依赖 Episode 的可见性
        // 否则隐藏 Episode 后整个试卷就从列表消失了
        // 按名称降序（试卷名以年份开头，如"2025年6月..."），最新的排在前面
        return dbContext.Albums
            .Where(a => a.CategoryId == categoryId && a.IsVisible)
            .OrderByDescending(a => a.Name.Chinese).ToArrayAsync();
    }

    public Task<Album> GetAlbumByIdAsync(Guid albumId)
    {
        return dbContext.Albums.FindAsync(albumId).AsTask();
    }

    public Task<Album?> FindAlbumByCategoryAndNameAsync(Guid categoryId, string nameChinese)
    {
        return dbContext.Albums
            .FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.Name.Chinese == nameChinese);
    }

    public async Task<int> GetMaxAlbumSequenceAsync(Guid categoryId)
    {
        var albums = dbContext.Albums.Where(a => a.CategoryId == categoryId);
        if (!await albums.AnyAsync())
            return 0;
        return await albums.MaxAsync(a => a.SequenceNumber);
    }

    public Task<Episode[]> GetAllEpisodesAsync(Guid albumId)
    {
        return dbContext.Episodes.Where(e => e.AlbumId == albumId && e.IsVisible)
            .OrderBy(e => e.SequenceNumber).ToArrayAsync();
    }

    public Task<Episode> GetEpisodeByIdAsync(Guid episodeId)
    {
        return dbContext.Episodes.FindAsync(episodeId).AsTask();
    }
}