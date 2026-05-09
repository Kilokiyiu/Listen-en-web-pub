namespace IdentitySerivce.Domain;

public static partial class IdentityHelper
{
    /// <summary>
    /// 将错误信息显示更清楚
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static string SumErrors(this IEnumerable<IdentityError> errors)
    {
        var strings = errors.Select(e => $"code={e.Code}, message={e.Description}");
        return string.Join("\n", strings);
    }
}
