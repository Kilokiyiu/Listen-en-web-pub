namespace ListenService.Infrastrucure;

public static class ListenCacheKeys
{
    public const string Categories = "listen:categories";

    public static string Albums(Guid categoryId) => $"listen:albums:{categoryId}";

    public static string Album(Guid albumId) => $"listen:album:{albumId}";

    public static string Episodes(Guid albumId) => $"listen:episodes:{albumId}";

    public static string Episode(Guid episodeId) => $"listen:episode:{episodeId}";

    public const string ListenPrefix = "listen:";
}
