using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ReviewAddAccountRequestSubmitViewModelValidator : AbstractValidator<ReviewAddAccountRequestSubmitViewModel>
{
    public const string AcceptAddAccountRequestValidationMessage = "Select if you want to add this training provider or not";

    public ReviewAddAccountRequestSubmitViewModelValidator()
    {
        RuleFor(s => s.AcceptAddAccountRequest)
            .NotNull()
            .WithMessage(AcceptAddAccountRequestValidationMessage);
    }
}
