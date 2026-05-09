namespace IdentityService.WebAPI.Events;

public record UserCreateEvent(Guid Id, string UserName, string Password, string Email);