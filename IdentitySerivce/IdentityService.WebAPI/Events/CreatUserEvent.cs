namespace IdentityService.WebAPI.Events;

public record CreatUserEvent(Guid Id, string UserName, string Password, string Email);