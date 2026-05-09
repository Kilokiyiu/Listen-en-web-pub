using System.Text;
using IdentitySerivce.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IdentitySerivce.Infrastructure;

public class IdentityRepo : IIdentityRepo
{
    private readonly IdentityUserManager userManager;
    private readonly RoleManager<Role> roleManager;
    private readonly ILogger<IIdentityRepo> logger;

    public IdentityRepo(IdentityUserManager userManager, RoleManager<Role> roleManager, ILogger<IIdentityRepo> logger)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.logger = logger;
    }

    public Task<User?> FindByIdAsync(Guid userId)
    {
        return userManager.FindByIdAsync(userId.ToString());
    }

    public Task<User?> FindByNameAsync(string userName)
    {
        return userManager.FindByNameAsync(userName);
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        return userManager.FindByEmailAsync(email);
    }

    public Task<IdentityResult> AccessFailedAsync(User user)
    {
        return userManager.AccessFailedAsync(user);
    }

    public Task<IdentityResult> CreateAsync(User user, string password)
    {
        return userManager.CreateAsync(user, password);
    }

    public Task<string> GenerateChangeEmailConfirmationTokenAsync(User user, string email)
    {
        return userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    /// <summary>
    /// 确认邮箱token
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="ArgumentException"></exception>
    public async Task ConfirmEmailAsync(Guid id)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            throw new ArgumentException($"id={id},"+nameof(id)+"is not found");
        }
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
    }

    /// <summary>
    /// 修改用户邮箱
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<SignInResult> ChangeEmailAsync(Guid userId, string email, string token)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ArgumentException($"{userId} is not found");
        }
        var changeResult = await userManager.ChangeEmailAsync(user, email, token);
        if (!changeResult.Succeeded)
        {
            await userManager.AccessFailedAsync(user);
            string errMsg = changeResult.Errors.SumErrors();
            logger.LogWarning($"修改邮箱失败，错误信息{errMsg}");
            return SignInResult.Failed;
        }
        else
        {
            await ConfirmEmailAsync(user.Id); //邮箱确认
            return SignInResult.Success;
        }
    }

    /// <summary>
    /// 管理员直接修改邮箱
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newEmail"></param>
    /// <exception cref="ArgumentException"></exception>
    public async Task UpdateEmailAsync(Guid id, string newEmail)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new ArgumentException($"id={id},"+nameof(id)+"is not found");
        }
        user.Email = newEmail;
        await  userManager.UpdateAsync(user);
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        var checkPwdResult = await userManager.CheckPasswordAsync(user, currentPassword);
        if (checkPwdResult == false)
        {
            IdentityError err = new IdentityError();
            err.Code = "PasswordMismatch";
            err.Description = "原密码错误";
            return IdentityResult.Failed(err);
        }
        else
        {
            if (newPassword.Length < 6)
            {
                IdentityError err = new IdentityError();
                err.Code = "PasswordTooShort";
                err.Description = "密码长度不能小于6";
                return IdentityResult.Failed(err);
            }
        }
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetPwdResult = await userManager.ResetPasswordAsync(user, token, newPassword);
        return resetPwdResult;
    }

    /// <summary>
    /// 随机重置密码
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<(IdentityResult, User?, string? password)> ResetPasswordAsync(Guid id)
    {
        var user = await FindByIdAsync(id);
        if (user == null)
        {
            return (ErrorResult("找不到用户"),  null, null);
        }

        string newPassword = GeneratePassword();
        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return (result, null, null);
        }

        return (IdentityResult.Success, user, newPassword);
    }

    /// <summary>
    /// 获取用户角色
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public Task<IList<string>> GetRolesAsync(User user)
    {
        return userManager.GetRolesAsync(user);
    }

    /// <summary>
    /// 添加用户Role
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public async Task<IdentityResult> AddToRoleAsync(User user, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            Role role = new Role {Name = roleName};
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return result;
            }
        }
        return await userManager.AddToRoleAsync(user, roleName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IdentityResult> RemoveUserAsync(Guid userId)
    {
        var user = await FindByIdAsync(userId);
        var userLoginStore = userManager.UserLoginStore;
        var noneCT = default(CancellationToken);
        //一定要删除aspnetuserlogins表中的数据，否则再次用这个外部登录登录的话
        //就会报错：The instance of entity type 'IdentityUserLogin<Guid>' cannot be tracked because another instance with the same key value for {'LoginProvider', 'ProviderKey'} is already being tracked.
        //而且要先删除aspnetuserlogins数据，再软删除User
        var logins = await userLoginStore.GetLoginsAsync(user, noneCT);
        foreach (var login in logins)
        {
            await userLoginStore.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey, noneCT);
        }
        user.SoftDelete();
        var result = await userManager.UpdateAsync(user);
        return result;
    }

    public async Task<SignInResult> CheckForSignInAsync(User user, string password, bool lockOutOnFailure)
    {
        if (await userManager.IsLockedOutAsync(user))
        {
            return SignInResult.LockedOut;
        }
        var success = await userManager.CheckPasswordAsync(user, password);
        if (success)
        {
            return SignInResult.Success;
        }
        else
        {
            if (lockOutOnFailure)
            {
                var result = await AccessFailedAsync(user);
                if (!result.Succeeded)
                {
                    throw new ApplicationException("AccessFailed failed");
                }
            }
            return SignInResult.Failed;
        }
    }

    public async Task<(IdentityResult, User?, string? password)> AddAdminUserAsync(string userName, string email)
    {
        if (await FindByNameAsync(userName) != null)
        {
            return (ErrorResult($"用户名已存在"), null, null);
        }

        if (await FindByEmailAsync(email) != null)
        {
            return (ErrorResult($"邮箱已存在"),  null, null);
        }

        User user = new User(userName);
        user.Email = email;
        user.EmailConfirmed = true;
        string password = GeneratePassword();
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (result, null, null);
        }
        result = await AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            return (result, null, null);
        }
        return (IdentityResult.Success, user, password);
    }
    
    /// <summary>
    /// 生成报错信息
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private static IdentityResult ErrorResult(string msg)
    {
        IdentityError idError = new IdentityError { Description = msg };
        return IdentityResult.Failed(idError);
    }
    
    /// <summary>
    /// 随机生成密码
    /// </summary>
    /// <returns></returns>
    private string GeneratePassword()
    {
        var options = userManager.Options.Password;
        int length = options.RequiredLength;
        bool nonAlphanumeric = options.RequireNonAlphanumeric;
        bool digit = options.RequireDigit;
        bool lowercase = options.RequireLowercase;
        bool uppercase = options.RequireUppercase;
        StringBuilder password = new StringBuilder();
        Random random = new Random();
        while (password.Length < length)
        {
            char c = (char)random.Next(32, 126);
            password.Append(c);
            if (char.IsDigit(c))
                digit = false;
            else if (char.IsLower(c))
                lowercase = false;
            else if (char.IsUpper(c))
                uppercase = false;
            else if (!char.IsLetterOrDigit(c))
                nonAlphanumeric = false;
        }

        if (nonAlphanumeric)
            password.Append((char)random.Next(33, 48));
        if (digit)
            password.Append((char)random.Next(48, 58));
        if (lowercase)
            password.Append((char)random.Next(97, 123));
        if (uppercase)
            password.Append((char)random.Next(65, 91));
        return password.ToString();
    }
}