using IdentitySerivce.Domain.Entity;

namespace IdentitySerivce.Domain;

public interface IIdentityRepo
{
    // ===== 查询用户 =====
    /// <summary>
    /// 根据id获取用户
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<User?> FindByIdAsync(Guid userId);
    
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<User?> FindByNameAsync(string userName);
    
    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> FindByEmailAsync(string email);
    
    /// <summary>
    /// 记录失败次数
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<IdentityResult> AccessFailedAsync(User user);
    
    
    // ===== 创建用户 =====
    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<IdentityResult> CreateAsync(User user, string password);
    

    
    // ===== 邮箱认证 =====
    /// <summary>
    /// 生成修改邮箱确认token
    /// </summary>
    /// <param name="user"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<string> GenerateChangeEmailConfirmationTokenAsync(User user, string email);
    
    /// <summary>
    /// 确认邮箱token
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task ConfirmEmailAsync(Guid id);

    
    // ===== 用户信息管理 =====
    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="email"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<SignInResult> ChangeEmailAsync(Guid userId, string email, string token);
    
    /// <summary>
    /// 由管理员修改用户邮箱
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newEmail"></param>
    /// <returns></returns>
    Task UpdateEmailAsync(Guid id, string newEmail);
    
    /// <summary>
    /// 用户自主修改密码修改用户密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="currentPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    
    /// <summary>
    /// 在用户忘记密码的情况下重置密码
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<(IdentityResult, User?, string? password)> ResetPasswordAsync(Guid id);
    
    
    // ===== 角色管理 =====
    /// <summary>
    /// 获取用户角色
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<IList<string>> GetRolesAsync(User user);
    
    /// <summary>
    /// 给user添加角色role
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roleName"></param>
    /// <returns></returns>
    Task<IdentityResult> AddToRoleAsync(User user, string roleName);
    
    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<IdentityResult> RemoveUserAsync(Guid userId);
    
    
    // ===== 登录验证 =====
    /// <summary>
    /// 登录时检查用户名和密码,以及是否被锁定
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="lockOutOnFailure"></param>
    /// <returns></returns>
    public Task<SignInResult> CheckForSignInAsync(User user, string password, bool lockOutOnFailure);
    

    // ===== 管理员操作 =====
    /// <summary>
    /// 添加管理员
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public Task<(IdentityResult, User?, string? password)> AddAdminUserAsync(string userName, string email);
    

}