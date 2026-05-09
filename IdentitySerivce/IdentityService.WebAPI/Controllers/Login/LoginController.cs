using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using IdentitySerivce.Domain;
using IdentitySerivce.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]

public class LoginController : ControllerBase
{
    private readonly IIdentityRepo repo;
    private readonly IdentityDomainService  domainService;

    public LoginController(IIdentityRepo repo, IdentityDomainService domainService)
    {
        this.repo = repo;
        this.domainService = domainService;
    }

    /// <summary>
    /// 项目第一次部署时，直接创建初始的用户名为admin的管理员账号
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateWorld()
    {
        if (await repo.FindByNameAsync("admin") != null)
        {
            return StatusCode((int)HttpStatusCode.Conflict, "已经被初始化");
        }
        
        //创建初始用户
        User user = new User("admin");
        var result = await repo.CreateAsync(user, "091623");
        Debug.Assert(result.Succeeded);
        
        //给初始用户绑定邮箱并确认
        await repo.UpdateEmailAsync(user.Id, "Kilokiyiu@outlook.com");
        await repo.ConfirmEmailAsync(user.Id);
        
        //给初始用户分配角色
        result = await repo.AddToRoleAsync(user, "User");
        Debug.Assert(result.Succeeded);
        result = await repo.AddToRoleAsync(user, "Admin");
        Debug.Assert(result.Succeeded);
        return Ok();
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserInfo()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await repo.FindByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            return  NotFound();
        }
        return new UserResponse(user.Id, user.Email, user.CreationTime);
    }

    /// <summary>
    /// 邮箱登录
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> LoginByEmailAndPwd(LoginByEmailAndPwdRequest req)
    {
        (var loginResult, string? token) = await domainService.LoginByEmailAndPwdAsync(req.Email, req.Password);
        if (loginResult.Succeeded)
        {
            var user = await repo.FindByEmailAsync(req.Email);
            return Ok(new { token, userName = user!.UserName });
        }
        else if (loginResult.IsLockedOut)
        {
            return StatusCode((int)HttpStatusCode.Locked, "此账号已经被锁定");
        }
        else
        {
            string msg = "登录失败";
            return StatusCode((int)HttpStatusCode.BadRequest, msg);
        }
    }

    /// <summary>
    /// 用户名登录
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> LoginByUserNameAndPwdAsync(LoginByUserNameAndPwdRequest req)
    {
        (var loginResult, string? token) = await domainService.LoginByUserNameAndPwdAsync(req.UserName, req.Password);
        if (loginResult.Succeeded)
        {
            var user = await repo.FindByNameAsync(req.UserName);
            return Ok(new { token, userName = user!.UserName });
        }
        else if (loginResult.IsLockedOut)
        {
            return StatusCode((int)HttpStatusCode.Locked, "用户已经被锁定，请30秒后重试");
        }
        else
        {
            string msg = "登录失败";
            return StatusCode((int)HttpStatusCode.BadRequest, msg);
        }
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> ChangePwdAsync(ChangePwdRequest req)
    {
        Guid userId = Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
        var resetPwdResult = await repo.ChangePasswordAsync(userId, req.OldPassword, req.NewPassword);
        if (resetPwdResult.Succeeded)
        {
            return Ok();
        }
        else
        {
            return BadRequest(resetPwdResult.Errors.SumErrors());
        }
    }

    /// <summary>
    /// 注册用户
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> RegisterAsync(RegisterRequest req)
    {
        var userExist = await repo.FindByNameAsync(req.UserName);
        if (userExist != null)
        {
            return BadRequest("用户已存在");
        }
        var emailExist = await repo.FindByEmailAsync(req.Email);
        if (emailExist != null)
        {
            return BadRequest("邮箱已被注册");
        }

        User user = new User(req.UserName);
        var result = await repo.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.SumErrors());
        }
        
        await repo.UpdateEmailAsync(user.Id, req.Email);
        await repo.ConfirmEmailAsync(user.Id);
        
        await repo.AddToRoleAsync(user, "User");
        return Ok();
    }
}