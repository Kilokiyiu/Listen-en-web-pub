using System.Security.Claims;
using IdentitySerivce.Domain;
using IdentitySerivce.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.WebAPI.Controllers;

[ApiController]
[Route("/api/identity/Admin/[action]")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IIdentityRepo repo;

    public AdminUsersController(IIdentityRepo repo)
    {
        this.repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? keyword,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (users, total) = await repo.QueryUsersAsync(keyword, role, page, pageSize);
        var items = new List<AdminUserItem>();
        foreach (var user in users)
        {
            var roles = await repo.GetRolesAsync(user);
            var locked = await repo.IsLockedOutAsync(user);
            items.Add(ToItem(user, roles, locked));
        }

        return Ok(new
        {
            code = 200,
            data = new { items, total, page, pageSize }
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("用户名和密码不能为空");
        }

        if (request.Password.Length < 6)
        {
            return BadRequest("密码至少 6 位");
        }

        if (await repo.FindByNameAsync(request.UserName.Trim()) != null)
        {
            return BadRequest("用户名已存在");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && await repo.FindByEmailAsync(request.Email.Trim()) != null)
        {
            return BadRequest("邮箱已存在");
        }

        var user = new User(request.UserName.Trim());
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email.Trim();
            user.EmailConfirmed = true;
        }

        var createResult = await repo.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(FormatErrors(createResult));
        }

        var roles = NormalizeRoles(request.Roles);
        foreach (var roleName in roles)
        {
            var roleResult = await repo.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                return BadRequest(FormatErrors(roleResult));
            }
        }

        return Ok(new { code = 200, data = new { id = user.Id, userName = user.UserName } });
    }

    [HttpPost]
    public async Task<IActionResult> SetRoles([FromBody] AdminSetRolesRequest request)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            return BadRequest("无效用户");
        }

        var user = await repo.FindByIdAsync(request.UserId);
        if (user == null || user.IsDeleted)
        {
            return BadRequest("用户不存在");
        }

        if (IsSelf(request.UserId) && !NormalizeRoles(request.Roles).Contains("Admin"))
        {
            return BadRequest("不能移除自己的管理员权限");
        }

        var currentRoles = await repo.GetRolesAsync(user);
        var targetRoles = NormalizeRoles(request.Roles);

        foreach (var role in currentRoles.Where(r => !targetRoles.Contains(r)))
        {
            var result = await repo.RemoveFromRoleAsync(user, role);
            if (!result.Succeeded)
            {
                return BadRequest(FormatErrors(result));
            }
        }

        foreach (var role in targetRoles.Where(r => !currentRoles.Contains(r)))
        {
            var result = await repo.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                return BadRequest(FormatErrors(result));
            }
        }

        return Ok(new { code = 200, message = "角色已更新" });
    }

    [HttpPost]
    public async Task<IActionResult> SetLock([FromBody] AdminSetLockRequest request)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            return BadRequest("无效用户");
        }

        if (IsSelf(request.UserId))
        {
            return BadRequest("不能封禁自己");
        }

        var user = await repo.FindByIdAsync(request.UserId);
        if (user == null || user.IsDeleted)
        {
            return BadRequest("用户不存在");
        }

        IdentityResult result;
        if (request.Locked)
        {
            DateTimeOffset? end = null;
            if (request.Days is > 0)
            {
                end = DateTimeOffset.UtcNow.AddDays(request.Days.Value);
            }

            result = await repo.LockUserAsync(request.UserId, end);
        }
        else
        {
            result = await repo.UnlockUserAsync(request.UserId);
        }

        if (!result.Succeeded)
        {
            return BadRequest(FormatErrors(result));
        }

        return Ok(new { code = 200, message = request.Locked ? "已封禁" : "已解除封禁" });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromBody] AdminUserIdRequest request)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            return BadRequest("无效用户");
        }

        var (result, _, password) = await repo.ResetPasswordAsync(request.UserId);
        if (!result.Succeeded)
        {
            return BadRequest(FormatErrors(result));
        }

        return Ok(new { code = 200, data = new { password }, message = "密码已重置" });
    }

    [HttpPost]
    public async Task<IActionResult> SetPassword([FromBody] AdminSetPasswordRequest request)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            return BadRequest("无效用户");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest("新密码至少 6 位");
        }

        var user = await repo.FindByIdAsync(request.UserId);
        if (user == null || user.IsDeleted)
        {
            return BadRequest("用户不存在");
        }

        var roles = await repo.GetRolesAsync(user);
        // 按需求：允许修改管理员分组成员密码；普通用户也允许管理员改密
        var result = await repo.SetPasswordAsync(request.UserId, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(FormatErrors(result));
        }

        return Ok(new
        {
            code = 200,
            message = roles.Contains("Admin") ? "管理员密码已更新" : "密码已更新"
        });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser([FromBody] AdminUserIdRequest request)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            return BadRequest("无效用户");
        }

        if (IsSelf(request.UserId))
        {
            return BadRequest("不能删除自己");
        }

        var result = await repo.RemoveUserAsync(request.UserId);
        if (!result.Succeeded)
        {
            return BadRequest(FormatErrors(result));
        }

        return Ok(new { code = 200, message = "用户已删除" });
    }

    private bool IsSelf(Guid userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var selfId) && selfId == userId;
    }

    private static List<string> NormalizeRoles(IEnumerable<string>? roles)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "User", "Admin" };
        var result = (roles ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Where(r => allowed.Contains(r))
            .Select(r => allowed.First(a => a.Equals(r, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (result.Count == 0)
        {
            result.Add("User");
        }

        return result;
    }

    private static string FormatErrors(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(e => e.Description));
    }

    private static AdminUserItem ToItem(User user, IList<string> roles, bool locked)
    {
        return new AdminUserItem
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            Roles = roles.ToList(),
            CreationTime = user.CreationTime,
            IsLocked = locked,
            LockoutEnd = user.LockoutEnd
        };
    }
}

public class AdminUserItem
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreationTime { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}

public class AdminCreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
}

public class AdminSetRolesRequest
{
    public Guid UserId { get; set; }
    public List<string>? Roles { get; set; }
}

public class AdminSetLockRequest
{
    public Guid UserId { get; set; }
    public bool Locked { get; set; }
    public int? Days { get; set; }
}

public class AdminUserIdRequest
{
    public Guid UserId { get; set; }
}

public class AdminSetPasswordRequest
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
