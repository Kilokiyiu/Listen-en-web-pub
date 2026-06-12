namespace MyCache;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int DefaultExpirationMinutes { get; set; } = 30;

    public string KeyPrefix { get; set; } = "listen_en_web:";
}
