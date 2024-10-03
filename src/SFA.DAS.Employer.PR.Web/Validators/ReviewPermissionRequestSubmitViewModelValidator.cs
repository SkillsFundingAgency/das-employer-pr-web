using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ReviewPermissionRequestSubmitViewModelValidator : AbstractValidator<ReviewPermissionRequestSubmitViewModel>
{
    public const string AcceptPermissionsValidationMessage = "Please select an option";

    public ReviewPermissionRequestSubmitViewModelValidator()
    {
        RuleFor(s => s.AcceptPermissions)
            .NotNull()
            .WithMessage(AcceptPermissionsValidationMessage);
    }
}
