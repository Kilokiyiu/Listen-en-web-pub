using System.Security.Claims;

namespace MyJWT;

public interface IGenerateToken
{
    string BuildToken(IEnumerable<Claim> claims, JWTOptions jwtOptions);
}