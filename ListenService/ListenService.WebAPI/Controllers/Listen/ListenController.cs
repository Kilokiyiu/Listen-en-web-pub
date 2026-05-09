using System.ComponentModel.DataAnnotations;
using ListenService.Domain;
using ListenService.WebAPI.Controllers.Listen.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ListenService.WebAPI.Controllers.Listen;

[ApiController]
[Route("[controller]/[action]")]
public class ListenController : ControllerBase
{
    private readonly IListenRepo repo;

    public ListenController(IListenRepo repo)
    {
        this.repo = repo;
    }

    /// <summary>
    /// 获取所有的分类
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<CategoryPesponse[]>> GetCategories()
    {
        var categories = await repo.GetAllCategoriesAsync();
        return categories.Select(e => new CategoryPesponse
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            SequenceNumber = e.SequenceNumber,
            CoverUrl = e.CoverUrl
        }).ToArray();
    }

    /// <summary>
    /// 获取分类下的所有试卷
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<AlbumResponse[]>> GetAlbumsByCategoryId([Required] Guid categoryId)
    {
        var albums = await repo.GetAllAlbumAsync(categoryId);
        return albums.Select(e => new AlbumResponse
        {
            Id = e.Id,
            Name = e.Name,
            CategoryId = e.CategoryId,
            SequenceNumber = e.SequenceNumber
        }).ToArray();
    }

    /// <summary>
    /// 获取一张试卷的所有题目
    /// </summary>
    /// <param name="albumId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<EpisodeResponse[]>> GetEpisodesByAlbumId([Required] Guid albumId)
    {
        var episodes = await repo.GetAllEpisodesAsync(albumId);
        return episodes.Select(e => new EpisodeResponse
        {
            Id = e.Id,
            Name = e.Name,
            AlbumId = e.AlbumId,
            AudioUrl = e.AudioUrl,
            DurationInSecond = e.DurationInSecond,
            SubtitleType = e.SubtitleType,
            Subtitle = e.Subtitle
        }).ToArray();
    }

    /// <summary>
    /// 获取详细的题目信息
    /// </summary>
    /// <param name="episodeId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<EpisodeDetailResponse>> GetDetailEpisodesByEpisodeId([Required] Guid episodeId)
    {
        var episode = await repo.GetEpisodeByIdAsync(episodeId);
        if (episode == null)
        {
            return NotFound("题目不存在");
        }
        return new EpisodeDetailResponse
        {
            Id = episode.Id,
            Name = episode.Name,
            AlbumId = episode.AlbumId,
            AudioUrl = episode.AudioUrl,
            DurationInSecond = episode.DurationInSecond,
            Subtitle = episode.Subtitle,
            SubtitleType = episode.SubtitleType
        };
    }
}
