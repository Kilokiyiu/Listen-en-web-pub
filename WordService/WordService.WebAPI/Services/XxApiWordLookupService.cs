using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using WordService.WebAPI.Models;
using WordService.WebAPI.Options;

namespace WordService.WebAPI.Services;

public partial class XxApiWordLookupService : IWordLookupService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient httpClient;
    private readonly XxApiOptions options;
    private readonly ILogger<XxApiWordLookupService> logger;

    public XxApiWordLookupService(
        HttpClient httpClient,
        IOptions<XxApiOptions> options,
        ILogger<XxApiWordLookupService> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<EnglishWordDetailDto?> LookupAsync(string word, CancellationToken cancellationToken = default)
    {
        var query = word.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var dictionary = await TryDictionaryAsync(query, cancellationToken);
        if (dictionary != null)
        {
            return dictionary;
        }

        var translation = await TranslateAsync(query, cancellationToken);
        if (string.IsNullOrWhiteSpace(translation))
        {
            return null;
        }

        return BuildTranslationOnlyResult(query, translation);
    }

    private async Task<EnglishWordDetailDto?> TryDictionaryAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{options.EnglishWordsUrl}?word={Uri.EscapeDataString(query)}");
            AddAuthHeader(request);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<XxApiDictionaryResponse>(body, JsonOptions);
            if (apiResponse?.Code != 200 || apiResponse.Data == null)
            {
                return null;
            }

            return MapDictionary(apiResponse.Data, query);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "xxapi englishwords lookup failed for {Query}", query);
            return null;
        }
    }

    private async Task<string?> TranslateAsync(string query, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            var sgResult = await TrySgTranslateAsync(query, cancellationToken);
            if (!string.IsNullOrWhiteSpace(sgResult))
            {
                return sgResult;
            }
        }

        return await TryFallbackTranslateAsync(query, cancellationToken);
    }

    private async Task<string?> TrySgTranslateAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{options.SgTranslateUrl}?text={Uri.EscapeDataString(query)}");
            AddAuthHeader(request);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<XxApiTranslateResponse>(body, JsonOptions);
            if (apiResponse?.Code == 200 && apiResponse.Data is string text && !string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "xxapi sgtranslate failed for {Query}", query);
        }

        return null;
    }

    private async Task<string?> TryFallbackTranslateAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            var url =
                $"{options.FallbackTranslateUrl}?text={Uri.EscapeDataString(query)}&from=en&to=zh-CN";
            using var response = await httpClient.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<AppWorldsTranslateResponse>(body, JsonOptions);
            if (apiResponse?.Code == 200 && !string.IsNullOrWhiteSpace(apiResponse.Data))
            {
                return apiResponse.Data.Trim();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "fallback translate failed for {Query}", query);
        }

        return null;
    }

    private void AddAuthHeader(HttpRequestMessage request)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
    }

    private static EnglishWordDetailDto MapDictionary(XxApiDictionaryData data, string query)
    {
        return new EnglishWordDetailDto
        {
            Word = string.IsNullOrWhiteSpace(data.Word) ? query : data.Word,
            Ukphone = data.Ukphone,
            Usphone = data.Usphone,
            Ukspeech = data.Ukspeech ?? BuildSpeechUrl(query, 1),
            Usspeech = data.Usspeech ?? BuildSpeechUrl(query, 2),
            Translations = data.Translations?.Select(t => new WordTranslationDto
            {
                Pos = t.Pos ?? string.Empty,
                TranCn = t.TranCn ?? string.Empty,
            }).ToList() ?? [],
            Sentences = data.Sentences?.Select(s => new WordSentenceDto
            {
                SContent = s.SContent ?? string.Empty,
                SCn = s.SCn ?? string.Empty,
            }).ToList() ?? [],
            Phrases = data.Phrases?.Select(p => new WordPhraseDto
            {
                PContent = p.PContent ?? string.Empty,
            }).ToList() ?? [],
            RelWords = data.RelWords?.Select(g => new WordRelGroupDto
            {
                Pos = g.Pos ?? string.Empty,
                Hwds = g.Hwds?.Select(w => new WordRelItemDto
                {
                    Hwd = w.Hwd ?? string.Empty,
                    Tran = w.Tran ?? string.Empty,
                }).ToList() ?? [],
            }).ToList() ?? [],
            Synonyms = data.Synonyms?.Select(g => new WordSynonymGroupDto
            {
                Pos = g.Pos ?? string.Empty,
                Hwds = g.Hwds?.Select(w => new WordSynonymItemDto
                {
                    Word = w.Word ?? string.Empty,
                }).ToList() ?? [],
            }).ToList() ?? [],
        };
    }

    private static EnglishWordDetailDto BuildTranslationOnlyResult(string query, string translation)
    {
        return new EnglishWordDetailDto
        {
            Word = query,
            Ukspeech = BuildSpeechUrl(query, 1),
            Usspeech = BuildSpeechUrl(query, 2),
            Translations =
            [
                new WordTranslationDto
                {
                    Pos = string.Empty,
                    TranCn = translation,
                },
            ],
            Sentences =
            [
                new WordSentenceDto
                {
                    SContent = query,
                    SCn = translation,
                },
            ],
        };
    }

    private static string BuildSpeechUrl(string text, int type) =>
        $"https://dict.youdao.com/dictvoice?audio={Uri.EscapeDataString(text)}&type={type}";

    private sealed class XxApiDictionaryResponse
    {
        public int Code { get; set; }
        public XxApiDictionaryData? Data { get; set; }
    }

    private sealed class XxApiDictionaryData
    {
        public string? Word { get; set; }
        public string? Ukphone { get; set; }
        public string? Usphone { get; set; }
        public string? Ukspeech { get; set; }
        public string? Usspeech { get; set; }
        public List<XxApiTranslation>? Translations { get; set; }
        public List<XxApiSentence>? Sentences { get; set; }
        public List<XxApiPhrase>? Phrases { get; set; }
        public List<XxApiRelGroup>? RelWords { get; set; }
        public List<XxApiSynonymGroup>? Synonyms { get; set; }
    }

    private sealed class XxApiTranslation
    {
        public string? Pos { get; set; }

        [JsonPropertyName("tran_cn")]
        public string? TranCn { get; set; }
    }

    private sealed class XxApiSentence
    {
        [JsonPropertyName("s_content")]
        public string? SContent { get; set; }

        [JsonPropertyName("s_cn")]
        public string? SCn { get; set; }
    }

    private sealed class XxApiPhrase
    {
        [JsonPropertyName("p_content")]
        public string? PContent { get; set; }
    }

    private sealed class XxApiRelGroup
    {
        public string? Pos { get; set; }
        public List<XxApiRelItem>? Hwds { get; set; }
    }

    private sealed class XxApiRelItem
    {
        public string? Hwd { get; set; }
        public string? Tran { get; set; }
    }

    private sealed class XxApiSynonymGroup
    {
        public string? Pos { get; set; }
        public List<XxApiSynonymItem>? Hwds { get; set; }
    }

    private sealed class XxApiSynonymItem
    {
        public string? Word { get; set; }
    }

    private sealed class XxApiTranslateResponse
    {
        public int Code { get; set; }
        public string? Data { get; set; }
    }

    private sealed class AppWorldsTranslateResponse
    {
        public int Code { get; set; }
        public string? Data { get; set; }
    }
}

public static partial class EnglishQueryValidator
{
    [GeneratedRegex(@"[\u4e00-\u9fff]", RegexOptions.Compiled)]
    private static partial Regex ChinesePattern();

    [GeneratedRegex(@"^[a-zA-Z0-9\s\-'.,!?;:()""/]+$", RegexOptions.Compiled)]
    private static partial Regex EnglishTextPattern();

    public static bool IsValid(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var query = text.Trim();
        if (query.Length > 500 || !query.Any(char.IsLetter))
        {
            return false;
        }

        if (ChinesePattern().IsMatch(query))
        {
            return false;
        }

        return EnglishTextPattern().IsMatch(query);
    }
}
