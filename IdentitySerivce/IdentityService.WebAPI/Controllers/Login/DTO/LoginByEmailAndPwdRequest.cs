using FluentValidation;

namespace IdentityService.WebAPI.Controllers;

public record LoginByEmailAndPwdRequest(string Email, string Password);

public class LoginByEmailAndPwdRequestValidator : AbstractValidator<LoginByEmailAndPwdRequest>
{
    public LoginByEmailAndPwdRequestValidator()
    {
        RuleFor(e => e.Email).NotNull().NotEmpty();
        RuleFor(e => e.Password).NotNull().NotEmpty();
    }
}
