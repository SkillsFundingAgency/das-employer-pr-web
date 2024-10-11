using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ReviewPermissionsRequestSubmitViewModelValidator : AbstractValidator<ReviewPermissionsRequestSubmitViewModel>
{
    public const string AcceptPermissionsValidationMessage = "Select if you want to accept this permissions request";

    public ReviewPermissionsRequestSubmitViewModelValidator()
    {
        RuleFor(s => s.AcceptPermissions)
            .NotNull()
            .WithMessage(AcceptPermissionsValidationMessage);
    }
}
