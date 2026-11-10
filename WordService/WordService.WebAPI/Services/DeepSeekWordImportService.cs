using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using WordService.WebAPI.Options;

namespace WordService.WebAPI.Services;

public class DeepSeekWordImportService : IDeepSeekWordImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string SystemPrompt =
        """
        你是英语单词本导入助手。把用户给的杂乱文本整理成单词条目 JSON 数组。
        规则：
        1. 只输出 JSON 数组，不要 markdown，不要解释。每项字段：word、definition、example。
        2. word 只要英文单词或短语，不要音标、不要中文。
        3. 音标、词性（n./vt./adj. 等）、中文释义一律放进 definition，例如："/əˈbændən/ vt. 放弃；遗弃"。
        4. example 有例句就填，没有就空字符串。
        5. 去重（同一单词保留信息更全的一条）；跳过明显不是单词的行。
        6. 尽量保留全部有效词条，不要随意截断；本块内有多少有效词就返回多少。
        """;

    private readonly HttpClient _httpClient;
    private readonly DeepSeekOptions _options;
    private readonly ILogger<DeepSeekWordImportService> _logger;

    public DeepSeekWordImportService(
        HttpClient httpClient,
        IOptions<DeepSeekOptions> options,
        ILogger<DeepSeekWordImportService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<IReadOnlyList<AiOrganizedWordItem>> OrganizeAsync(
        string rawText,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("未配置 DeepSeek API Key，无法使用 AI 整理");

        var text = NormalizeInput(rawText);
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("内容为空");

        if (text.Length > _options.MaxInputChars)
            throw new ArgumentException($"内容过长，请控制在 {_options.MaxInputChars} 字以内");

        var chunks = SplitChunks(text, Math.Max(1000, _options.MaxChunkChars));
        var merged = new Dictionary<string, AiOrganizedWordItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = await CallModelAsync(chunk, cancellationToken);
            foreach (var item in items)
            {
                var word = (item.Word ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(word) || word.Length > 200)
                    continue;

                var definition = TrimOrEmpty(item.Definition, 1000);
                var example = TrimOrEmpty(item.Example, 2000);
                var key = word.ToLowerInvariant();

                if (!merged.TryGetValue(key, out var existing))
                {
                    merged[key] = new AiOrganizedWordItem
                    {
                        Word = word,
                        Definition = definition,
                        Example = example,
                    };
                    continue;
                }

                // 保留更完整的释义/例句
                if (definition.Length > (existing.Definition?.Length ?? 0))
                    existing.Definition = definition;
                if (example.Length > (existing.Example?.Length ?? 0))
                    existing.Example = example;
            }

            var maxEntries = Math.Max(500, _options.MaxOutputEntries);
            if (merged.Count >= maxEntries)
                break;
        }

        var limit = Math.Max(500, _options.MaxOutputEntries);
        return merged.Values.Take(limit).ToList();
    }

    private async Task<List<AiOrganizedWordItem>> CallModelAsync(
        string chunk,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = string.IsNullOrWhiteSpace(_options.Model) ? "deepseek-chat" : _options.Model,
            temperature = 0.1,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new
                {
                    role = "user",
                    content = "请整理以下内容为单词 JSON 数组：\n\n" + chunk,
                },
            },
        };

        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            CombineUrl(_options.BaseUrl, "/chat/completions"));
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey!.Trim());
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var res = await _httpClient.SendAsync(req, cancellationToken);
        var body = await res.Content.ReadAsStringAsync(cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogWarning("DeepSeek API failed: {Status} {Body}", (int)res.StatusCode, Truncate(body, 300));
            throw new InvalidOperationException(DescribeApiError((int)res.StatusCode, body));
        }

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return ParseEntries(content);
    }

    internal static List<AiOrganizedWordItem> ParseEntries(string? content)
    {
        var json = ExtractJsonArray(content);
        if (string.IsNullOrWhiteSpace(json))
            return new List<AiOrganizedWordItem>();

        try
        {
            var list = JsonSerializer.Deserialize<List<AiOrganizedWordItem>>(json, JsonOptions);
            return list ?? new List<AiOrganizedWordItem>();
        }
        catch (JsonException)
        {
            // 容错：有时模型返回对象包一层
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (prop.Value.ValueKind != JsonValueKind.Array) continue;
                        var list = JsonSerializer.Deserialize<List<AiOrganizedWordItem>>(
                            prop.Value.GetRawText(), JsonOptions);
                        if (list != null) return list;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return new List<AiOrganizedWordItem>();
        }
    }

    private static string? ExtractJsonArray(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        var text = content.Trim();
        text = Regex.Replace(text, @"^```(?:json)?\s*", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\s*```$", "");
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        if (start < 0 || end <= start) return null;
        return text[start..(end + 1)];
    }

    private static List<string> SplitChunks(string text, int maxChars)
    {
        if (text.Length <= maxChars)
            return new List<string> { text };

        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var chunks = new List<string>();
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            if (sb.Length > 0 && sb.Length + line.Length + 1 > maxChars)
            {
                chunks.Add(sb.ToString());
                sb.Clear();
            }

            if (line.Length > maxChars)
            {
                if (sb.Length > 0)
                {
                    chunks.Add(sb.ToString());
                    sb.Clear();
                }

                for (var i = 0; i < line.Length; i += maxChars)
                {
                    var len = Math.Min(maxChars, line.Length - i);
                    chunks.Add(line.Substring(i, len));
                }

                continue;
            }

            if (sb.Length > 0) sb.Append('\n');
            sb.Append(line);
        }

        if (sb.Length > 0) chunks.Add(sb.ToString());
        return chunks;
    }

    private static string NormalizeInput(string rawText) =>
        Regex.Replace((rawText ?? string.Empty).Replace("\uFEFF", ""), @"\n{3,}", "\n\n").Trim();

    private static string TrimOrEmpty(string? s, int max)
    {
        var t = (s ?? string.Empty).Trim();
        return t.Length <= max ? t : t[..max];
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        var b = (baseUrl ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrWhiteSpace(b)) b = "https://api.deepseek.com";
        return b + path;
    }

    private static string Truncate(string s, int n) =>
        string.IsNullOrEmpty(s) ? "" : (s.Length <= n ? s : s[..n] + "…");

    private static string DescribeApiError(int status, string body)
    {
        if (status == 401 || status == 403) return "AI 服务鉴权失败，请检查 API Key";
        if (status == 429) return "AI 调用过于频繁，请稍后再试";
        if (status >= 500) return "AI 服务暂时不可用，请稍后再试";
        return "AI 整理失败，请稍后重试";
    }
}
