using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class ReviewAddAccountRequestSubmitViewModelValidator : AbstractValidator<ReviewAddAccountRequestSubmitViewModel>
{
    public const string AcceptAddAccountRequestValidationMessage = "Please select an option";

    public ReviewAddAccountRequestSubmitViewModelValidator()
    {
        RuleFor(s => s.AcceptAddAccountRequest)
            .NotNull()
            .WithMessage(AcceptAddAccountRequestValidationMessage);
    }
}
