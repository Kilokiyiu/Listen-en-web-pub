namespace MyJWT;

/// <summary>
/// 配置JWT
/// </summary>
public class JWTOptions
{
    public string Issuer { get; set; } = string.Empty; //签发
    public string Audience { get; set; } = string.Empty; //收取
    public string Key { get; set; } = string.Empty; //密钥
    public int ExpireSeconds { get; set; } //过期时间
}