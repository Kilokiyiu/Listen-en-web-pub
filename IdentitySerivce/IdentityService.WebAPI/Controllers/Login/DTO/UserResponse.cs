namespace IdentityService.WebAPI.Controllers;

public record UserResponse (Guid Id, string Email, DateTime CreationTime);
