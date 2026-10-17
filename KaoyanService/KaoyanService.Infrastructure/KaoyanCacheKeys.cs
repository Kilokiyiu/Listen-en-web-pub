namespace KaoyanService.Infrastructure;

public static class KaoyanCacheKeys
{
    public const string Prefix = "kaoyan:";

    public static string VisiblePapers(string? series) =>
        string.IsNullOrWhiteSpace(series)
            ? "kaoyan:papers:all"
            : $"kaoyan:papers:{series.Trim().ToLowerInvariant()}";

    public static string Paper(Guid paperId) => $"kaoyan:paper:{paperId}";

    public static string Sections(Guid paperId) => $"kaoyan:sections:{paperId}";

    public static string Questions(Guid paperId) => $"kaoyan:questions:{paperId}";
}
