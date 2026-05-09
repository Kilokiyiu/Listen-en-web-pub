using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MyJWT;

/// <summary>
/// Authentication JWT token
/// </summary>
public static class AuthenticationToken
{
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, JWTOptions jwtoptions)
    {
        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtoptions.Issuer,
                ValidAudience = jwtoptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtoptions.Key)),
            };
        });
    }
}