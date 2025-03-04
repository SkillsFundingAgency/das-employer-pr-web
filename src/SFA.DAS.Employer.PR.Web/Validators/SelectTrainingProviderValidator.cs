using FluentValidation;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Validators;

public class SelectTrainingProviderValidator : AbstractValidator<SelectTrainingProviderViewModel>
{
    public const string NoTrainingProviderSelectedErrorMessage = "Type a name or UKPRN and select a provider";

    public SelectTrainingProviderValidator()
    {
        RuleFor(s => s.SearchTerm)
            .NotEmpty()
            .WithMessage(NoTrainingProviderSelectedErrorMessage);
    }
}