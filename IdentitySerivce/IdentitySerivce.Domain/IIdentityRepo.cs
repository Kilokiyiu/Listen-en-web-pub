using IdentitySerivce.Domain.Entity;

namespace IdentitySerivce.Domain;

public interface IIdentityRepo
{
    Task<User?> FindByIdAsync(Guid userId);
    Task<User?> FindByNameAsync(string userName);
    Task<User?> FindByEmailAsync(string email);
    Task<IdentityResult> AccessFailedAsync(User user);
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<string> GenerateChangeEmailConfirmationTokenAsync(User user, string email);
    Task ConfirmEmailAsync(Guid id);
    Task<SignInResult> ChangeEmailAsync(Guid userId, string email, string token);
    Task UpdateEmailAsync(Guid id, string newEmail);
    Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<(IdentityResult, User?, string? password)> ResetPasswordAsync(Guid id);
    Task<IList<string>> GetRolesAsync(User user);
    Task<IdentityResult> AddToRoleAsync(User user, string roleName);
    Task<IdentityResult> RemoveFromRoleAsync(User user, string roleName);
    Task<(List<User> users, int total)> QueryUsersAsync(string? keyword, string? role, int page, int pageSize);
    Task<IdentityResult> SetPasswordAsync(Guid userId, string newPassword);
    Task<IdentityResult> LockUserAsync(Guid userId, DateTimeOffset? lockoutEnd);
    Task<IdentityResult> UnlockUserAsync(Guid userId);
    Task<bool> IsLockedOutAsync(User user);
    Task<IdentityResult> RemoveUserAsync(Guid userId);
    Task<SignInResult> CheckForSignInAsync(User user, string password, bool lockOutOnFailure);
    Task<(IdentityResult, User?, string? password)> AddAdminUserAsync(string userName, string email);
}
