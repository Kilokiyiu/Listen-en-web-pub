using FluentValidation;

namespace IdentityService.WebAPI.Controllers;

public record RegisterRequest(string UserName, string Email, string Password);

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(e => e.UserName).NotNull().NotEmpty();
        RuleFor(e => e.Email).NotNull().NotEmpty();
        RuleFor(e => e.Password).NotNull().NotEmpty();
    }
}