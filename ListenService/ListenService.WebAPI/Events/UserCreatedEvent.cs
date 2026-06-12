namespace ListenService.WebAPI.Events;

public record UserCreatedEvent(Guid UserId, string UserName, string Email);
