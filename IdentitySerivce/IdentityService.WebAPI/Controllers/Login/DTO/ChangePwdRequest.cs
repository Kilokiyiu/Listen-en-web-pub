using FluentValidation;

namespace IdentityService.WebAPI.Controllers;

public record ChangePwdRequest(string OldPassword, string NewPassword);
public class ChangePwdRequestValidator : AbstractValidator<ChangePwdRequest>
{
    public ChangePwdRequestValidator()
    {
        RuleFor(e => e.OldPassword).NotNull().NotEmpty();
        RuleFor(e => e.NewPassword).NotNull().NotEmpty();
    }
}