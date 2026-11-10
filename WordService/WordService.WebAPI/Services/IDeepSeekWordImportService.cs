namespace WordService.WebAPI.Services;

public interface IDeepSeekWordImportService
{
    bool IsConfigured { get; }

    Task<IReadOnlyList<AiOrganizedWordItem>> OrganizeAsync(
        string rawText,
        CancellationToken cancellationToken = default);
}

public class AiOrganizedWordItem
{
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
}
