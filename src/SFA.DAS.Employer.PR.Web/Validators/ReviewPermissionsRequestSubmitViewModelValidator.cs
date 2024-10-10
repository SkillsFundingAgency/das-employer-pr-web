using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ReviewPermissionsRequestSubmitViewModelValidator : AbstractValidator<ReviewPermissionsRequestSubmitViewModel>
{
    public const string AcceptPermissionsValidationMessage = "Please select an option";

    public ReviewPermissionsRequestSubmitViewModelValidator()
    {
        RuleFor(s => s.AcceptPermissions)
            .NotNull()
            .WithMessage(AcceptPermissionsValidationMessage);
    }
}
