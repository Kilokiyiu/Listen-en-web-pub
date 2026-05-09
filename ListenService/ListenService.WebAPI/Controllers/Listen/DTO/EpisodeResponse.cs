using DomainCommons;

namespace ListenService.WebAPI.Controllers.Listen.DTO;

public class EpisodeResponse
{
    public Guid Id { get; set; }
    public MultilingualString Name { get; set; }
    public Guid AlbumId { get; set; }
    public string AudioUrl { get; set; } = "";
    public double DurationInSecond { get; set; }
    public string SubtitleType { get; set; } = "";
    public string Subtitle { get; set; } = "";
}