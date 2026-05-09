using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MyCommonTools.JsonConverters;

namespace MyCommonTools;

public static class JsonExtensions
{
    public readonly static JavaScriptEncoder Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

    public static JsonSerializerOptions CreateJsonSerializerOptions(bool camelCase = false)
    {
        JsonSerializerOptions opt = new JsonSerializerOptions { Encoder = Encoder };
        if (camelCase)
        {
            opt.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
        opt.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
        return opt;
    }
    
    public static string ToJsonString(this object value, bool camelCase = false)
    {
        var opt = CreateJsonSerializerOptions(camelCase);
        return JsonSerializer.Serialize(value, value.GetType(), opt);
    }
    
    public static T? ParseJson<T>(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default(T);
        }

        var opt = CreateJsonSerializerOptions();
        return JsonSerializer.Deserialize<T>(value, opt);
    }
}