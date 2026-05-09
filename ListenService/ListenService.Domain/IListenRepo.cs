using ListenService.Domain.Entity;

namespace ListenService.Domain;

public interface IListenRepo
{
    /// <summary>
    /// 用于获取所有的Category数据
    /// </summary>
    /// <returns></returns>
    Task<Category[]> GetAllCategoriesAsync();

    /// <summary>
    /// 根据Id获取其中一个Category
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    Task<Category> GetCategoryByIdAsync(Guid categoryId);
    
    Task<Category?> FindCategoryByCodeAsync(string code);

    /// <summary>
    /// 获取所有的Album
    /// </summary>
    /// <returns></returns>
    Task<Album[]> GetAllAlbumAsync(Guid categoryId);
    
    /// <summary>
    /// 根据Id获取其中一个Album
    /// </summary>
    /// <param name="albumId"></param>
    /// <returns></returns>
    Task<Album> GetAlbumByIdAsync(Guid albumId);
    
    Task<Album?> FindAlbumByCategoryAndNameAsync(Guid categoryId, string nameChinese);
    
    Task<int> GetMaxAlbumSequenceAsync(Guid categoryId);
    
    /// <summary>
    /// 获取所有的Episode
    /// </summary>
    /// <returns></returns>
    Task<Episode[]> GetAllEpisodesAsync(Guid albumId);
    
    /// <summary>
    /// 根据Id获取其中一个Episode
    /// </summary>
    /// <param name="episodeId"></param>
    /// <returns></returns>
    Task<Episode> GetEpisodeByIdAsync(Guid episodeId);
}