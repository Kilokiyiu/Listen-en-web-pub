using DomainCommons;

namespace ListenService.WebAPI.Controllers.Listen.DTO;

public class CategoryPesponse
{
    public Guid Id { get; set; }
    public MultilingualString Name { get; set; }
    public string Code { get; set; } = "";
    public int SequenceNumber { get; set; }
    public string? CoverUrl { get; set; }
}