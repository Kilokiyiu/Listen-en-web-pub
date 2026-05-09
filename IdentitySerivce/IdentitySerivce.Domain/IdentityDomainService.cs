using System.Security.Claims;
using IdentitySerivce.Domain.Entity;
using Microsoft.Extensions.Options;
using MyJWT;

namespace IdentitySerivce.Domain;

public class IdentityDomainService
{
    private readonly IIdentityRepo repo;
    private readonly IGenerateToken tokenGenerator;
    private readonly IOptions<JWTOptions> jwtOptions;

    public IdentityDomainService(IIdentityRepo repo, IGenerateToken tokenGenerator, IOptions<JWTOptions> jwtOptions)
    {
        this.repo = repo;
        this.tokenGenerator = tokenGenerator;
        this.jwtOptions = jwtOptions;
    }

    /// <summary>
    /// 通过邮箱登录
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<(SignInResult result, string? token)> LoginByEmailAndPwdAsync(string email, string password)
    {
        var result = await CheckEmailAndPwdAsync(email, password);
        if (result.Succeeded)
        {
            var user = await repo.FindByEmailAsync(email);
            string token = await BuildTokenAsync(user);
            return(SignInResult.Success, token);
        }
        else
        {
            return (result, null);
        }
    }

    /// <summary>
    /// 通过用户名登录
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<(SignInResult result, string? token)> LoginByUserNameAndPwdAsync(string userName, string password)
    {
        var result = await CheckUserNameAndPwdAsync(userName, password);
        if (result.Succeeded)
        {
            var user = await repo.FindByNameAsync(userName);
            string token = await BuildTokenAsync(user);
            return (SignInResult.Success, token);
        }
        else
        {
            return (result, null);
        }
    }

    /// <summary>
    /// 登录时检查：邮箱是否存在，是否锁定
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task<SignInResult> CheckEmailAndPwdAsync(string userEmail, string password)
    {
        var email = await repo.FindByEmailAsync(userEmail);
        if (email == null)
        {
            return SignInResult.Failed;
        }

        var result = await repo.CheckForSignInAsync(email, password, true);
        return result;
    }
    
    /// <summary>
    /// 登录时检查：用户名是否存在，是否锁定
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task<SignInResult> CheckUserNameAndPwdAsync(string userName, string password)
    {
        var user = await repo.FindByNameAsync(userName);
        if (user == null)
        {
            return SignInResult.Failed;
        }
        var result = await repo.CheckForSignInAsync(user, password, true);
        return result;
    }

    /// <summary>
    /// 生成JWT，用户每次登录时发送给客户端
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<string> BuildTokenAsync(User user)
    {
        var roles = await repo.GetRolesAsync(user);
        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return tokenGenerator.BuildToken(claims, jwtOptions.Value);
    }
}