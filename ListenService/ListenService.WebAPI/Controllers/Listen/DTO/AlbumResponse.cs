using DomainCommons;

namespace ListenService.WebAPI.Controllers.Listen.DTO;

public class AlbumResponse
{
    public Guid Id { get; set; }
    public MultilingualString Name { get; set; }
    public Guid CategoryId { get; set; }
    public int SequenceNumber { get; set; }
}