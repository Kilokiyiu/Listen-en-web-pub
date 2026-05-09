namespace IdentityService.WebAPI.Events;

public record ResetPwdEvent(Guid Id, string UserName, String Password, String Email);